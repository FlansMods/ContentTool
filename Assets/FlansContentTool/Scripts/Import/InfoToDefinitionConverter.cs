using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Windows;

public static class InfoToDefinitionConverter
{
    public static void Convert(EDefinitionType type, InfoType input, Definition output)
	{
		switch(type)
		{
			case EDefinitionType.part: 			PartConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.bullet: 		MagazineConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.attachment:	AttachmentConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.grenade:		GrenadeConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.gun: 			GunConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.aa: 			AAGunConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.vehicle: 		VehicleConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.plane: 		PlaneConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.mechaItem: 	MechaItemConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.mecha: 		MechaConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.tool: 			ToolConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.armour: 		ArmourConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.armourBox: 	ArmourBoxConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.box: 			GunBoxConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.playerClass:   LoadoutConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.team: 			TeamConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.itemHolder: 	ItemHolderConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.rewardBox: 	RewardBoxConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.loadout: 		LoadoutPoolConverter.inst.CastConvert(input, output); break;
		}
	}

	public static void GetAdditionalImports(EDefinitionType type, InfoType input, List<AdditionalImportOperation> operations)
	{
		switch (type)
		{
			case EDefinitionType.part:			PartConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.bullet:		MagazineConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.attachment:	AttachmentConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.grenade:		GrenadeConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.gun:			GunConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.aa:			AAGunConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.vehicle:		VehicleConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.plane:			PlaneConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.mechaItem:		MechaItemConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.mecha:			MechaConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.tool:			ToolConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.armour:		ArmourConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.armourBox:		ArmourBoxConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.box:			GunBoxConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.playerClass:   LoadoutConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.team:			TeamConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.itemHolder:	ItemHolderConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.rewardBox:		RewardBoxConverter.inst.CastGetAdditionalOperations(input, operations); break;
			case EDefinitionType.loadout:		LoadoutPoolConverter.inst.CastGetAdditionalOperations(input, operations); break;
		}
	}
}




public abstract class Converter<TInfo, TDefinition> 
	where TInfo : InfoType 
	where TDefinition : Definition
{
	public void CastConvert(InfoType input, Definition output)
	{
		try
		{
			DoConversion((TInfo)input, (TDefinition)output);
		}
		catch(Exception)
		{
			Debug.LogError($"Type {output} was not of the expected type {typeof(TDefinition)}");
		}
	}
	public void Convert(TInfo input, TDefinition output)
	{
		DoConversion(input, output);
	}
	public ItemStackDefinition ImportStack(string txt)
	{
		string amount = "1";
		string itemName = txt;
		string damage = "-1";
		if(itemName.Contains("x"))
		{
			amount = itemName.Substring(0, itemName.IndexOf("x"));
			itemName = itemName.Substring(itemName.IndexOf("x") + 1);
			damage = "-1";
		}
		if(itemName.Contains("."))
		{
			damage = itemName.Substring(itemName.IndexOf(".") + 1);
			itemName = itemName.Substring(0, itemName.IndexOf("."));
		}
		return new ItemStackDefinition()
		{
			item = itemName,
			count = int.Parse(amount),
			damage = int.Parse(damage),
		};
	}
	public ItemStackDefinition ImportStackFromTypeName(string typeName)
	{
		return new ItemStackDefinition()
		{
			item = Utils.ToLowerWithUnderscores(typeName),
		};
	}
	public ModifierDefinition BaseStat(string stat, float value)
	{
		return new ModifierDefinition
		{
			stat = stat,
			accumulators = new StatAccumulatorDefinition[] {
				new StatAccumulatorDefinition {
					operation = EAccumulationOperation.BaseAdd,
					value = value,
				}
			}
		};
	}
	public ModifierDefinition MultiplierStat(string stat, float value, string groupPath = "")
	{
		return new ModifierDefinition
		{
			stat = stat,
			matchGroupPaths = groupPath.Length > 0 ? new string[] { groupPath } : new string[0],
			accumulators = new StatAccumulatorDefinition[] {
				new StatAccumulatorDefinition {
					operation = EAccumulationOperation.StackablePercentage,
					value = 100.0f * (value - 1.0f),
				}
			}
		};
	}
	public ModifierDefinition StringStat(string stat, string value)
	{
		return new ModifierDefinition
		{
			stat = stat,
			setValue = value,
		};
	}

	protected abstract void DoConversion(TInfo input, TDefinition output);

	public void CastGetAdditionalOperations(InfoType input, List<AdditionalImportOperation> operations)
	{
		try
		{
			GetAdditionalOperations((TInfo)input, operations);
		}
		catch (Exception)
		{
			Debug.LogError($"Type {input} was not of the expected type {typeof(TInfo)}");
		}
	}
	public abstract void GetAdditionalOperations(TInfo input, List<AdditionalImportOperation> operations);

	protected void AddDefaultIconOperations(TInfo inf, List<AdditionalImportOperation> operations)
	{
		operations.Add(new CopyImportOperation($"/assets/flansmod/textures/items/{inf.iconPath}.png",
											   $"/textures/item/{inf.ConvertedName}.png"));
		operations.Add(new IconModelOperation($"",
											  $"/models/item/{inf.ConvertedName}.prefab",
											  inf.ConvertedName));
	}
	protected void AddDefaultSkinOperation(TInfo inf, List<AdditionalImportOperation> operations)
	{
		operations.Add(new CopyImportOperation($"/assets/flansmod/skins/{inf.texture}.png",
											   $"/textures/skins/{inf.ConvertedName}.png"));

	}
}

public class PaintableConverter // : Converter<PaintableType, PaintableDefinition>
{
	public static PaintableConverter inst = new PaintableConverter();
	public void DoConversion(PaintableType inf, PaintableDefinition def)
	{
		def.paintjobs = new PaintjobDefinition[inf.paintjobs.Count];
		for (int i = 0; i < inf.paintjobs.Count; i++)
		{
			def.paintjobs[i] = new PaintjobDefinition()
			{
				textureName = Utils.ToLowerWithUnderscores(inf.paintjobs[i].iconName),
			};
		}
	}
	public void GetAdditionalOperations(PaintableType inf, List<AdditionalImportOperation> operations)
	{
		foreach (Paintjob paintjob in inf.paintjobs)
		{
			operations.Add(new CopyImportOperation(
				$"assets/flansmod/textures/items/{paintjob.iconName}.png",
				$"textures/item/{Utils.ToLowerWithUnderscores(paintjob.iconName)}.png"));
			operations.Add(new CopyImportOperation(
				$"assets/flansmod/skins/{paintjob.textureName}.png",
				$"textures/skins/{Utils.ToLowerWithUnderscores(paintjob.textureName)}.png"));
		}
	}
}

public class GunConverter : Converter<GunType, GunDefinition>
{
	public static GunConverter inst = new GunConverter();

	// Hacky but meh
	public static EAnimationType OldAnimType = EAnimationType.NONE;
	public static float UntiltTime = 0.0f;
	public static float TiltTime = 0.0f;
	public static float UnloadTime = 0.0f;
	public static float LoadTime = 0.0f;
	// ------------------------------------------------------------

	protected override void DoConversion(GunType inf, GunDefinition def)
	{
		PaintableConverter.inst.DoConversion(inf, def.paints);

		def.itemSettings.maxStackSize = 1;
		def.itemSettings.tags = GetTags(inf);
		def.magazines = new MagazineSlotSettingsDefinition[]
		{
			new MagazineSlotSettingsDefinition()
			{
				key = "primary",
				matchByNames = GetMags(inf),
			}
		};

		def.reloads = new ReloadDefinition[] 
		{
			new ReloadDefinition()
			{
				manualReloadAllowed = inf.canForceReload,
				startActionKey = "primary_reload_start",
				ejectActionKey = "primary_reload_eject",
				loadOneActionKey = "primary_reload_load_one",
				endActionKey = "primary_reload_end",
			}
		};

		List<ActionGroupDefinition> actionGroups = new List<ActionGroupDefinition>();
		actionGroups.Add(CreatePrimaryActions(inf));
		actionGroups.Add(CreateSecondaryActions(inf));
		actionGroups.Add(CreateLookAtActions(inf));
		actionGroups.Add(CreateStartReloadActions(inf));
		actionGroups.Add(CreateEjectReloadActions(inf));
		actionGroups.Add(CreateLoadOneReloadActions(inf));
		actionGroups.Add(CreateEndReloadActions(inf));
		def.actionGroups = actionGroups.ToArray();

		def.inputHandlers = new HandlerDefinition[]
		{
			// Primary - Fire if we can, or start a reload
			new HandlerDefinition() {
				inputType = EPlayerInput.Fire1,
				nodes = new HandlerNodeDefinition[] {
					new HandlerNodeDefinition() {
						actionGroupToTrigger = "primary_fire",
					},
					new HandlerNodeDefinition() {
						actionGroupToTrigger = "reload_primary_start",
					},
				},
			},
			// Secondary - If we have a sight attachment, try that. Then check our grip for Fire2. Then apply our default ads action
			new HandlerDefinition() {
				inputType = EPlayerInput.Fire2,
				nodes = new HandlerNodeDefinition[] {
					new HandlerNodeDefinition() {
						deferToAttachment = true,
						attachmentType = EAttachmentType.Sights,
						attachmentIndex = 0,
					},
					new HandlerNodeDefinition() {
						deferToAttachment = true,
						attachmentType = EAttachmentType.Grip,
						attachmentIndex = 0,
					},
					new HandlerNodeDefinition() {
						actionGroupToTrigger = "ads",
						canTriggerWhileReloading = true,
					},
				},
			},
			// Look at, simple enough
			new HandlerDefinition() {
				inputType = EPlayerInput.SpecialKey1,
				nodes = new HandlerNodeDefinition[] {
					new HandlerNodeDefinition() {
						actionGroupToTrigger = "look_at",
					},
				},
			},
			// Manual reload. We might have a barrel launcher, so check there first
			new HandlerDefinition() {
				inputType = EPlayerInput.Reload1,
				nodes = new HandlerNodeDefinition[] {
					new HandlerNodeDefinition() {
						deferToAttachment = true,
						attachmentType = EAttachmentType.Grip,
						attachmentIndex = 0,
					},
					new HandlerNodeDefinition() {
						actionGroupToTrigger = "reload_primary_start",
					},
				},
			},
			// Gadget toggle - doesn't do anything by default, but good to have it assigned so attachments can toggle modes
			new HandlerDefinition() {
				inputType = EPlayerInput.SpecialKey2,
				nodes = new HandlerNodeDefinition[] {
					new HandlerNodeDefinition() {
						deferToAttachment = true,
						attachmentType = EAttachmentType.Barrel,
						attachmentIndex = 0,
						andContinueEvaluating = true,
					},
					new HandlerNodeDefinition() {
						deferToAttachment = true,
						attachmentType = EAttachmentType.Sights,
						attachmentIndex = 0,
						andContinueEvaluating = true,
					},
					new HandlerNodeDefinition() {
						deferToAttachment = true,
						attachmentType = EAttachmentType.Stock,
						attachmentIndex = 0,
						andContinueEvaluating = true,
					},
					new HandlerNodeDefinition() {
						deferToAttachment = true,
						attachmentType = EAttachmentType.Grip,
						attachmentIndex = 0,
						andContinueEvaluating = true,
					},
				}
			}
		};

		//def.idleSound = inf.idleSound;
		//def.idleSoundLength = inf.idleSoundLength;
		//def.showAttachments = inf.showAttachments;
		//def.showDamage = inf.showDamage;
		//def.showRecoil = inf.showRecoil;
		//def.showSpread = inf.showSpread;
		//def.showReloadTime = inf.showReloadTime;

		def.barrelAttachments = new AttachmentSettingsDefinition()
		{
			numAttachmentSlots = inf.allowBarrelAttachments ? 1 : 0,
			matchNames = inf.allowedAttachments.ToArray(),
		};
		def.gripAttachments = new AttachmentSettingsDefinition()
		{
			numAttachmentSlots = inf.allowGripAttachments ? 1 : 0,
			matchNames = inf.allowedAttachments.ToArray(),
		};
		def.stockAttachments = new AttachmentSettingsDefinition()
		{
			numAttachmentSlots = inf.allowStockAttachments ? 1 : 0,
			matchNames = inf.allowedAttachments.ToArray(),
		};
		def.scopeAttachments = new AttachmentSettingsDefinition()
		{
			numAttachmentSlots = inf.allowScopeAttachments ? 1 : 0,
			matchNames = inf.allowedAttachments.ToArray(),
		};
		//def.genericAttachments
		//def.userMoveSpeedModifier = inf.moveSpeedModifier;
		//def.userKnockbackResist = inf.knockbackModifier;

		def.animationSet = OldAnimType.Convert();
		//switch(inf.animationType)
		//{
		//	case EAnimationType.REVOLVER: 
		//		def.AmmoConsumeMode = EAmmoConsumeMode.RoundRobin; 
		//		break;
		//	default: 
		//		def.AmmoConsumeMode = EAmmoConsumeMode.LastNonEmpty;
		//		break;
		//}

	}

