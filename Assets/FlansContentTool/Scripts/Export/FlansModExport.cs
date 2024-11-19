using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using static Definition;

public static class FlansModExport
{
	public static IVerificationLogger LastExportOperation = null; // new ExportLogger();
	public static IVerificationLogger GetFreshExportLogger(string opName)
	{
		LastExportOperation = new ExportLogger(opName);
		return LastExportOperation;
	}
	private static readonly char[] SLASHES = new char[] { '/', '\\' };

	// Store in ContentManager because that exists in scene and gets saved
	public static string EXPORT_ROOT { get { return ContentManager.inst.ExportRoot; } }

	// ----------------------------------------------------------------------------------------------------
	#region Single Asset Export
	// ----------------------------------------------------------------------------------------------------
	public static string[] GetExportPaths(string packName, UnityEngine.Object asset)
	{
		ResourceLocation loc = asset.GetLocation();
		if (asset is RootNode)
			return new string[] { $"{EXPORT_ROOT}/assets/{packName}/{loc.ID}.json" };
		if (asset is Texture2D)
			return new string[] { $"{EXPORT_ROOT}/assets/{packName}/{loc.ID}.png" };
		if (asset is AudioClip)
			return new string[] { $"{EXPORT_ROOT}/assets/{packName}/{loc.ID}.ogg" };

		// Some server-only assets
		if (asset is TeamDefinition || asset is LoadoutPoolDefinition || asset is ClassDefinition)
			return new string[] { $"{EXPORT_ROOT}/data/{packName}/{loc.ID}.json" };

		// Duplicate data that needs to be on client and server
		return new string[]
		{
			$"{EXPORT_ROOT}/data/{packName}/{loc.ID}.json",
			$"{EXPORT_ROOT}/assets/{packName}/{loc.ID}.json"
		};
	}

	public static void CreateDirectories(string filePath)
	{
		string folderName = filePath.Substring(0, filePath.LastIndexOfAny(SLASHES));
		if (!Directory.Exists(folderName))
			Directory.CreateDirectory(folderName);
	}

	public static bool ExportedAssetAlreadyExists(string packName, UnityEngine.Object asset)
	{
		foreach (string exportPath in GetExportPaths(packName, asset))
			if (File.Exists(exportPath))
				return true;
		return false;
	}

	public static List<AssetExporter> Exporters = new List<AssetExporter>(new AssetExporter[] {

		// Textures
		TextureExporter.ITEMS,
		TextureExporter.BLOCKS,
		TextureExporter.SKINS,
		TextureExporter.MAGS,
		TextureExporter.ENTITY,
		TextureExporter.GUI,
		TextureExporter.MOB_EFFECT,

		SoundExporter.INST,

		// Sounds

		// Animations
		UnityAnimationExporter.INST,
		DefinitionExporter.FLANIMATIONS,

		// Models
		NodeModelExporter.TURBO_MODELS,
		NodeModelExporter.VANILLA_BLOCKS,
		NodeModelExporter.VANILLA_ICONS,
		NodeModelExporter.VANILLA_JSON_MODELS,

		// Definitions
		DefinitionExporter.ARMOURS,
		DefinitionExporter.ATTACHMENTS,
		DefinitionExporter.BULLETS,
		DefinitionExporter.CLASSES,
		DefinitionExporter.TRAITS,
		DefinitionExporter.GRENADES,
		DefinitionExporter.GUNS,
		DefinitionExporter.LOADOUT_POOLS,
		DefinitionExporter.MAGAZINES,
		DefinitionExporter.MATERIALS,
		DefinitionExporter.NPCS,
		DefinitionExporter.PARTS,
		DefinitionExporter.REWARD_BOXES,
		DefinitionExporter.TEAMS,
		DefinitionExporter.TOOLS,
		DefinitionExporter.VEHICLES,
		DefinitionExporter.WORKBENCHES,
		DefinitionExporter.CONTROL_SCHEMES,

		// A fallback that will probably not put the definition where you want it
		DefinitionExporter.DEFAULT,

	});

	public static bool IsExportableType(Type type) { return TryGetExporter(type, out AssetExporter ignore); }
	public static bool IsExportableAsset(UnityEngine.Object asset) { return TryGetExporter(asset, out AssetExporter ignore); }
	public static bool TryGetExporter(Type type, out AssetExporter result)
	{
		foreach (AssetExporter exporter in Exporters)
		{
			if (exporter.MatchesAssetType(type))
			{
				result = exporter;
				return true;
			}
		}
		result = null;
		return false;
	}
	public static bool TryGetExporter(UnityEngine.Object asset, out AssetExporter result)
	{
		foreach(AssetExporter exporter in Exporters)
		{
			if (exporter.MatchesAssetType(asset.GetType()))
			{
				if (exporter.MatchesAsset(asset))
				{
					result = exporter;
					return true;
				}
			}
		}
		result = null;
		return false;
	}

