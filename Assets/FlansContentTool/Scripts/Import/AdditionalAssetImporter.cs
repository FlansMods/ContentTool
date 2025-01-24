using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AdditionalImportOperation
{
	protected string Input;
	protected string Output;
	public string DstPackName(string srcPackName) { return Utils.ToLowerWithUnderscores(srcPackName); }
	public string InputPath(string srcPackName) { return $"{AdditionalAssetImporter.IMPORT_ROOT}/{srcPackName}/{Input}"; }
	public string OutputPath(string srcPackName) { return $"{AdditionalAssetImporter.ASSET_ROOT}/{DstPackName(srcPackName)}/{Output}"; }

	public AdditionalImportOperation(string input, string output)
	{
		Input = input;
		Output = output;
	}

	public abstract void Do(string srcPackName, IFileAccess fileAccess, IVerificationLogger logger = null);
}
public class CopyImportOperation : AdditionalImportOperation
{
	public CopyImportOperation(string input, string output) : base(input, output) { }
	public override void Do(string srcPackName, IFileAccess fileAccess, IVerificationLogger logger = null)
	{
		fileAccess.Copy(InputPath(srcPackName), OutputPath(srcPackName), logger);
	}
}
public class IconModelOperation : AdditionalImportOperation
{
	protected string IconName;
	public IconModelOperation(string input, string output, string iconName) : base(input, output) 
	{
		IconName = iconName;
	}
	public override void Do(string srcPackName, IFileAccess fileAccess, IVerificationLogger logger = null)
	{
		if (fileAccess.TryCreatePrefab(() =>
			{
				GameObject go = new GameObject(IconName);
				VanillaIconRootNode iconNode = go.AddComponent<VanillaIconRootNode>();
				iconNode.AddDefaultTransforms();
				iconNode.Icons.Add(new NamedTexture("default", new ResourceLocation(DstPackName(srcPackName), IconName)));
				return go;
			},
			OutputPath(srcPackName),
			true,
			logger))
		{
			logger?.Success($"Created ItemModel at '{OutputPath(srcPackName)}'");
		}
	}
}
public class CubeModelOperation : AdditionalImportOperation
{
	protected string BlockName;
	protected string TopTextureName;
	protected string SideTextureName;
	protected string BottomTextureName;

	public CubeModelOperation(string input, string output, string blockName, string top, string side, string bottom) : base(input, output)
	{
		BlockName = blockName;
		TopTextureName = top;
		SideTextureName = side;
		BottomTextureName = bottom;
	}

	public override void Do(string srcPackName, IFileAccess fileAccess, IVerificationLogger logger = null)
	{
		if (fileAccess.TryCreatePrefab(() =>
			{
				ResourceLocation top = new ResourceLocation(DstPackName(srcPackName), TopTextureName);
				ResourceLocation side = new ResourceLocation(DstPackName(srcPackName), SideTextureName);
				ResourceLocation bottom = new ResourceLocation(DstPackName(srcPackName), BottomTextureName);


				GameObject go = new GameObject(BlockName);
				VanillaCubeRootNode cubeNode = go.AddComponent<VanillaCubeRootNode>();
				cubeNode.AddDefaultTransforms();
				cubeNode.Textures.Add(new NamedTexture("up", top));
				cubeNode.Textures.Add(new NamedTexture("down", bottom));
				cubeNode.Textures.Add(new NamedTexture("north", side));
				cubeNode.Textures.Add(new NamedTexture("east", side));
				cubeNode.Textures.Add(new NamedTexture("south", side));
				cubeNode.Textures.Add(new NamedTexture("west", side));
				cubeNode.Textures.Add(new NamedTexture("particle", side));
				return go;
			},
			OutputPath(srcPackName),
			true,
			logger))
		{
			logger?.Success($"Created Cube block model at '{OutputPath(srcPackName)}'");
		}
	}
}
public class TurboImportOperation : AdditionalImportOperation
{
	protected List<string> SkinNames;
	protected List<string> IconNames;
	public TurboImportOperation(string input, string output, List<string> skinNames, List<string> iconNames) : base(input, output)
	{
		SkinNames = skinNames;
		IconNames = iconNames;
	}

	public string ModelInputPath() { return $"{AdditionalAssetImporter.MODEL_IMPORT_ROOT}/{Input}"; }