	private string[] GetMags(GunType input)
	{
		List<string> mags = new List<string>();
		if(input.numAmmoItemsInGun > 1)
		{
			mags.Add($"{input.shortName}_internal_mag_{input.numAmmoItemsInGun}");
		}
		else
		{
			foreach(string ammoType in input.ammo)
				mags.Add($"{Utils.ToLowerWithUnderscores(ammoType)}");
		}
		return mags.ToArray();
	}

	private ResourceLocation[] GetTags(GunType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod:gun"));
		return tags.ToArray();
	}

	private ActionGroupDefinition CreateStartReloadActions(GunType inf)
	{
		List<ActionDefinition> reloadActions = new List<ActionDefinition>();
		if(TiltTime > 0.0f)
		{
			reloadActions.Add(new ActionDefinition() 
			{ 
				actionType = EActionType.Animation, 
				duration = inf.reloadTime * TiltTime / 20f,
				anim = "reload_start",
			});
		}
		if(inf.reloadSound != null && inf.reloadSound.Length > 0)
		{
			reloadActions.Add(new ActionDefinition()
			{
				actionType = EActionType.PlaySound,
				sounds = new SoundDefinition[] {
					new SoundDefinition()
					{
						sound = new ResourceLocation(Utils.ToLowerWithUnderscores(inf.reloadSound)),
					}
				}
			});
		}
		return new ActionGroupDefinition() 
		{
			key = "reload_primary_start",
			actions = reloadActions.ToArray()
		};
	}

	private ActionGroupDefinition CreateEjectReloadActions(GunType inf)
	{
		List<ActionDefinition> reloadActions = new List<ActionDefinition>();
		if(UnloadTime > 0.0f &&
		   OldAnimType != EAnimationType.END_LOADED)
		{
			reloadActions.Add(new ActionDefinition() 
			{ 
				actionType = EActionType.Animation, 
				duration = inf.reloadTime * UnloadTime / 20f,
				anim = "reload_eject",
			});
		}
		return new ActionGroupDefinition()
		{
			key = "reload_primary_eject",
			actions = reloadActions.ToArray()
		};
	}

	private ActionGroupDefinition CreateLoadOneReloadActions(GunType inf)
	{
		List<ActionDefinition> reloadActions = new List<ActionDefinition>();
		if(LoadTime > 0.0f)
		{
			reloadActions.Add(new ActionDefinition() 
			{ 
				actionType = EActionType.Animation, 
				duration = (inf.reloadTime * LoadTime / 20f) / inf.numAmmoItemsInGun,
				anim = "reload_load_one",
			});
		}
		return new ActionGroupDefinition()
		{
			key = "reload_primary_load_one",
			actions = reloadActions.ToArray()
		};
	}

	private ActionGroupDefinition CreateEndReloadActions(GunType inf)
	{
		List<ActionDefinition> reloadActions = new List<ActionDefinition>();

		if(UntiltTime > 0.0f)
		{
			reloadActions.Add(new ActionDefinition() 
			{ 
				actionType = EActionType.Animation, 
				duration = inf.reloadTime * UntiltTime / 20f,
				anim = "reload_end",
			});
		}
		return new ActionGroupDefinition()
		{
			key = "reload_primary_end",
			actions = reloadActions.ToArray()
		};
	}

	private ActionGroupDefinition CreateLookAtActions(GunType inf)
	{
		List<ActionDefinition> lookAtActions = new List<ActionDefinition>();

		lookAtActions.Add(new ActionDefinition()
		{
			actionType = EActionType.Animation,
			anim = "look_at",
			duration = 2.5f,
		});

		return new ActionGroupDefinition() 
		{
			key = "look_at",
			actions = lookAtActions.ToArray(),
		};
	}
	
	private ActionGroupDefinition CreatePrimaryActions(GunType inf)
	{
		List<ActionDefinition> primaryActions = new List<ActionDefinition>();
		List<ModifierDefinition> mods = new List<ModifierDefinition>();

		if (inf.shootDelay > 0.0f)
		{
			primaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Shoot,
						
				sounds = new SoundDefinition[0],
				itemStack = "", // Not used by this action
				scopeOverlay = "", // Not used by this action
				anim = "", // Not used by this action

	
				duration = inf.shootDelay / 20f,

				// warmup
				// loop
				// cooldown
			});
			primaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.PlaySound,
				sounds = new SoundDefinition[] {
					new SoundDefinition()
					{
						sound = new ResourceLocation(Utils.ToLowerWithUnderscores(inf.shootSound)),
						length = inf.shootSoundLength,
						minPitchMultiplier = inf.distortSound ? 1.0f / 1.2f : 1.0f,
						maxPitchMultiplier = inf.distortSound ? 1.0f / 0.8f : 1.0f,
					},
				},
			});
			mods.AddRange(CreateShotModifiers(inf));
		}
		else if(inf.meleeDamage > 0.0f)
		{
			primaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Melee,
				sounds = new SoundDefinition[] {
					new SoundDefinition()
					{
						sound = new ResourceLocation(Utils.ToLowerWithUnderscores(inf.meleeSound)),
					},
				},
			});
			mods.Add(BaseStat(Constants.STAT_TOOL_REACH, 5.0f));
			mods.Add(BaseStat(Constants.STAT_MELEE_DAMAGE, inf.meleeDamage));
		}			

		if(inf.dropItemOnShoot != null && inf.dropItemOnShoot.Length > 0)
		{
			primaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Drop,
				//drop = inf.dropItemOnShoot,
			});
		}
		if(inf.consumeGunUponUse)
		{
			primaryActions.Add(new ActionDefinition()
			{
				//actionType = EActionType.ConsumeGun,
			});
		}

		primaryActions.Add(new ActionDefinition()
		{
			actionType = EActionType.Animation,
			anim = "shoot",
			// "Instant" semi-automatics might have 1tick delay, but they want more than a 1tick anim
			duration = inf.shootDelay <= 1.0f ? 0.1f : inf.shootDelay / 20f,
		});

		// gunSlide

		// pumpHandle

		return new ActionGroupDefinition()
		{
			key = "primary_fire",
			canActUnderwater = inf.canShootUnderwater,
			canActUnderOtherLiquid = false,
			canBeOverriden = true,
			twoHanded = !inf.oneHanded,

			repeatMode = inf.mode,
			repeatCount = inf.numBurstRounds,
			repeatDelay = inf.shootDelay / 20f,
			spinUpDuration = inf.minigunStartSpeed / 10f,

			actions = primaryActions.ToArray(),
			modifiers = mods.ToArray(),
		};
	}

	public ModifierDefinition[] CreateShotModifiers(GunType type)
	{
		List<ModifierDefinition> mods = new List<ModifierDefinition>();

		mods.Add(BaseStat(Constants.STAT_SHOT_VERTICAL_RECOIL, type.recoil));
		mods.Add(BaseStat(Constants.STAT_SHOT_HORIZONTAL_RECOIL, type.recoil));
		mods.Add(BaseStat(Constants.STAT_SHOT_SPREAD, type.bulletSpread));
		mods.Add(BaseStat(Constants.STAT_SHOT_SPEED, type.bulletSpeed));
		mods.Add(BaseStat(Constants.STAT_SHOT_BULLET_COUNT, type.numBullets));
		mods.Add(BaseStat(Constants.STAT_IMPACT_DAMAGE, type.damage));
		mods.Add(BaseStat(Constants.STAT_IMPACT_KNOCKBACK, type.knockback));

		return mods.ToArray();
	}

	private ActionGroupDefinition CreateSecondaryActions(GunType inf)
	{
		List<ActionDefinition> secondaryActions = new List<ActionDefinition>();
		List<ModifierDefinition> mods = new List<ModifierDefinition>();

		if(inf.shield)
		{
			secondaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Shield,
				//shieldOrigin = shieldOrigin
				// shieldDimensions
				// shieldDamageAbsorption
			});
		}
		else if(inf.hasScopeOverlay)
		{
			secondaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Scope,
				scopeOverlay = inf.defaultScopeTexture,
			});
			mods.Add(BaseStat(Constants.STAT_ZOOM_FOV_FACTOR, Mathf.Approximately(inf.FOVFactor, 1.0f) ? inf.zoomLevel : inf.FOVFactor));
		}
		else if(inf.FOVFactor > 1.0f)
		{	
			secondaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.AimDownSights,
			});
			mods.Add(BaseStat(Constants.STAT_ZOOM_FOV_FACTOR, inf.FOVFactor));
			// Add animation action?
			secondaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Animation,
				anim = "aim_down_sights",
			});
		}
		return new ActionGroupDefinition()
		{
			key = "ads",
			canActUnderwater = true,
			canActUnderOtherLiquid = true,
			canBeOverriden = true,

			repeatMode = ERepeatMode.Toggle,
			repeatDelay = 0.05f,

			actions = secondaryActions.ToArray(),
			modifiers = mods.ToArray(),
		};
	}

	public override void GetAdditionalOperations(GunType inf, List<AdditionalImportOperation> operations)
	{
		PaintableConverter.inst.GetAdditionalOperations(inf, operations);

		if (inf.modelString != null && inf.modelString.Length > 0)
		{
			List<string> skinNames = new List<string>();
			List<string> iconNames = new List<string>();
			skinNames.Add(inf.texture);
			iconNames.Add(inf.iconPath);
			foreach (Paintjob paintjob in inf.paintjobs)
			{
				skinNames.Add(paintjob.textureName);
				iconNames.Add(paintjob.iconName);
			}
			operations.Add(new TurboImportOperation(
				$"{inf.modelFolder}/Model{inf.modelString}.java",
				$"/models/item/{Utils.ToLowerWithUnderscores(inf.shortName)}.prefab",
				skinNames,
				iconNames
			));
		}
	}

}

