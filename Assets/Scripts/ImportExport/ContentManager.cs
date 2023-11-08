using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static Definition;

[ExecuteInEditMode]
public class ContentManager : MonoBehaviour
{
	private static ContentManager _inst = null;
	public static ContentManager inst
	{
		get
		{
			if (_inst == null)
				_inst = FindObjectOfType<ContentManager>();
			if(_inst == null)
			{
				GameObject go = new GameObject("DefinitionImporter");
				_inst = go.AddComponent<ContentManager>();
			}
			return _inst;
		}
	}

	public const string IMPORT_ROOT = "Import/Content Packs";
	public const string MODEL_IMPORT_ROOT = "Import/Java Models";
	public const string ASSET_ROOT = "Assets/Content Packs";

	public void OnEnable()
	{
		EditorApplication.update += EditorUpdate;
	}
	public void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
	}

	private void EditorUpdate()
	{
		ModelEditingSystem.ApplyAllQueuedActions();
	}

	// ---------------------------------------------------------------------------------------
	#region Pre-Import Pack storage
	// ---------------------------------------------------------------------------------------
	private class PreImportPack
	{
		public string PackName;
		public string RootLocation { get { return $"{IMPORT_ROOT}/{PackName}"; } }

		// Lazy initialize all these things
		private Dictionary<EDefinitionType, Folder> SubFolders = null;
		private class Folder
		{
			public List<string> FileNames = null;
			private Dictionary<string, AdditionalImportInfo> CachedImportInfo = new Dictionary<string, AdditionalImportInfo>();

			public bool TryGetAddtionalImportInfo(string packName, EDefinitionType type, string txtFileName, out List<string> inputs, out List<string> outputs)
			{
				if (FileNames.Contains(txtFileName))
				{
					if (!CachedImportInfo.TryGetValue(txtFileName, out AdditionalImportInfo additionalInfo))
					{
						additionalInfo = new AdditionalImportInfo();

						// Cache the dependencies of this InfoType by doing a really rough read of the file
						string[] lines = File.ReadAllLines($"{IMPORT_ROOT}/{packName}/{type.Folder()}/{txtFileName}");
						string shortName = Utils.ToLowerWithUnderscores(txtFileName.Split('.')[0]);
						foreach (string line in lines)
						{
							string[] splits = line.Split(' ');
							if (splits[0] == "Icon")
							{
								additionalInfo.Inputs.Add($"{IMPORT_ROOT}/{packName}/assets/flansmod/textures/items/{splits[1]}.png");
								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/textures/items/{Utils.ToLowerWithUnderscores(splits[1])}.png");
							}
							if (splits[0] == "Texture")
							{
								additionalInfo.Inputs.Add($"{IMPORT_ROOT}/{packName}/assets/flansmod/skins/{splits[1]}.png");
								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/textures/skins/{Utils.ToLowerWithUnderscores(splits[1])}.png");
							}
							if (splits[0] == "Paintjob")
							{
								additionalInfo.Inputs.Add($"{IMPORT_ROOT}/{packName}/assets/flansmod/textures/items/{splits[1]}.png");
								additionalInfo.Inputs.Add($"{IMPORT_ROOT}/{packName}/assets/flansmod/skins/{splits[2]}.png");

								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/textures/items/{Utils.ToLowerWithUnderscores(splits[1])}.png");
								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/textures/skins/{Utils.ToLowerWithUnderscores(splits[2])}.png");
								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/models/{shortName}/{Utils.ToLowerWithUnderscores(splits[2])}_icon.asset");
							}
							if (splits[0] == "Model" || splits[0] == "DeployedModel")
							{
								string[] modelSteps = splits[1].Split('.');
								if (modelSteps.Length == 2)
								{
									additionalInfo.Inputs.Add($"{MODEL_IMPORT_ROOT}/{modelSteps[0]}/{modelSteps[1]}.java");
									additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/models/{shortName}.asset");
									additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/models/{shortName}/{shortName}_3d.asset");
									additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/models/{shortName}/{shortName}_icon.asset");
								}
							}
						}

						CachedImportInfo.Add(txtFileName, additionalInfo);
					}
					inputs = additionalInfo.Inputs;
					outputs = additionalInfo.Outputs;
					return true;
				}

				inputs = EMPTY_LIST;
				outputs = EMPTY_LIST;
				return false;
			}
			public bool HasFullImportMap(string packName, EDefinitionType type, bool generate = false)
			{
				if (generate)
				{
					foreach (string fileName in FileNames)
						TryGetAddtionalImportInfo(packName, type, fileName, out List<string> inputs, out List<string> outputs);
					return true;
				}
				else
				{
					foreach (string fileName in FileNames)
						if (!CachedImportInfo.ContainsKey(fileName))
							return false;
					return true;
				}
			}
			public bool TryGetFullImportCount(out int inputCount, out int outputCount)
			{
				inputCount = 0;
				outputCount = 0;
				foreach (string fileName in FileNames)
					if (CachedImportInfo.TryGetValue(fileName, out AdditionalImportInfo info))
					{
						inputCount += info.Inputs.Count;
						outputCount += info.Outputs.Count;
					}
					else return false;
				return true;
			}

			private class AdditionalImportInfo
			{
				public List<string> Inputs = new List<string>();
				public List<string> Outputs = new List<string>();
			}
		}
		private void CheckInitCache()
		{
			if (SubFolders == null)
			{
				SubFolders = new Dictionary<EDefinitionType, Folder>();
				for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
				{
					EDefinitionType defType = (EDefinitionType)i;
					Dictionary<string, string> defImports = new Dictionary<string, string>();
					DirectoryInfo dir = new DirectoryInfo($"{RootLocation}/{defType.Folder()}");
					if (dir.Exists)
					{
						List<string> fileNames = new List<string>();
						foreach (FileInfo file in dir.EnumerateFiles())
						{
							fileNames.Add(file.Name);
						}
						if (fileNames.Count > 0)
						{
							SubFolders.Add(defType, new Folder()
							{
								FileNames = fileNames,
							});
						}
					}
				}
			}
		}

		public int GetTotalNumAssets()
		{
			CheckInitCache();
			int count = 0;
			foreach (var kvp in SubFolders)
				return count += kvp.Value.FileNames.Count;
			return count;
		}
		public int GetNumAssetsInFolder(EDefinitionType type)
		{
			CheckInitCache();
			if (SubFolders.TryGetValue(type, out Folder folder))
				return folder.FileNames.Count;
			return 0;
		}
		public IEnumerable<string> GetAllAssetNames()
		{
			CheckInitCache();
			foreach (var kvp in SubFolders)
				foreach (string fileName in kvp.Value.FileNames)
					yield return fileName;
		}
		public IEnumerable<string> GetAssetNamesInFolder(EDefinitionType type)
		{
			CheckInitCache();
			if (SubFolders.TryGetValue(type, out Folder folder))
				foreach (string fileName in folder.FileNames)
					yield return fileName;
		}
		public bool TryGetAddtionalImportInfo(EDefinitionType type, string txtFileName, out List<string> inputs, out List<string> outputs)
		{
			CheckInitCache();
			if (SubFolders.TryGetValue(type, out Folder folder))
				return folder.TryGetAddtionalImportInfo(PackName, type, txtFileName, out inputs, out outputs);

			inputs = EMPTY_LIST;
			outputs = EMPTY_LIST;
			return false;
		}
		public bool HasFullImportMap(EDefinitionType type, bool generate = false)
		{
			CheckInitCache();
			if (SubFolders.TryGetValue(type, out Folder folder))
				return folder.HasFullImportMap(PackName, type, generate);
			return true;
		}
		public bool TryGetFullImportCount(EDefinitionType type, out int inputCount, out int outputCount)
		{
			CheckInitCache();
			if (SubFolders.TryGetValue(type, out Folder folder))
				return folder.TryGetFullImportCount(out inputCount, out outputCount);
			inputCount = 0;
			outputCount = 0;
			return true;
		}
	}
	[SerializeField]
	private List<PreImportPack> PreImportPacks = null;
	private void CachePreImportPacks()
	{
		PreImportPacks = new List<PreImportPack>();
		foreach (DirectoryInfo subDir in new DirectoryInfo(IMPORT_ROOT).EnumerateDirectories())
		{
			PreImportPacks.Add(new PreImportPack() { PackName = subDir.Name });
		}
	}
	private bool TryGetPreImportPack(string packName, out PreImportPack result)
	{
		foreach (PreImportPack pack in PreImportPacks)
			if (pack.PackName == packName)
			{
				result = pack;
				return true;
			}
		result = null;
		return false;
	}
	public List<string> GetPreImportPackNames()
	{
		Refresh();
		List<string> names = new List<string>(PreImportPacks.Count);
		foreach(PreImportPack pack in PreImportPacks)
		{
			names.Add(pack.PackName);
		}
		return names;
	}
	public int GetNumAssetsInPack(string packName)
	{
		Refresh();
		int count = 0;
		if (TryGetPreImportPack(packName, out PreImportPack pack))
		{
			for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
				count += pack.GetNumAssetsInFolder((EDefinitionType)i);
		}
		return count;
	}
	public int GetNumAssetsInPack(string packName, EDefinitionType type)
	{
		Refresh();
		if(TryGetPreImportPack(packName, out PreImportPack pack))
		{
			return pack.GetNumAssetsInFolder(type);
		}
		return 0;
	}
	private static readonly List<string> EMPTY_LIST = new List<string>();
	public IEnumerable<string> GetAssetNamesInPack(string packName, EDefinitionType type)
	{
		Refresh();
		if (TryGetPreImportPack(packName, out PreImportPack pack))
		{
			return pack.GetAssetNamesInFolder(type);
		}
		return EMPTY_LIST;
	}
	public string GetInfoTypeImportPath(string packName, EDefinitionType type, string txtFileName)
	{
		return $"{IMPORT_ROOT}/{packName}/{type.Folder()}/{txtFileName}";
	}
	public string GetTargetAssetPathFor(string packName, EDefinitionType type, string txtFileName)
	{
		return $"{ASSET_ROOT}/{packName}/{type.OutputFolder()}/{Utils.ToLowerWithUnderscores(txtFileName.Split(".")[0])}.asset";
	}
	public bool TryGetImportMap(string packName, EDefinitionType type, string txtFileName, out List<string> inputs, out List<string> outputs)
	{
		if(TryGetPreImportPack(packName, out PreImportPack pack))
		{
			return pack.TryGetAddtionalImportInfo(type, txtFileName, out inputs, out outputs);
		}
		inputs = EMPTY_LIST;
		outputs = EMPTY_LIST;
		return false;
	}
	public bool GenerateFullImportMap(string packName)
	{
		return HasFullImportMap(packName, true);
	}
	public bool HasFullImportMap(string packName, bool generate = false)
	{
		if (TryGetPreImportPack(packName, out PreImportPack pack))
		{
			if (generate)
			{
				for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
					pack.HasFullImportMap((EDefinitionType)i, generate);
				return true;
			}
			else
			{
				for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
					if (!pack.HasFullImportMap((EDefinitionType)i))
						return false;
				return true;
			}
		}
		return false;
	}
	public bool HasFullImportMap(string packName, EDefinitionType type, bool generate = false)
	{
		if (TryGetPreImportPack(packName, out PreImportPack pack))
		{
			return pack.HasFullImportMap(type, generate);
		}
		return true;
	}
	public bool TryGetFullImportCount(string packName, out int inputCount, out int outputCount)
	{
		inputCount = 0;
		outputCount = 0;
		if (TryGetPreImportPack(packName, out PreImportPack pack))
		{
			for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
				if (pack.TryGetFullImportCount((EDefinitionType)i, out int typedInputCount, out int typedOutputCount))
				{
					inputCount += typedInputCount;
					outputCount += typedOutputCount;
				}
		}
		return true;
	}

	public bool TryGetFullImportCount(string packName, EDefinitionType type, out int inputCount, out int outputCount)
	{
		if (TryGetPreImportPack(packName, out PreImportPack pack))
		{
			return pack.TryGetFullImportCount(type, out inputCount, out outputCount);
		}
		inputCount = 0;
		outputCount = 0;
		return true;
	}

	public void ForceRefresh()
	{
		Refresh(true);
	}

	public void Refresh(bool force = false)
	{
		if (force || PreImportPacks == null || (DateTime.Now - LastContentCheck).TotalSeconds >= RefreshEveryT)
		{
			CachePreImportPacks();
			LastContentCheck = DateTime.Now;
		}
	}
	#endregion
	// ---------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------------
	#region Loaded Packs
	// ---------------------------------------------------------------------------------------
	public const float RefreshEveryT = 10;
	private DateTime LastContentCheck = DateTime.Now;
	public List<ContentPack> Packs
	{ 
		get
		{
			for (int i = _Packs.Count - 1; i >= 0; i--)
				if (_Packs[i] == null)
					_Packs.RemoveAt(i);
			return _Packs;
		}
	}
	[SerializeField, FormerlySerializedAs("Packs")]
	private List<ContentPack> _Packs = new List<ContentPack>();

	public ContentPack FindContentPack(string packName)
	{
		// First, see if we already cached it
		foreach (ContentPack pack in Packs)
			if (pack != null)
				if (pack.name == packName)
					return pack;

		// Second, try to find it in the asset database
		foreach (string cpPath in AssetDatabase.FindAssets("t:ContentPack"))
		{
			ContentPack loadedPack = AssetDatabase.LoadAssetAtPath<ContentPack>(cpPath);
			if (loadedPack != null)
			{
				Packs.Add(loadedPack);
				return loadedPack;
			}
		}

		return null;
	}
	#endregion
	// ---------------------------------------------------------------------------------------

	// ---------------------------------------------------------------------------------------
	#region The Import Process
	// ---------------------------------------------------------------------------------------
	// TODO: Can we not use the TYPE_LOOKUP? Do InfoTypes need to reference each other during import?
	//
	public static Dictionary<string, InfoType>[] TYPE_LOOKUP;
	public static bool TryGetType<T>(EDefinitionType type, string key, out T infoType) where T : InfoType
	{
		if (TYPE_LOOKUP[(int)type].TryGetValue(key, out InfoType temp))
		{
			infoType = (T)temp;
			return true;
		}
		foreach (var kvp in TYPE_LOOKUP[(int)type])
		{
			if (kvp.Value.shortName == key)
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
	// --- TYPE_LOOKUP ---

	public bool ImportPack(string packName, List<Verification> errors, bool overwrite = false)
	{
		// Check our name
		if(packName == null || packName.Length == 0)
		{
			errors.Add(Verification.Failure($"Pack name '{packName}' is invalid"));
			return false;
		}
		string sanitisedName = Utils.ToLowerWithUnderscores(packName);
		if(sanitisedName != packName)
		{
			errors.Add(Verification.Failure($"Pack name '{packName}' is not in Minecraft format. Try '{sanitisedName}'.",
			() => { 
				Debug.Log($"Copied '{sanitisedName}' to the clipboard"); 
				GUIUtility.systemCopyBuffer = sanitisedName; 
			}));
			return false;
		}

		// See if this is actually a pack we know about
		ForceRefresh();
		if (!TryGetPreImportPack(packName, out PreImportPack inputPack))
		{
			errors.Add(Verification.Failure($"Pack name '{packName}' was not found in the Import folder"));
			return false;
		}

		// Find the content pack we are importing to
		ContentPack outputPack = FindContentPack(packName);
		if(outputPack == null)
		{
			outputPack = ScriptableObject.CreateInstance<ContentPack>();
			CreateUnityAsset(outputPack, $"{ASSET_ROOT}/{packName}/{packName}.asset");
			Packs.Add(outputPack);
		}

		// For each possible import, import it if it is new, or if we are overwriting

		// Hmmmm
		TYPE_LOOKUP = new Dictionary<string, InfoType>[DefinitionTypes.NUM_TYPES];

		GenerateFullImportMap(packName);
		for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
		{
			
			EDefinitionType defType = (EDefinitionType)i;
			EditorUtility.DisplayProgressBar($"Importing Content", $"Importing {defType}", (float)(i+1) / DefinitionTypes.NUM_TYPES);
			TYPE_LOOKUP[i] = new Dictionary<string, InfoType>();
			foreach (string fileName in inputPack.GetAssetNamesInFolder(defType))
			{
				ImportContent_Internal(inputPack, outputPack, defType, fileName, errors, overwrite);
			}
		}

		// Release the InfoType objects now we are done
		TYPE_LOOKUP = null;

		ImportLangFiles_Internal(packName);
		ImportSounds_Internal(packName, outputPack);
		EditorUtility.ClearProgressBar();

		outputPack.ForceRefreshAssets();
		AssetDatabase.Refresh();


		return true;
	}

	private void ImportContent_Internal(PreImportPack fromPack, ContentPack toPack, EDefinitionType defType, string fileName, List<Verification> errors, bool overwrite)
	{
		if (fromPack == null || toPack == null)
			return;

		// Check existing content if we aren't ok to overwrite
		if (!overwrite)
		{
			if (fromPack.TryGetAddtionalImportInfo(defType, fileName, out List<string> inputFiles, out List<string> outputFiles))
			{
				for(int i = outputFiles.Count - 1; i >= 0; i--)
				{
					if (File.Exists(outputFiles[i]))
					{
						outputFiles.RemoveAt(i);
					}
				}
			}
		}

		// We are okay to do the import
		// Step 1. Import InfoType, our old format
		InfoType imported = ImportType_Internal(toPack, defType, fileName.Split(".")[0]);
		if (imported == null)
		{
			errors.Add(Verification.Failure($"Failed to import {fileName} as InfoType"));
			return;
		}
		TYPE_LOOKUP[(int)defType].Add(imported.shortName, imported);

		// Step 2. Convert to Definition
		Definition def = ConvertDefinition_Internal(toPack.ModName, defType, imported.shortName, imported);
		if (def == null)
		{
			errors.Add(Verification.Failure($"Failed to convert {imported.shortName} from InfoType to Definition"));
			return;
		}

		VerifyDefinition(def, toPack);
		EditorUtility.SetDirty(def);
		AssetDatabase.SaveAssetIfDirty(def);

		// Step 3. Import all additional assets
		List<string> outputPaths = AdditionalAssetImporter.GetOutputPaths(fromPack.PackName, imported);
		if (!overwrite)
		{
			for (int i = outputPaths.Count - 1; i >= 0; i--)
			{
				if (File.Exists(outputPaths[i]))
					outputPaths.RemoveAt(i);
			}
		}
		AdditionalAssetImporter.ImportAssets(fromPack.PackName, imported, outputPaths, errors);
	}

	private InfoType ImportType_Internal(ContentPack pack, EDefinitionType type, string fileName)
	{
		Debug.Log($"Importing {pack.name}:{type}/{fileName}");
		string importFilePath = $"{IMPORT_ROOT}/{pack.name}/{type.Folder()}/{fileName}.txt";

		// Read the .txt file
		TypeFile file = new TypeFile(fileName);
		string[] lines = File.ReadAllLines(importFilePath);
		foreach (string line in lines)
		{
			file.addLine(line);
		}

		// -------
		// This is hacky, meh
		if (type == EDefinitionType.gun)
		{
			string modelFolder = "";
			string modelName = "";
			foreach (string line in lines)
			{
				if(line.StartsWith("Model"))	
				{
					string modelPath = line.Split(' ')[1];
					if (modelPath.Contains('.'))
					{
						modelFolder = modelPath.Split('.')[0];
						modelName = modelPath.Split('.')[1];
					}
					else modelName = modelPath;
					break;
				}
			}
			// Load the model and hunt for 5 specific lines.
			string modelFilePath = $"{MODEL_IMPORT_ROOT}/{modelFolder}/Model{modelName}.java";
			if (File.Exists(modelFilePath))
			{
				string[] modelLines = File.ReadAllLines(modelFilePath);
				foreach (string line in modelLines)
				{
					if (line.Contains("="))
					{
						if (line.Contains("animationType"))
						{
							for (int i = 0; i < AnimationTypes.NUM_TYPES; i++)
							{
								if (line.Contains($"{(EAnimationType)i}"))
								{
									GunConverter.OldAnimType = (EAnimationType)i;
									break;
								}
							}
						}
						else if (line.Contains("tiltGunTime"))
						{
							if (JavaModelImporter.MatchSetFloatParameter(line, out string parameter, out float time))
								GunConverter.TiltTime = time;
						}
						else if (line.Contains("untiltGunTime"))
						{
							if (JavaModelImporter.MatchSetFloatParameter(line, out string parameter, out float time))
								GunConverter.UntiltTime = time;
						}
						else if (line.Contains("loadClipTime"))
						{
							if (JavaModelImporter.MatchSetFloatParameter(line, out string parameter, out float time))
								GunConverter.LoadTime = time;
						}
						else if (line.Contains("unloadClipTime"))
						{
							if (JavaModelImporter.MatchSetFloatParameter(line, out string parameter, out float time))
								GunConverter.UnloadTime = time;
						}
					}
				}
			}
		}
		// ---

		// Load it into a legacy InfoType
		InfoType infoType = TxtImport.Import(file, type);
		return infoType;
	}

	private Definition ConvertDefinition_Internal(string packName, EDefinitionType type, string shortName, InfoType infoType, List<Verification> verifications = null)
	{
		// All clear, let's import it
		Definition def = type.CreateInstance();
		if (def == null)
			return null;

		// Convert it to a definition
		InfoToDefinitionConverter.Convert(type, infoType, def);
		def.name = infoType.shortName;

		string assetPath = $"{ASSET_ROOT}/{packName}/{type.OutputFolder()}/{Utils.ToLowerWithUnderscores(shortName)}.asset";
		CreateUnityAsset(def, assetPath);
		if (verifications != null)
			verifications.Add(Verification.Success($"Saved {def} to {assetPath}"));
		return AssetDatabase.LoadAssetAtPath<Definition>(assetPath);
	}

	private void ImportLangFiles_Internal(string packName, List<Verification> verifications = null)
	{
		DirectoryInfo langFolder = new DirectoryInfo($"{IMPORT_ROOT}/{packName}/assets/flansmod/lang/");
		string itemNamePrefix = $"item.{packName}.";
		string magNamePrefix = $"magazine.{packName}.";
		string materialNamePrefix = $"material.{packName}.";
		ContentPack pack = FindContentPack(packName);

		try
		{
			if (langFolder.Exists && pack != null)
			{
				foreach (FileInfo langFile in langFolder.EnumerateFiles())
				{
					string langName = langFile.Name;
					if (langName.Contains('.'))
						langName = langName.Substring(0, langName.IndexOf('.'));
					if (Enum.TryParse(langName, out Definition.ELang lang))
					{
						int stringsImported = 0;
						using (StringReader stringReader = new StringReader(File.ReadAllText(langFile.FullName)))
						using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
						{
							JObject translations = JObject.Load(jsonReader);
							foreach (var kvp in translations)
							{
								if (TryImportLocalisationLine(itemNamePrefix, lang, kvp.Key, kvp.Value.ToString(), pack))
								{ }
								else if (TryImportLocalisationLine(magNamePrefix, lang, kvp.Key, kvp.Value.ToString(), pack))
								{ }
								else if (TryImportLocalisationLine(materialNamePrefix, lang, kvp.Key, kvp.Value.ToString(), pack))
								{ }
								else // Had no matching prefix. Let's add it to the generic extras
								{
									pack.ExtraLocalisation.Add(new LocalisedExtra()
									{
										Lang = lang,
										Unlocalised = kvp.Key,
										Localised = kvp.Value.ToString(),
									});
								}
								stringsImported++;
							}
						}
						Debug.Log($"Imported {stringsImported} strings from {langFile.FullName}");
					}
					else Debug.LogError($"Could not match {langName} to a known language");
				}
			}
		}
		catch(Exception e)
		{
			if (verifications != null)
				verifications.Add(Verification.Failure(e));
		}
	}

	private bool TryImportLocalisationLine(string prefix, ELang lang, string key, string value, ContentPack pack)
	{
		if (key.StartsWith(prefix))
		{
			string itemName = key.Substring(prefix.Length);
			if (pack.TryGetContent(itemName, out Definition def))
			{
				def.LocalisedNames.Add(new LocalisedName()
				{
					Lang = lang,
					Name = value,
				});
				return true;
			}
		}
		return false;
	}

	private void ImportSounds_Internal(string packName, ContentPack pack)
	{
		string soundJson = $"{IMPORT_ROOT}/{packName}/assets/flansmod/sounds.json";
		if (File.Exists(soundJson))
		{
			using (StreamReader fileReader = new StreamReader(soundJson))
			using (JsonReader jsonReader = new JsonTextReader(fileReader))
			{
				JObject soundRoot = JObject.Load(jsonReader);
				JObject soundCopy = new JObject();
				foreach (var kvp in soundRoot)
				{
					string soundKey = kvp.Key;
					JObject soundData = kvp.Value.ToObject<JObject>();
					string category = soundData["category"].ToString();
					JArray sounds = soundData["sounds"].ToObject<JArray>();

					JObject jCopy = new JObject();
					jCopy.Add("category", category);
					JArray jSoundsCopy = new JArray();

					foreach (JToken entry in sounds)
					{
						string soundName = entry.ToString();
						soundName = soundName.Replace("flansmod:", "");
						AudioClip clip = ImportSound(packName, soundName.ToLower(), Utils.ToLowerWithUnderscores(soundName));
						//if (pack != null && clip != null)
						//	pack.Sounds.Add(clip);

						JObject newSoundBlob = new JObject();
						newSoundBlob.Add("name", $"{pack.ModName}:{Utils.ToLowerWithUnderscores(soundName)}");
						newSoundBlob.Add("type", "file");
						jSoundsCopy.Add(newSoundBlob);
					}
					jCopy.Add("sounds", jSoundsCopy);
					soundCopy.Add(Utils.ToLowerWithUnderscores(soundKey), jCopy);
				}

				string soundJsonOutput = $"{ASSET_ROOT}/{packName}/sounds.json";
				using (StreamWriter fileWriter = new StreamWriter(soundJsonOutput))
				using (JsonWriter jsonWriter = new JsonTextWriter(fileWriter))
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
		if (!Directory.Exists(soundsDir))
			Directory.CreateDirectory(soundsDir);

		if (soundName != null && soundName.Length > 0)
		{
			string path = $"{IMPORT_ROOT}/{packName}/assets/flansmod/sounds/{soundName}.ogg";
			string outputPath = $"{ASSET_ROOT}/{packName}/sounds/{outputSoundName}.ogg";
			if (File.Exists(path))
			{
				Debug.Log($"Copying ogg sound from {path} to {outputPath}");
				File.Copy(path, outputPath, true);
				return AssetDatabase.LoadAssetAtPath<AudioClip>(outputPath);
			}
			else
			{
				path = $"{IMPORT_ROOT}/{packName}/assets/flansmod/sounds/{soundName}.mp3";
				outputPath = $"{ASSET_ROOT}/{packName}/sounds/{outputSoundName}.mp3";
				if (File.Exists(path))
				{
					Debug.Log($"Copying mp3 sound from {path} to {outputPath}");
					File.Copy(path, outputPath, true);
					return AssetDatabase.LoadAssetAtPath<AudioClip>(outputPath);
				}
			}
		}
		return null;
	}

	public static void CreateUnityAsset(UnityEngine.Object asset, string path)
	{
		string folder = path.Substring(0, path.LastIndexOf('/'));
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
		AssetDatabase.CreateAsset(asset, path);
	}
	#endregion
	// ---------------------------------------------------------------------------------------

	// ---------------------------------------------------------------------------------------
	#region Verifications
	// ---------------------------------------------------------------------------------------
	private void VerifyDefinition(Definition definition, ContentPack pack)
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
	#endregion
	// ---------------------------------------------------------------------------------------

	// ---------------------------------------------------------------------------------------
	#region The Export Process
	// ---------------------------------------------------------------------------------------
	public string ExportRoot = "Export";
	private static readonly char[] SLASHES = new char[] { '/', '\\' };
	public string GetExportPath(string packName, UnityEngine.Object asset)
	{
		ResourceLocation loc = asset.GetLocation();
		if(asset is MinecraftModel)
			return $"{ExportRoot}/assets/{packName}/{loc.ID}.json";
		if (asset is Texture2D)
			return $"{ExportRoot}/assets/{packName}/{loc.ID}.png";

		return $"{ExportRoot}/data/{packName}/{loc.ID}.json";
	}

	public bool ExportedAssetAlreadyExists(string packName, UnityEngine.Object asset)
	{
		string exportPath = GetExportPath(packName, asset);
		return File.Exists(exportPath);
	}

	public void ExportAsset(string packName, UnityEngine.Object asset, List<Verification> verifications = null)
	{
		string exportPath = GetExportPath(packName, asset);
		string exportFolder = exportPath.Substring(0, exportPath.LastIndexOfAny(SLASHES));
		if (!Directory.Exists(exportFolder))
			Directory.CreateDirectory(exportFolder);
		if (asset is Definition def)
		{
			try
			{
				def.CheckAndExportToFile(exportPath);
			}
			catch (Exception e)
			{
				if(verifications != null)
					verifications.Add(Verification.Failure(e));
			}
		}
		else if(asset is MinecraftModel model)
		{
			try
			{
				List<string> outputFiles = new List<string>();
				ExportDirectory exportDir = new ExportDirectory($"{ExportRoot}/assets/{packName}");
				model.ExportToModelJsonFiles(exportDir, outputFiles);
			}
			catch (Exception e)
			{
				if (verifications != null)
					verifications.Add(Verification.Failure(e));
			}
		}
		else if(asset is Texture2D texture)
		{
			try
			{
				texture.CheckAndExportToFile(exportPath);
			}
			catch (Exception e)
			{
				if (verifications != null)
					verifications.Add(Verification.Failure(e));
			}
		}
		else if(asset is AudioClip audio)
		{
			// TODO:
			//sound.CheckAndExportToFile(outputDir.File())
			//string src = AssetDatabase.GetAssetPath(sound);
			//string dst = $"{soundsExportFolder}/{sound.name}{new FileInfo(src).Extension}";
			//
			//Debug.Log($"Copying sound from {src} to {dst}");
			//File.Copy(src, dst, true);
		}
		else
		{
			if (verifications != null)
				verifications.Add(Verification.Failure($"Unknown asset type {asset.GetType()} for {asset}"));
		}
	}

	public string GetLangExportPath(string packName, ELang lang)
	{
		return $"{ExportRoot}/assets/{packName}/lang/{lang}.json";
	}
	public string GetNamePrefix(Definition def)
	{
		if (def is MagazineDefinition)
			return "magazine";
		if (def is MaterialDefinition)
			return "material";
		return "item";
	}
	public void ExportLangJson(string packName, ELang lang, List<Verification> verifications = null)
	{
		string exportPath = GetLangExportPath(packName, lang);
		string exportFolder = exportPath.Substring(0, exportPath.LastIndexOfAny(SLASHES));
		if (!Directory.Exists(exportFolder))
			Directory.CreateDirectory(exportFolder);
		ContentPack pack = FindContentPack(packName);
		if (pack == null)
		{
			Debug.LogError($"Failed to find pack {packName}");
			return;
		}

		try
		{
			JObject jRoot = new JObject();
			foreach (Definition def in pack.AllContent)
			{
				foreach (LocalisedName locName in def.LocalisedNames)
					if (locName.Lang == lang)
						jRoot.Add($"{GetNamePrefix(def)}.{packName}.{def.name}", locName.Name);

				foreach (LocalisedExtra locExtra in def.LocalisedExtras)
					if (locExtra.Lang == lang)
						jRoot.Add(locExtra.Unlocalised, locExtra.Localised);
			}

			// And add pack localisation
			foreach (LocalisedExtra locExtra in pack.ExtraLocalisation)
				if (locExtra.Lang == lang)
					jRoot.Add(locExtra.Unlocalised, locExtra.Localised);

			if (jRoot.HasValues)
			{
				using (StringWriter stringWriter = new StringWriter())
				using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
				{
					jsonWriter.Formatting = Formatting.Indented;
					jRoot.WriteTo(jsonWriter);
					File.WriteAllText(exportPath, stringWriter.ToString());
				}
			}
		}
		catch(Exception e)
		{
			if (verifications != null)
				verifications.Add(Verification.Failure(e));
		}
	}

	public void ExportSoundsJson(string packName, List<Verification> verifications = null)
	{
		// TODO:
		// Copy sound.json
		//{
		//	string src = $"{ASSET_ROOT}/{packName}/sounds.json";
		//	if (File.Exists(src))
		//	{
		//		string dst = $"{assetExportFolder}/sounds.json";
		//		Debug.Log($"Copying sounds.json from {src} to {dst}");
		//		File.Copy(src, dst, true);
		//	}
		//}
	}

	public void ExportPack(string packName, bool overwrite, List<Verification> verifications = null)
	{
		ContentPack pack = FindContentPack(packName);
		if(pack == null)
		{
			Debug.LogError($"Failed to find pack {packName}");
			return;
		}

		try
		{
			// ----------------------------------------------------------------------------------
			// Export Content
			int contentCount = pack.ContentCount;
			int processedCount = 0;
			foreach (Definition def in pack.AllContent)
			{
				EditorUtility.DisplayProgressBar("Exporting Content", $"Exporting {processedCount + 1}/{contentCount} - {def.name}", (float)processedCount / contentCount);
				ExportAsset(pack.ModName, def, verifications);
			}
			EditorUtility.ClearProgressBar();
			// ----------------------------------------------------------------------------------

			// ----------------------------------------------------------------------------------
			// Export Textures
			int textureCount = pack.TextureCount;
			processedCount = 0;
			foreach (Texture2D texture in pack.AllTextures)
			{
				EditorUtility.DisplayProgressBar("Exporting Textures", $"Exporting {processedCount + 1}/{textureCount} - {texture.name}", (float)processedCount / textureCount);
				ExportAsset(pack.ModName, texture, verifications);
			}
			EditorUtility.ClearProgressBar();
			// ----------------------------------------------------------------------------------

			// ----------------------------------------------------------------------------------
			// Export Models
			int modelCount = pack.ModelCount;
			processedCount = 0;
			foreach (MinecraftModel model in pack.AllModels)
			{
				EditorUtility.DisplayProgressBar("Exporting Textures", $"Exporting {processedCount + 1}/{modelCount} - {model.name}", (float)processedCount / modelCount);
				ExportAsset(pack.ModName, model, verifications);
			}
			EditorUtility.ClearProgressBar();
			// ----------------------------------------------------------------------------------

			// ----------------------------------------------------------------------------------
			// Export Sounds
			int soundCount = pack.SoundCount;
			processedCount = 0;
			foreach (AudioClip sound in pack.AllSounds)
			{
				EditorUtility.DisplayProgressBar("Exporting Sounds", $"Exporting {processedCount + 1}/{soundCount} - {sound.name}", (float)processedCount / soundCount);
				ExportAsset(pack.ModName, sound, verifications);
			}
			EditorUtility.ClearProgressBar();
			// ----------------------------------------------------------------------------------

			// Export sounds.json
			EditorUtility.DisplayProgressBar("Exporting Misc Assets", "Exporting sounds.json", 0.0f);
			ExportSoundsJson(packName, verifications);

			// Export lang.jsons
			for (int i = 0; i < Langs.NUM_LANGS; i++)
			{
				EditorUtility.DisplayProgressBar("Exporting Misc Assets", $"Exporting {(ELang)i}.json", 0.5f + 0.5f * ((float)(i + 1) / Langs.NUM_LANGS));
				ExportLangJson(packName, (ELang)i, verifications);
			}
			EditorUtility.ClearProgressBar();

			
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}
	#endregion
	// ---------------------------------------------------------------------------------------
}
