using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ENewDefinitionType
{
    part,
    bullet,
    attachment,
    grenade,
    gun,
	vehicle,
	workbench,
	tool,
	armour,
	playerClass,
	team, 
	rewardBox,
	loadout,
	animation,
	magazine,
	npc,
}

public enum EDefinitionType
{
    part,
    bullet,
    attachment,
    grenade,
    gun,
    aa,
    vehicle,
    plane,
    mechaItem,
    mecha,
    tool,
    armour,
    armourBox,
    box,
    playerClass,
    team,
    itemHolder,
    rewardBox,
    loadout,

    NUM_TYPES
}

public static class DefinitionTypes
{
	public const int NUM_TYPES = (int)EDefinitionType.NUM_TYPES;

	public static EDefinitionType GetFromObject(InfoType type)
	{
		if(type is PartType) return EDefinitionType.part;
		if(type is MechaItemType) return EDefinitionType.mechaItem;
		if(type is BulletType) return EDefinitionType.bullet;
		if(type is AttachmentType) return EDefinitionType.attachment;
		if(type is GrenadeType) return EDefinitionType.grenade;
		if(type is GunType) return EDefinitionType.gun;
		if(type is VehicleType) return EDefinitionType.vehicle;
		if(type is MechaType) return EDefinitionType.mecha;
		if(type is PlaneType) return EDefinitionType.plane;
		if(type is AAGunType) return EDefinitionType.aa;
		if(type is ToolType) return EDefinitionType.tool;
		if(type is ArmourType) return EDefinitionType.armour;
		if(type is GunBoxType) return EDefinitionType.box;
		if(type is ArmourBoxType) return EDefinitionType.armourBox;
		if(type is PlayerClass) return EDefinitionType.playerClass;
		if(type is Team) return EDefinitionType.team;
		if(type is RewardBox) return EDefinitionType.rewardBox;
		if(type is LoadoutPool) return EDefinitionType.loadout;

		return EDefinitionType.part;
	}

	public static ENewDefinitionType GetFromObject(Definition def)
	{
		if(def is PartDefinition) return ENewDefinitionType.part;
		if(def is BulletDefinition) return ENewDefinitionType.bullet;
		if(def is AttachmentDefinition) return ENewDefinitionType.attachment;
		if(def is GrenadeDefinition) return ENewDefinitionType.grenade;
		if(def is GunDefinition) return ENewDefinitionType.gun;
		if(def is VehicleDefinition) return ENewDefinitionType.vehicle;
		if(def is ToolDefinition) return ENewDefinitionType.tool;
		if(def is ArmourDefinition) return ENewDefinitionType.armour;
		if(def is WorkbenchDefinition) return ENewDefinitionType.workbench;
		if(def is ClassDefinition) return ENewDefinitionType.playerClass;
		if(def is TeamDefinition) return ENewDefinitionType.team;
		if(def is RewardBoxDefinition) return ENewDefinitionType.rewardBox;
		if(def is LoadoutPoolDefinition) return ENewDefinitionType.loadout;
		if(def is AnimationDefinition) return ENewDefinitionType.animation;
		if(def is MagazineDefinition) return ENewDefinitionType.magazine;
		if(def is NpcDefinition) return ENewDefinitionType.npc;
		
		return ENewDefinitionType.part;
	}