public class PartConverter : Converter<PartType, PartDefinition>
{
	public static PartConverter inst = new PartConverter();
	protected override void DoConversion(PartType input, PartDefinition output)
	{
		output.itemSettings.maxStackSize = input.stackSize;
		output.itemSettings.tags = GetTags(input);
		List<string> compatTags = new List<string>();
		if(input.worksWith.Contains(EDefinitionType.mecha))
			compatTags.Add("mecha");
		if(input.worksWith.Contains(EDefinitionType.vehicle))
			compatTags.Add("groundVehicle");
		if(input.worksWith.Contains(EDefinitionType.plane))
			compatTags.Add("plane");
		output.compatiblityTags = compatTags.ToArray();
		output.engine = new EngineDefinition()
		{
			maxAcceleration = input.engineSpeed,
			maxDeceleration = input.engineSpeed,
			maxSpeed = input.engineSpeed,
			fuelType = input.useRFPower ? EFuelType.FE : EFuelType.Smeltable,
			fuelConsumptionFull = input.fuelConsumption,
			fuelConsumptionIdle = input.fuelConsumption * 0.25f,
			solidFuelSlots = input.useRFPower ? 0 : 1,
			liquidFuelCapacity = 0,
			batterySlots = input.useRFPower ? 1 : 0,
			FECapacity = input.useRFPower ? 1000000 : 0,
		};
	}

	private ResourceLocation[] GetTags(PartType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod:part"));
		tags.Add(new ResourceLocation($"flansmod:{input.category.ToString().ToLower()}"));
		if(input.isAIChip)
			tags.Add(new ResourceLocation("flansmod:apocalypse_trigger"));
		return tags.ToArray();
	}

	public override void GetAdditionalOperations(PartType inf, List<AdditionalImportOperation> operations)
	{
		AddDefaultIconOperations(inf, operations);
	}
}

public class MagazineConverter : Converter<BulletType, MagazineDefinition>
{
	public static MagazineConverter inst = new MagazineConverter();
	protected override void DoConversion(BulletType input, MagazineDefinition output)
	{
		output.allRoundsMustBeIdentical = false;
		output.numRounds = input.roundsPerItem;
		output.matchingBullets = new ItemCollectionDefinition()
		{
			itemTagFilters = new LocationFilterDefinition[] {
				new LocationFilterDefinition()
				{
					filterType = EFilterType.Allow,
					matchResourceLocations = new ResourceLocation[] {
						new ResourceLocation("flansmod", "default_bullet"),
					}
				}
			}
		};
		output.upgradeCost = 0;
		output.ammoLoadMode = EAmmoLoadMode.FullMag;
		output.ammoConsumeMode = EAmmoConsumeMode.FirstNonEmpty;
	}

	public override void GetAdditionalOperations(BulletType inf, List<AdditionalImportOperation> operations)
	{
		operations.Add(new CopyImportOperation($"/assets/flansmod/textures/items/{inf.iconPath}.png",
											   $"/textures/mags/{inf.ConvertedName}.png"));
	}
}

public class BulletConverter : Converter<BulletType, BulletDefinition>
{
	public static BulletConverter inst = new BulletConverter();
	protected override void DoConversion(BulletType input, BulletDefinition output)
	{
		//output.shootStats.gravityFactor = input.fallSpeed;
		output.itemSettings.maxStackSize = input.maxStackSize;
		output.itemSettings.tags = GetTags(input);
		output.roundsPerItem = input.roundsPerItem;

		List<AbilityDefinition> abilityDefinitions = new List<AbilityDefinition>();
		if (input.dropItemOnShoot != null && input.dropItemOnShoot.Length > 0)
		{
			abilityDefinitions.Add(new AbilityDefinition() {
				startTriggers = new AbilityTriggerDefinition[] {
					new AbilityTriggerDefinition() {
						triggerType = EAbilityTrigger.BulletConsumed,
					}
				},
				targets = new AbilityTargetDefinition[] {
					new AbilityTargetDefinition() {
						targetType = EAbilityTarget.Shooter,
					}
				},
				effects = new AbilityEffectDefinition[] {
					new AbilityEffectDefinition() {
						effectType = EAbilityEffect.SpawnEntity,
						modifiers = new ModifierDefinition[] {
							StringStat(Constants.KEY_ENTITY_ID, "minecraft:item"),
							StringStat(Constants.KEY_ENTITY_TAG, "{Item:{id:\""+input.dropItemOnShoot+"\",Count:1b}}")
						}
					}
				}
			});
		}
		if(input.dropItemOnReload != null && input.dropItemOnReload.Length > 0)
		{
			abilityDefinitions.Add(new AbilityDefinition()
			{
				startTriggers = new AbilityTriggerDefinition[] {
					new AbilityTriggerDefinition() {
						triggerType = EAbilityTrigger.ReloadEject,
					}
				},
				targets = new AbilityTargetDefinition[] {
					new AbilityTargetDefinition() {
						targetType = EAbilityTarget.Shooter,
					}
				},
				effects = new AbilityEffectDefinition[] {
					new AbilityEffectDefinition() {
						effectType = EAbilityEffect.SpawnEntity,
						modifiers = new ModifierDefinition[] {
							StringStat(Constants.KEY_ENTITY_ID, "minecraft:item"),
							StringStat(Constants.KEY_ENTITY_TAG, "{Item:{id:\""+input.dropItemOnReload+"\",Count:1b}}")
						}
					}
				}
			});
		}
		output.triggers = abilityDefinitions.ToArray();

		List<ModifierDefinition> modifiers = new List<ModifierDefinition>();
		if (input.damageVsLiving != 1.0f)
			modifiers.Add(MultiplierStat(Constants.STAT_IMPACT_MULTIPLIER_VS_PLAYERS, input.damageVsLiving));
		if (input.damageVsDriveable != 1.0f)
			modifiers.Add(MultiplierStat(Constants.STAT_IMPACT_MULTIPLIER_VS_VEHICLES, input.damageVsDriveable));
		
		//if(input.)

		// TODO:
		//output.shootStats = 
		//ShotDefinition()
		//
		//// All N/A? Set by gun?
		//verticalRecoil = 0f,
		//horizontalRecoil = 0f,
		//hitscan = true,
		//speed = 0f,
		//spreadPattern = ESpreadPattern.FilledCircle,
		//spread = input.bulletSpread,
		//bulletCount = input.numBullets,
		//breaksMaterials = input.breaksGlass ? new string[] { "glass" } : new string[0],
		//penetrationPower = input.penetratingPower,
		//trailParticles = input.trailParticles ? input.trailParticleType : "",

		//impact = new ImpactDefinition()
		//{
		//	decal = "flansmod:effects/bullet_decal.png",
		//	damageToTarget = input.damageVsLiving,
		//	multiplierVsPlayers = 1f,
		//	multiplierVsVehicles = input.damageVsDriveable / input.damageVsLiving,
		//	knockback = 0f, // Not present?
		//	splashDamageRadius = 0f,
		//	splashDamageFalloff = 0f,
		//	setFireToTarget = input.setEntitiesOnFire ? 1f : 0f,
		//	fireSpreadRadius = input.fireRadius,
		//	fireSpreadAmount = 0.5f,
		//	hitSounds = input.hitSound.Length > 0 
		//	? new SoundDefinition[] {
		//		new SoundDefinition()
		//		{
		//			sound = Utils.ToLowerWithUnderscores(input.hitSound),
		//			maxRange = input.hitSoundRange
		//		}
		//	} : new SoundDefinition[0],
		//}
		//};
	}

	private ResourceLocation[] GetTags(BulletType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod", "bullet"));
		if(input.fireRadius > 0)
			tags.Add(new ResourceLocation("flansmod", "flammable"));
		if(input.explosionRadius > 0)
			tags.Add(new ResourceLocation("flansmod", "explosive"));
		return tags.ToArray();
	}

	public override void GetAdditionalOperations(BulletType inf, List<AdditionalImportOperation> operations)
	{
		AddDefaultIconOperations(inf, operations);
	}
}

public class AttachmentConverter : Converter<AttachmentType, AttachmentDefinition>
{
	public static AttachmentConverter inst = new AttachmentConverter();
	protected override void DoConversion(AttachmentType input, AttachmentDefinition output)
	{
		//PaintableConverter.inst.DoConversion(input, output.paints);
		output.itemSettings.maxStackSize = 1;
		output.itemSettings.tags = GetTags(input);
		output.attachmentType = input.type;
		List<ModifierDefinition> mods = new List<ModifierDefinition>();

		if(input.silencer)
		{
			mods.Add(MultiplierStat(Constants.STAT_GROUP_LOUDNESS, 0.1f));
			mods.Add(MultiplierStat(Constants.STAT_SOUND_PITCH, 1.5f));
		}

		if(input.flashlight)
		{
			mods.Add(BaseStat(Constants.STAT_LIGHT_STRENGTH, input.flashlightStrength));
			mods.Add(BaseStat(Constants.STAT_LIGHT_RANGE, input.flashlightRange));
		}


		if (input.spreadMultiplier != 1f)
			mods.Add(MultiplierStat(Constants.STAT_SHOT_SPREAD, input.spreadMultiplier));

		if (input.recoilMultiplier != 1f)
		{
			mods.Add(MultiplierStat(Constants.STAT_SHOT_VERTICAL_RECOIL, input.recoilMultiplier));
			mods.Add(MultiplierStat(Constants.STAT_SHOT_HORIZONTAL_RECOIL, input.recoilMultiplier));
		}

		if(input.damageMultiplier != 1f)
			mods.Add(MultiplierStat(Constants.STAT_IMPACT_DAMAGE, input.damageMultiplier));

		if (input.meleeDamageMultiplier != 1f)
			mods.Add(MultiplierStat(Constants.STAT_MELEE_DAMAGE, input.meleeDamageMultiplier));

		if (input.bulletSpeedMultiplier != 1f)
			mods.Add(MultiplierStat(Constants.STAT_SHOT_SPEED, input.bulletSpeedMultiplier));

		if (input.reloadTimeMultiplier != 1f)
			mods.Add(MultiplierStat(Constants.STAT_DURATION, input.reloadTimeMultiplier, "reload"));


		if (input.type == EAttachmentType.Sights)
		{
			if(input.hasScopeOverlay)
			{
				output.handlerOverrides = new HandlerDefinition[]
				{
					new HandlerDefinition()
					{
						inputType = EPlayerInput.Fire2,
						nodes = new HandlerNodeDefinition[]
						{
							new HandlerNodeDefinition()
							{
								actionGroupToTrigger = "scope",
							}
						}
					}
				};
				output.actionOverrides = new ActionGroupDefinition[]
				{
					new ActionGroupDefinition()
					{
						key = "scope",
						actions = new ActionDefinition[]
						{
							new ActionDefinition()
							{
								actionType = EActionType.Scope,
								scopeOverlay = input.zoomOverlay,
							}
						},
						modifiers = new ModifierDefinition[] {
							BaseStat(Constants.STAT_ZOOM_FOV_FACTOR, Mathf.Approximately(input.FOVZoomLevel, 1.0f) ? input.zoomLevel : input.FOVZoomLevel)
						},
						repeatMode = ERepeatMode.Toggle,
					}
				};
			}
			else
			{
				output.handlerOverrides = new HandlerDefinition[]
				{
					new HandlerDefinition()
					{
						inputType = EPlayerInput.Fire2,
						nodes = new HandlerNodeDefinition[] 
						{
							new HandlerNodeDefinition()
							{
								actionGroupToTrigger = "ads",
							}
						}
					}
				};
				output.actionOverrides = new ActionGroupDefinition[]
				{
					new ActionGroupDefinition()
					{
						key = "ads",
						actions = new ActionDefinition[]
						{
							new ActionDefinition()
							{
								actionType = EActionType.AimDownSights,
								scopeOverlay = input.zoomOverlay,
							}
						},
						modifiers = new ModifierDefinition[] {
							BaseStat(Constants.STAT_ZOOM_FOV_FACTOR, input.FOVZoomLevel)
						},
						repeatMode = ERepeatMode.Toggle,
					}
				};
			}
		}
		
		output.modifiers = mods.ToArray();
	}