	public static void ExportSingleAsset(UnityEngine.Object asset, bool overwrite = false)
	{
		string loggerName = $"Export {asset.name} {(overwrite ? "(Overrwriting conflict)" : "(Skipping conflict)")}";
		using (IVerificationLogger logger = GetFreshExportLogger(loggerName))
		{
			ExportAsset(asset, logger, overwrite);
		}
	}

	public static void ExportAsset(UnityEngine.Object asset, IVerificationLogger verifications, bool overwrite = false)
	{
		if (asset is ContentPack pack)
		{
			ExportPack(pack, false);
		}
		else if (TryGetExporter(asset, out AssetExporter exporter))
		{
			exporter.Export(asset, overwrite, verifications);
		}
		else
		{
			verifications?.Failure($"Could not find an exporter for {asset}");
		}
	}




	#endregion
	// ----------------------------------------------------------------------------------------------------


	// ----------------------------------------------------------------------------------------------------
	#region Content Pack Export
	// ----------------------------------------------------------------------------------------------------
	public static void ExportPack(ContentPack pack, bool overwrite = true)
	{
		string loggerName = $"Export {pack.ModName} {(overwrite ? "(Overrwriting conflicts)" : "(Skipping conflicts)")}";
		using (IVerificationLogger logger = GetFreshExportLogger(loggerName))
		{
			ExportPack(pack, logger, overwrite);
		}
	}
	public static void ExportPack(ContentPack pack, IVerificationLogger verifications, bool overwrite = true)
	{
		try
		{
			// ----------------------------------------------------------------------------------
			// Export Content
			int contentCount = pack.ContentCount;
			int processedCount = 0;
			foreach (Definition def in pack.AllContent)
			{
				EditorUtility.DisplayProgressBar("Exporting Content", $"Exporting {processedCount + 1}/{contentCount} - {def.name}", (float)processedCount / contentCount);
				ExportAsset(def, verifications, overwrite);

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
				ExportAsset(texture, verifications, overwrite);
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
				ExportAsset(model, verifications, overwrite);
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
						ExportAsset(clip, verifications, overwrite);
					}
				}
				processedCount++;
			}
			EditorUtility.ClearProgressBar();
			// ----------------------------------------------------------------------------------

			// Export sounds.json
			EditorUtility.DisplayProgressBar("Exporting Misc Assets", "Exporting sounds.json", 0.0f);
			ExportSoundsJson(pack, verifications);