	public override void Do(string srcPackName, IFileAccess fileAccess, IVerificationLogger logger = null)
	{
		List<NamedTexture> skins = new List<NamedTexture>();
		List<NamedTexture> icons = new List<NamedTexture>();
		for (int i = 0; i < SkinNames.Count; i++)
		{
			string skinName = Minecraft.SanitiseID(SkinNames[i]);
			skins.Add(new NamedTexture(i == 0 ? "default" : skinName,
									   new ResourceLocation(DstPackName(srcPackName), skinName)));
		}
		for (int i = 0; i < IconNames.Count; i++)
		{
			string iconName = Minecraft.SanitiseID(IconNames[i]);
			icons.Add(new NamedTexture(i == 0 ? "default" : iconName, new ResourceLocation(DstPackName(srcPackName), iconName)));
		}

		AdditionalAssetImporter.ImportTurboRootNode(
			ModelInputPath(),
			OutputPath(srcPackName),
			skins,
			icons,
			fileAccess,
			logger);
	}
}
public class AdditionalDefinitionOperation<TDefType> : AdditionalImportOperation where TDefType : Definition
{
	private Action<TDefType> InitFunc;

	public AdditionalDefinitionOperation(string input, string output, Action<TDefType> initFunc) : base(input, output)
	{
		InitFunc = initFunc;
	}

	public override void Do(string srcPackName, IFileAccess fileAccess, IVerificationLogger logger = null)
	{
		if(fileAccess.TryCreateScriptableObject(
			OutputPath(srcPackName),
			InitFunc,
			logger))
		{
			logger?.Success($"Created extra Definition ({typeof(TDefType)}) at '{OutputPath(srcPackName)}'");
		}
	}
}

public static class AdditionalAssetImporter
{
	public static readonly string IMPORT_ROOT = ContentManager.IMPORT_ROOT;
	public static readonly string MODEL_IMPORT_ROOT = ContentManager.MODEL_IMPORT_ROOT;
	public static readonly string ASSET_ROOT = ContentManager.ASSET_ROOT;


	public static void ImportTurboRootNode(string from, string to, List<NamedTexture> skins, List<NamedTexture> icons, IFileAccess fileAccess, IVerificationLogger logger = null)
	{
		if (!fileAccess.CanExport(to))
			return;

		// Step 1: Import the Java as a RootNode
		VerificationList importVerification = new VerificationList($"Import Model '{from}'");
		TurboRootNode rootNode = JavaModelImporter.ImportJavaModel(from, importVerification);
		VerifyType result = Verification.GetWorstState(importVerification);

		if (result == VerifyType.Fail)
		{
			logger?.Failure($"Failed to import {from} as a RootNode model");
			logger?.AsList().AddRange(importVerification.AsList());
			return;
		}

		// Step 2: Add additional data from outside the .java
		rootNode.Icons.AddRange(icons);
		rootNode.Textures.AddRange(skins);
		rootNode.AddDefaultTransforms();
		string nodeName = to.Substring(to.LastIndexOfAny(Utils.SLASHES) + 1);
		nodeName = nodeName.Substring(0, nodeName.LastIndexOf('.'));
		rootNode.name = nodeName;
		rootNode.SelectTexture("default");

		// Step 3: Save that as a prefab
		fileAccess.SaveAsPrefab(rootNode.gameObject, to, true, logger);

		if (result == VerifyType.Neutral)
			logger?.Neutral($"Imported {from} as RootNode model with some warnings");
		else
			logger?.Success($"Imported {from} as RootNode successfully");
	}

