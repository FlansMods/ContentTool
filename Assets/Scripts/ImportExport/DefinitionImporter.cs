using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class DefinitionImporter : MonoBehaviour
{
	private static DefinitionImporter _inst = null;
	public static DefinitionImporter inst
	{
		get
		{
			if (_inst == null)
				_inst = FindObjectOfType<DefinitionImporter>();
			if(_inst == null)
			{
				GameObject go = new GameObject("DefinitionImporter");
				_inst = go.AddComponent<DefinitionImporter>();
			}
			return _inst;
		}
	}

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
			if(pack != null)
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

		TYPE_LOOKUP = new Dictionary<string, InfoType>[DefinitionTypes.NUM_TYPES];
		//MODEL_LOOKUP = new Dictionary<string, Model>[DefinitionTypes.NUM_TYPES];

		ImportLangFiles(packName);
		ImportSounds(packName, pack);
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
		DirectoryInfo modelsFolder = new DirectoryInfo($"{ASSET_ROOT}/{pack.ModName}/models/");
		if (!modelsFolder.Exists)
			modelsFolder.Create();

		for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
		{
			TYPE_LOOKUP[i] = new Dictionary<string, InfoType>();
			EDefinitionType defType = (EDefinitionType)i;
			DirectoryInfo dir = new DirectoryInfo($"{IMPORT_ROOT}/{pack.ModName}/{defType.Folder()}");
			if(dir.Exists)
			{
				DirectoryInfo assetDir = new DirectoryInfo($"{ASSET_ROOT}/{pack.ModName}/{defType.OutputFolder()}");
				if(!assetDir.Exists)
					assetDir.Create();
					

				foreach (FileInfo file in dir.EnumerateFiles())
				{
					//try
					{
						InfoType imported = ImportType(pack, defType, file.Name.Split(".")[0]);
						if(imported != null)
						{
							TYPE_LOOKUP[i].Add(imported.shortName, imported);

							if(imported.modelString != null && imported.modelString.Length > 0)
							{
								string modelPath = $"{MODEL_IMPORT_ROOT}/{imported.modelFolder}/Model{imported.modelString}.java";
								TurboRig rig = JavaModelImporter.ImportTurboModel(pack.ModName, modelPath, imported);
								if (rig != null)
								{
									string importToPath = $"{ASSET_ROOT}/{pack.ModName}/models/{rig.name}.asset";
									AssetDatabase.CreateAsset(rig, importToPath);
								}
								else
									Debug.LogError($"Failed to import {pack.name}:{file.Name}");
							}
						}
						else
						{
							Debug.LogWarning($"Failed to import {pack.name}:{file.Name}");
						}
					}
					//catch(Exception e)
					{
					//	Debug.LogError($"Failed to load {file.Name} due to exception: {e.Message}, {e.StackTrace}");
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
					VerifyDefinition(def, pack);
					EditorUtility.SetDirty(def);
					AssetDatabase.SaveAssetIfDirty(def);
				}
			}
		}
		pack.ForceRefreshAssets();
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

	private void ImportLangFiles(string packName)
	{
		DirectoryInfo langFolder = new DirectoryInfo($"{IMPORT_ROOT}/{packName}/assets/flansmod/lang/");

		if(langFolder.Exists)
		{
			string langExportFolder = $"{ASSET_ROOT}/{packName}/lang/";
			if(!Directory.Exists(langExportFolder))
				Directory.CreateDirectory(langExportFolder);

			foreach(FileInfo langFile in langFolder.EnumerateFiles())
			{
				string src = langFile.FullName;
				string dst = $"{ASSET_ROOT}/{packName}/lang/{langFile.Name}";
				Debug.Log($"Imported lang file from {src} to {dst}");
				File.Copy(src, dst, true);
			}
		}
	}

	private void ImportSounds(string packName, ContentPack pack)
	{
		string soundJson = $"{IMPORT_ROOT}/{packName}/assets/flansmod/sounds.json";
		if(File.Exists(soundJson))
		{
			using(StreamReader fileReader = new StreamReader(soundJson))
			using(JsonReader jsonReader = new JsonTextReader(fileReader))
			{
				JObject soundRoot = JObject.Load(jsonReader);
				JObject soundCopy = new JObject();
				foreach(var kvp in soundRoot)
				{
					string soundKey = kvp.Key;
					JObject soundData = kvp.Value.ToObject<JObject>();
					string category = soundData["category"].ToString();
					JArray sounds = soundData["sounds"].ToObject<JArray>();

					JObject jCopy = new JObject();
					jCopy.Add("category", category);
					JArray jSoundsCopy = new JArray();

					foreach(JToken entry in sounds)
					{
						string soundName = entry.ToString();
						soundName = soundName.Replace("flansmod:", "");
						AudioClip clip = ImportSound(packName, soundName.ToLower(), Utils.ToLowerWithUnderscores(soundName));
						if(pack != null && clip != null)
							pack.Sounds.Add(clip);

						JObject newSoundBlob = new JObject();
						newSoundBlob.Add("name", $"{pack.ModName}:{Utils.ToLowerWithUnderscores(soundName)}");
						newSoundBlob.Add("type", "file");
						jSoundsCopy.Add(newSoundBlob);
					}
					jCopy.Add("sounds", jSoundsCopy);
					soundCopy.Add(Utils.ToLowerWithUnderscores(soundKey), jCopy);
				}

				string soundJsonOutput = $"{ASSET_ROOT}/{packName}/sounds.json";
				using(StreamWriter fileWriter = new StreamWriter(soundJsonOutput))
				using(JsonWriter jsonWriter = new JsonTextWriter(fileWriter))
				{ 
					jsonWriter.Formatting = Formatting.Indented;
					soundCopy.WriteTo(jsonWriter);
					Debug.Log($"Wrote new sounds.json at {soundJsonOutput}");
				}
			}
		}
	}

	private AudioClip ImportSound(string packName, string soundName, string outputSoundName)
	{
		string soundsDir = $"{ASSET_ROOT}/{packName}/sounds/";
		if(!Directory.Exists(soundsDir))
			Directory.CreateDirectory(soundsDir);

		if(soundName != null && soundName.Length > 0)
		{
			string path = $"{IMPORT_ROOT}/{packName}/assets/flansmod/sounds/{soundName}.ogg";
			string outputPath = $"{ASSET_ROOT}/{packName}/sounds/{outputSoundName}.ogg";
			if(File.Exists(path))
			{
				Debug.Log($"Copying ogg sound from {path} to {outputPath}");
				File.Copy(path, outputPath, true);
				return AssetDatabase.LoadAssetAtPath<AudioClip>(outputPath);
			}
			else
			{
				path = $"{IMPORT_ROOT}/{packName}/assets/flansmod/sounds/{soundName}.mp3";
				outputPath = $"{ASSET_ROOT}/{packName}/sounds/{outputSoundName}.mp3";
				if(File.Exists(path))
				{
					Debug.Log($"Copying mp3 sound from {path} to {outputPath}");
					File.Copy(path, outputPath, true);
					return AssetDatabase.LoadAssetAtPath<AudioClip>(outputPath);
				}
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
				def.AdditionalTextures.Add(new Definition.AdditionalTexture()
				{
					name = paintjob.textureName,
					texture = ImportTexture(packName, $"skins/{paintjob.textureName}.png", $"skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png")
				
				});
				def.AdditionalTextures.Add(new Definition.AdditionalTexture()
				{
					name = $"{paintjob.iconName}_Icon",
					texture = ImportTexture(packName, $"textures/items/{paintjob.iconName}.png", $"items/{paintjob.iconName}.png")
				});
			}
		}
		if(infoType is BoxType box)
		{	
			// Boxes need a model made from their side textures
			CubeModel cubeModel = JavaModelImporter.ImportBlock(packName, box);
			if(cubeModel != null)
			{
				string importPath = $"{IMPORT_ROOT}/{packName}/models/{cubeModel.name}.asset";
				AssetDatabase.CreateAsset(cubeModel, importPath);
			}
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

	public void VerifyDefinition(Definition definition, ContentPack pack)
	{
		VerifyObject(definition, pack, definition.name);
	}

	private void VerifyObject(object obj, ContentPack pack, string debug)
	{
		if(obj != null)
		{
			if(obj is SoundDefinition soundDef)
			{
				Debug.Log("Found sound " + obj + " at " + debug);
				if(!soundDef.sound.Contains(":"))
				{
					soundDef.sound = $"{pack.ModName}:{soundDef.sound}";
				}
				foreach(SoundLODDefinition soundLOD in soundDef.LODs)
				{
					if(!soundLOD.sound.Contains(":"))
					{
						soundLOD.sound = $"{pack.ModName}:{soundLOD.sound}";
					}
				}
			}
			else if(obj is ItemStackDefinition itemStackDef)
			{
				itemStackDef.item = ValidateItemId(itemStackDef.item, itemStackDef.damage, pack);
			}
			else if(obj is IngredientDefinition ingredientDef)
			{
				ingredientDef.itemName = ValidateItemId(ingredientDef.itemName, ingredientDef.maxAllowedDamage, pack);
			}

			foreach(FieldInfo field in obj.GetType().GetFields())
			{
				if(field.GetCustomAttribute<JsonFieldAttribute>() != null)
				{
					if(field.FieldType.IsClass)
					{
						VerifyObject(field.GetValue(obj), pack, $"{debug}/{field.Name}");
					}
					if(field.FieldType.IsArray)
					{
						int count = 0;
						foreach(object entry in (IEnumerable)field.GetValue(obj))
						{
							VerifyObject(entry, pack, $"{debug}/{field.Name}_{count}");
							count++;
						}
					}
				}
			}
		}
	}

	private string ResolveLegacyDamageItem(string itemName, int damage)
	{
		switch(itemName)
		{
			case "dye_powder": 
			{
				switch(damage)
				{
					case 0: return "black_dye";
					case 1: return "red_dye";
					case 2: return "green_dye";
					case 3: return "brown_dye";
					case 4: return "blue_dye";
					case 5: return "purple_dye";
					case 6: return "cyan_dye";
					case 7: return "light_gray_dye";
					case 8: return "gray_dye";
					case 9: return "pink_dye";
					case 10: return "lime_dye";
					case 11: return "yellow_dye";
					case 12: return "light_blue_dye";
					case 13: return "magenta_dye";
					case 14: return "orange_dye";
					case 15: return "white_dye";
					default: return null;
				}
			}
			case "log":
			{
				switch(damage)
				{
					case 0: return "oak_log";
					case 1: return "spruce_log";
					case 2: return "birch_log";
					case 3: return "jungle_log";
					default: return null;
				}
			}
			case "planks":
			{
				switch(damage)
				{
					case 0: return "oak_planks";
					case 1: return "spruce_planks";
					case 2: return "birch_planks";
					case 3: return "jungle_planks";
					default: return null;
				}
			}
		}
		return null;
	}

	private string ValidateItemId(string item, int damage, ContentPack currentPack)
	{
		item = Utils.ToLowerWithUnderscores(item);
		string legacy = ResolveLegacyDamageItem(item, damage);
		if(legacy != null)
			item = legacy;

		// Apply legacy fixes
		switch(item)
		{
			case "door_iron": item = "iron_door"; break;
			case "clay_item": item = "clay_ball"; break;
			case "ingot_iron": item = "iron_ingot"; break;
			case "iron": item = "iron_ingot"; break;
			case "ingot_gold": item = "gold_ingot"; break;
			case "slimeball": item = "slime_ball"; break;
			case "skull": item = "wither_skeleton_skull"; break;
		}

		if(!item.Contains(":"))
		{
			bool found = false;
			if(currentPack.HasContent(item))
			{
				item = $"{currentPack.ModName}:{item}";
				found = true;
			}
			else
			{
				foreach(ContentPack cp in Packs)
				{
					if(cp != null && cp.HasContent(item))
					{
						item = $"{cp.ModName}:{item}";
						found = true;
						break;
					}
				}
			}
			if(!found)
			{
				item = $"minecraft:{item}";
			}
		}
		return item;
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
		string magTextureExportFolder = $"{ExportRoot}/assets/{pack.ModName}/textures/magazine";
		string skinsExportFolder = $"{ExportRoot}/assets/{pack.ModName}/textures/skins";
		string soundsExportFolder = $"{ExportRoot}/assets/{pack.ModName}/sounds";
		string guiTextureExportFolder = $"{ExportRoot}/assets/{pack.ModName}/textures/gui";
		string langExportFolder = $"{ExportRoot}/assets/{pack.ModName}/lang";

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
		if(!Directory.Exists(magTextureExportFolder))
			Directory.CreateDirectory(magTextureExportFolder);
		if(!Directory.Exists(soundsExportFolder))
			Directory.CreateDirectory(soundsExportFolder);
		if(!Directory.Exists(langExportFolder))
			Directory.CreateDirectory(langExportFolder);

		foreach(Definition def in pack.AllContent)
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

						string src = $"{ASSET_ROOT}/{packName}/textures/items/{paintName}.png";
						string dst = $"{itemTextureExportFolder}/{paintName}.png";
						File.Copy(src, dst, true);
						Debug.Log($"Copied paint icon {src} to {dst}");
					}
				}

				if(def.Model != null)
				{
					QuickJSONBuilder itemModelBuilder = new QuickJSONBuilder();
					if(def.Icon != null)
						def.Model.icon = Utils.ToLowerWithUnderscores(def.Icon.name);
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

					// TODO: Or other paintable
					if (def is GunDefinition gun)
					{
						{
							QuickJSONBuilder paintjobRootExporter = new QuickJSONBuilder();
							if (JsonModelExporter.ExportInventorySkinSwitcherModel(gun.paints, def.Model, pack.ModName, item_name, paintjobRootExporter))
							{
								using (StringWriter stringWriter = new StringWriter())
								using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
								{
									jsonWriter.Formatting = Formatting.Indented;
									jsonWriter.Indentation = 4;
									paintjobRootExporter.Root.WriteTo(jsonWriter);
									string exportModelTo = $"{itemModelsExportFolder}/{item_name}_inventory.json";
									Debug.Log($"Exporting {def.Model.name} paintjob root model to {exportModelTo}");
									File.WriteAllText(exportModelTo, stringWriter.ToString());
								}
							}
						}
						if (!Directory.Exists($"{itemModelsExportFolder}/{item_name}"))
							Directory.CreateDirectory($"{itemModelsExportFolder}/{item_name}");
						for (int i = 0; i < gun.paints.paintjobs.Length; i++)
						{

							QuickJSONBuilder paintjobInstanceExporter = new QuickJSONBuilder();
							JsonModelExporter.ExportVanillaItem($"{pack.ModName}:item/{gun.paints.paintjobs[i].textureName}", paintjobInstanceExporter);
							using (StringWriter stringWriter = new StringWriter())
							using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
							{
								jsonWriter.Formatting = Formatting.Indented;
								jsonWriter.Indentation = 4;
								paintjobInstanceExporter.Root.WriteTo(jsonWriter);
								string exportModelTo = $"{itemModelsExportFolder}/{item_name}/{gun.paints.paintjobs[i].textureName}.json";
								Debug.Log($"Exporting {def.Model.name} paintjob variant model to {exportModelTo}");
								File.WriteAllText(exportModelTo, stringWriter.ToString());
							}
						}

						QuickJSONBuilder paintjobDefaultExporter = new QuickJSONBuilder();
						JsonModelExporter.ExportVanillaItem($"{pack.ModName}:item/{gun.name}", paintjobDefaultExporter);
						{
							using (StringWriter stringWriter = new StringWriter())
							using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
							{
								jsonWriter.Formatting = Formatting.Indented;
								jsonWriter.Indentation = 4;
								paintjobDefaultExporter.Root.WriteTo(jsonWriter);
								string exportModelTo = $"{itemModelsExportFolder}/{item_name}/default.json";
								Debug.Log($"Exporting {def.Model.name} inventory variant model to {exportModelTo}");
								File.WriteAllText(exportModelTo, stringWriter.ToString());
							}
						}
					}
					else
					{
						QuickJSONBuilder itemVariantModelExporter = new QuickJSONBuilder();
						if (JsonModelExporter.ExportInventoryVariantModel(def.Model, pack.ModName, item_name, itemVariantModelExporter))
						{
							using (StringWriter stringWriter = new StringWriter())
							using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
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
					
					if(def is MagazineDefinition magDef)
					{
						string src = $"{ASSET_ROOT}/{packName}/Textures/mags/{def.Icon.name}.png";
						string dst = $"{magTextureExportFolder}/{Utils.ToLowerWithUnderscores(def.Icon.name)}.png";
						
						Debug.Log($"Copying mag texture from {src} to {dst}");
						File.Copy(src, dst, true);
					}
					else
					{
						string src = $"{ASSET_ROOT}/{packName}/Textures/items/{def.Icon.name}.png";
						string dst = $"{itemTextureExportFolder}/{Utils.ToLowerWithUnderscores(def.Icon.name)}.png";
						
						Debug.Log($"Copying icon texture from {src} to {dst}");
						File.Copy(src, dst, true);
					}
				}
				//foreach(Definition.AdditionalTexture texture in def.AdditionalTextures)
				//{
				//	string src = AssetDatabase.GetAssetPath(texture.texture);
				//	string dst = $"{assetExportFolder}/{texture.name}.png";
//
				//	Debug.Log($"Copying additional texture from {src} to {dst}");
				//	File.Copy(src, dst, true);
				//}
				

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

		foreach(AudioClip sound in pack.Sounds)
		{
			string src = AssetDatabase.GetAssetPath(sound);
			string dst = $"{soundsExportFolder}/{sound.name}{new FileInfo(src).Extension}";
			
			Debug.Log($"Copying sound from {src} to {dst}");
			File.Copy(src, dst, true);
		}
		// Copy sound.json
		{
			string src = $"{ASSET_ROOT}/{packName}/sounds.json";
			if(File.Exists(src))
			{
				string dst = $"{assetExportFolder}/sounds.json";
				Debug.Log($"Copying sounds.json from {src} to {dst}");
				File.Copy(src, dst, true);
			}
		}
		// Copy lang.jsons
		Dictionary<string, JObject> langJsons = new Dictionary<string, JObject>();
		DirectoryInfo langFolder = new DirectoryInfo($"{ASSET_ROOT}/{packName}/lang/");
		if(langFolder.Exists)
		{
			foreach(FileInfo langFile in langFolder.EnumerateFiles())
			{
				if(!langFile.Extension.Contains("meta"))
				{
					string src = langFile.FullName;
					using(StringReader stringReader = new StringReader(File.ReadAllText(src)))
					using(JsonTextReader jsonReader = new JsonTextReader(stringReader))
					{
						langJsons.Add(langFile.Name, JObject.Load(jsonReader));
					}
					Debug.Log($"Importing lang file from {src}");
				}
			}
		}
		foreach(Definition def in pack.AllContent)
		{
			foreach(Definition.LocalisedName locName in def.LocalisedNames)
			{
				string langFileName = $"{locName.Lang}.json";
				if(!langJsons.ContainsKey(langFileName))
					langJsons.Add(langFileName, new JObject());

				string prefix = "item";
				if (def is MagazineDefinition) 
					prefix = "magazine";
				if (def is MaterialDefinition) 
					prefix = "material";
				langJsons[langFileName].Add($"{prefix}.{pack.ModName}.{def.name}", locName.Name);
			}
			foreach(Definition.LocalisedExtra locExtra in def.LocalisedExtras)
			{
				string langFileName = $"{locExtra.Lang}.json";
				if(!langJsons.ContainsKey(langFileName))
					langJsons.Add(langFileName, new JObject());
					
				langJsons[langFileName].Add(locExtra.Unlocalised, locExtra.Localised);
			}
		}
		foreach(Definition.LocalisedExtra locExtra in pack.ExtraLocalisation)
		{
			string langFileName = $"{locExtra.Lang}.json";
			if(!langJsons.ContainsKey(langFileName))
				langJsons.Add(langFileName, new JObject());
				
			langJsons[langFileName].Add(locExtra.Unlocalised, locExtra.Localised);
		}

		foreach(var kvp in langJsons)
		{
			using(StringWriter stringWriter = new StringWriter())						
			using(JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
			{
				jsonWriter.Formatting = Formatting.Indented;
				string dst = $"{assetExportFolder}/lang/{kvp.Key}";
				kvp.Value.WriteTo(jsonWriter);
				File.WriteAllText(dst, stringWriter.ToString());
				Debug.Log($"Exporting lang file to {dst}");
			}
		}
	}
	
}