	private ResourceLocation[] GetTags(AttachmentType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod:attachment"));
		tags.Add(new ResourceLocation($"flansmod:{input.type.ToString().ToLower()}"));
		return tags.ToArray();
	}

	public override void GetAdditionalOperations(AttachmentType inf, List<AdditionalImportOperation> operations)
	{
		PaintableConverter.inst.GetAdditionalOperations(inf, operations);
	}
}

public class GrenadeConverter : Converter<GrenadeType, GrenadeDefinition>
{
	public static GrenadeConverter inst = new GrenadeConverter();
	protected override void DoConversion(GrenadeType input, GrenadeDefinition output)
	{
		output.itemSettings.maxStackSize = input.maxStackSize;
		output.itemSettings.tags = GetTags(input);
		output.primaryActions = CreatePrimaryActions(input);
		output.secondaryActions = CreateSecondaryActions(input);
		output.bounciness = input.bounciness;
		output.sticky = input.sticky;
		output.canStickToThrower = input.stickToThrower;
		output.livingProximityTrigger = input.livingProximityTrigger;
		output.vehicleProximityTrigger = input.driveableProximityTrigger;
		output.detonateWhenShot = input.detonateWhenShot;
		output.detonateWhenDamaged = input.detonateWhenShot;
		output.fuse = input.fuse;
		output.spinForceX = input.spinWhenThrown ? 10.0f : 0.0f;
		output.spinForceY = input.spinWhenThrown ? 10.0f : 0.0f;
		// TODO:
		//output.impact = new ImpactDefinition()
		//{
		//	fireSpreadAmount = input.fireRadius,
		//	fireSpreadRadius = input.fireRadius,
		//	setFireToTarget = input.fireRadius,
		//	damageToTarget = input.damageVsLiving,
		//	splashDamageRadius = input.explosionRadius,
		//	splashDamageFalloff = input.explosionDamageVsLiving,
		//	multiplierVsPlayers = 1.0f,
		//	multiplierVsVehicles =  input.explosionDamageVsLiving <= 0.01f ? 0.0f : input.explosionDamageVsDriveable / input.explosionDamageVsLiving,
		//	hitSounds = new SoundDefinition[0],
		//	knockback = 0f,
		//	decal = new ResourceLocation("flansmod", "effects/bullet_decal"),
		//};
		output.lifetimeAfterDetonation = input.smokeTime;
		output.effectsToApplyInSmoke = input.smokeEffects.ToArray();
		output.smokeParticles = new string[] { input.smokeParticleType };
		output.smokeRadius = input.smokeRadius;

		// TODO: Deployable bags

	}

	private ActionDefinition[] CreatePrimaryActions(GrenadeType inf)
	{
		List<ActionDefinition> primaryActions = new List<ActionDefinition>();
		if(inf.canThrow)
		{
			primaryActions.Add(new ActionDefinition() 
			{ 
				actionType = EActionType.Animation, 
				anim = "GrenadeThrow",
				sounds = new SoundDefinition[] {
					new SoundDefinition()
					{
						sound = new ResourceLocation(Utils.ToLowerWithUnderscores(inf.throwSound)),
						length = 1,
					}
				}
			});
			primaryActions.Add(new ActionDefinition() 
			{ 
				//actionType = EActionType.ThrowGrenade,
			});
		}
		
		return primaryActions.ToArray();
	}
	
	private ActionDefinition[] CreateSecondaryActions(GrenadeType inf)
	{
		List<ActionDefinition> secondaryActions = new List<ActionDefinition>();
		secondaryActions.Add(new ActionDefinition() 
		{ 
			actionType = EActionType.Animation, 
			anim = "GrenadeCook",
		});
		secondaryActions.Add(new ActionDefinition() 
		{ 
			//actionType = EActionType.CookGrenade,

		});
		return secondaryActions.ToArray();
	}

	private ResourceLocation[] GetTags(GrenadeType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod:grenade"));
		return tags.ToArray();
	}

	public override void GetAdditionalOperations(GrenadeType inf, List<AdditionalImportOperation> operations)
	{
		AddDefaultIconOperations(inf, operations);
	}
}

public class VehiclePartBuilder
{
	private Dictionary<string, VehiclePartDefinition> Parts = new Dictionary<string, VehiclePartDefinition>();

	public VehiclePartBuilder()
	{
		Parts.Add("body", new VehiclePartDefinition()
		{
			partName = "body",
			attachedTo = "",
			localPosition = Vector3.zero,
			localEulerAngles = Vector3.zero,
		});
	}

	public VehiclePartDefinition GetPart(string partName)
	{
		if (!Parts.ContainsKey(partName))
		{
			Parts[partName] = new VehiclePartDefinition()
			{
				partName = partName,
				attachedTo = "body",
			};
		}
		return Parts[partName];
	}

	public VehiclePartDefinition[] Build()
	{
		// This step ensures we have a complete hierarchy, by adding missing parents as children of "body"
		List<VehiclePartDefinition> parts = new List<VehiclePartDefinition>(Parts.Values);
		foreach(VehiclePartDefinition part in parts)
		{
			GetPart(part.attachedTo);
		}

		return Parts.Values.ToArray();
	}
}

public class DriveableConverter : Converter<DriveableType, VehicleDefinition>
{
	public static DriveableConverter inst = new DriveableConverter();
	protected override void DoConversion(DriveableType input, VehicleDefinition output)
	{
		string definitionID = Minecraft.SanitiseID(input.shortName);
		output.itemSettings.maxStackSize = 1;
		output.itemSettings.tags = GetTags(input);

		VehiclePartBuilder builder = new VehiclePartBuilder();

		// Add health boxes
		foreach(var kvp in input.health)
		{
			string partName = kvp.Key;
			CollisionBox box = kvp.Value;
			builder.GetPart(Minecraft.ConvertPartName(partName)).damage.hitboxCenter = box.getCentre();
			builder.GetPart(Minecraft.ConvertPartName(partName)).damage.hitboxHalfExtents = new Vector3(box.w / 2f, box.h / 2f, box.d / 2f);
			builder.GetPart(Minecraft.ConvertPartName(partName)).damage.maxHealth = box.health;
		}

		if (input.seats != null)
		{
			foreach(Seat seat in input.seats)
			{
				VehiclePartDefinition parentDef = builder.GetPart(Minecraft.ConvertPartName(seat.part));
				SeatDefinition seatDef = new SeatDefinition()
				{
					maxYaw = seat.maxYaw,
					minYaw = seat.minYaw,
					maxPitch = seat.maxPitch,
					minPitch = seat.minPitch,
					gyroStabilised = false,
					offsetFromAttachPoint = new Vector3(seat.x, seat.y, seat.z),
				};
				if (seat.gunType != null && seat.gunType.Length > 0)
				{
					seatDef.inputs = new InputDefinition[] {
						new InputDefinition() {
							key = EPlayerInput.Fire1,
							guns = new MountedGunInputDefinition[] {
								new MountedGunInputDefinition() {
									gunName = $"gun_{seat.id}",
									toggle = false,
								}
							}
						}
					};
					VehiclePartDefinition gunYawPartDef = new VehiclePartDefinition()
					{
						partName = $"gun_{seat.id}_yaw",
						attachedTo = seat.part,
						articulation = new ArticulatedPartDefinition()
						{
							active = true,
							minYaw = seat.minYaw,
							maxYaw = seat.maxYaw,
							minParameter = -180f,
							maxParameter = 180f,
							cyclic = true,
						},
					};
					VehiclePartDefinition gunPitchPartDef = new VehiclePartDefinition()
					{
						partName = $"gun_{seat.id}_pitch",
						attachedTo = $"gun_{seat.id}_yaw",
						articulation = new ArticulatedPartDefinition()
						{
							active = true,
							minPitch = seat.minPitch,
							maxPitch = seat.maxPitch,
							minParameter = -90f,
							maxParameter = 90f,
							cyclic = false,
						},
						guns = new MountedGunDefinition[] {
							new MountedGunDefinition()
							{
								//shootPointOffset
								gun = new ResourceLocation($"{definitionID}_seat_gun_{seat.id}"),
							}
						}
					};
				}

				List<SeatDefinition> seats = new List<SeatDefinition>(parentDef.seats);
				seats.Add(seatDef);
				parentDef.seats = seats.ToArray();
			}
		}

		//PaintableConverter.inst.DoConversion(input, output.paints);

		for (int i = 0; i < input.wheelPositions.Length; i++)
		{
			VehiclePartDefinition wheelPart = builder.GetPart(Minecraft.ConvertPartName(input.wheelPositions[i].part));
			wheelPart.attachedTo = "body";
			wheelPart.localPosition = input.wheelPositions[i].position;
			wheelPart.wheels = new WheelDefinition[] {
				new WheelDefinition() {
					controlHints = GetDefaultWheelHints(i),
					visualOffset = Vector3.zero,
					gravityScale = 1.0f,
					buoyancy = input.buoyancy,
					floatOnWater = input.floatOnWater,
					springStrength = input.wheelSpringStrength,
					stepHeight = input.wheelStepHeight,
					mass = 1.0f,
					maxForwardTorque = input.maxThrottle,
					maxReverseTorque = input.maxNegativeThrottle,
					radius = input.wheelStepHeight * 0.5f,
					torqueResponsiveness = 1.0f,
					yawResponsiveness = 1.0f,
				}
			};
		}

		for(int i = 0; i < input.shootPointsPrimary.Count; i++)
		{
			VehiclePartDefinition partDef = builder.GetPart($"pilot_gun_primary_{i}");
			ShootPoint point = input.shootPointsPrimary[i];
			partDef.localPosition = point.rootPos.position;
			partDef.guns = new MountedGunDefinition[] {
				new MountedGunDefinition() {
					gun = new ResourceLocation($"{definitionID}_pilot_gun_primary"),
				}
			};
		}
		for(int i = 0; i < input.shootPointsSecondary.Count; i++)
		{
			VehiclePartDefinition partDef = builder.GetPart($"pilot_gun_secondary_{i}");
			ShootPoint point = input.shootPointsSecondary[i];
			partDef.localPosition = point.rootPos.position;
			partDef.guns = new MountedGunDefinition[] {
				new MountedGunDefinition() {
					gun = new ResourceLocation($"{definitionID}_pilot_gun_secondary"),
				}
			};
		}

		output.parts = builder.Build();
	}