	public static Definition CreateInstance(this EDefinitionType defType)
	{
		switch(defType)
		{
			case EDefinitionType.part: return ScriptableObject.CreateInstance<PartDefinition>();
			case EDefinitionType.bullet: return ScriptableObject.CreateInstance<BulletDefinition>();
			case EDefinitionType.attachment: return ScriptableObject.CreateInstance<AttachmentDefinition>();
			case EDefinitionType.grenade: return ScriptableObject.CreateInstance<GrenadeDefinition>();
			case EDefinitionType.gun: return ScriptableObject.CreateInstance<GunDefinition>();
			case EDefinitionType.aa: return ScriptableObject.CreateInstance<VehicleDefinition>();
			case EDefinitionType.vehicle: return ScriptableObject.CreateInstance<VehicleDefinition>();
			case EDefinitionType.plane: return ScriptableObject.CreateInstance<VehicleDefinition>();
			case EDefinitionType.mechaItem: return ScriptableObject.CreateInstance<PartDefinition>();
			case EDefinitionType.mecha: return ScriptableObject.CreateInstance<VehicleDefinition>();
			case EDefinitionType.tool: return ScriptableObject.CreateInstance<ToolDefinition>();
			case EDefinitionType.armour: return ScriptableObject.CreateInstance<ArmourDefinition>();
			case EDefinitionType.armourBox: return ScriptableObject.CreateInstance<WorkbenchDefinition>();
			case EDefinitionType.box: return ScriptableObject.CreateInstance<WorkbenchDefinition>();
			case EDefinitionType.playerClass: return ScriptableObject.CreateInstance<ClassDefinition>();
			case EDefinitionType.team: return ScriptableObject.CreateInstance<TeamDefinition>();
			case EDefinitionType.itemHolder: return ScriptableObject.CreateInstance<WorkbenchDefinition>();
			case EDefinitionType.rewardBox: return ScriptableObject.CreateInstance<RewardBoxDefinition>();
			case EDefinitionType.loadout: return ScriptableObject.CreateInstance<LoadoutPoolDefinition>();


			default: return null;
		}
	}

	public static System.Type GetScriptableObjectType(this EDefinitionType defType)
	{
		switch(defType)
		{
			case EDefinitionType.part: 			return typeof(PartDefinition);
			case EDefinitionType.bullet: 		return typeof(BulletDefinition);
			case EDefinitionType.attachment: 	return typeof(AttachmentDefinition);
			case EDefinitionType.grenade: 		return typeof(GrenadeDefinition);
			case EDefinitionType.gun: 			return typeof(GunDefinition);
			case EDefinitionType.aa: 			return typeof(VehicleDefinition);
			case EDefinitionType.vehicle: 		return typeof(VehicleDefinition);
			case EDefinitionType.plane: 		return typeof(VehicleDefinition);
			case EDefinitionType.mechaItem: 	return typeof(PartDefinition);
			case EDefinitionType.mecha: 		return typeof(VehicleDefinition);
			case EDefinitionType.tool: 			return typeof(ToolDefinition);
			case EDefinitionType.armour: 		return typeof(ArmourDefinition);
			case EDefinitionType.armourBox: 	return typeof(WorkbenchDefinition);
			case EDefinitionType.box: 			return typeof(WorkbenchDefinition);
			case EDefinitionType.playerClass: 	return typeof(ClassDefinition);
			case EDefinitionType.team:			return typeof(TeamDefinition);
			case EDefinitionType.itemHolder: 	return typeof(WorkbenchDefinition);
			case EDefinitionType.rewardBox:		return typeof(RewardBoxDefinition); 
			case EDefinitionType.loadout: 		return typeof(LoadoutPoolDefinition);
			default: return typeof(Definition);
		}
	}

	public static System.Type GetTxtImportType(this EDefinitionType defType)
	{
		switch(defType)
		{
			case EDefinitionType.part: 			return typeof(PartType);
			case EDefinitionType.bullet: 		return typeof(BulletType);
			case EDefinitionType.attachment: 	return typeof(AttachmentType);
			case EDefinitionType.grenade: 		return typeof(GrenadeType);
			case EDefinitionType.gun: 			return typeof(GunType);
			case EDefinitionType.aa: 			return typeof(AAGunType);
			case EDefinitionType.vehicle: 		return typeof(VehicleType);
			case EDefinitionType.plane: 		return typeof(PlaneType);
			case EDefinitionType.mechaItem: 	return typeof(MechaItemType);
			case EDefinitionType.mecha: 		return typeof(MechaType);
			case EDefinitionType.tool: 			return typeof(ToolType);
			case EDefinitionType.armour: 		return typeof(ArmourType);
			case EDefinitionType.armourBox: 	return typeof(ArmourBoxType);
			case EDefinitionType.box: 			return typeof(GunBoxType);
			case EDefinitionType.playerClass: 	return typeof(PlayerClass);
			case EDefinitionType.team:			return typeof(Team);
			case EDefinitionType.itemHolder: 	return typeof(ItemHolderType);
			case EDefinitionType.rewardBox:		return typeof(RewardBox); 
			case EDefinitionType.loadout: 		return typeof(LoadoutPool);
			default: return typeof(InfoType);
		}
	}