	//public static List<string> GetOutputPaths(string srcPackName, InfoType inputType)
	//{
	//	string dstPackName = Utils.ToLowerWithUnderscores(srcPackName);
	//	string dstShortName = Utils.ToLowerWithUnderscores(inputType.shortName);
	//
	//	List<string> outputs = new List<string>();
	//	switch(DefinitionTypes.GetFromObject(inputType))
	//	{
	//		case EDefinitionType.gun:
	//		case EDefinitionType.attachment:
	//			if (inputType is PaintableType paintable)
	//			{
	//				foreach (Paintjob paintjob in paintable.paintjobs)
	//				{
	//					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{Utils.ToLowerWithUnderscores(paintjob.iconName)}.png");
	//					outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png");
	//				}
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png");
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
	//			}
	//			break;
	//		case EDefinitionType.aa:
	//		case EDefinitionType.vehicle:
	//		case EDefinitionType.plane:
	//		case EDefinitionType.mecha:
	//		case EDefinitionType.bullet:
	//		case EDefinitionType.grenade:
	//			// Single entity model, skinned
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png");
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/entity/{dstShortName}.prefab");
	//			break;
	//		case EDefinitionType.part:
	//		case EDefinitionType.tool:
	//		case EDefinitionType.rewardBox:
	//		case EDefinitionType.mechaItem:
	//			// Single item model with icon
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png");
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
	//			break;
	//		case EDefinitionType.armour:
	//			// Armour skin/model
	//			// TODO:
	//			break;
	//		case EDefinitionType.armourBox:
	//		case EDefinitionType.box:
	//		case EDefinitionType.itemHolder:
	//			// Block model
	//			if(inputType is BoxType box)
	//			{
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.topTexturePath)}.png");
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.sideTexturePath)}.png");
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.bottomTexturePath)}.png");
	//			}
	//			else
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(inputType.texture)}.png");
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/block/{dstShortName}.prefab");
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab");
	//			outputs.Add($"{ASSET_ROOT}/{dstPackName}/blockstates/{dstShortName}.json");
	//			break;
	//		case EDefinitionType.team:
	//		case EDefinitionType.loadout:
	//		case EDefinitionType.playerClass:
	//			// No models or textures for these
	//			break;
	//	}
	//	return outputs;
	//}
	//public static void ImportAssets(
	//	string srcPackName,
	//	InfoType inputType,
	//	IFileAccess fileAccess,
	//	IVerificationLogger logger = null)
	//{
	//	string dstPackName = Utils.ToLowerWithUnderscores(srcPackName);
	//	string dstShortName = Utils.ToLowerWithUnderscores(inputType.shortName);
	//
	//	// --------------------------------------------------------------
	//	// Texture Imports
	//	bool exportIcon = true;
	//	bool exportSkin = true;
	//	switch (DefinitionTypes.GetFromObject(inputType))
	//	{
	//		case EDefinitionType.gun:
	//		case EDefinitionType.vehicle:
	//		case EDefinitionType.mecha:
	//		case EDefinitionType.plane:
	//		case EDefinitionType.grenade:
	//		case EDefinitionType.attachment:
	//			if(inputType is PaintableType paintable)
	//			{
	//				foreach(Paintjob paintjob in paintable.paintjobs)
	//				{
	//					fileAccess.Copy($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/textures/items/{paintjob.iconName}.png",
	//									$"{ASSET_ROOT}/{dstPackName}/textures/item/{Utils.ToLowerWithUnderscores(paintjob.iconName)}.png",
	//									logger);
	//					fileAccess.Copy($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/skins/{paintjob.textureName}.png",
	//									$"{ASSET_ROOT}/{dstPackName}/textures/skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png",
	//									logger);
	//				}
	//			}				
	//			break;
	//
	//		case EDefinitionType.bullet:
	//			exportSkin = false;
	//			break;
	//
	//		case EDefinitionType.armourBox:
	//		case EDefinitionType.box:
	//			if (inputType is BoxType boxType)
	//			{
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.topTexturePath)}.png");
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.sideTexturePath)}.png");
	//				outputs.Add($"{ASSET_ROOT}/{dstPackName}/textures/block/{Utils.ToLowerWithUnderscores(box.bottomTexturePath)}.png");
	//			}
	//			break;
	//		case EDefinitionType.itemHolder:
	//			
	//
	//			// Block exports
	//
	//			break;
	//		case EDefinitionType.armour:
	//			// Armour texture
	//
	//			break;
	//		case EDefinitionType.aa:
	//		case EDefinitionType.part:
	//		case EDefinitionType.mechaItem:
	//		case EDefinitionType.tool:
	//		case EDefinitionType.rewardBox:
	//			// Nothing extra to add / remove
	//			break;
	//		case EDefinitionType.team:
	//		case EDefinitionType.playerClass:
	//		case EDefinitionType.loadout:
	//			// The "non-item" types
	//			exportIcon = false;
	//			exportSkin = false;
	//			break;
	//	}
	//
	//	if(exportIcon)
	//	{
	//		fileAccess.Copy($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/textures/items/{inputType.iconPath}.png",
	//						$"{ASSET_ROOT}/{dstPackName}/textures/item/{dstShortName}.png",
	//						logger);
	//	}
	//	if (exportSkin)
	//	{
	//		fileAccess.Copy($"{IMPORT_ROOT}/{srcPackName}/assets/flansmod/skins/{inputType.texture}.png",
	//						$"{ASSET_ROOT}/{dstPackName}/textures/skins/{dstShortName}.png",
	//						logger);
	//	}
	//	// --------------------------------------------------------------
	//	// Model Imports
	//	// --------------------------------------------------------------
	//	bool exportBasicItemModel = true;
	//	switch (DefinitionTypes.GetFromObject(inputType))
	//	{
	//		case EDefinitionType.gun:
	//			// Complicated stuff
	//			if (inputType is PaintableType paintable)
	//			{
	//				List<NamedTexture> icons = new List<NamedTexture>();
	//				List<NamedTexture> skins = new List<NamedTexture>();
	//
	//				// Default skin + icon			
	//				ResourceLocation defaultSkinLocation = new ResourceLocation(dstPackName, dstShortName);
	//				ResourceLocation defaultIconLocation = new ResourceLocation(dstPackName, dstShortName);
	//				skins.Add(new NamedTexture("default",defaultSkinLocation, "textures/skins"));
	//				icons.Add(new NamedTexture("default", defaultIconLocation, "textures/item"));
	//
	//				// And then paintjobs
	//				foreach (Paintjob paintjob in paintable.paintjobs)
	//				{
	//					string key = Utils.ToLowerWithUnderscores(paintjob.textureName);
	//					ResourceLocation skinLoc = new ResourceLocation(dstPackName, Utils.ToLowerWithUnderscores(paintjob.textureName));
	//					ResourceLocation iconLoc = new ResourceLocation(dstPackName, Utils.ToLowerWithUnderscores(paintjob.iconName));
	//					skins.Add(new NamedTexture(key, skinLoc, "textures/skins"));
	//					icons.Add(new NamedTexture(key, iconLoc, "textures/item"));
	//				}
	//
	//				// Create the 3D model
	//				if (inputType.modelString != null && inputType.modelString.Length > 0)
	//				{
	//					ImportTurboRootNode($"{MODEL_IMPORT_ROOT}/{inputType.modelFolder}/Model{inputType.modelString}.java",
	//										$"{ASSET_ROOT}/{dstPackName}/models/item/{dstShortName}.prefab",
	//										skins,
	//										icons,
	//										fileAccess,
	//										logger);
	//					exportBasicItemModel = false;
	//				}
	//
	//			}
	//			break;
	//		case EDefinitionType.vehicle:
	//		case EDefinitionType.mecha:
	//		case EDefinitionType.plane:
	//			// TODO: Try and import the rig, not guaranteed to work yet
	//			break;
	//		case EDefinitionType.bullet:
	//		case EDefinitionType.grenade:
	//		case EDefinitionType.attachment:
	//			break;
	//
	//		case EDefinitionType.armourBox:
	//		case EDefinitionType.box:
	//		case EDefinitionType.itemHolder:
	//			if (inputType is BoxType boxType)
	//			{
	//				// Block exports
	//				CreateBlockModel_Internal($"{ASSET_ROOT}/{dstPackName}/models/block/{dstShortName}.prefab",
	//					new ResourceLocation(boxType.topTexturePath),
	//					new ResourceLocation(boxType.sideTexturePath),
	//					new ResourceLocation(boxType.bottomTexturePath), fileAccess, logger);
	//			}
	//			break;
	//		case EDefinitionType.armour:
	//			// Armour texture
	//
	//			break;
	//		case EDefinitionType.aa:
	//		case EDefinitionType.part:
	//		case EDefinitionType.mechaItem:
	//		case EDefinitionType.tool:
	//		case EDefinitionType.rewardBox:
	//			// Nothing extra to add / remove
	//			break;
	//		case EDefinitionType.team:
	//		case EDefinitionType.playerClass:
	//		case EDefinitionType.loadout:
	//			// The "non-item" types
	//			exportBasicItemModel = false;
	//			break;
	//	}
	//
	//	if(exportBasicItemModel)
	//	{
	//		
	//	}
	//}
}