	private ResourceLocation[] GetTags(DriveableType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod:vehicle"));
		return tags.ToArray();
	}
	public static VehiclePartDefinition GetPart(VehicleDefinition output, string partName)
	{
		foreach (VehiclePartDefinition partDef in output.parts)
			if (partDef.partName == partName)
				return partDef;
		return null;
	}

	private string GetAmmoTag(EWeaponType weaponType)
	{
		switch(weaponType)
		{
			case EWeaponType.MISSILE: return "flansmod:missile";
			case EWeaponType.BOMB: return "flansmod:bomb";
			case EWeaponType.SHELL: return "flansmod:shell";
			case EWeaponType.MINE: return "flansmod:mine";
			default: return "";
		}
	}
	public void ExportPrimaryGun(DriveableType input, GunDefinition output)
	{
		switch(input.primary)
		{
			case EWeaponType.SHELL:
			case EWeaponType.MISSILE:
			case EWeaponType.BOMB:
			case EWeaponType.MINE:
				output.actionGroups = new ActionGroupDefinition[] {
					new ActionGroupDefinition() {
						key = "primary_fire",
						actions = new ActionDefinition[] {
							new ActionDefinition() {
								actionType = EActionType.Shoot,
								sounds = new SoundDefinition[] {
									new SoundDefinition() {
										sound = new ResourceLocation(Minecraft.SanitiseID(input.shootSoundPrimary)),
										length = input.shootDelayPrimary,
										minPitchMultiplier = 1.0f / 1.2f,
										maxPitchMultiplier = 1.0f / 0.8f,
									}
								},
							},
							new ActionDefinition() {
								actionType = EActionType.Animation,
								anim = "FireMissile",
							}
						},
						repeatMode = input.modePrimary,
						repeatDelay = input.shootDelayPrimary,
					},
					new ActionGroupDefinition() {
						key = "primary_reload_start",
						actions = new ActionDefinition[] {
							new ActionDefinition() {
								actionType = EActionType.PlaySound,
								sounds = new SoundDefinition[] {
									new SoundDefinition() {
										sound = new ResourceLocation(Minecraft.SanitiseID(input.reloadSoundPrimary)),
									}
								}
							}
						},
						repeatDelay = input.reloadTimePrimary * 0.25f,
					},
					new ActionGroupDefinition() {
						key = "primary_reload_eject",
						actions = new ActionDefinition[0],
						repeatDelay = input.reloadTimePrimary * 0.25f,
					},
					new ActionGroupDefinition() {
						key = "primary_reload_load_one",
						actions = new ActionDefinition[0],
						repeatDelay = input.reloadTimePrimary * 0.25f,
					},
					new ActionGroupDefinition() {
						key = "primary_reload_end",
						actions = new ActionDefinition[0],
						repeatDelay = input.reloadTimePrimary * 0.25f,
					},
				};
				output.reloads = new ReloadDefinition[] {
					new ReloadDefinition() {
						key = "primary",
						startActionKey = "primary_reload_start",
						ejectActionKey = "primary_reload_eject",
						loadOneActionKey = "primary_reload_load_one",
						endActionKey = "primary_reload_end",
					},
				};
				output.magazines = new MagazineSlotSettingsDefinition[] {
					new MagazineSlotSettingsDefinition() {
						key = "primary",
						matchByNames = input.ammo.ToArray(),
						matchByTags = input.acceptAllAmmo ? new string[] { GetAmmoTag(input.primary) } : new string[0],
					}
				};
				break;
			default:
				
				break;
		}
	}
	public void ExportSecondaryGun(DriveableType input, GunDefinition output)
	{
		switch (input.secondary)
		{
			case EWeaponType.SHELL:
			case EWeaponType.MISSILE:
			case EWeaponType.BOMB:
			case EWeaponType.MINE:
				output.actionGroups = new ActionGroupDefinition[] {
					new ActionGroupDefinition() {
						key = "secondary_fire",
						actions = new ActionDefinition[] {
							new ActionDefinition() {
								actionType = EActionType.Shoot,
								sounds = new SoundDefinition[] {
									new SoundDefinition() {
										sound = new ResourceLocation(Minecraft.SanitiseID(input.shootSoundSecondary)),
										length = input.shootDelaySecondary,
										minPitchMultiplier = 1.0f / 1.2f,
										maxPitchMultiplier = 1.0f / 0.8f,
									}
								},
							},
							new ActionDefinition() {
								actionType = EActionType.Animation,
								anim = "FireMissile",
							}
						},
						repeatMode = input.modeSecondary,
						repeatDelay = input.shootDelaySecondary,
					},
					new ActionGroupDefinition() {
						key = "secondary_reload_start",
						actions = new ActionDefinition[] {
							new ActionDefinition() {
								actionType = EActionType.PlaySound,
								sounds = new SoundDefinition[] {
									new SoundDefinition() {
										sound = new ResourceLocation(Minecraft.SanitiseID(input.reloadSoundSecondary)),
									}
								}
							}
						},
						repeatDelay = input.reloadTimeSecondary * 0.25f,
					},
					new ActionGroupDefinition() {
						key = "secondary_reload_eject",
						actions = new ActionDefinition[0],
						repeatDelay = input.reloadTimeSecondary * 0.25f,
					},
					new ActionGroupDefinition() {
						key = "secondary_reload_load_one",
						actions = new ActionDefinition[0],
						repeatDelay = input.reloadTimeSecondary * 0.25f,
					},
					new ActionGroupDefinition() {
						key = "secondary_reload_end",
						actions = new ActionDefinition[0],
						repeatDelay = input.reloadTimeSecondary * 0.25f,
					},
				};
				output.reloads = new ReloadDefinition[] {
					new ReloadDefinition() {
						key = "secondary",
						startActionKey = "secondary_reload_start",
						ejectActionKey = "secondary_reload_eject",
						loadOneActionKey = "secondary_reload_load_one",
						endActionKey = "secondary_reload_end",
					},
				};
				output.magazines = new MagazineSlotSettingsDefinition[] {
					new MagazineSlotSettingsDefinition() {
						key = "secondary",
						matchByNames = input.ammo.ToArray(),
						matchByTags = input.acceptAllAmmo ? new string[] { GetAmmoTag(input.secondary) } : new string[0],
					}
				};
				break;
			default:

				break;
		}
	}
	private EControlLogicHint[] GetDefaultWheelHints(int wheelIndex)
	{
		switch (wheelIndex)
		{
			case 0:
			case 1:
				return new EControlLogicHint[] { EControlLogicHint.Rear, EControlLogicHint.Drive };
			case 2:
			case 3:
				return new EControlLogicHint[] { EControlLogicHint.Front, EControlLogicHint.Steering };
			default:
				return new EControlLogicHint[] { EControlLogicHint.Fixed };
		}
	}

	public delegate void GunExportFunc(DriveableType input, GunDefinition output);
	public Dictionary<string, GunExportFunc> GetGunExportNames(DriveableType input)
	{
		string id = Minecraft.SanitiseID(input.shortName);
		Dictionary<string, GunExportFunc> exporters = new Dictionary<string, GunExportFunc>();
		if (input.primary != EWeaponType.GUN && input.primary != EWeaponType.NONE)
			exporters.Add($"{id}_pilot_gun_primary", (i, o) => { ExportPrimaryGun(i, o); });
		if (input.secondary != EWeaponType.GUN && input.secondary != EWeaponType.NONE)
			exporters.Add($"{id}_pilot_gun_secondary", (i, o) => { ExportSecondaryGun(i, o); });

		return exporters;
	}
	public override void GetAdditionalOperations(DriveableType inf, List<AdditionalImportOperation> operations)
	{
		string definitionID = Minecraft.SanitiseID(inf.shortName);
		if (inf.primary != EWeaponType.GUN && inf.primary != EWeaponType.NONE)
		{
			operations.Add(new AdditionalDefinitionOperation<GunDefinition>(
				inf.shortName,
				$"{definitionID}_pilot_gun_primary",
				(gunDef) => { ExportPrimaryGun(inf, gunDef); }));
		}
		if (inf.secondary != EWeaponType.GUN && inf.secondary != EWeaponType.NONE)
		{
			operations.Add(new AdditionalDefinitionOperation<GunDefinition>(
				inf.shortName,
				$"{definitionID}_pilot_gun_secondary",
				(gunDef) => { ExportSecondaryGun(inf, gunDef); }));
		}

		PaintableConverter.inst.GetAdditionalOperations(inf, operations);

		if (inf.modelString != null && inf.modelString.Length > 0)
		{
			List<string> skinNames = new List<string>();
			List<string> iconNames = new List<string>();
			skinNames.Add(inf.texture);
			iconNames.Add(inf.iconPath);
			foreach (Paintjob paintjob in inf.paintjobs)
			{
				skinNames.Add(paintjob.textureName);
				iconNames.Add(paintjob.iconName);
			}
			operations.Add(new TurboImportOperation(
				$"{inf.modelFolder}/Model{inf.modelString}.java",
				$"/models/item/{definitionID}.prefab",
				skinNames,
				iconNames
			));
		}
	}
}

