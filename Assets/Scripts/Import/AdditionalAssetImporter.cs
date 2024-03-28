using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.FilePathAttribute;

public static class AdditionalAssetImporter
{
	public static readonly string IMPORT_ROOT = ContentManager.IMPORT_ROOT;
	public static readonly string MODEL_IMPORT_ROOT = ContentManager.MODEL_IMPORT_ROOT;
	public static readonly string ASSET_ROOT = ContentManager.ASSET_ROOT;

	public static List<string> GetOutputPaths(string srcPackName, InfoType inputType)
	{
		string dstPackName = Utils.ToLowerWithUnderscores(srcPackName);
		string dstShortName = Utils.ToLowerWithUnderscores(inputType.shortName);

		List<string> outputs = new List<string>();
		switch(DefinitionTypes.GetFromObject(inputType))
		{
			case EDefinitionType.gun:
			case EDefinitionType.attachment:
				if (inputType is PaintableType paintable)
				{
					foreach (Paintjob paintjob in paintable.paintjobs)
					{
						outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{Utils.ToLowerWithUnderscores(paintjob.iconName)}.png");
						outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png");
					}
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
				}
				break;
			case EDefinitionType.aa:
			case EDefinitionType.vehicle:
			case EDefinitionType.plane:
			case EDefinitionType.mecha:
			case EDefinitionType.bullet:
			case EDefinitionType.grenade:
				// Single entity model, skinned
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/entity/{dstShortName}.prefab");
				break;
			case EDefinitionType.part:
			case EDefinitionType.tool:
			case EDefinitionType.rewardBox:
			case EDefinitionType.mechaItem:
				// Single item model with icon
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
				break;
			case EDefinitionType.armour:
				// Armour skin/model
				// TODO:
				break;
			case EDefinitionType.armourBox:
			case EDefinitionType.box:
			case EDefinitionType.itemHolder:
				// Block model
				if(inputType is BoxType box)
				{
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.topTexturePath)}.png");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.sideTexturePath)}.png");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.bottomTexturePath)}.png");
				}
				else
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(inputType.texture)}.png");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/block/{dstShortName}.prefab");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/blockstates/{dstShortName}.json");
				break;
			case EDefinitionType.team:
			case EDefinitionType.loadout:
			case EDefinitionType.playerClass:
				// No models or textures for these
				break;
		}
		return outputs;
	}

	public static void ImportAssets(
		string srcPackName,
		InfoType inputType,
		List<string> allowedOutputs,
		IVerificationLogger logger = null)
	{
		string dstPackName = Utils.ToLowerWithUnderscores(srcPackName);
		string dstShortName = Utils.ToLowerWithUnderscores(inputType.shortName);

		// --------------------------------------------------------------
		// Texture Imports
		bool exportIcon = true;
		bool exportSkin = true;
		switch (DefinitionTypes.GetFromObject(inputType))
		{
			case EDefinitionType.gun:
			case EDefinitionType.vehicle:
			case EDefinitionType.mecha:
			case EDefinitionType.plane:
			case EDefinitionType.grenade:
			case EDefinitionType.attachment:
				if(inputType is PaintableType paintable)
				{
					foreach(Paintjob paintjob in paintable.paintjobs)
					{
						CopyTexture_Internal($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/textures/items/{paintjob.iconName}.png",
									 $"{ASSET_ROOT}/{dstPackName}/textures/item/{Utils.ToLowerWithUnderscores(paintjob.iconName)}.png",
									 allowedOutputs, logger);
						CopyTexture_Internal($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/skins/{paintjob.textureName}.png",
									$"{ASSET_ROOT}/{dstPackName}/textures/skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png",
									allowedOutputs, logger);
					}
				}				
				break;

			case EDefinitionType.bullet:
				exportSkin = false;
				break;

			case EDefinitionType.armourBox:
			case EDefinitionType.box:
			case EDefinitionType.itemHolder:
				// Block exports

				break;
			case EDefinitionType.armour:
				// Armour texture

				break;
			case EDefinitionType.aa:
			case EDefinitionType.part:
			case EDefinitionType.mechaItem:
			case EDefinitionType.tool:
			case EDefinitionType.rewardBox:
				// Nothing extra to add / remove
				break;
			case EDefinitionType.team:
			case EDefinitionType.playerClass:
			case EDefinitionType.loadout:
				// The "non-item" types
				exportIcon = false;
				exportSkin = false;
				break;
		}

		if(exportIcon)
		{
			CopyTexture_Internal($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/textures/items/{inputType.iconPath}.png",
									 $"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png",
									 allowedOutputs, logger);
		}
		if (exportSkin)
		{
			CopyTexture_Internal($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/skins/{inputType.texture}.png",
									 $"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png",
									 allowedOutputs, logger);
		}
		// --------------------------------------------------------------
		// Model Imports
		// --------------------------------------------------------------
		bool exportBasicItemModel = true;
		switch (DefinitionTypes.GetFromObject(inputType))
		{
			case EDefinitionType.gun:
				// Complicated stuff
				if (inputType is PaintableType paintable)
				{
					List<NamedTexture> icons = new List<NamedTexture>();
					List<NamedTexture> skins = new List<NamedTexture>();

					// Default skin + icon			
					ResourceLocation defaultSkinLocation = new ResourceLocation(dstPackName, dstShortName);
					ResourceLocation defaultIconLocation = new ResourceLocation(dstPackName, dstShortName);
					skins.Add(new NamedTexture("default",defaultSkinLocation, "textures/skins"));
					icons.Add(new NamedTexture("default", defaultIconLocation, "textures/item"));

					// And then paintjobs
					foreach (Paintjob paintjob in paintable.paintjobs)
					{
						string key = Utils.ToLowerWithUnderscores(paintjob.textureName);
						ResourceLocation skinLoc = new ResourceLocation(dstPackName, Utils.ToLowerWithUnderscores(paintjob.textureName));
						ResourceLocation iconLoc = new ResourceLocation(dstPackName, Utils.ToLowerWithUnderscores(paintjob.iconName));
						skins.Add(new NamedTexture(key, skinLoc, "textures/skins"));
						icons.Add(new NamedTexture(key, iconLoc, "textures/item"));
					}

					// Create the 3D model
					if (inputType.modelString != null && inputType.modelString.Length > 0)
					{
						ImportTurboRootNode($"{MODEL_IMPORT_ROOT}/{inputType.modelFolder}/Model{inputType.modelString}.java",
											$"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab",
											skins,
											icons,
											allowedOutputs,
											logger);
						exportBasicItemModel = false;
					}

				}
				break;
			case EDefinitionType.vehicle:
			case EDefinitionType.mecha:
			case EDefinitionType.plane:
				// TODO: Try and import the rig, not guaranteed to work yet
				break;
			case EDefinitionType.bullet:
			case EDefinitionType.grenade:
			case EDefinitionType.attachment:
				break;

			case EDefinitionType.armourBox:
			case EDefinitionType.box:
			case EDefinitionType.itemHolder:
				// Block exports

				break;
			case EDefinitionType.armour:
				// Armour texture

				break;
			case EDefinitionType.aa:
			case EDefinitionType.part:
			case EDefinitionType.mechaItem:
			case EDefinitionType.tool:
			case EDefinitionType.rewardBox:
				// Nothing extra to add / remove
				break;
			case EDefinitionType.team:
			case EDefinitionType.playerClass:
			case EDefinitionType.loadout:
				// The "non-item" types
				exportBasicItemModel = false;
				break;
		}

		if(exportBasicItemModel)
		{
			
		}
	}

	public static void ImportTurboRootNode(string from, string to, List<NamedTexture> skins, List<NamedTexture> icons, List<string> allowedOutputs, IVerificationLogger logger = null)
	{
		if (!allowedOutputs.Contains(to))
			return;

		// Step 1: Import the Java as a RootNode
		VerificationList importVerification = new VerificationList($"Import Model '{from}'");
		TurboRootNode rootNode = JavaModelImporter.ImportJavaModel(from, importVerification);
		VerifyType result = Verification.GetWorstState(importVerification);

		if (result == VerifyType.Fail)
		{
			logger?.Failure($"Failed to import {from} as a RootNode model");
			logger?.GetVerifications().AddRange(importVerification.GetVerifications());
			return;
		}

		// Step 2: Add additional data from outside the .java
		rootNode.Icons.AddRange(icons);
		rootNode.Textures.AddRange(skins);
		rootNode.AddDefaultTransforms();
		string nodeName = to.Substring(to.LastIndexOfAny(Utils.SLASHES)+1);
		nodeName = nodeName.Substring(0, nodeName.LastIndexOf('.'));
		rootNode.name = nodeName;
		rootNode.SelectTexture("default");

		// Step 3: Save that as a prefab
		try
		{
			string folderPath = to.Substring(0, to.LastIndexOf('/'));
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			PrefabUtility.SaveAsPrefabAsset(rootNode.gameObject, to);
			UnityEngine.Object.DestroyImmediate(rootNode.gameObject);
		}
		catch(Exception e)
		{
			logger?.Exception(e);
			return;
		}

		if (result == VerifyType.Neutral)
			logger?.Neutral($"Imported {from} as RootNode model with some warnings");
		else
			logger?.Success($"Imported {from} as RootNode successfully");
	}

	private static void CreateIconModel_Internal(ResourceLocation iconLocation, string location, List<string> allowedOutputs, IVerificationLogger logger = null)
	{
		if (!allowedOutputs.Contains(location))
			return;

		GameObject go = new GameObject(location);
		VanillaIconRootNode iconNode = go.AddComponent<VanillaIconRootNode>();
		iconNode.AddDefaultTransforms();
		iconNode.Icons.Add(new NamedTexture("default", iconLocation));
		SaveAsPrefab(go, location, true, logger);
		logger?.Success($"Created ItemModel at '{location}'");
	}

	private static void SaveAsPrefab(GameObject go, string path, bool destroyInstance = true, IVerificationLogger logger = null)
	{
		try
		{
			string folderPath = path.Substring(0, path.LastIndexOf('/'));
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			PrefabUtility.SaveAsPrefabAsset(go, path);
			if(destroyInstance)
				UnityEngine.Object.DestroyImmediate(go);
		}
		catch (Exception e)
		{
			logger?.Exception(e);
			return;
		}
	}

	private static void CopyTexture_Internal(string from, string to, List<string> allowedOutputs, IVerificationLogger logger = null)
	{
		if (!allowedOutputs.Contains(to))
			return;

		try
		{
			if (!File.Exists(from))
			{
				logger?.Neutral($"Failed to import {from} - FileNotFound");
				return;
			}

			string folderPath = to.Substring(0, to.LastIndexOf('/'));
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			File.Copy(from, to, true);
			logger?.Success($"Copied file from '{from}' to '{to}'");
		}
		catch(Exception e)
		{
			logger?.Exception(e);
		}
	}
}
