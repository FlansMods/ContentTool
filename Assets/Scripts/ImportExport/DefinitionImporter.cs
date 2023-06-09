using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class DefinitionImporter : MonoBehaviour
{
	public const string IMPORT_ROOT = "Import/Content Packs";
	public const string MODEL_IMPORT_ROOT = "Import/Java Models";
	public const string ASSET_ROOT = "Assets/Content Packs";
	public string ExportRoot = "Export";

	public List<ContentPack> Packs = new List<ContentPack>();
	public List<string> UnimportedPacks = new List<string>();

	public void CheckInit()
	{
		UnimportedPacks.Clear();
		foreach(DirectoryInfo subDir in new DirectoryInfo(IMPORT_ROOT).EnumerateDirectories())
		{
			ContentPack existing = FindContentPack(subDir.Name);
			if(existing == null)
			{
				UnimportedPacks.Add(subDir.Name);
			}
		}
	}


	public ContentPack FindContentPack(string packName)
	{
		// First, see if we already cached it
		foreach(ContentPack pack in Packs)
			if(pack.name == packName)
				return pack;
		
		// Second, try to find it in the asset database
		foreach(string cpPath in AssetDatabase.FindAssets("t:ContentPack"))
		{
			ContentPack loadedPack = AssetDatabase.LoadAssetAtPath<ContentPack>(cpPath);
			if(loadedPack != null)
			{
				Packs.Add(loadedPack);
				return loadedPack;
			}
		}

		return null;
	}

	public bool ImportPack(string packName)
	{
		string modName = packName;
		string assetPath = $"{ASSET_ROOT}/{packName}/{packName}.asset";
		if(File.Exists(assetPath))
		{
			Debug.Log($"--------------------");
			Debug.Log($"Creating backup of {packName}");
			for(int i = Packs.Count - 1; i >= 0; i--)
			{
				if(Packs[i] == null)
					Packs.RemoveAt(i);
				else if(Packs[i].name == packName)
				{
					modName = Packs[i].ModName;
					Packs.RemoveAt(i);
				}
			}
			File.Copy(assetPath, $"{assetPath}.bak", true);
			File.Delete(assetPath);
			AssetDatabase.DeleteAsset(assetPath);
			AssetDatabase.Refresh();
		}
		else
		{
			Debug.Log($"--------------------");
			Debug.Log($"Existing {packName} not found");
		}

		ContentPack pack = FindContentPack(packName);
		if(pack != null)
		{
			modName = pack.ModName;
			Packs.Remove(pack);
		}
		else
		{
			pack = ScriptableObject.CreateInstance<ContentPack>();
		}

		Debug.Log($"--------------------");
		Debug.Log($"Importing {packName}");

		pack.name = modName;
		pack.ModName = modName;

		TYPE_LOOKUP = new Dictionary<string, InfoType>[DefinitionTypes.NUM_TYPES];
		//MODEL_LOOKUP = new Dictionary<string, Model>[DefinitionTypes.NUM_TYPES];

		ImportAllTypeFiles(pack);
		ConvertAllTypesToDefinitions(packName, pack);

		//MODEL_LOOKUP = null;
		TYPE_LOOKUP = null;

		Debug.Log($"--------------------");
		Debug.Log($"Saving {pack} to {assetPath}");

		AssetDatabase.CreateAsset(pack, assetPath);
		if(!Packs.Contains(pack))
			Packs.Add(pack);

		return true;
	}

	public static Dictionary<string, InfoType>[] TYPE_LOOKUP;
	public static bool TryGetType<T>(EDefinitionType type, string key, out T infoType) where T : InfoType
	{
		if(TYPE_LOOKUP[(int)type].TryGetValue(key, out InfoType temp))
		{
			infoType = (T)temp;
			return true;
		}
		foreach(var kvp in TYPE_LOOKUP[(int)type])
		{
			if(kvp.Value.shortName == key)
			{
				infoType = (T)kvp.Value;
				return true;
			}
		}

		infoType = null;
		return false;
	}
	public static bool TryGetType(EDefinitionType type, string key, out InfoType infoType)
	{
		return TYPE_LOOKUP[(int)type].TryGetValue(key, out infoType);
	}

	public void ImportAllTypeFiles(ContentPack pack)
	{
		for(int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
		{
			TYPE_LOOKUP[i] = new Dictionary<string, InfoType>();
			EDefinitionType defType = (EDefinitionType)i;
			DirectoryInfo dir = new DirectoryInfo($"{IMPORT_ROOT}/{pack.name}/{defType.Folder()}");
			if(dir.Exists)
			{
				DirectoryInfo assetDir = new DirectoryInfo($"{ASSET_ROOT}/{pack.name}/{defType.OutputFolder()}");
				if(!assetDir.Exists)
					assetDir.Create();

				foreach(FileInfo file in dir.EnumerateFiles())
				{
					try
					{
						InfoType imported = ImportType(pack, defType, file.Name.Split(".")[0]);
						if(imported != null)
						{
							TYPE_LOOKUP[i].Add(imported.shortName, imported);

							if(imported.modelString != null && imported.modelString.Length > 0)
							{
								string modelPath = $"{MODEL_IMPORT_ROOT}/{imported.modelFolder}/Model{imported.modelString}.java";
								imported.model = JavaModelImporter.ImportJava(modelPath, imported);			
							}
						}
						else
						{
							Debug.LogWarning($"Failed to import {pack.name}:{file.Name}");
						}
					}
					catch(Exception e)
					{
						Debug.LogError($"Failed to load {file.Name} due to exception: {e.Message}, {e.StackTrace}");
					}
				}
			}
			else
			{
				Debug.Log($"Could not find directory {dir}");
			}
		}
	}

	public void ConvertAllTypesToDefinitions(string packName, ContentPack pack)
	{
		for(int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
		{
			foreach(var kvp in TYPE_LOOKUP[i])
			{
				Definition def = ConvertDefinition(packName, (EDefinitionType)i, kvp.Key, kvp.Value);
				if(def != null)
				{ 
					pack.Content.Add(def);
				}
			}
		}
	}

	private Texture2D ImportTexture(string packName, string textureName)
	{
		return ImportTexture(packName, textureName, Utils.ToLowerWithUnderscores(textureName));
	}

	private Texture2D ImportTexture(string packName, string textureName, string outputTextureName)
	{
		if(textureName != null && textureName.Length > 0)
		{
			string path = $"{IMPORT_ROOT}/{packName}/assets/flansmod/{textureName}";
			if(File.Exists(path))
			{
				string importDst = $"{ASSET_ROOT}/{packName}/textures/{outputTextureName}";
				if(!Directory.Exists(new FileInfo(importDst).DirectoryName))
				{
					Directory.CreateDirectory(new FileInfo(importDst).DirectoryName);
				}
				Debug.Log($"Imported texture from {path} to {importDst}");
				File.Copy(path, importDst, true);
				return AssetDatabase.LoadAssetAtPath<Texture2D>(importDst);
			}
			else
			{
				Debug.LogError($"Could not find texture at {path}");
			}
		}
		return null;
	}


    public InfoType ImportType(ContentPack pack, EDefinitionType type, string fileName)
	{
		Debug.Log($"Importing {pack.name}:{type}/{fileName}");
		string importFilePath = $"{IMPORT_ROOT}/{pack.name}/{type.Folder()}/{fileName}.txt";

		// Read the .txt file
		TypeFile file = new TypeFile(fileName);
		string[] lines = File.ReadAllLines(importFilePath);
		foreach(string line in lines)
		{
			file.addLine(line);
		}

		// Load it into a legacy InfoType
		InfoType infoType = TxtImport.Import(file, type);
		return infoType;
	}

	public Definition ConvertDefinition(string packName, EDefinitionType type, string shortName, InfoType infoType)
	{
		// All clear, let's import it
		Definition def = type.CreateInstance();
		if(def == null)
			return null;

		// Convert it to a definition
		InfoToDefinitionConverter.Convert(type, infoType, def);
		def.name = infoType.shortName;
		def.Model = infoType.model;

		// Copy textures
		if(infoType.texture != null && infoType.texture.Length > 0)
			def.Skin = ImportTexture(packName, $"skins/{infoType.texture}.png", $"skins/{Utils.ToLowerWithUnderscores(infoType.texture)}.png");
		if(infoType.iconPath != null && infoType.iconPath.Length > 0)
		{
			def.Icon = ImportTexture(packName, $"textures/items/{infoType.iconPath}.png", $"items/{infoType.iconPath}.png");
			if(def.Model == null)
			{
				def.Model = new Model()
				{
					Type = Model.ModelType.Item,
				};
			}
			def.Model.icon = Utils.ToLowerWithUnderscores(infoType.iconPath);
		}
		if(infoType is PaintableType paintable)
		{
			foreach(Paintjob paintjob in paintable.paintjobs)
			{
				ImportTexture(packName, $"skins/{paintjob.textureName}.png", $"skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png");
				ImportTexture(packName, $"textures/items/{paintjob.iconName}.png", $"items/{paintjob.iconName}.png");
			}
		}
		if(infoType is BoxType box)
		{	
			// Boxes need a model made from their side textures
			def.Model = JavaModelImporter.CreateBoxModel(packName, box);
			def.AdditionalTextures.Add(new Definition.AdditionalTexture()
			{
				name = $"textures/blocks/{Utils.ToLowerWithUnderscores(box.bottomTexturePath)}.png",
				texture = ImportTexture(packName, $"blocks/{box.bottomTexturePath}.png")
			});
			def.AdditionalTextures.Add(new Definition.AdditionalTexture()
			{
				name = $"textures/blocks/{Utils.ToLowerWithUnderscores(box.sideTexturePath)}.png",
				texture = ImportTexture(packName, $"blocks/{box.sideTexturePath}.png")
			});
			def.AdditionalTextures.Add(new Definition.AdditionalTexture()
			{
				name = $"textures/blocks/{Utils.ToLowerWithUnderscores(box.topTexturePath)}.png",
				texture = ImportTexture(packName, $"blocks/{box.topTexturePath}.png")
			});
		}


		//if(pack.HasContent(shortName))
		//	return false;

		string assetPath = $"{ASSET_ROOT}/{packName}/{type.OutputFolder()}/{Utils.ToLowerWithUnderscores(shortName)}.asset";
		Debug.Log($"Saving {def} to {assetPath}");
		AssetDatabase.CreateAsset(def, assetPath);
		return AssetDatabase.LoadAssetAtPath<Definition>(assetPath);
	}

	public void ExportPack(string packName)
	{
		ContentPack pack = FindContentPack(packName);
		if(pack == null)
		{
			Debug.LogError($"Failed to find pack {packName}");
			return;
		}
		string dataExportFolder = $"{ExportRoot}/data/{pack.ModName}";
		string assetExportFolder = $"{ExportRoot}/assets/{pack.ModName}";
		string itemModelsExportFolder = $"{ExportRoot}/assets/{pack.ModName}/models/item";
		string blockModelsExportFolder = $"{ExportRoot}/assets/{pack.ModName}/models/block";
		string blockstatesExportFolder = $"{ExportRoot}/assets/{pack.ModName}/blockstates";
		string itemTextureExportFolder = $"{ExportRoot}/assets/{pack.ModName}/textures/item";
		string skinsExportFolder = $"{ExportRoot}/assets/{pack.ModName}/textures/skins";
		string guiTextureExportFolder = $"{ExportRoot}/assets/{pack.ModName}/textures/gui";

		if(!Directory.Exists(dataExportFolder))
			Directory.CreateDirectory(dataExportFolder);
		if(!Directory.Exists(assetExportFolder))
			Directory.CreateDirectory(assetExportFolder);
		if(!Directory.Exists(blockstatesExportFolder))
			Directory.CreateDirectory(blockstatesExportFolder);
		if(!Directory.Exists(itemModelsExportFolder))
			Directory.CreateDirectory(itemModelsExportFolder);
		if(!Directory.Exists(blockModelsExportFolder))
			Directory.CreateDirectory(blockModelsExportFolder);
		if(!Directory.Exists(skinsExportFolder))
			Directory.CreateDirectory(skinsExportFolder);
		if(!Directory.Exists(guiTextureExportFolder))
			Directory.CreateDirectory(guiTextureExportFolder);
		if(!Directory.Exists(itemTextureExportFolder))
			Directory.CreateDirectory(itemTextureExportFolder);

		foreach(Definition def in pack.Content)
		{
			string item_name = Utils.ToLowerWithUnderscores(def.name);

			try
			{
				
				string exportFolder = $"{dataExportFolder}/{DefinitionTypes.GetFromObject(def).OutputFolder()}";
				string exportTo = $"{exportFolder}/{item_name}.json";
				if(!Directory.Exists(exportFolder))
					Directory.CreateDirectory(exportFolder);
				JToken jToken = JsonExporter.Export(def);
				JObject jRoot = jToken as JObject;
				using(StringWriter stringWriter = new StringWriter())
				using(JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
				{
					jsonWriter.Formatting = Formatting.Indented;
					jsonWriter.Indentation = 4;
					jRoot.WriteTo(jsonWriter);
					Debug.Log($"Exporting {item_name} to {exportTo}");
					File.WriteAllText(exportTo, stringWriter.ToString());
				}

				Dictionary<string, string> textures = new Dictionary<string, string>();
				if(def is GunDefinition gunDef)
				{
					foreach(PaintjobDefinition paintDef in gunDef.paints.paintjobs)
					{
						string paintName = Utils.ToLowerWithUnderscores(paintDef.textureName);
						textures.Add(paintName, $"{pack.ModName}:skins/{paintName}");
					}
				}

				if(def.Model != null)
				{
					QuickJSONBuilder itemModelBuilder = new QuickJSONBuilder();
					if(JsonModelExporter.ExportItemModel(def.Model, textures, pack.ModName, item_name, itemModelBuilder))
					{
						using(StringWriter stringWriter = new StringWriter())						
						using(JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
						{
							jsonWriter.Formatting = Formatting.Indented;
							jsonWriter.Indentation = 4;
							itemModelBuilder.Root.WriteTo(jsonWriter);
							string exportModelTo = $"{itemModelsExportFolder}/{item_name}.json";
							Debug.Log($"Exporting {def.Model.name} item model to {exportModelTo}");
							File.WriteAllText(exportModelTo, stringWriter.ToString());
						}
					}

					QuickJSONBuilder blockModelBuilder = new QuickJSONBuilder();
					if(JsonModelExporter.ExportBlockModel(def.Model, pack.ModName, item_name, blockModelBuilder))
					{
						using(StringWriter stringWriter = new StringWriter())						
						using(JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
						{
							jsonWriter.Formatting = Formatting.Indented;
							jsonWriter.Indentation = 4;
							blockModelBuilder.Root.WriteTo(jsonWriter);
							string exportModelTo = $"{blockModelsExportFolder}/{item_name}.json";
							Debug.Log($"Exporting {def.Model.name} block model to {exportModelTo}");
							File.WriteAllText(exportModelTo, stringWriter.ToString());
						}
					}

					QuickJSONBuilder itemVariantModelExporter = new QuickJSONBuilder();
					if(JsonModelExporter.ExportInventoryVariantModel(def.Model, pack.ModName, item_name, itemVariantModelExporter))
					{
						using(StringWriter stringWriter = new StringWriter())						
						using(JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
						{
							jsonWriter.Formatting = Formatting.Indented;
							jsonWriter.Indentation = 4;
							itemVariantModelExporter.Root.WriteTo(jsonWriter);
							string exportModelTo = $"{itemModelsExportFolder}/{item_name}_inventory.json";
							Debug.Log($"Exporting {def.Model.name} inventory variant model to {exportModelTo}");
							File.WriteAllText(exportModelTo, stringWriter.ToString());
						}
					}
				}
				if(def.Skin != null)
				{
					foreach(var kvp in textures)
					{
						string src = $"{ASSET_ROOT}/{packName}/Textures/skins/{kvp.Key}.png";
						string dst = $"{skinsExportFolder}/{kvp.Key}.png";
						
						Debug.Log($"Copying skin texture from {src} to {dst}");
						File.Copy(src, dst, true);
					}

					{
						string src = $"{ASSET_ROOT}/{packName}/Textures/skins/{def.Skin.name}.png";
						string dst = $"{skinsExportFolder}/{def.Skin.name}.png";
						
						Debug.Log($"Copying skin texture from {src} to {dst}");
						File.Copy(src, dst, true);
					}
				}
				if(def.Icon != null)
				{
					string src = $"{ASSET_ROOT}/{packName}/Textures/items/{def.Icon.name}.png";
					string dst = $"{itemTextureExportFolder}/{Utils.ToLowerWithUnderscores(def.Icon.name)}.png";
					
					Debug.Log($"Copying icon texture from {src} to {dst}");
					File.Copy(src, dst, true);
				}
				foreach(Definition.AdditionalTexture texture in def.AdditionalTextures)
				{
					string src = AssetDatabase.GetAssetPath(texture.texture);
					string dst = $"{assetExportFolder}/{texture.name}";

					Debug.Log($"Copying additional texture from {src} to {dst}");
					File.Copy(src, dst, true);
				}
				

				//if(TryGetModel(DefinitionTypes.GetFromObject(def)))

				// Export icons

			
				

				

				// Export blockstates


				// Export textures


			}
			catch(Exception e)
			{
				Debug.LogError($"Failed to export {item_name} because of {e.Message} at {e.StackTrace}");
			}
		}
	}
}