public class AAGunConverter : Converter<AAGunType, VehicleDefinition>
{
	public static AAGunConverter inst = new AAGunConverter();
	protected override void DoConversion(AAGunType input, VehicleDefinition output)
	{
		string definitionID = Minecraft.SanitiseID(input.shortName);
		VehiclePartBuilder builder = new VehiclePartBuilder();

		VehiclePartDefinition yawPart = builder.GetPart("yaw_platform");
		yawPart.articulation = new ArticulatedPartDefinition()
		{
			active = true,
			minYaw = -180f,
			maxYaw = 180f,
			minParameter = -1f,
			startParameter = 0f,
			maxParameter = 1f,
			cyclic = true,
			traveseIndependently = true,
		};
		yawPart.seats = new SeatDefinition[] {
			new SeatDefinition() {
				offsetFromAttachPoint = new Vector3(input.gunnerX, input.gunnerY, input.gunnerZ) / 16f,
				gyroStabilised = true,
				inputs = new InputDefinition[]
				{
					new InputDefinition()
					{
						key = EPlayerInput.YawLeft,
						articulations = new ArticulationInputDefinition[]
						{
							new ArticulationInputDefinition()
							{
								partName = "yawPlatform",
								type = EArticulationInputType.FollowLookAxis,
								speed = 1.0f,
							}
						}
					},
					new InputDefinition()
					{
						key = EPlayerInput.YawRight,
						articulations = new ArticulationInputDefinition[]
						{
							new ArticulationInputDefinition()
							{
								partName = "yawPlatform",
								type = EArticulationInputType.FollowLookAxis,
								speed = 1.0f,
							}
						}
					},
					new InputDefinition()
					{
						key = EPlayerInput.Fire1,
						guns = GetMountedGunInputs(input),
						alternateInputs = input.fireAlternately,
					}
				}
			}
		};

		VehiclePartDefinition pitchPart = builder.GetPart("pitch_barrel");
		pitchPart.attachedTo = yawPart.partName;
		pitchPart.articulation = new ArticulatedPartDefinition()
		{
			active = true,
			minPitch = input.bottomViewLimit,
			maxPitch = input.topViewLimit,
			minParameter = -1f,
			startParameter = 0f,
			maxParameter = 1f,
			traveseIndependently = true,
		};
		pitchPart.guns = GetMountedGuns(definitionID, input);
	}

	private MountedGunDefinition[] GetMountedGuns(string definitionID, AAGunType input)
	{
		MountedGunDefinition[] guns = new MountedGunDefinition[input.numBarrels];
		for (int i = 0; i < input.numBarrels; i++)
		{
			guns[i] = new MountedGunDefinition()
			{
				shootPointOffset = new Vector3(input.barrelX[i], input.barrelY[i], input.barrelZ[i]),
				gun = new ResourceLocation($"{definitionID}_gun"),
			};
		}
		return guns;
	}

	private MountedGunInputDefinition[] GetMountedGunInputs(AAGunType input)
	{
		MountedGunInputDefinition[] defs = new MountedGunInputDefinition[input.numBarrels];
		for(int i = 0; i < input.numBarrels; i++)
		{
			defs[i] = new MountedGunInputDefinition()
			{
				gunName = $"aa_gun_{i}",
			};
		}
		return defs;
	}

	public override void GetAdditionalOperations(AAGunType inf, List<AdditionalImportOperation> operations)
	{
		string definitionID = Minecraft.SanitiseID(inf.shortName);

		AddDefaultIconOperations(inf, operations);
		//DriveableConverter.inst.GetAdditionalOperations(inf, operations);

		operations.Add(new AdditionalDefinitionOperation<GunDefinition>(
			inf.shortName,
			$"{definitionID}_gun",
			(gunDef) => { ExportGunDef(inf, gunDef); }));
	}

	public void ExportGunDef(AAGunType input, GunDefinition output)
	{
		output.actionGroups = new ActionGroupDefinition[] {
			new ActionGroupDefinition() {
				key = "primary",
				repeatMode = ERepeatMode.FullAuto,
				repeatDelay = input.shootDelay,
				actions = new ActionDefinition[] {
					new ActionDefinition() {
						actionType = EActionType.Shoot,
						sounds = new SoundDefinition[] {
							new SoundDefinition() {
								sound = new ResourceLocation(input.shootSound),
							}
						}
					},
					new ActionDefinition() {
						actionType = EActionType.Animation,
						anim = "shoot",
					}
				},
				modifiers = new ModifierDefinition[] {
					//BaseStat(Constants.STAT_SHOT_HORIZONTAL_RECOIL, input.recoil),
					BaseStat(Constants.STAT_SHOT_VERTICAL_RECOIL, input.recoil),
					BaseStat(Constants.STAT_SHOT_SPREAD, input.accuracy),
					BaseStat(Constants.STAT_IMPACT_DAMAGE, input.damage),
				}
			},
			new ActionGroupDefinition() {
				key = "primary_reload_start",
				repeatDelay = input.reloadTime * 0.25f,
				actions = new ActionDefinition[] {
					new ActionDefinition() {
						actionType = EActionType.PlaySound,
						sounds = new SoundDefinition[] {
							new SoundDefinition() {
								sound = new ResourceLocation(input.reloadSound),
							}
						}
					}
				}
			},
			new ActionGroupDefinition() {
				key = "primary_reload_eject",
				repeatDelay = input.reloadTime * 0.25f,
			},
			new ActionGroupDefinition() {
				key = "primary_reload_load_one",
				repeatDelay = input.reloadTime * 0.25f,
			},
			new ActionGroupDefinition() {
				key = "primary_reload_end",
				repeatDelay = input.reloadTime * 0.25f,
			},

		};
		output.reloads = new ReloadDefinition[] {
			new ReloadDefinition() {
				key = "primary",
				startActionKey = "primary_reload_start",
				ejectActionKey = "primary_reload_eject",
				loadOneActionKey = "primary_reload_load_one",
				endActionKey = "primary_reload_end",
				autoReloadWhenEmpty = true,
				manualReloadAllowed = true,
			}
		};
		output.magazines = new MagazineSlotSettingsDefinition[] {
			new MagazineSlotSettingsDefinition() {
				key = "primary",
				matchByNames = input.ammo.ToArray(),
			}			 
		};
		output.inputHandlers = new HandlerDefinition[] {
			new HandlerDefinition() {
				nodes = new HandlerNodeDefinition[] {
					new HandlerNodeDefinition() {
						actionGroupToTrigger = "primary_fire",
					},
					new HandlerNodeDefinition() {
						actionGroupToTrigger = "primary_reload_start",
					}
				}
			}
		};
	}
}

public class VehicleConverter : Converter<VehicleType, VehicleDefinition>
{
	public static VehicleConverter inst = new VehicleConverter();
	protected override void DoConversion(VehicleType input, VehicleDefinition output)
	{
		DriveableConverter.inst.Convert(input, output);

		List<InputDefinition> driverInputs = new List<InputDefinition>();
		if (input.hasDoor)
		{
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.SpecialKey1,
				articulations = new ArticulationInputDefinition[] {
					new ArticulationInputDefinition() {
						partName = "door",
						type = EArticulationInputType.CycleKeyframes,
					}
				}
			});
		}

		foreach (VehiclePartDefinition partDef in output.parts)
		{
			if(partDef.partName == "body")
			{
				if(partDef.seats.Length > 0)
				{
					SeatDefinition driverSeat = partDef.seats[0];
					driverSeat.inputs = driverInputs.ToArray();
					driverSeat.controllerOptions = new VehicleControlOptionDefinition[] {
						new VehicleControlOptionDefinition()
						{
							key = "default",
							controlScheme = new ResourceLocation("flansmod", input.tank ? "legacy_tank" : "legacy_car"),
						}
					};
				}
			}
		}
		// TODO: Move seats onto the turret?
	}

	public override void GetAdditionalOperations(VehicleType inf, List<AdditionalImportOperation> operations)
	{
		DriveableConverter.inst.GetAdditionalOperations(inf, operations);
	}
}

public class PlaneConverter : Converter<PlaneType, VehicleDefinition>
{
	public static PlaneConverter inst = new PlaneConverter();
	protected override void DoConversion(PlaneType input, VehicleDefinition output)
	{
		DriveableConverter.inst.Convert(input, output);

		List<InputDefinition> driverInputs = new List<InputDefinition>();
		if(input.hasDoor)
		{		
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.SpecialKey1,
				articulations = new ArticulationInputDefinition[] {
					new ArticulationInputDefinition()
					{
						partName = "door",
						type = EArticulationInputType.CycleKeyframes,
					}
				}
			});
		}
		List<VehicleControlOptionDefinition> moveModes = new List<VehicleControlOptionDefinition>();
		List<String> inputModeKeys = new List<string>();
		switch(input.mode)
		{
			case EPlaneMode.PLANE:
				CreatePropellers(input, input.propellers, output);
				moveModes.Add(new VehicleControlOptionDefinition()
				{
					key = "default",
					controlScheme = new ResourceLocation("flansmod", "legacy_plane")
				});
				inputModeKeys.Add("default");
				break;
			case EPlaneMode.VTOL:
				CreatePropellers(input, input.propellers, output);
				CreatePropellers(input, input.heliPropellers, output);
				CreatePropellers(input, input.heliTailPropellers, output);
				moveModes.Add(new VehicleControlOptionDefinition()
				{
					key = "plane",
					controlScheme = new ResourceLocation("flansmod", "legacy_plane"),
					modalCheck = "vtol:plane",
				});
				moveModes.Add(new VehicleControlOptionDefinition()
				{
					key = "heli",
					controlScheme = new ResourceLocation("flansmod", "legacy_helicopter"),
					modalCheck = "vtol:heli",
				});
				inputModeKeys.Add("plane");
				inputModeKeys.Add("heli");
				break;
			case EPlaneMode.HELI:
				CreatePropellers(input, input.heliPropellers, output);
				CreatePropellers(input, input.heliTailPropellers, output);
				moveModes.Add(new VehicleControlOptionDefinition()
				{
					key = "default",
					controlScheme = new ResourceLocation("flansmod", "legacy_helicopter"),
				});
				inputModeKeys.Add("default");
				break;
		}

		foreach (VehiclePartDefinition partDef in output.parts)
		{
			if (partDef.partName == "body")
			{
				if (partDef.seats.Length > 0)
				{
					SeatDefinition driverSeat = partDef.seats[0];
					driverSeat.inputs = driverInputs.ToArray();
					driverSeat.controllerOptions = moveModes.ToArray();
				}
			}
		}
	}
	private void CreatePropellers(PlaneType input, List<Propeller> props, VehicleDefinition output)
	{
		for (int i = 0; i < props.Count; i++)
		{
			VehiclePartDefinition partDef = DriveableConverter.GetPart(output, props[i].planePart);
			if(partDef != null)
			{
				List<PropellerDefinition> propDefs = new List<PropellerDefinition>(partDef.propellers);
				propDefs.Add(new PropellerDefinition()
				{
					visualOffset = props[i].getPosition(),
					forceOffset = Vector3.zero,
				});
				partDef.propellers = propDefs.ToArray();
			}
		}
	}
	public override void GetAdditionalOperations(PlaneType inf, List<AdditionalImportOperation> operations)
	{
		DriveableConverter.inst.GetAdditionalOperations(inf, operations);
	}
}

