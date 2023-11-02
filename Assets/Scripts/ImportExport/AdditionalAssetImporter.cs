using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.FilePathAttribute;

public static class AdditionalAssetImporter
{
	public static readonly string IMPORT_ROOT = DefinitionImporter.IMPORT_ROOT;
	public static readonly string MODEL_IMPORT_ROOT = DefinitionImporter.MODEL_IMPORT_ROOT;
	public static readonly string ASSET_ROOT = DefinitionImporter.ASSET_ROOT;

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
						outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/{dstShortName}/{paintjob.iconName}_icon.asset");
					}
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}/{dstShortName}_3d.asset");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}/{dstShortName}_icon.asset");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}/{dstShortName}_default_icon.asset");
					outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.asset");
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
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.asset");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/entity/{dstShortName}.asset");
				break;
			case EDefinitionType.part:
			case EDefinitionType.tool:
			case EDefinitionType.rewardBox:
			case EDefinitionType.mechaItem:
				// Single item model with icon
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.asset");
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
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/block/{dstShortName}.asset");
				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.asset");
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
		List<Verification> errors)
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
			case EDefinitionType.bullet:
			case EDefinitionType.grenade:
			case EDefinitionType.attachment:
				if(inputType is PaintableType paintable)
				{
					foreach(Paintjob paintjob in paintable.paintjobs)
					{
						CopyTexture_Internal($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/textures/items/{paintjob.iconName}.png",
									 $"{ASSET_ROOT}/{dstPackName}/textures/item/{Utils.ToLowerWithUnderscores(paintjob.iconName)}.png",
									 allowedOutputs, errors);
						CopyTexture_Internal($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/skins/{paintjob.textureName}.png",
									$"{ASSET_ROOT}/{dstPackName}/textures/skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png",
									allowedOutputs, errors);
					}
				}				
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
									 allowedOutputs, errors);
		}
		if (exportSkin)
		{
			CopyTexture_Internal($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/skins/{inputType.texture}.png",
									 $"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png",
									 allowedOutputs, errors);
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
					// Create the array of icon models and the skin switcher that points to them
					List<ResourceLocation> iconLocations = new List<ResourceLocation>();
					List<ResourceLocation> skinLocations = new List<ResourceLocation>();

					// Default skin + icon
					ResourceLocation defaultIconLocation = new ResourceLocation(dstPackName, dstShortName);
					ResourceLocation defaultSkinLocation = new ResourceLocation(dstPackName, dstShortName);
					iconLocations.Add(defaultIconLocation);
					skinLocations.Add(defaultSkinLocation);
					CreateIconModel_Internal(defaultIconLocation, $"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}/{dstShortName}_default_icon.asset", allowedOutputs, errors);

					// And then paintjobs
					foreach (Paintjob paintjob in paintable.paintjobs)
					{
						ResourceLocation iconLoc = new ResourceLocation(dstPackName, Utils.ToLowerWithUnderscores(paintjob.iconName));
						ResourceLocation skinLoc = new ResourceLocation(dstPackName, Utils.ToLowerWithUnderscores(paintjob.textureName));
						iconLocations.Add(iconLoc);
						skinLocations.Add(skinLoc);
						CreateIconModel_Internal(iconLoc, $"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}/{iconLoc.ID}_icon.asset", allowedOutputs, errors);
					}
					
					CreateSkinSwitcher_Internal(iconLocations, $"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}/{dstShortName}_icon.asset", allowedOutputs, errors);

					// Create the 3D model
					ImportRig_Internal($"{MODEL_IMPORT_ROOT}/{inputType.modelFolder}/Model{inputType.modelString}.java",
										$"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}/{dstShortName}_3d.asset",
										skinLocations, allowedOutputs, errors);

					// And link them all together with a MultiModel
					ResourceLocation loc3d = new ResourceLocation(dstPackName, $"models/item/{dstShortName}/{dstShortName}_3d");
					ResourceLocation locSwitcher = new ResourceLocation(dstPackName, $"models/item/{dstShortName}/{dstShortName}_icon");
					CreateMultiModel_Internal(loc3d, loc3d, locSwitcher, loc3d, loc3d, loc3d, $"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.asset", allowedOutputs, errors);
					exportBasicItemModel = false;
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

	private static void CreateSkinSwitcher_Internal(List<ResourceLocation> skinLocations, string location, List<string> allowedOutputs, List<Verification> errors)
	{
		if (!allowedOutputs.Contains(location))
			return;

		SkinSwitcherModel switcher = ScriptableObject.CreateInstance<SkinSwitcherModel>();
		switcher.AddDefaultTransforms();
		switcher.Icons = new List<MinecraftModel.NamedTexture>(skinLocations.Count);
		for(int i = 0; i < skinLocations.Count; i++)
		{
			switcher.Icons.Add(new MinecraftModel.NamedTexture()
			{
				Key = $"{i + 1}",
				Location = skinLocations[i],
				Texture = skinLocations[i].Load<Texture2D>("textures/item")
			});
		}
		DefinitionImporter.CreateUnityAsset(switcher, location);
		errors.Add(Verification.Success($"Created SkinSwitcher model at '{location}'"));
	}

	private static void CreateIconModel_Internal(ResourceLocation iconLocation, string location, List<string> allowedOutputs, List<Verification> errors)
	{
		if (!allowedOutputs.Contains(location))
			return;

		ItemModel item = ScriptableObject.CreateInstance<ItemModel>();
		item.AddDefaultTransforms();
		item.IconLocation = iconLocation;
		item.Icon = iconLocation.Load<Texture2D>("textures/item");
		DefinitionImporter.CreateUnityAsset(item, location);
		errors.Add(Verification.Success($"Created ItemModel at '{location}'"));
	}

	private static void CreateMultiModel_Internal(ResourceLocation firstPersonModel, ResourceLocation thirdPersonModel,
													ResourceLocation guiModel, ResourceLocation fixedModel,
													ResourceLocation groundModel, ResourceLocation headModel,
													string location, List<string> allowedOutputs, List<Verification> errors)
	{
		if (!allowedOutputs.Contains(location))
			return;

		MultiModel multi = ScriptableObject.CreateInstance<MultiModel>();
		multi.AddDefaultTransforms();
		multi.FirstPersonModel = firstPersonModel.Load<MinecraftModel>();
		multi.ThirdPersonModel = thirdPersonModel.Load<MinecraftModel>();
		multi.GUIModel = guiModel.Load<MinecraftModel>();
		multi.FixedModel = fixedModel.Load<MinecraftModel>();
		multi.GroundModel = groundModel.Load<MinecraftModel>();
		multi.HeadModel = headModel.Load<MinecraftModel>();
		DefinitionImporter.CreateUnityAsset(multi, location);
		errors.Add(Verification.Success($"Created MultiModel at '{location}'"));
	}

	private static void ImportRig_Internal(string from, string to, List<ResourceLocation> textureLocations, List<string> allowedOutputs, List<Verification> errors)
	{
		if (!allowedOutputs.Contains(to))
			return;

		TurboRig rig = JavaModelImporter.ImportTurboModel("", from, null);
		if(rig == null)
		{
			errors.Add(Verification.Failure($"TurboRig import from '{from}' failed"));
			return;
		}

		for(int i = 0; i < textureLocations.Count; i++)
		{
			rig.Textures.Add(new MinecraftModel.NamedTexture()
			{
				Key = $"{i + 1}",
				Location = textureLocations[i],
				Texture = textureLocations[i].Load<Texture2D>("textures/skins")
			});
		}

		DefinitionImporter.CreateUnityAsset(rig, to);
		errors.Add(Verification.Success($"Imported TurboRig to '{rig}'"));
	}

	private static void CopyTexture_Internal(string from, string to, List<string> allowedOutputs, List<Verification> errors)
	{
		if (!allowedOutputs.Contains(to))
			return;

		try
		{
			if (!File.Exists(from))
			{
				errors.Add(Verification.Failure($"Failed to import {from} - FileNotFound"));
				return;
			}

			string folderPath = to.Substring(0, to.LastIndexOf('/'));
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			File.Copy(from, to);
			errors.Add(Verification.Success($"Copied file from '{from}' to '{to}'"));
		}
		catch(Exception e)
		{
			errors.Add(Verification.Failure(e));
		}
	}

	

}