	public static string Folder(this EDefinitionType defType)
	{
		switch(defType)
		{
			case EDefinitionType.part: return "parts";
			case EDefinitionType.bullet: return "bullets";
			case EDefinitionType.attachment: return "attachments";
			case EDefinitionType.grenade: return "grenades";
			case EDefinitionType.gun: return "guns";
			case EDefinitionType.aa: return "aa";
			case EDefinitionType.vehicle: return "vehicles";
			case EDefinitionType.plane: return "planes";
			case EDefinitionType.mechaItem: return "mechaItems";
			case EDefinitionType.mecha: return "mechas";
			case EDefinitionType.tool: return "tools";
			case EDefinitionType.armour: return "armour";
			case EDefinitionType.armourBox: return "armourBoxes";
			case EDefinitionType.box: return "boxes";
			case EDefinitionType.playerClass: return "playerClasses";
			case EDefinitionType.team: return "teams";
			case EDefinitionType.itemHolder: return "itemHolders";
			case EDefinitionType.rewardBox: return "rewardBoxes";
			case EDefinitionType.loadout: return "loadouts";
			default: return "";
		}
	}

	public static string OutputFolder(this EDefinitionType defType)
	{
		switch(defType)
		{
			case EDefinitionType.part: return "parts";
			case EDefinitionType.bullet: return "bullets";
			case EDefinitionType.attachment: return "attachments";
			case EDefinitionType.grenade: return "grenades";
			case EDefinitionType.gun: return "guns";
			case EDefinitionType.aa: return "vehicles";
			case EDefinitionType.vehicle: return "vehicles";
			case EDefinitionType.plane: return "vehicles";
			case EDefinitionType.mechaItem: return "attachments";
			case EDefinitionType.mecha: return "vehicles";
			case EDefinitionType.tool: return "tools";
			case EDefinitionType.armour: return "armour";
			case EDefinitionType.armourBox: return "workbenches";
			case EDefinitionType.box: return "workbenches";
			case EDefinitionType.playerClass: return "playerClasses";
			case EDefinitionType.team: return "teams";
			case EDefinitionType.itemHolder: return "workbenches";
			case EDefinitionType.rewardBox: return "rewardBoxes";
			case EDefinitionType.loadout: return "loadouts";
			default: return "";
		}
	}

	public static string OutputFolder(this ENewDefinitionType defType)
	{
		switch(defType)
		{
			case ENewDefinitionType.part: return "parts";
			case ENewDefinitionType.bullet: return "bullets";
			case ENewDefinitionType.attachment: return "attachments";
			case ENewDefinitionType.grenade: return "grenades";
			case ENewDefinitionType.gun: return "guns";
			case ENewDefinitionType.vehicle: return "vehicles";
			case ENewDefinitionType.tool: return "tools";
			case ENewDefinitionType.armour: return "armour";
			case ENewDefinitionType.playerClass: return "playerClasses";
			case ENewDefinitionType.team: return "teams";
			case ENewDefinitionType.rewardBox: return "rewardBoxes";
			case ENewDefinitionType.loadout: return "loadouts";
			case ENewDefinitionType.workbench: return "workbenches";
			case ENewDefinitionType.animation: return "animations";
			case ENewDefinitionType.magazine: return "magazines";
			case ENewDefinitionType.npc: return "npcs";
			default: return "";
		}
	}
}