public class MechaItemConverter : Converter<MechaItemType, AttachmentDefinition>
{
	public static MechaItemConverter inst = new MechaItemConverter();
	protected override void DoConversion(MechaItemType input, AttachmentDefinition output)
	{
		switch(input.type)
		{
			case EMechaItemType.upgrade:
				output.attachmentType = EAttachmentType.Generic;
				break;
			case EMechaItemType.tool:
				output.attachmentType = EAttachmentType.Tool;
				break;
			case EMechaItemType.armUpgrade:
				output.attachmentType = EAttachmentType.Arm;
				break;
			case EMechaItemType.legUpgrade:
				output.attachmentType = EAttachmentType.Leg;
				break;
			case EMechaItemType.feetUpgrade:
				output.attachmentType = EAttachmentType.Feet;
				break;
			case EMechaItemType.hipsUpgrade:
				output.attachmentType = EAttachmentType.Hips;
				break;
			case EMechaItemType.shoulderUpgrade:
				output.attachmentType = EAttachmentType.Shoulder;
				break;
			case EMechaItemType.headUpgrade:
				output.attachmentType = EAttachmentType.Head;
				break;
			default:
				output.attachmentType = EAttachmentType.Generic;
				break;
		}

		List<ActionDefinition> primaryActions = new List<ActionDefinition>();
		List<ActionDefinition> secondaryActions = new List<ActionDefinition>();
		List<ModifierDefinition> mods = new List<ModifierDefinition>();
		switch (input.function)
		{
			case EMechaToolType.pickaxe:
				primaryActions.Add(new ActionDefinition() { actionType = EActionType.Pickaxe, });
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_LEVEL, input.toolHardness));
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_SPEED, input.speed));
				mods.Add(BaseStat(Constants.STAT_TOOL_REACH, input.reach));
				break;
			case EMechaToolType.axe:
				primaryActions.Add(new ActionDefinition() { actionType = EActionType.Axe });
				secondaryActions.Add(new ActionDefinition() { actionType = EActionType.Strip });
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_LEVEL, input.toolHardness));
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_SPEED, input.speed));
				mods.Add(BaseStat(Constants.STAT_TOOL_REACH, input.reach));
				break;
			case EMechaToolType.shovel:
				primaryActions.Add(new ActionDefinition() { actionType = EActionType.Shovel });
				secondaryActions.Add(new ActionDefinition() { actionType = EActionType.Flatten });
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_LEVEL, input.toolHardness));
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_SPEED, input.speed));
				mods.Add(BaseStat(Constants.STAT_TOOL_REACH, input.reach));
				break;
			case EMechaToolType.shears:
				primaryActions.Add(new ActionDefinition() { actionType = EActionType.Shear });
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_LEVEL, input.toolHardness));
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_SPEED, input.speed));
				mods.Add(BaseStat(Constants.STAT_TOOL_REACH, input.reach));
				break;
			case EMechaToolType.sword:
				primaryActions.Add(new ActionDefinition() { actionType = EActionType.Melee });
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_LEVEL, input.toolHardness));
				mods.Add(BaseStat(Constants.STAT_TOOL_HARVEST_SPEED, input.speed));
				mods.Add(BaseStat(Constants.STAT_TOOL_REACH, input.reach));
				break;
		}

		output.actionOverrides = new ActionGroupDefinition[]
		{
			new ActionGroupDefinition()
			{
				key = "primary",
				actions = primaryActions.ToArray(),
			},
			new ActionGroupDefinition()
			{
				key = "secondary",
				actions = secondaryActions.ToArray(),
			}
		};

		
		if (!Mathf.Approximately(input.speedMultiplier, 1f))
			mods.Add(MultiplierStat(Constants.STAT_MOVEMENT_SPEED, input.speedMultiplier));
		if(!Mathf.Approximately(input.damageResistance, 1f))
			mods.Add(MultiplierStat(Constants.STAT_DAMAGE_RESISTANCE, input.damageResistance));

		if (input.lightLevel != 0)
		{
			mods.Add(BaseStat(Constants.STAT_LIGHT_STRENGTH, input.lightLevel));
			mods.Add(BaseStat(Constants.STAT_LIGHT_RANGE, input.lightLevel));
		}

		// TODO:
		//if(input.stopMechaFallDamage)
		//	mods.Add(MultiplierStat("fall_damage", 0f));
		//if (input.forceBlockFallDamage)
		//	mods.Add(BaseStat("transfer_fall_damage_into_explosion", 1.0f));
		//
		//if(!Mathf.Approximately(input.fortuneCoal, 1f))
		//	mods.Add(new ModifierDefinition()
		//	{
		//		Stat = "fortune",
		//		Filter = "minecraft:coal_ore_block",
		//		Multiply = input.fortuneCoal,
		//	});
		//if(!Mathf.Approximately(input.fortuneIron, 1f))
		//	mods.Add(new ModifierDefinition()
		//	{
		//		Stat = "fortune",
		//		Filter = "minecraft:iron_ore_block",
		//		Multiply = input.fortuneIron,
		//	});
		//if(!Mathf.Approximately(input.fortuneEmerald, 1f))
		//	mods.Add(new ModifierDefinition()
		//	{
		//		Stat = "fortune",
		//		Filter = "minecraft:emerald_ore_block",
		//		Multiply = input.fortuneEmerald,
		//	});
		//if(!Mathf.Approximately(input.fortuneRedstone, 1f))
		//	mods.Add(new ModifierDefinition()
		//	{
		//		Stat = "fortune",
		//		Filter = "minecraft:redstone_ore_block",
		//		Multiply = input.fortuneRedstone,
		//	});
		//if(!Mathf.Approximately(input.fortuneDiamond, 1f))
		//	mods.Add(new ModifierDefinition()
		//	{
		//		Stat = "fortune",
		//		Filter = "minecraft:diamond_ore_block",
		//		Multiply = input.fortuneDiamond,
		//	});
		output.modifiers = mods.ToArray();

		List<EMechaEffect> effects = new List<EMechaEffect>();
		if(input.vacuumItems)
			effects.Add(EMechaEffect.vacuumItems);
		if(input.refineIron)
		{
			effects.Add(EMechaEffect.smeltOres);
			output.mechaEffectFilter = "minecraft:raw_iron";
		}
		if(input.autoCoal)
			effects.Add(EMechaEffect.autoCoal);
		if(input.autoRepair)
			effects.Add(EMechaEffect.autoRepair);
		if(input.rocketPack)
			effects.Add(EMechaEffect.rocketPack);
		if(input.diamondDetect)
		{
			effects.Add(EMechaEffect.detectBlock);
			output.mechaEffectFilter = "minecraft:diamond_ore"; // TODO: Tag based
		}
		if(input.infiniteAmmo)
			effects.Add(EMechaEffect.infiniteAmmo);
		if(input.forceDark)
			effects.Add(EMechaEffect.forceDark);
		if(input.wasteCompact)
		{
			effects.Add(EMechaEffect.wasteCompact);
			output.mechaEffectFilter = "minecraft:dirt_block";
		}
		if(input.flameBurst)
			effects.Add(EMechaEffect.flameBurst);
		if(input.floater)
			effects.Add(EMechaEffect.floater);
		output.mechaEffects = effects.ToArray();
	}

	public override void GetAdditionalOperations(MechaItemType inf, List<AdditionalImportOperation> operations)
	{
		AddDefaultIconOperations(inf, operations);
	}
}

public class MechaConverter : Converter<MechaType, VehicleDefinition>
{
	public static MechaConverter inst = new MechaConverter();
	protected override void DoConversion(MechaType input, VehicleDefinition output)
	{
		DriveableConverter.inst.Convert(input, output);

		VehiclePartDefinition partDef = DriveableConverter.GetPart(output, "body");
		if (partDef != null)
		{
			partDef.legs = new LegsDefinition[]
			{
				new LegsDefinition()
				{
					attachedTo = "body",
					visualOffset = new Vector3(),
					physicsOffset = new Vector3(),
					stepHeight = input.stepHeight,
					jumpHeight = input.jumpHeight,
					jumpVelocity = input.jumpVelocity,
					rotateSpeed = input.rotateSpeed,
					bodyMinYaw = input.limitHeadTurn ? -input.limitHeadTurnValue : -360f,
					bodyMaxYaw = input.limitHeadTurn ? input.limitHeadTurnValue : 360f,
					legLength = input.legLength,
					negateFallDamageRatio = input.takeFallDamage ? 1.0f - input.fallDamageMultiplier : 0f,
					transferFallDamageIntoEnvironmentRatio = input.damageBlocksFromFalling ? input.blockDamageFromFalling : 0f,
				}
			};
			partDef.arms = new ArmDefinition[]
			{
				new ArmDefinition()
				{
					name = "right",
					attachedTo = "body",
					right = true,
					origin = input.rightArmOrigin,
					armLength = input.armLength,
					hasHoldingSlot = true,
					numUpgradeSlots = 1,
					canFireGuns = true,
					canUseMechaTools = true,
					heldItemScale = input.heldItemScale,
					reach = input.reach
				},
				new ArmDefinition()
				{
					name = "left",
					attachedTo = "body",
					right = false,
					origin = input.leftArmOrigin,
					armLength = input.armLength,
					hasHoldingSlot = true,
					numUpgradeSlots = 1,
					canFireGuns = true,
					canUseMechaTools = true,
					heldItemScale = input.heldItemScale,
					reach = input.reach
				}
			};
			if (partDef.seats.Length > 0)
			{
				partDef.seats[0].controllerOptions = new VehicleControlOptionDefinition[]
				{
					new VehicleControlOptionDefinition() {
						key = "default",
						controlScheme = new ResourceLocation("flansmod", "mecha_default"),
					}
				};
			}
		}
	}

	public override void GetAdditionalOperations(MechaType inf, List<AdditionalImportOperation> operations)
	{
		DriveableConverter.inst.GetAdditionalOperations(inf, operations);
	}
}

public class ToolConverter : Converter<ToolType, ToolDefinition>
{
	public static ToolConverter inst = new ToolConverter();
	protected override void DoConversion(ToolType input, ToolDefinition output)
	{
		output.itemSettings.tags = GetTags(input);
		output.primaryActions = CreatePrimaryActions(input);
		output.secondaryActions = CreateSecondaryActions(input);
		output.hasDurability = input.toolLife != 0;
		output.maxDurability = input.toolLife;
		output.destroyWhenBroken = input.destroyOnEmpty;
		output.usesPower = input.EUPerCharge > 0;
		output.internalFEStorage = 0;
		output.primaryFEUsage = input.EUPerCharge;
		output.secondaryFEUsage = input.EUPerCharge;
		output.spendFEOnFailRatio = 0.0f;
		output.foodValue = input.foodness;
	}

	private ResourceLocation[] GetTags(ToolType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod:tool"));
		return tags.ToArray();
	}

	private ActionDefinition[] CreatePrimaryActions(ToolType input)
	{
		List<ActionDefinition> primaryActions = new List<ActionDefinition>();

		if(input.parachute)
		{
			// TODO
		}
		if(input.remote)
		{
			// TODO
		}
		if(input.healAmount > 0)
		{
			// TODO
		}

		return primaryActions.ToArray();
	}

