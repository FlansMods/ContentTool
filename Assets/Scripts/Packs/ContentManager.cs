using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static Definition;
using static UnityEditor.ObjectChangeEventStream;

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
		
	}

	// ---------------------------------------------------------------------------------------
	#region Pre-Import Pack storage
	// ---------------------------------------------------------------------------------------
	public class PreImportPack
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
								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/textures/item/{Utils.ToLowerWithUnderscores(splits[1])}.png");
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

								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/textures/item/{Utils.ToLowerWithUnderscores(splits[1])}.png");
								additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/textures/skins/{Utils.ToLowerWithUnderscores(splits[2])}.png");
							}
							if (splits[0] == "Model" || splits[0] == "DeployedModel")
							{
								string[] modelSteps = splits[1].Split('.');
								if (modelSteps.Length == 2)
								{
									additionalInfo.Inputs.Add($"{MODEL_IMPORT_ROOT}/{modelSteps[0]}/Model{modelSteps[1]}.java");
									additionalInfo.Outputs.Add($"{ASSET_ROOT}/{packName}/models/item/{shortName}.prefab");
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
	public class ModelList
	{
		public string FolderName;
		public string PackName = "";
		public List<string> Models = new List<string>();
		private static string ConvertName(string modelName)
		{
			return Minecraft.SanitiseID(modelName.Substring("Model".Length, modelName.Length - "Model".Length - ".java".Length));
		}
		public IEnumerable<KeyValuePair<string, string>> ImportMappings 
		{ 
			get 
			{
				foreach (string modelName in Models)
				{
					yield return new KeyValuePair<string, string>(
						$"{MODEL_IMPORT_ROOT}/{FolderName}/{modelName}", 
						$"{ASSET_ROOT}/{PackName}/models/item/{ConvertName(modelName)}.prefab");
				}
			} 
		}
	}
	[SerializeField]
	private List<PreImportPack> PreImportPacks = null;
	private Dictionary<string, ModelList> PreImportModels = null;
	private void CachePreImportPacks()
	{
		PreImportPacks = new List<PreImportPack>();
		foreach (DirectoryInfo subDir in new DirectoryInfo(IMPORT_ROOT).EnumerateDirectories())
		{
			PreImportPacks.Add(new PreImportPack() { PackName = subDir.Name });
		}
		if (PreImportModels == null)
		{
			PreImportModels = new Dictionary<string, ModelList>();
			foreach (DirectoryInfo subDir in new DirectoryInfo(MODEL_IMPORT_ROOT).EnumerateDirectories())
			{
				ModelList modelList = new ModelList();
				modelList.FolderName = subDir.Name;
				foreach (FileInfo modelFile in subDir.EnumerateFiles())
				{
					modelList.Models.Add(modelFile.Name);
				}
				PreImportModels[subDir.Name] = modelList;
			}
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
	public Dictionary<string, ModelList> GetPreImportModelList()
	{
		Refresh();
		return PreImportModels;
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
	public List<PreImportPack> GetPreImportPacks()
	{
		Refresh();
		return PreImportPacks;
	}
	public Dictionary<PreImportPack, ContentPack> GetImportPackMap()
	{
		Refresh();
		Dictionary<PreImportPack, ContentPack> dict = new Dictionary<PreImportPack, ContentPack>();
		foreach (PreImportPack pack in PreImportPacks)
		{
			dict.Add(pack, FindContentPack(pack.PackName, false));
		}
		return dict;
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

	public ContentPack FindContentPack(string packName, bool canSearchAssets = true)
	{
		// First, see if we already cached it
		foreach (ContentPack pack in Packs)
			if (pack != null)
				if (pack.name == packName)
					return pack;

		// Second, try to find it in the asset database
		if (canSearchAssets)
		{
			foreach (string cpPath in AssetDatabase.FindAssets("t:ContentPack"))
			{
				ContentPack loadedPack = AssetDatabase.LoadAssetAtPath<ContentPack>(cpPath);
				if (loadedPack != null)
				{
					Packs.Add(loadedPack);
					return loadedPack;
				}
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
	public static List<Verification> LastImportOperationResults = new List<Verification>();
	public static string LastImportOperation = "None";
	public static List<Verification> GetFreshImportLogger(string opName) 
	{
		LastImportOperation = opName;
		LastImportOperationResults.Clear(); 
		return LastImportOperationResults; 
	}
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
				return null;
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
		string blockNamePrefix = $"block.{packName}.";
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
								else if(TryImportLocalisationLine(blockNamePrefix, lang, kvp.Key, kvp.Value.ToString(), pack))
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
				verifications.Add(Verification.Exception(e));
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
				if(soundDef.sound.Namespace == "minecraft")
				{
					soundDef.sound = new ResourceLocation(pack.ModName, soundDef.sound.ID);
				}
				foreach(SoundLODDefinition soundLOD in soundDef.LODs)
				{
					if (soundLOD.sound.Namespace == "minecraft")
					{
						soundLOD.sound = new ResourceLocation(pack.ModName, soundLOD.sound.ID);
					}
				}
			}
			else if(obj is ItemStackDefinition itemStackDef)
			{
				itemStackDef.item = ValidateItemId(itemStackDef.item, itemStackDef.damage, pack);
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
	public static string LastExportOperation = "None";
	public static List<Verification> LastExportOperationResults = new List<Verification>();
	public static List<Verification> GetFreshExportLogger(string opName)
	{
		LastExportOperation = opName;
		LastExportOperationResults.Clear();
		return LastExportOperationResults;
	}
	private static readonly char[] SLASHES = new char[] { '/', '\\' };
	public string[] GetExportPaths(string packName, UnityEngine.Object asset)
	{
		ResourceLocation loc = asset.GetLocation();
		if(asset is RootNode)
			return new string[] { $"{ExportRoot}/assets/{packName}/{loc.ID}.json" };
		if (asset is Texture2D)
			return new string[] { $"{ExportRoot}/assets/{packName}/{loc.ID}.png" };
		if (asset is AudioClip)
			return new string[] { $"{ExportRoot}/assets/{packName}/{loc.ID}.ogg" };

		// Some server-only assets
		if(asset is TeamDefinition || asset is LoadoutPoolDefinition || asset is ClassDefinition)
			return new string[] { $"{ExportRoot}/data/{packName}/{loc.ID}.json" };

		// Duplicate data that needs to be on client and server
		return new string[] 
		{ 
			$"{ExportRoot}/data/{packName}/{loc.ID}.json",
			$"{ExportRoot}/assets/{packName}/{loc.ID}.json"
		};
	}

	public bool ExportedAssetAlreadyExists(string packName, UnityEngine.Object asset)
	{
		foreach (string exportPath in GetExportPaths(packName, asset))
			if (File.Exists(exportPath))
				return true;
		return false;
	}

	public void ExportAsset(string packName, UnityEngine.Object asset, List<Verification> verifications = null)
	{
		foreach (string exportPath in GetExportPaths(packName, asset))
		{
			string exportFolder = exportPath.Substring(0, exportPath.LastIndexOfAny(SLASHES));
			if (!Directory.Exists(exportFolder))
				Directory.CreateDirectory(exportFolder);
			if (asset is Definition def)
			{
				JsonExporter.Export(def, exportPath, verifications);
			}
			else if(asset is TurboRootNode rootNode)
			{
				JObject jObject = ExportNodeModel.ExportRoot(rootNode);
				JsonExporter.Export(jObject, exportPath, verifications);
			}
			else if(asset is VanillaIconRootNode iconRootNode)
			{
				JsonExporter.CreateVanillaItemIcon(iconRootNode.GetLocation(), exportPath, verifications);
			}
			else if(asset is VanillaJsonRootNode jsonRootNode)
			{
				JsonExporter.CreateVanillaJson(jsonRootNode.Json, exportPath, verifications);
			}
			else if (asset is Texture2D texture)
			{
				CopyAsset(texture, exportPath);
			}
			else if (asset is AudioClip audio)
			{
				CopyAsset(audio, exportPath);
			}
			else
			{
				if (verifications != null)
					verifications.Add(Verification.Failure($"Unknown asset type {asset.GetType()} for {asset}"));
			}
		}
	}
	public void CopyAsset(UnityEngine.Object asset, string outputPath, List<Verification> verifications = null)
	{
		try
		{
			string assetPath = AssetDatabase.GetAssetPath(asset);
			File.Copy(assetPath, outputPath, true);
			string successMsg = $"Copied asset '{asset.name}' to {outputPath}";
			Debug.Log(successMsg);
			if (verifications != null)
				verifications.Add(Verification.Success(successMsg));
		}
		catch(Exception e)
		{
			if (verifications != null)
				verifications.Add(Verification.Exception(e));
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
		if (def is WorkbenchDefinition)
			return "block";
		if (def is NpcDefinition)
			return "entity";
		if (def is AbilityDefinition)
			return "ability";
		return "item";
	}
	public void ExportLangJson(string packName, ELang lang, List<Verification> verifications = null)
	{
		string exportPath = GetLangExportPath(packName, lang);
		string exportFolder = exportPath.Substring(0, exportPath.LastIndexOfAny(SLASHES));
		if (!Directory.Exists(exportFolder))
			Directory.CreateDirectory(exportFolder);
		ContentPack pack = FindContentPack(packName);
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
				verifications.Add(Verification.Exception(e));
		}
	}
	public string GetPartialTagJsonExportPath(string tagModID, string providerModID, string tagName)
	{
		return $"{ExportRoot}/data/{providerModID}/partial_data/{tagModID}/tags/{tagName}.json";
	}
	public string GetDevTagJsonExportPath(string tagModID, string tagPath)
	{
		return $"{ExportRoot}/data/{tagModID}/tags/{tagPath}.json";
	}
	public string GetSoundJsonExportPath(string packName)
	{
		return $"{ExportRoot}/assets/{packName}/sounds.json";
	}
	public void ExportSoundsJson(string packName, List<Verification> verifications = null)
	{
		ContentPack pack = FindContentPack(packName);
		QuickJSONBuilder builder = new QuickJSONBuilder();
		foreach(SoundEventEntry entry in pack.AllSounds)
		{
			using (builder.Indentation(entry.Key))
			{
				builder.Current.Add("category", entry.Category);
				using (builder.Tabulation("sounds"))
				{
					foreach (ResourceLocation soundLoc in entry.SoundLocations)
					{
						using (builder.TableEntry())
						{
							builder.Current.Add("name", soundLoc.ExportAsSoundPath());
							builder.Current.Add("type", "file");
						}
					}
				}
			}
		}

		using (StringWriter stringWriter = new StringWriter())
		using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.Formatting = Formatting.Indented;
			builder.Root.WriteTo(jsonWriter);
			string dst = GetSoundJsonExportPath(packName);
			File.WriteAllText(dst, stringWriter.ToString());
			verifications?.Add(Verification.Success($"Exported sound json to '{dst}'"));
		}
	}

	public void ExportItemTags(ContentPack pack, List<Verification> verifications = null)
	{
		Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
		foreach(Definition def in pack.AllContent)
		{
			ItemDefinition itemSettings = def.GetItemSettings();
			if(itemSettings != null)
			{
				foreach(ResourceLocation tag in itemSettings.tags)
				{
					string tagExportName = tag.ResolveWithSubdir(def.GetTagExportFolder()); ;
					if (!tags.ContainsKey(tagExportName))
						tags.Add(tagExportName, new List<string>());
					tags[tagExportName].Add($"{pack.ModName}:{def.GetLocation().IDWithoutPrefixes()}");
				}
			}
		}

		foreach(var kvp in tags)
		{
			try
			{
				using (StringWriter stringWriter = new StringWriter())
				using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
				{
					// Write out the tags to a json array
					jsonWriter.Formatting = Formatting.Indented;
					QuickJSONBuilder builder = new QuickJSONBuilder();
					builder.Current.Add("replace", "false");
					using (builder.Tabulation("values"))
					{
						foreach (string itemLoc in kvp.Value)
							builder.CurrentTable.Add(itemLoc);
					}
					builder.Root.WriteTo(jsonWriter);

					// Bit weird, but Minecraft's tag system is not great for developing multiple addons/whatever in the same
					// workspace. So we export partial tags to a temp location. These won't do anything to your dev env,
					// but will be picked up by gradle pack publishing, if you use that.
					ResourceLocation tagLoc = new ResourceLocation(kvp.Key);
					string partialTagJsonPath = GetPartialTagJsonExportPath(tagLoc.Namespace, pack.ModName, tagLoc.ID);
					string tagJsonFolder = partialTagJsonPath.Substring(0, partialTagJsonPath.LastIndexOfAny(SLASHES));
					if (!Directory.Exists(tagJsonFolder))
						Directory.CreateDirectory(tagJsonFolder);

					
					File.WriteAllText(partialTagJsonPath, stringWriter.ToString());
					verifications?.Add(Verification.Success($"Exporting partial tag file {partialTagJsonPath}"));
				}
			}
			catch(Exception e)
			{
				verifications?.Add(Verification.Exception(e));
			}
		}
	}

	private class TagCollection
	{
		public string TagModID;
		public List<string> SupplierModIDs = new List<string>();

		public class TagValues
		{
			public List<string> ElementIDs = new List<string>();
		}
		public Dictionary<string, TagValues> Tags = new Dictionary<string, TagValues>();
	}
	private static readonly Regex TagMatcher = new Regex("data[\\/\\\\]([a-z-_]*)[\\\\\\/]partial_data[\\\\\\/]([a-z-_]*)[\\\\\\/]tags[\\\\\\/]([a-z-_\\\\\\/]*).json");
	public void CompileAllItemTags(List<Verification> verifications = null)
	{
		// Here, we compile our partial tag files e.g.
		//	resources/data/flansvendersgame/partial_tags/flansmod/item/gun.json
		//  resources/data/flansbasicparts/partial_tags/flansmod/item/gun.json
		//  ...
		// into
		//  resources/data/flansmod/tags/item/gun.json
		// 
		// The former are needed for pack export
		// The latter are needed for running in dev environment

		Dictionary<string, TagCollection> tagCollections = new Dictionary<string, TagCollection>();

		foreach (string modDir in Directory.EnumerateDirectories($"{ExportRoot}/data/"))
		{
			string modID = modDir.Substring(modDir.LastIndexOfAny(SLASHES) + 1);
			if (Directory.Exists($"{modDir}/partial_data/"))
			{
				// Dig arbitrarily deep for tags
				foreach (string tagFilePath in Directory.EnumerateFiles($"{modDir}/partial_data/", "*.json", SearchOption.AllDirectories))
				{
					Match match = TagMatcher.Match(tagFilePath);
					if (match.Success)
					{
						// modID = match.Groups[1]
						string tagModID = match.Groups[2].Value;
						string tagPath = match.Groups[3].Value;
						if (!tagCollections.ContainsKey(tagModID))
							tagCollections.Add(tagModID, new TagCollection());

						// Note (for debug) that we supplied tags from this mod
						if (!tagCollections[tagModID].SupplierModIDs.Contains(modID))
							tagCollections[tagModID].SupplierModIDs.Add(modID);
						verifications?.Add(Verification.Success($"Found partial tag file {tagFilePath}"));

						if (!tagCollections[tagModID].Tags.ContainsKey(tagPath))
							tagCollections[tagModID].Tags.Add(tagPath, new TagCollection.TagValues());

						// Now add all the tags from inside this json to our storage
						try
						{
							JObject jTagRoot = JObject.Parse(File.ReadAllText(tagFilePath));
							if (jTagRoot.TryGetValue("values", out JToken jToken) && jToken is JArray jArray)
							{
								foreach (JToken entry in jArray)
									tagCollections[tagModID].Tags[tagPath].ElementIDs.Add(entry.ToString());
							}
						}
						catch (Exception e)
						{
							verifications?.Add(Verification.Exception(e, $"Failed to parse json for partial tag file at '{tagFilePath}'"));
						}
					}
				}
			}
		}

		foreach(var kvp in tagCollections)
		{
			string tagModID = kvp.Key;
			if (!Directory.Exists($"{tagModID}/tags/"))
				Directory.CreateDirectory($"{tagModID}/tags/");

			TagCollection tagCollection = kvp.Value;

			foreach(var kvp2 in tagCollection.Tags)
			{
				string tagPath = kvp2.Key;
				TagCollection.TagValues values = kvp2.Value;

				try
				{
					using (StringWriter stringWriter = new StringWriter())
					using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
					{
						jsonWriter.Formatting = Formatting.Indented;
						QuickJSONBuilder builder = new QuickJSONBuilder();
						builder.Current.Add("replace", "false");
						using (builder.Tabulation("values"))
						{
							foreach (string itemLoc in values.ElementIDs)
								builder.CurrentTable.Add(itemLoc);
						}
						builder.Root.WriteTo(jsonWriter);

						string tagJsonPath = GetDevTagJsonExportPath(tagModID, tagPath);
						string tagJsonFolder = tagJsonPath.Substring(0, tagJsonPath.LastIndexOfAny(SLASHES));
						if (!Directory.Exists(tagJsonFolder))
							Directory.CreateDirectory(tagJsonFolder);

						verifications?.Add(Verification.Success($"Exporting compiled tag file '{tagJsonPath}' for dev-environment"));
						File.WriteAllText(tagJsonPath, stringWriter.ToString());
					}
				}
				catch (Exception e)
				{
					verifications?.Add(Verification.Exception(e));
				}
			}
		}

		/*
		{ 




			try
			{
				Dictionary<string, List<string>> allTags = new Dictionary<string, List<string>>();
				if (Directory.Exists($"{modDir}/partial_data/"))
				{
					// Dig arbitrarily deep for tags
					foreach (string tagFilePath in Directory.EnumerateFiles($"{modDir}/partial_data/", "*.json", SearchOption.AllDirectories))
					{
						//string tagID = tagFilePath.Substring(tagFilePath.LastIndexOfAny(SLASHES) + 1);
						//tagID = tagID.Substring(0, tagID.LastIndexOf('.'));

						string tagAndFolders = tagFilePath.Substring(tagFilePath.IndexOf("partial_data") + "partial_data/".Length);
						// Remove the '/{modID}/'
						tagAndFolders = tagAndFolders.Substring(tagAndFolders.IndexOfAny(SLASHES) + 1);
						// Remove the '/tags/'
						tagAndFolders = tagAndFolders.Substring(tagAndFolders.IndexOfAny(SLASHES) + 1);
						// Remove the '.json'
						tagAndFolders = tagAndFolders.Substring(0, tagAndFolders.LastIndexOf('.'));

						verifications?.Add(Verification.Success($"Found partial tag file {tagAndFolders}"));
						try
						{
							JObject jTagRoot = JObject.Parse(File.ReadAllText(tagFilePath));
							if (jTagRoot.TryGetValue("values", out JToken jToken) && jToken is JArray jArray)
							{
								if (!allTags.ContainsKey(tagAndFolders))
									allTags.Add(tagAndFolders, new List<string>());

								foreach (JToken entry in jArray)
									allTags[tagAndFolders].Add(entry.ToString());
							}
						}
						catch (Exception e)
						{
							verifications?.Add(Verification.Exception(e, $"Failed to parse json for partial tag file at '{tagFilePath}'"));
						}
					}

					if (allTags.Count > 0)
					{
						if (!Directory.Exists($"{modDir}/tags/"))
						{
							Directory.CreateDirectory($"{modDir}/tags/");
						}

						// Now repackage the tags into a final .json with all in it
						foreach (var kvp in allTags)
						{
							using (StringWriter stringWriter = new StringWriter())
							using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
							{
								jsonWriter.Formatting = Formatting.Indented;
								QuickJSONBuilder builder = new QuickJSONBuilder();
								builder.Current.Add("replace", "false");
								using (builder.Tabulation("values"))
								{
									foreach (string itemLoc in kvp.Value)
										builder.CurrentTable.Add(itemLoc);
								}
								builder.Root.WriteTo(jsonWriter);


								string tagFolder = "";
								string tagName = kvp.Key;
								int lastSlash = tagName.LastIndexOfAny(SLASHES);
								if(lastSlash != -1)
								{
									tagFolder = tagName.Substring(0, lastSlash);
									tagName = tagName.Substring(lastSlash + 1);
								}
								
								string tagJsonPath = GetDevTagJsonExportPath(new ResourceLocation(new ResourceLocation(modID, tagName).ResolveWithSubdir(tagFolder)));
								string tagJsonFolder = tagJsonPath.Substring(0, tagJsonPath.LastIndexOfAny(SLASHES));
								if (!Directory.Exists(tagJsonFolder))
									Directory.CreateDirectory(tagJsonFolder);

								verifications?.Add(Verification.Success($"Exporting compiled tag file '{tagJsonPath}' for dev-environment"));
								File.WriteAllText(tagJsonPath, stringWriter.ToString());
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				verifications?.Add(Verification.Exception(e));
			}
		}
		*/
	}

	public void ExportPack(string packName, bool overwrite, List<Verification> verifications = null)
	{
		ContentPack pack = FindContentPack(packName);
		if(pack == null)
		{
			verifications?.Add(Verification.Failure($"Failed to find pack {packName}"));
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
				processedCount++;
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
				processedCount++;
			}
			EditorUtility.ClearProgressBar();
			// ----------------------------------------------------------------------------------

			// ----------------------------------------------------------------------------------
			// Export Models
			int modelCount = pack.ModelCount;
			processedCount = 0;
			foreach (RootNode model in pack.AllModels)
			{
				EditorUtility.DisplayProgressBar("Exporting Models", $"Exporting {processedCount + 1}/{modelCount} - {model.name}", (float)processedCount / modelCount);
				ExportAsset(pack.ModName, model, verifications);
				processedCount++;
			}
			EditorUtility.ClearProgressBar();
			// ----------------------------------------------------------------------------------

			// ----------------------------------------------------------------------------------
			// Export Sounds
			int soundCount = pack.SoundCount;
			processedCount = 0;
			foreach (SoundEventEntry sound in pack.AllSounds)
			{
				EditorUtility.DisplayProgressBar("Exporting Sounds", $"Exporting {processedCount + 1}/{soundCount} - {sound.Key}", (float)processedCount / soundCount);
				foreach (ResourceLocation soundLoc in sound.SoundLocations)
				{
					if (soundLoc.TryLoad(out AudioClip clip))
					{
						EditorUtility.DisplayProgressBar("Exporting Sounds", $"Exporting {processedCount + 1}/{soundCount} - {sound.Key} - {clip.name}", (float)processedCount / soundCount);
						ExportAsset(pack.ModName, clip, verifications);
					}
				}
				processedCount++;
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

			ExportItemTags(pack, verifications);
			CompileAllItemTags(verifications);
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}
	#endregion
	// ---------------------------------------------------------------------------------------
}
