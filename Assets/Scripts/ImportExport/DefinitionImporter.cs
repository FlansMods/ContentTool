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

		pack.name = packName;
		pack.ModName = packName;

		TYPE_LOOKUP = new Dictionary<string, InfoType>[DefinitionTypes.NUM_TYPES];

		ImportAllTypeFiles(pack);
		ConvertAllTypesToDefinitions(packName, pack);

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
					
					InfoType imported = ImportType(pack, defType, file.Name.Split(".")[0]);
					if(imported != null)
					{
						TYPE_LOOKUP[i].Add(imported.shortName, imported);
					}
					else
					{
						Debug.LogWarning($"Failed to import {pack.name}:{file.Name}");
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

		//if(pack.HasContent(shortName))
		//	return false;

		string assetPath = $"{ASSET_ROOT}/{packName}/{type.OutputFolder()}/{shortName}.asset";
		Debug.Log($"Saving {def} to {assetPath}");
		AssetDatabase.CreateAsset(def, assetPath);
		return AssetDatabase.LoadAssetAtPath<Definition>(assetPath);
	}

	public void ExportPack(ContentPack pack)
	{
		foreach(Definition def in pack.Content)
		{
			string exportFolder = $"{ExportRoot}/data/{pack.ModName}/{DefinitionTypes.GetFromObject(def).ToString()}";
			string exportTo = $"{exportFolder}/{def.name}.json";
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
				Debug.Log($"Exporting {def.name} to {exportTo}");
				File.WriteAllText(exportTo, stringWriter.ToString());
			}
		}
		for(int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
		{
			
		}
	}
}