	private ActionDefinition[] CreateSecondaryActions(ToolType input)
	{
		List<ActionDefinition> secondaryActions = new List<ActionDefinition>();

		return secondaryActions.ToArray();
	}

	public override void GetAdditionalOperations(ToolType inf, List<AdditionalImportOperation> operations)
	{
		AddDefaultIconOperations(inf, operations);
	}
}

public class ArmourConverter : Converter<ArmourType, ArmourDefinition>
{
	public static ArmourConverter inst = new ArmourConverter();
	protected override void DoConversion(ArmourType input, ArmourDefinition output)
	{
		output.itemSettings.tags = GetTags(input);
		output.armourType = (EArmourType)input.type;
		output.maxDurability = input.Durability;
		output.armourToughness = input.Toughness;
		output.enchantability = input.Enchantability;
		output.damageReduction = input.DamageReductionAmount;
		output.armourTextureName = input.armourTextureName;
		List<ModifierDefinition> mods = new List<ModifierDefinition>();
		if(!Mathf.Approximately(input.moveSpeedModifier, 1f))
			mods.Add(MultiplierStat(Constants.STAT_MOVEMENT_SPEED, input.moveSpeedModifier));
		if(!Mathf.Approximately(input.knockbackModifier, 1f))
			mods.Add(MultiplierStat(Constants.STAT_IMPACT_KNOCKBACK, input.knockbackModifier));
		output.modifiers = mods.ToArray();
		output.nightVision = input.nightVision;
		output.screenOverlay = input.overlay;
		List<string> immunities = new List<string>();
		if(input.smokeProtection)
			immunities.Add("Smoke");
		if(input.negateFallDamage)
			immunities.Add("FallDamage");
		output.immunities = immunities.ToArray();
	}

	private ResourceLocation[] GetTags(ArmourType input)
	{
		List<ResourceLocation> tags = new List<ResourceLocation>();
		tags.Add(new ResourceLocation("flansmod:armour"));
		return tags.ToArray();
	}

	public override void GetAdditionalOperations(ArmourType inf, List<AdditionalImportOperation> operations)
	{
		AddDefaultIconOperations(inf, operations);

		operations.Add(new CopyImportOperation($"/assets/flansmod/armor/{inf.armourTextureName}_1.png",
											   $"/textures/armor/{Minecraft.SanitiseID(inf.armourTextureName)}_1.png"));
		operations.Add(new CopyImportOperation($"/assets/flansmod/armor/{inf.armourTextureName}_2.png",
											   $"/textures/armor/{Minecraft.SanitiseID(inf.armourTextureName)}_2.png"));
	}
}

public class BoxConverter
{
	public static BoxConverter inst = new BoxConverter();
	protected void DoConversion(BoxType input, WorkbenchDefinition output)
	{

	}
	public void GetAdditionalOperations(BoxType inf, List<AdditionalImportOperation> operations)
	{
		operations.Add(new CubeModelOperation("",
											  $"models/block/{inf.ConvertedName}.prefab",
											  inf.ConvertedName,
											  inf.topTexturePath,
											  inf.sideTexturePath,
											  inf.bottomTexturePath));
	}
}


public class ArmourBoxConverter : Converter<ArmourBoxType, WorkbenchDefinition>
{
	public static ArmourBoxConverter inst = new ArmourBoxConverter();
	protected override void DoConversion(ArmourBoxType input, WorkbenchDefinition output)
	{
		output.armourCrafting.FECostPerCraft = 0;
		output.armourCrafting.isActive = true;
		List<ResourceLocation> itemIDs = new List<ResourceLocation>();
		for (int p = 0; p < input.pages.Count; p++)
		{
			ArmourBoxEntry page = input.pages[p];
			for (int n = 0; n < page.armours.Length; n++)
				itemIDs.Add(new ResourceLocation(page.armours[n]));
		}

		output.armourCrafting.craftableArmour = new ItemCollectionDefinition()
		{
			itemIDFilters = new LocationFilterDefinition[1] {
				new LocationFilterDefinition()
				{
					filterType = EFilterType.Allow,
					matchResourceLocations = itemIDs.ToArray()
				}
			}
		};

		// TODO: Also output the recipes
	}

	public override void GetAdditionalOperations(ArmourBoxType inf, List<AdditionalImportOperation> operations)
	{
		BoxConverter.inst.GetAdditionalOperations(inf, operations);
	}
}

public class GunBoxConverter : Converter<GunBoxType, WorkbenchDefinition>
{
	public static GunBoxConverter inst = new GunBoxConverter();
	protected override void DoConversion(GunBoxType input, WorkbenchDefinition output)
	{
		output.gunCrafting.FECostPerCraft = 0;
		output.gunCrafting.isActive = true;

		List<ResourceLocation> itemIDs = new List<ResourceLocation>();
		foreach(GunBoxPage page in input.pages)
			foreach(GunBoxEntryTopLevel topLevel in page.entries)
				foreach(GunBoxEntry entry in topLevel.childEntries)
					itemIDs.Add(new ResourceLocation(entry.type));

		output.gunCrafting.craftableGuns = new ItemCollectionDefinition()
		{
			itemIDFilters = new LocationFilterDefinition[1] {
				new LocationFilterDefinition() {
					filterType = EFilterType.Allow,
					matchResourceLocations = itemIDs.ToArray(),
				}
			}
		};

		// TODO: Export to .json recipes
	}

	public override void GetAdditionalOperations(GunBoxType inf, List<AdditionalImportOperation> operations)
	{
		BoxConverter.inst.GetAdditionalOperations(inf, operations);
	}
}

public class LoadoutConverter : Converter<PlayerClass, LoadoutDefinition>
{
	public static LoadoutConverter inst = new LoadoutConverter();
	protected override void DoConversion(PlayerClass input, LoadoutDefinition output)
	{
		output.hat = ImportStack(input.hat);
		output.chest = ImportStack(input.chest);
		output.legs = ImportStack(input.legs);
		output.shoes = ImportStack(input.shoes);
		output.playerSkinOverride = new ResourceLocation(input.playerSkinOverride);
		output.spawnOnEntity = input.horse ? "minecraft:horse" : "";
		output.startingItems = new ItemStackDefinition[input.startingItemStrings.Count];
		for(int i = 0; i < input.startingItemStrings.Count; i++)
		{
			output.startingItems[i] = ImportStack(input.startingItemStrings[i][0]);
		}
	}
	public override void GetAdditionalOperations(PlayerClass inf, List<AdditionalImportOperation> operations)
	{
	}
}

public class TeamConverter : Converter<Team, TeamDefinition>
{
	public static TeamConverter inst = new TeamConverter();
	protected override void DoConversion(Team input, TeamDefinition output)
	{
		output.loadouts = new ResourceLocation[input.classes.Count];
		for(int i = 0; i < input.classes.Count; i++)
		{
			output.loadouts[i] = new ResourceLocation(input.classes[i]);
		}
		output.flagColour = new ColourDefinition()
		{
			red = ((input.teamColour >> 16) & 0xff) / 255f,
			green = ((input.teamColour >> 8) & 0xff) / 255f,
			blue = (input.teamColour & 0xff) / 255f,
		};
		output.textColour = input.textColour;
		output.hat = ImportStack(input.hat);
		output.chest = ImportStack(input.chest);
		output.legs = ImportStack(input.legs);
		output.shoes = ImportStack(input.shoes);

		// Guess this wasn't localised before now
		output.LocalisedNames.Add(new Definition.LocalisedName()
		{
			Lang = Definition.ELang.en_us,
			Name = input.longName,
		});
	}
	public override void GetAdditionalOperations(Team inf, List<AdditionalImportOperation> operations)
	{
	}
}

public class ItemHolderConverter : Converter<ItemHolderType, WorkbenchDefinition>
{
	public static ItemHolderConverter inst = new ItemHolderConverter();
	protected override void DoConversion(ItemHolderType input, WorkbenchDefinition output)
	{
		output.itemHolding.allow = "";
		output.itemHolding.slots = new ItemHoldingSlotDefinition[] {
			new ItemHoldingSlotDefinition()
			{
				name = "display",
				stackSize = 1,
			}
		};
		output.itemHolding.maxStackSize = 1;
	}
	public override void GetAdditionalOperations(ItemHolderType inf, List<AdditionalImportOperation> operations)
	{
	}
}

public class RewardBoxConverter : Converter<RewardBox, RewardBoxDefinition>
{
	public static RewardBoxConverter inst = new RewardBoxConverter();
	protected override void DoConversion(RewardBox input, RewardBoxDefinition output)
	{
		output.commonChance = input.weightPerRarity[0];
		output.uncommonChance = input.weightPerRarity[1];
		output.rareChance = input.weightPerRarity[2];
		output.legendaryChance = input.weightPerRarity[3];
		output.paintjobs = new PaintjobUnlockDefinition[input.paintjobs.Count];
		for(int i = 0; i < input.paintjobs.Count; i++)
		{
			output.paintjobs[i] = new PaintjobUnlockDefinition()
			{
				forItem = input.paintjobs[i].Split('_')[0],
				name = input.paintjobs[i],
			};
		}
	}
	public override void GetAdditionalOperations(RewardBox inf, List<AdditionalImportOperation> operations)
	{
	}
}

public class LoadoutPoolConverter : Converter<LoadoutPool, LoadoutPoolDefinition>
{
	public static LoadoutPoolConverter inst = new LoadoutPoolConverter();
	protected override void DoConversion(LoadoutPool input, LoadoutPoolDefinition output)
	{
		output.maxLevel = input.maxLevel;
		output.xpForKill = input.XPForKill;
		output.xpForDeath = input.XPForDeath;
		output.xpForKillstreakBonus = input.XPForKillstreakBonus;
		//output.defaultLoadouts = new LoadoutDefinition[input.defaultLoadouts.Length];
		//for(int i = 0; i < input.defaultLoadouts.Length; i++)
		//{
		//	output.defaultLoadouts[i] = new LoadoutDefinition()
		//	{
		//		slots = input.defaultLoadouts[i].slots,	
		//	};
		//}
		output.levelUps = new LevelUpDefinition[input.maxLevel];
		for(int i = 0; i < input.maxLevel; i++)
		{
			List<ItemUnlockDefinition> unlocks = new List<ItemUnlockDefinition>();
			foreach(string reward in input.rewardsPerLevel[i])
			{
				unlocks.Add(new ItemUnlockDefinition()
				{
					name = reward,
				});
			}
			int unlockSlot = -1;
			for(int s = 0; s < 5; s++)
			{
				if(input.slotUnlockLevels[s] == i)
					unlockSlot = s;
			}

			output.levelUps[i] = new LevelUpDefinition()
			{
				xpToLevel = input.XPPerLevel[i],
				paintjobs = new PaintjobUnlockDefinition[0],
				items = unlocks.ToArray(),
				unlockSlot = unlockSlot,
			};
		}
		output.availableRewardBoxes = input.rewardBoxes;
	}
	public override void GetAdditionalOperations(LoadoutPool inf, List<AdditionalImportOperation> operations)
	{
	}
}