			// Export lang.jsons
			for (int i = 0; i < Langs.NUM_LANGS; i++)
			{
				EditorUtility.DisplayProgressBar("Exporting Misc Assets", $"Exporting {(ELang)i}.json", 0.5f + 0.5f * ((float)(i + 1) / Langs.NUM_LANGS));
				ExportLangJson(pack, (ELang)i, verifications);
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
	public static void ExportItemTags(ContentPack pack, IVerificationLogger verifications = null)
	{
		Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();

		// Compile "Extra" tags, tags we couldn't set up as attached to a Definition for whatever reason
		foreach(TextAsset text in pack.AllExtraTags)
		{
			string fullPath = AssetDatabase.GetAssetPath(text);
			int idx = fullPath.LastIndexOf("tags");
			if(idx != -1)
			{
				string tagPath = fullPath.Substring(idx + "tags/".Length);
				tagPath = tagPath.Substring(0, tagPath.LastIndexOf("."));
				int firstSlash = tagPath.IndexOfAny(Utils.SLASHES);
				if (firstSlash != -1)
				{
					string modID = tagPath.Substring(0, firstSlash);
					tagPath = tagPath.Substring(firstSlash + 1);
					string modKeyTag = $"{modID}:{tagPath}";

					if (!tags.ContainsKey(modKeyTag))
					{
						tags.Add(modKeyTag, new List<string>());
					}

					using (StringReader stringReader = new StringReader(text.text))
					using (JsonReader jReader = new JsonTextReader(stringReader))
					{
						JObject root = JObject.Load(jReader);
						if (root.ContainsKey("values"))
						{
							if (root["values"] is JArray array)
							{
								foreach (JToken entry in array)
								{
									string tagEntry = entry.ToString();
									if (!tags[modKeyTag].Contains(tagEntry))
										tags[modKeyTag].Add(tagEntry);
								}
								verifications.Success($"Copied {array.Count} tags from existing Tag File '{modKeyTag}'");
							}
						}
					}
				}
					
			}
		}


		foreach (Definition def in pack.AllContent)
		{
			ItemDefinition itemSettings = def.GetItemSettings();
			if (itemSettings != null)
			{
				foreach (ResourceLocation tag in itemSettings.tags)
				{
					string tagExportName = tag.ResolveWithSubdir(def.GetTagExportFolder());
					if (!tags.ContainsKey(tagExportName))
						tags.Add(tagExportName, new List<string>());
					string tagEntry = $"{pack.ModName}:{def.GetLocation().IDWithoutPrefixes()}";
					if(!tags[tagExportName].Contains(tagEntry))
						tags[tagExportName].Add(tagEntry);
				}
			}
		}

		foreach (var kvp in tags)
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
					verifications?.Success($"Exporting partial tag file {partialTagJsonPath}");
				}
			}
			catch (Exception e)
			{
				verifications?.Exception(e);
			}
		}
	}
	public static void ExportSoundsJson(ContentPack pack, IVerificationLogger verifications = null)
	{
		QuickJSONBuilder builder = new QuickJSONBuilder();
		foreach (SoundEventEntry entry in pack.AllSounds)
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
			string dst = GetSoundJsonExportPath(pack);
			File.WriteAllText(dst, stringWriter.ToString());
			verifications?.Success($"Exported sound json to '{dst}'");
		}
	}
	public static void ExportLangJson(ContentPack pack, ELang lang, IVerificationLogger verifications = null)
	{
		string exportPath = GetLangExportPath(pack, lang);
		string exportFolder = exportPath.Substring(0, exportPath.LastIndexOfAny(Utils.SLASHES));
		if (!Directory.Exists(exportFolder))
			Directory.CreateDirectory(exportFolder);
		try
		{
			JObject jRoot = new JObject();
			foreach (Definition def in pack.AllContent)
			{
				foreach (LocalisedName locName in def.LocalisedNames)
					if (locName.Lang == lang)
						jRoot.Add($"{def.GetLocalisationPrefix()}.{pack.ModName}.{def.name}", locName.Name);

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
		catch (Exception e)
		{
			if (verifications != null)
				verifications.Exception(e);
		}
	}
	public static string GetPartialTagJsonExportPath(string tagModID, string providerModID, string tagName)
	{
		return $"{EXPORT_ROOT}/data/{providerModID}/partial_data/{tagModID}/tags/{tagName}.json";
	}
	public static string GetDevTagJsonExportPath(string tagModID, string tagPath)
	{
		return $"{EXPORT_ROOT}/data/{tagModID}/tags/{tagPath}.json";
	}
	public static string GetSoundJsonExportPath(ContentPack pack)
	{
		return $"{EXPORT_ROOT}/assets/{pack.ModName}/sounds.json";
	}
	public static string GetLangExportPath(ContentPack pack, ELang lang)
	{
		return $"{EXPORT_ROOT}/assets/{pack.ModName}/lang/{lang}.json";
	}
	#endregion
	// ----------------------------------------------------------------------------------------------------

	// ----------------------------------------------------------------------------------------------------
	#region Dev Environment Fixes
	// ----------------------------------------------------------------------------------------------------
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
	public static void CompileAllItemTags(IVerificationLogger verifications = null)
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

		foreach (string modDir in Directory.EnumerateDirectories($"{EXPORT_ROOT}/data/"))
		{
			string modID = modDir.Substring(modDir.LastIndexOfAny(Utils.SLASHES) + 1);
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
						verifications?.Success($"Found partial tag file {tagFilePath}");

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
							verifications?.Exception(e, $"Failed to parse json for partial tag file at '{tagFilePath}'");
						}
					}
				}
			}
		}

		foreach (var kvp in tagCollections)
		{
			string tagModID = kvp.Key;
			if (!Directory.Exists($"{tagModID}/tags/"))
				Directory.CreateDirectory($"{tagModID}/tags/");

			TagCollection tagCollection = kvp.Value;

			foreach (var kvp2 in tagCollection.Tags)
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

						verifications?.Success($"Exporting compiled tag file '{tagJsonPath}' for dev-environment");
						File.WriteAllText(tagJsonPath, stringWriter.ToString());
					}
				}
				catch (Exception e)
				{
					verifications?.Exception(e);
				}
			}
		}
	}
    #endregion
    // ----------------------------------------------------------------------------------------------------


    public static JArray ToJson(this Vector3 v) { return new JArray() { v.x, v.y, v.z }; }
    public static JArray ToJson(this Vector2 v) { return new JArray() { v.x, v.y }; }
    public static JArray ToJson(this Vector2Int v) { return new JArray() { v.x, v.y }; }
}
