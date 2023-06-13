using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InfoToDefinitionConverter
{
    public static void Convert(EDefinitionType type, InfoType input, Definition output)
	{
		switch(type)
		{
			case EDefinitionType.part: 			PartConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.bullet: 		BulletConverter.inst.CastConvert(input, output); break;
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
			case EDefinitionType.playerClass: 	PlayerClassConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.team: 			TeamConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.itemHolder: 	ItemHolderConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.rewardBox: 	RewardBoxConverter.inst.CastConvert(input, output); break;
			case EDefinitionType.loadout: 		LoadoutConverter.inst.CastConvert(input, output); break;
		}
	}
}

public abstract class Converter<TInfo, TDefinition> 
	where TInfo : InfoType 
	where TDefinition : Definition
{
	public void CastConvert(InfoType input, Definition output)
	{
		DoConversion((TInfo)input, (TDefinition)output);
	}
	public void Convert(TInfo input, TDefinition output)
	{
		DoConversion(input, output);
	}
	public IngredientDefinition ImportIngredient(string txt)
	{
		return new IngredientDefinition()
		{
			itemName = txt,
		};
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
			item = typeName,
		};
	}

	protected abstract void DoConversion(TInfo input, TDefinition output);
}

public class GunConverter : Converter<GunType, GunDefinition>
{
	public static GunConverter inst = new GunConverter();

	protected override void DoConversion(GunType inf, GunDefinition def)
	{
		def.reload = new ReloadDefinition()
		{
			manualReloadAllowed = inf.canForceReload,
			start = new ReloadStageDefinition()
			{
				duration = inf.reloadTime * inf.tiltGunTime,
				sound = new SoundDefinition() 
				{
					sound = inf.reloadSound,
				},
				actions = CreateStartReloadActions(inf),
			},
			eject = new ReloadStageDefinition()
			{
				duration = inf.reloadTime * inf.unloadClipTime,
				sound = new SoundDefinition(),
			},
			loadOne = new ReloadStageDefinition()
			{
				duration = inf.reloadTime * inf.loadClipTime,
				sound = new SoundDefinition(),
				actions = CreateLoadOneReloadActions(inf),
			},
			end = new ReloadStageDefinition()
			{
				duration = inf.reloadTime * inf.untiltGunTime,
				sound = new SoundDefinition(),
				actions = CreateEndReloadActions(inf),
			},
		};

		def.primaryActions = CreatePrimaryActions(inf);
		def.secondaryActions = CreateSecondaryActions(inf);

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
			attachToMesh = "body",
			attachPoint = inf.barrelAttachPoint,
			allowAll = inf.allowAllAttachments,
			allowlist = inf.allowedAttachments.ToArray(),
		};
		def.gripAttachments = new AttachmentSettingsDefinition()
		{
			numAttachmentSlots = inf.allowGripAttachments ? 1 : 0,
			attachToMesh = inf.gripIsOnPump ? "pump" : "body",
			attachPoint = inf.gripAttachPoint,
			allowAll = inf.allowAllAttachments,
			allowlist = inf.allowedAttachments.ToArray(),
		};
		def.stockAttachments = new AttachmentSettingsDefinition()
		{
			numAttachmentSlots = inf.allowStockAttachments ? 1 : 0,
			attachToMesh = "body",
			attachPoint = inf.stockAttachPoint,
			allowAll = inf.allowAllAttachments,
			allowlist = inf.allowedAttachments.ToArray(),
		};
		def.scopeAttachments = new AttachmentSettingsDefinition()
		{
			numAttachmentSlots = inf.allowScopeAttachments ? 1 : 0,
			attachToMesh = inf.scopeIsOnBreakAction ? "breakAction" : (inf.scopeIsOnSlide ? "slide" : "body"),
			attachPoint = inf.scopeAttachPoint,
			allowAll = inf.allowAllAttachments,
			allowlist = inf.allowedAttachments.ToArray(),
		};
		//def.genericAttachments
		//def.userMoveSpeedModifier = inf.moveSpeedModifier;
		//def.userKnockbackResist = inf.knockbackModifier;



	}

	private ActionDefinition[] CreateStartReloadActions(GunType inf)
	{
		List<ActionDefinition> reloadActions = new List<ActionDefinition>();
		reloadActions.Add(new ActionDefinition() 
		{ 
			actionType = EActionType.Animation, 
			anim = "TiltGunForReload",
			//remainUntilEnd = true,
		});
		switch(inf.animationType)
		{
			case EAnimationType.BREAK_ACTION:
			{
				reloadActions.Add(new ActionDefinition() 
				{ 
					actionType = EActionType.Animation, 
					anim = "BreakActionOpen",
					//angle = inf.breakAngle,
					//remainUntilEnd = true,
				});
				break;
			}
			case EAnimationType.REVOLVER:
			{
				reloadActions.Add(new ActionDefinition() 
				{ 
					actionType = EActionType.Animation, 
					anim = "RevolverOpen",
					//angle = inf.revolverFlipAngle,
					//revolverPivotLocation = revolverFlipPoint;
					//remainUntilEnd = true,
				});
				break;
			}
		}
		return reloadActions.ToArray();
	}

	private ActionDefinition[] CreateLoadOneReloadActions(GunType inf)
	{
		List<ActionDefinition> reloadActions = new List<ActionDefinition>();
		switch(inf.animationType)
		{
			case EAnimationType.END_LOADED:
			{
				reloadActions.Add(new ActionDefinition() 
				{ 
					actionType = EActionType.Animation, 
					anim = "LoadRocketLauncher",
					//distance = inf.endLoadedAmmoDistance,
				});
				break;
			}
		}
		return reloadActions.ToArray();
	}

	private ActionDefinition[] CreateEndReloadActions(GunType inf)
	{
		List<ActionDefinition> reloadActions = new List<ActionDefinition>();
		switch(inf.animationType)
		{
			case EAnimationType.BREAK_ACTION:
			{
				reloadActions.Add(new ActionDefinition() 
				{ 
					actionType = EActionType.Animation, 
					anim = "BreakActionClose",
					//angle = inf.breakAngle,
					//remainUntilEnd = true,
				});
				break;
			}
			case EAnimationType.REVOLVER:
			{
				reloadActions.Add(new ActionDefinition() 
				{ 
					actionType = EActionType.Animation, 
					anim = "RevolverClose",
					//angle = inf.revolverFlipAngle,
					//revolverPivotLocation = revolverFlipPoint;
					//remainUntilEnd = true,
				});
				break;
			}
		}

		if(inf.pumpTime > 0.0f)
		{
			reloadActions.Add(new ActionDefinition() 
			{ 
				actionType = EActionType.Animation, 
				anim = "PumpAction",
				duration = inf.pumpTime,
				//delay = inf.pumpDelayAfterReload,
				//distance = pumpHandleDistance,
			});
		}
		if(inf.spinningCocking)
		{
			reloadActions.Add(new ActionDefinition() 
			{ 
				actionType = EActionType.Animation, 
				anim = "SpinBackwards",
				duration = 0.5f,
				//delay = 0.0f,
				//spinLocation = inf.spinPoint,
			});
		}

		return reloadActions.ToArray();
	}

	
	private ActionDefinition[] CreatePrimaryActions(GunType inf)
	{
		List<ActionDefinition> primaryActions = new List<ActionDefinition>();

		if(inf.shootDelay > 0.0f)
		{
			primaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Shoot,
				canActUnderwater = inf.canShootUnderwater,
				canActUnderOtherLiquid = false,
				FireMode = inf.mode,

				sound = new SoundDefinition()
				{
					sound = inf.shootSound,
					length = inf.shootSoundLength,
					minPitchMultiplier = inf.distortSound ? 1.0f / 1.2f : 1.0f,
					maxPitchMultiplier = inf.distortSound ? 1.0f / 0.8f : 1.0f,
				},

				// burstShotCount
				// minigunWarmupDuration
				// warmup
				// loop
				// cooldown

				// numMagazineSlots = inf.numAmmoItemsInGun,
				// ammoWithTags = ,
				// ammoTypes = ,
				shootStats = CreateShotDefinition(inf),
			});
		}
		else if(inf.meleeDamage > 0.0f)
		{
			primaryActions.Add(new ActionDefinition()
			{
				actionType = EActionType.Melee,
				//damage = inf.meleeDamage,
				sound = new SoundDefinition() 
				{
					sound = inf.meleeSound,
				},
			});
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
			anim = "Shoot",
			duration = inf.shootDelay,
		});

		// gunSlide

		// pumpHandle



		return primaryActions.ToArray();
	}

	public ShotDefinition CreateShotDefinition(GunType type)
	{
		return new ShotDefinition()
		{
			verticalReocil = type.recoil,
			horizontalRecoil = 0.0f,
			spread = type.bulletSpread,
			hitscan = type.bulletSpeed <= 0.0f,
			speed = type.bulletSpeed,
			count = type.numBullets,
			timeToNextShot = type.shootDelay,
			impact = new ImpactDefinition()
			{
				damageToTarget = type.damage,
				multiplierVsPlayers = 1.0f,
				multiplierVsVehicles = 1.0f,
				splashDamageRadius = 0.0f,
				splashDamageFalloff = 0.0f,
				setFireToTarget = 0.0f,
				fireSpreadAmount = 0.0f,
				fireSpreadRadius = 0.0f,
				knockback = type.knockback,
				decal = "flansmod:effects/bullet_decal.png",
			},
		};
	}

	private ActionDefinition[] CreateSecondaryActions(GunType inf)
	{
		List<ActionDefinition> secondaryActions = new List<ActionDefinition>();

		if(inf.shield)
		{
			secondaryActions.Add(new ActionDefinition()
			{
				canBeOverriden = true,
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
				canBeOverriden = true,
				actionType = EActionType.Scope,
				scopeOverlay = inf.defaultScopeTexture,
				//zoomFactor = inf.zoomLevel,
				fovFactor = Mathf.Approximately(inf.FOVFactor, 1.0f) ? inf.zoomLevel : inf.FOVFactor,
			});
		}
		else if(inf.FOVFactor > 1.0f)
		{	
			secondaryActions.Add(new ActionDefinition()
			{
				canBeOverriden = true,
				actionType = EActionType.AimDownSights,
				fovFactor = inf.FOVFactor,
			});
		}


		return secondaryActions.ToArray();
	}
}

public class PartConverter : Converter<PartType, PartDefinition>
{
	public static PartConverter inst = new PartConverter();
	protected override void DoConversion(PartType input, PartDefinition output)
	{
		output.maxStackSize = input.stackSize;
		output.triggersApocalypse = input.isAIChip;
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
			fuelConsumptionRate = input.fuelConsumption,
			solidFuelSlots = input.useRFPower ? 0 : 1,
			liquidFuelCapacity = 0,
			batterySlots = input.useRFPower ? 1 : 0,
			FECapacity = input.useRFPower ? 1000000 : 0,
		};
	}
}

public class BulletConverter : Converter<BulletType, BulletDefinition>
{
	public static BulletConverter inst = new BulletConverter();
	protected override void DoConversion(BulletType input, BulletDefinition output)
	{
		output.gravityFactor = input.fallSpeed;
		output.maxStackSize = input.maxStackSize;
		output.roundsPerItem = input.roundsPerItem;

		List<ActionDefinition> onShoot = new List<ActionDefinition>();
		if(input.dropItemOnShoot != null && input.dropItemOnShoot.Length > 0)
		{
			onShoot.Add(new ActionDefinition()
			{
				actionType = EActionType.Drop,
				itemStack = input.dropItemOnShoot,
			});
		}
		output.onShootActions = onShoot.ToArray();

		List<ActionDefinition> onReload = new List<ActionDefinition>();
		if(input.dropItemOnReload != null && input.dropItemOnReload.Length > 0)
		{
			onReload.Add(new ActionDefinition()
			{
				actionType = EActionType.Drop,
				itemStack = input.dropItemOnReload,
			});
		}
		output.onReloadActions = onReload.ToArray();


		output.shootStats = new ShotDefinition()
		{
			// All N/A? Set by gun?
			verticalReocil = 0f,
			horizontalRecoil = 0f,
			hitscan = true,
			speed = 0f,
			timeToNextShot = 0f,
			spreadPattern = ESpreadPattern.Circle,

			spread = input.bulletSpread,
			count = input.numBullets,
			breaksMaterials = input.breaksGlass ? new string[] { "glass" } : new string[0],
			penetrationPower = input.penetratingPower,
			trailParticles = input.trailParticles ? input.trailParticleType : "",

			impact = new ImpactDefinition()
			{
				decal = "flansmod:effects/bullet_decal.png",
				damageToTarget = input.damageVsLiving,
				multiplierVsPlayers = 1f,
				multiplierVsVehicles = input.damageVsDriveable / input.damageVsLiving,
				knockback = 0f, // Not present?
				splashDamageRadius = 0f,
				splashDamageFalloff = 0f,
				setFireToTarget = input.setEntitiesOnFire ? 1f : 0f,
				fireSpreadRadius = input.fireRadius,
				fireSpreadAmount = 0.5f,
				hitSound = new SoundDefinition()
				{
					sound = input.hitSound,
					maxRange = input.hitSoundRange
				}
			}
		};
	}
}

public class AttachmentConverter : Converter<AttachmentType, AttachmentDefinition>
{
	public static AttachmentConverter inst = new AttachmentConverter();
	protected override void DoConversion(AttachmentType input, AttachmentDefinition output)
	{
		output.attachmentType = input.type;
		List<ModifierDefinition> mods = new List<ModifierDefinition>();

		if(input.silencer)
		{
			mods.Add(new ModifierDefinition()
			{
				Stat = "soundVolume",
				Multiply = 0.1f,
			});
			mods.Add(new ModifierDefinition()
			{
				Stat = "soundPitch",
				Multiply = 1.5f,
			});
		}

		if(input.flashlight)
		{
			mods.Add(new ModifierDefinition()
			{
				Stat = "flashlightStrength",
				Add = input.flashlightStrength,
			});
			mods.Add(new ModifierDefinition()
			{
				Stat = "flashlightRange",
				Add = input.flashlightRange,
			});
		}


		if(input.spreadMultiplier != 1f)
			mods.Add(new ModifierDefinition()
			{
				Stat = "spread",
				Multiply = input.spreadMultiplier,
			});
		if(input.recoilMultiplier != 1f)
			mods.Add(new ModifierDefinition()
			{
				Stat = "recoil",
				Multiply = input.recoilMultiplier,
			});
		if(input.damageMultiplier != 1f)
			mods.Add(new ModifierDefinition()
			{
				Stat = "damage",
				Multiply = input.damageMultiplier,
			});
		if(input.meleeDamageMultiplier != 1f)
			mods.Add(new ModifierDefinition()
			{
				Stat = "melee",
				Multiply = input.meleeDamageMultiplier,
			});
		if(input.bulletSpeedMultiplier != 1f)
			mods.Add(new ModifierDefinition()
			{
				Stat = "bulletSpeed",
				Multiply = input.bulletSpeedMultiplier,
			});
		if(input.reloadTimeMultiplier != 1f)
			mods.Add(new ModifierDefinition()
			{
				Stat = "reloadTime",
				Multiply = input.reloadTimeMultiplier,
			});


		if(input.type == EAttachmentType.Sights)
		{
			if(input.hasScopeOverlay)
			{
				output.secondaryActions = new ActionDefinition[] {
					new ActionDefinition()
					{
						actionType = EActionType.Scope,
						scopeOverlay = input.zoomOverlay,
						// Legacy, people much prefer FOV zoom to regular ZOOM
						fovFactor = Mathf.Approximately(input.FOVZoomLevel, 1.0f) ? input.zoomLevel : input.FOVZoomLevel, 
					}
				};
			}
			else
			{
				output.secondaryActions = new ActionDefinition[] {
					new ActionDefinition()
					{
						actionType = EActionType.AimDownSights,
						scopeOverlay = input.zoomOverlay,
						fovFactor = input.FOVZoomLevel,
					}
				};
			}
		}
		
		output.modifiers = mods.ToArray();
	}
}

public class GrenadeConverter : Converter<GrenadeType, GrenadeDefinition>
{
	public static GrenadeConverter inst = new GrenadeConverter();
	protected override void DoConversion(GrenadeType input, GrenadeDefinition output)
	{
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
		output.impact = new ImpactDefinition()
		{
			fireSpreadAmount = input.fireRadius,
			fireSpreadRadius = input.fireRadius,
			setFireToTarget = input.fireRadius,
			damageToTarget = input.damageVsLiving,
			splashDamageRadius = input.explosionRadius,
			splashDamageFalloff = input.explosionDamageVsLiving,
			multiplierVsPlayers = 1.0f,
			multiplierVsVehicles =  input.explosionDamageVsLiving <= 0.01f ? 0.0f : input.explosionDamageVsDriveable / input.explosionDamageVsLiving,
			hitSound = new SoundDefinition(),
			knockback = 0f,
			decal = "",
		};
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
				sound = new SoundDefinition()
				{
					sound = inf.throwSound,
					length = 1,
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
			sound = new SoundDefinition(),
		});
		secondaryActions.Add(new ActionDefinition() 
		{ 
			//actionType = EActionType.CookGrenade,

		});
		return secondaryActions.ToArray();
	}
}

public class DriveableConverter : Converter<DriveableType, VehicleDefinition>
{
	public static DriveableConverter inst = new DriveableConverter();
	protected override void DoConversion(DriveableType input, VehicleDefinition output)
	{
		if(input.seats != null)
		{
			output.seats = new SeatDefinition[input.seats.Length];
			for(int i = 0; i < input.seats.Length; i++)
			{
				if(input.seats[i] == null)
				{
					Debug.LogWarning($"Imported seat {i} in {input.shortName} was null");
					continue;
				}

				List<InputDefinition> inputDefs = new List<InputDefinition>();
				if(input.seats[i].gunType != null && input.seats[i].gunType.Length > 0)
				{
					inputDefs.Add(new InputDefinition()
					{
						key = EPlayerInput.Fire1,
						guns = new MountedGunInputDefinition[] {
							new MountedGunInputDefinition()
							{
								gunName = $"seat_{i}_gun",
								toggle = false,
							}
						}
					});
				}

				output.seats[i] = new SeatDefinition()
				{
					attachedTo = input.seats[i].part,
					offsetFromAttachPoint = input.seats[i].rotatedOffset,
					minYaw = input.seats[i].minYaw,
					maxYaw = input.seats[i].maxYaw,
					minPitch = input.seats[i].minPitch,
					maxPitch = input.seats[i].maxPitch,
					gyroStabilised = false,
					inputs = inputDefs.ToArray(),
				};
			}
		}

		List<MountedGunDefinition> mountedGuns = new List<MountedGunDefinition>();
		mountedGuns.AddRange(DriveableConverter.inst.CreatePassengerMountedGunDefs(input));
		for(int i = 0; i < input.shootPointsPrimary.Count; i++)
		{
			ShootPoint point = input.shootPointsPrimary[i];
			mountedGuns.Add(new MountedGunDefinition()
			{
				name = $"pilot_gun_primary_{i}",
				primaryActions = CreateGunActions(input, input.primary, input.alternatePrimary,
					input.shootDelayPrimary, input.modePrimary, input.damageModifierPrimary, point, 
					input.primary == EWeaponType.GUN ? input.pilotGuns[i] : null),
				attachedTo = "body", // Tanks should be on the turret
				shootPointOffset = point.offPos,
				minYaw = 0f,
				maxYaw = 0f,
				minPitch = 0f,
				maxPitch = 0f,
				aimingSpeed = 0f,
			});
		}
		for(int i = 0; i < input.shootPointsSecondary.Count; i++)
		{
			ShootPoint point = input.shootPointsSecondary[i];
			mountedGuns.Add(new MountedGunDefinition()
			{
				name = $"pilot_gun_secondary_{i}",
				primaryActions = CreateGunActions(input, input.secondary, input.alternateSecondary,
					input.shootDelaySecondary, input.modeSecondary, input.damageModifierSecondary, point, 
					input.secondary == EWeaponType.GUN ? input.pilotGuns[i] : null),
				attachedTo = "body", // Tanks should be on the turret
				shootPointOffset = point.offPos,
				minYaw = 0f,
				maxYaw = 0f,
				minPitch = 0f,
				maxPitch = 0f,
				aimingSpeed = 0f,
			});
		}
		output.guns = mountedGuns.ToArray();
	
	}

	public List<MountedGunDefinition> CreatePassengerMountedGunDefs(DriveableType input)
	{
		List<MountedGunDefinition> gunDefs = new List<MountedGunDefinition>();

		if(input.seats != null)
		{
			for(int i = 0; i < input.seats.Length; i++)
			{
				if(input.seats[i] == null)
				{
					Debug.LogWarning($"Imported seat {i} in {input.shortName} was null");
					continue;
				}

				if(input.seats[i].gunType != null && input.seats[i].gunType.Length > 0)
				{
					gunDefs.Add(new MountedGunDefinition()
					{
						name = $"seat_{i}_gun",
						attachedTo = input.seats[i].part,
						recoil = Vector3.zero,
						shootPointOffset = input.seats[i].rotatedOffset,
						minYaw = input.seats[i].minYaw,
						minPitch = input.seats[i].minPitch,
						maxYaw = input.seats[i].maxYaw,
						maxPitch = input.seats[i].maxPitch,
						aimingSpeed = input.seats[i].aimingSpeed.x,
						lockSeatToGunAngles = false,
						traveseIndependently = input.seats[i].yawBeforePitch,
						yawSound = new SoundDefinition()
						{ 
							sound = input.seats[i].yawSound,
							length = input.seats[i].yawSoundLength
						},
						pitchSound = new SoundDefinition()
						{ 
							sound = input.seats[i].pitchSound,
							length = input.seats[i].pitchSoundLength,
						},
						primaryActions = CreateGunActions(input.seats[i]),
					});
				}
			}
		}

		return gunDefs;
	}

	private ActionDefinition[] CreateGunActions(DriveableType input, EWeaponType weaponType, bool alternate,
		int shootDelay, EFireMode fireMode, int damageModifier, ShootPoint shootPoint, PilotGun gun)
	{
		List<ActionDefinition> actions = new List<ActionDefinition>();

		switch(weaponType)
		{
			// This is a reference to a GunType and its corresponding stats
			// We are going to in-line them for simplicity
			case EWeaponType.GUN:
			{
				if(gun.type != null && gun.type.Length > 0 && DefinitionImporter.TryGetType<GunType>(EDefinitionType.gun, gun.type, out GunType gunType))
				{
					actions.Add(new ActionDefinition()
					{
						actionType = EActionType.Shoot,
						canActUnderwater = false,
						canActUnderOtherLiquid = false,
						FireMode = gunType.mode,
						sound = new SoundDefinition()
						{
							sound = gunType.shootSound,
							length = gunType.shootSoundLength,
							minPitchMultiplier = gunType.distortSound ? 1.0f / 1.2f : 1.0f,
							maxPitchMultiplier = gunType.distortSound ? 1.0f / 0.8f : 1.0f,
						},
						shootStats = GunConverter.inst.CreateShotDefinition(gunType),
					});
				}
				else Debug.LogError($"Could not find gun {gun.type}");
				
				break;
			}
	
			// These weapon types are all "custom" made, and just use a BulletType for their stat block
			case EWeaponType.SHELL:
			case EWeaponType.MINE:
			case EWeaponType.MISSILE:
			case EWeaponType.BOMB:
			{
				actions.Add(new ActionDefinition()
				{
					actionType = EActionType.Shoot,
					//shootStats = BulletConverter.
				});
				actions.Add(new ActionDefinition()
				{
					actionType = EActionType.Animation,
					anim = "FireMissile",
				});
				break;
			}
		}

		return actions.ToArray();
	}

	private ActionDefinition[] CreateGunActions(Seat seat)
	{
		List<ActionDefinition> primaryActions = new List<ActionDefinition>();

		if(seat.gunType != null && seat.gunType.Length > 0)
		{
			if(DefinitionImporter.TryGetType<GunType>(EDefinitionType.gun, seat.gunType, out GunType gunType))
			{
				primaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Shoot,
					canActUnderwater = false,
					canActUnderOtherLiquid = false,
					FireMode = gunType.mode,
					sound = new SoundDefinition()
					{
						sound = gunType.shootSound,
						length = gunType.shootSoundLength,
						minPitchMultiplier = gunType.distortSound ? 1.0f / 1.2f : 1.0f,
						maxPitchMultiplier = gunType.distortSound ? 1.0f / 0.8f : 1.0f,
					},
					shootStats = GunConverter.inst.CreateShotDefinition(gunType),
				});
			}
		}

		return primaryActions.ToArray();
	}

	public WheelDefinition[] CreateWheelDefs(DriveableType input)
	{
		WheelDefinition[] wheelDefs = new WheelDefinition[input.wheelPositions.Length];

		for(int i = 0; i < input.wheelPositions.Length; i++)
		{
			wheelDefs[i] = new WheelDefinition()
			{
				attachedTo = input.wheelPositions[i].part,
				physicsOffset = input.wheelPositions[i].position,
				visualOffset = input.wheelPositions[i].position,
				springStrength = input.wheelSpringStrength,
				stepHeight = input.wheelStepHeight,
			};
		}

		return wheelDefs;
	}
}

public class AAGunConverter : Converter<AAGunType, VehicleDefinition>
{
	public static AAGunConverter inst = new AAGunConverter();
	protected override void DoConversion(AAGunType input, VehicleDefinition output)
	{
		output.articulatedParts = new ArticulatedPartDefinition[]
		{
			new ArticulatedPartDefinition()
			{
				partName = "yawPlatform",
			},
			new ArticulatedPartDefinition()
			{
				partName = "barrelPivotPoint",
			}
		};
		List<MountedGunDefinition> guns = new List<MountedGunDefinition>();
		for(int i = 0; i < input.numBarrels; i++)
		{
			guns.Add(new MountedGunDefinition()
			{
				name = $"aa_gun_{i}",
				primaryActions = new ActionDefinition[]
				{
					new ActionDefinition()
					{
						actionType = EActionType.Shoot,
						shootStats = new ShotDefinition()
						{
							// TODO
						}
					}
				},
				recoil = new Vector3(input.recoil, 0f, 0f),
				attachedTo = "barrelPivotPoint",
				shootPointOffset = new Vector3(input.barrelX[i], input.barrelY[i], input.barrelZ[i]),
				minYaw = -360f,
				maxYaw = 360f,
				minPitch = input.bottomViewLimit,
				maxPitch = input.topViewLimit,
				aimingSpeed = 1.0f,
				lockSeatToGunAngles = false,
				traveseIndependently = true,
			});
		}
		output.guns = guns.ToArray();
		output.seats = new SeatDefinition[] 
		{
			new SeatDefinition()
			{
				attachedTo = "yawPlatform",
				offsetFromAttachPoint = new Vector3(input.gunnerX, input.gunnerY, input.gunnerZ) / 16f,
				minYaw = -360f,
				maxYaw = 360f,
				minPitch = input.bottomViewLimit,
				maxPitch = input.topViewLimit,
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
}

public class VehicleConverter : Converter<VehicleType, VehicleDefinition>
{
	public static VehicleConverter inst = new VehicleConverter();
	protected override void DoConversion(VehicleType input, VehicleDefinition output)
	{
		DriveableConverter.inst.Convert(input, output);

		if(output.seats.Length > 0)
		{
			List<InputDefinition> driverInputs = new List<InputDefinition>();
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.MoveForward,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.Accelerate,
						force = input.maxThrottle, // TODO: What is this?
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.MoveBackward,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.Decelerate,
						force = input.maxNegativeThrottle, // TODO: What is this?
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.YawLeft,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.YawLeft,
						force = input.turnLeftModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.YawRight,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.YawRight,
						force = input.turnRightModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.Jump,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.Handbrake,
						force = 1.0f,
						toggle = false,
					}
				}
			});

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

			output.seats[0].inputs = driverInputs.ToArray();
		}

		output.movementModes = new VehicleMovementDefinition[]
		{
			input.tank ? 
			new VehicleMovementDefinition()
			{
				name = "tank",
				type = EVehicleMovementType.Tank,
				wheels = DriveableConverter.inst.CreateWheelDefs(input),
			}
			:
			new VehicleMovementDefinition()
			{
				name = "car",
				type = EVehicleMovementType.Car,
				wheels = DriveableConverter.inst.CreateWheelDefs(input),
			}
		};

		
	}
}

public class PlaneConverter : Converter<PlaneType, VehicleDefinition>
{
	public static PlaneConverter inst = new PlaneConverter();
	protected override void DoConversion(PlaneType input, VehicleDefinition output)
	{
		DriveableConverter.inst.Convert(input, output);

		if(output.seats.Length > 0)
		{
			List<InputDefinition> driverInputs = new List<InputDefinition>();
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.MoveForward,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.Accelerate,
						force = input.maxThrottle, // TODO: What is this?
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.MoveBackward,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.Decelerate,
						force = input.maxNegativeThrottle, // TODO: What is this?
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.MoveLeft,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.YawLeft,
						force = input.turnLeftModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.MoveRight,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.YawRight,
						force = input.turnRightModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.Jump,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.Handbrake,
						force = 1.0f,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.RollLeft,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.RollLeft,
						force = input.rollLeftModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.RollRight,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.RollRight,
						force = input.rollRightModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.PitchUp,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.PitchUp,
						force = input.lookUpModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.PitchDown,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.PitchDown,
						force = input.lookDownModifier,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.GearUp,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.GearUp,
						force = 1.0f,
						toggle = false,
					}
				}
			});
			driverInputs.Add(new InputDefinition()
			{
				key = EPlayerInput.GearDown,
				driving = new DrivingInputDefinition[] {
					new DrivingInputDefinition()
					{
						control = EDrivingControl.GearDown,
						force = 1.0f,
						toggle = false,
					}
				}
			});

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

			output.seats[0].inputs = driverInputs.ToArray();
		}



		List<VehicleMovementDefinition> moveModes = new List<VehicleMovementDefinition>();
		if(input.mode == EPlaneMode.PLANE || input.mode == EPlaneMode.VTOL)
		{
			moveModes.Add(new VehicleMovementDefinition()
			{
				name = "plane",
				type = EVehicleMovementType.Plane,
				wheels = DriveableConverter.inst.CreateWheelDefs(input),
				propellers = CreatePropellers(input, input.propellers),
			});
		}
		if(input.mode == EPlaneMode.HELI || input.mode == EPlaneMode.VTOL)
		{
			PropellerDefinition[] heliProps = CreatePropellers(input, input.heliPropellers);
			PropellerDefinition[] tailProps = CreatePropellers(input, input.heliTailPropellers);
			PropellerDefinition[] both = new PropellerDefinition[heliProps.Length + tailProps.Length];
			for(int i = 0; i < heliProps.Length; i++)
				both[i] = heliProps[i];
			for(int j = 0; j < tailProps.Length; j++)
				both[heliProps.Length + j] = tailProps[j];

			moveModes.Add(new VehicleMovementDefinition()
			{
				name = "helicopter",
				type = EVehicleMovementType.Helicopter,
				wheels = DriveableConverter.inst.CreateWheelDefs(input),
				propellers = both,
			});
		}
		output.movementModes = moveModes.ToArray();
		
	}

	private PropellerDefinition[] CreatePropellers(PlaneType input, List<Propeller> props)
	{
		PropellerDefinition[] propDefs = new PropellerDefinition[props.Count];
		for(int i = 0; i < props.Count; i++)
		{
			propDefs[i] = new PropellerDefinition()
			{
				attachedTo = props[i].planePart,
				visualOffset = props[i].getPosition(),
				forceOffset = Vector3.zero,
			};
		}
		return propDefs;
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
		switch(input.function)
		{
			case EMechaToolType.pickaxe:
			{
				primaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Pickaxe,
					toolLevel = input.toolHardness,
					harvestSpeed = input.speed,
					reach = input.reach,
				});
				break;
			}
			case EMechaToolType.axe:
			{
				primaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Axe,
					toolLevel = input.toolHardness,
					harvestSpeed = input.speed,
					reach = input.reach,
				});
				secondaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Strip,
					toolLevel = input.toolHardness,
					harvestSpeed = input.speed,
					reach = input.reach,
				});
				break;
			}
			case EMechaToolType.shovel:
			{
				primaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Shovel,
					toolLevel = input.toolHardness,
					harvestSpeed = input.speed,
					reach = input.reach,
				});
				secondaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Flatten,
					toolLevel = input.toolHardness,
					harvestSpeed = input.speed,
					reach = input.reach,
				});
				break;
			}
			case EMechaToolType.shears:
			{
				primaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Shear,
					toolLevel = input.toolHardness,
					harvestSpeed = input.speed,
					reach = input.reach,
				});
				break;
			}
			case EMechaToolType.sword:
			{
				primaryActions.Add(new ActionDefinition()
				{
					actionType = EActionType.Melee,
					toolLevel = input.toolHardness,
					harvestSpeed = input.speed,
					reach = input.reach,
				});
				break;
			}
		}

		output.primaryActions = primaryActions.ToArray();
		output.secondaryActions = secondaryActions.ToArray();

		List<ModifierDefinition> mods = new List<ModifierDefinition>();
		if(!Mathf.Approximately(input.speedMultiplier, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "movementSpeed",
				Multiply = input.speedMultiplier,
			});
		if(!Mathf.Approximately(input.damageResistance, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "damageResistance",
				Add = input.damageResistance,
			});
		if(!Mathf.Approximately(input.damageResistance, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "damageResistance",
				Add = input.damageResistance,
			});
		if(input.lightLevel != 0)
		{
			mods.Add(new ModifierDefinition()
			{
				Stat = "flashlightStrength",
				Add = input.lightLevel,
			});
			mods.Add(new ModifierDefinition()
			{
				Stat = "flashlightRange",
				Add = input.lightLevel,
			});
		}
		if(input.stopMechaFallDamage)
			mods.Add(new ModifierDefinition()
			{
				Stat = "fallDamage",
				Multiply = 0f,
			});
		if(input.forceBlockFallDamage)
			mods.Add(new ModifierDefinition()
			{
				Stat = "explodeOnFallDamage",
				Add = 1f,
			});

		if(!Mathf.Approximately(input.fortuneCoal, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "fortune",
				Filter = "minecraft:coal_ore_block",
				Multiply = input.fortuneCoal,
			});
		if(!Mathf.Approximately(input.fortuneIron, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "fortune",
				Filter = "minecraft:iron_ore_block",
				Multiply = input.fortuneIron,
			});
		if(!Mathf.Approximately(input.fortuneEmerald, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "fortune",
				Filter = "minecraft:emerald_ore_block",
				Multiply = input.fortuneEmerald,
			});
		if(!Mathf.Approximately(input.fortuneRedstone, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "fortune",
				Filter = "minecraft:redstone_ore_block",
				Multiply = input.fortuneRedstone,
			});
		if(!Mathf.Approximately(input.fortuneDiamond, 1f))
			mods.Add(new ModifierDefinition()
			{
				Stat = "fortune",
				Filter = "minecraft:diamond_ore_block",
				Multiply = input.fortuneDiamond,
			});
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
}

public class MechaConverter : Converter<MechaType, VehicleDefinition>
{
	public static MechaConverter inst = new MechaConverter();
	protected override void DoConversion(MechaType input, VehicleDefinition output)
	{
		DriveableConverter.inst.Convert(input, output);

		output.movementModes = new VehicleMovementDefinition[]
		{
			new VehicleMovementDefinition()
			{
				type = EVehicleMovementType.Mecha,
				legs = new LegsDefinition[] 
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
				},
				arms = new ArmDefinition[]
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
				}
				
				
			}
		};
	}
}

public class ToolConverter : Converter<ToolType, ToolDefinition>
{
	public static ToolConverter inst = new ToolConverter();
	protected override void DoConversion(ToolType input, ToolDefinition output)
	{
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
}

public class ArmourConverter : Converter<ArmourType, ArmourDefinition>
{
	public static ArmourConverter inst = new ArmourConverter();
	protected override void DoConversion(ArmourType input, ArmourDefinition output)
	{
		output.slot = (EArmourSlot)input.type;
		output.maxDurability = input.Durability;
		output.toughness = input.Toughness;
		output.enchantability = input.Enchantability;
		output.damageReduction = input.DamageReductionAmount;
		output.armourTextureName = input.armourTextureName;
		List<ModifierDefinition> mods = new List<ModifierDefinition>();
		if(!Mathf.Approximately(input.moveSpeedModifier, 1f))
			mods.Add(new ModifierDefinition() { 
				Stat = "MoveSpeed", 
				Multiply = input.moveSpeedModifier,
			});
		if(!Mathf.Approximately(input.knockbackModifier, 1f))
			mods.Add(new ModifierDefinition() { 
				Stat = "Knockback", 
				Multiply = input.knockbackModifier,
			});
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
}

public class ArmourBoxConverter : Converter<ArmourBoxType, WorkbenchDefinition>
{
	public static ArmourBoxConverter inst = new ArmourBoxConverter();
	protected override void DoConversion(ArmourBoxType input, WorkbenchDefinition output)
	{
		output.armourCrafting.FECostPerCraft = 0;
		output.armourCrafting.isActive = true;
		output.armourCrafting.pages = new ArmourCraftingPageDefinition[input.pages.Count];
		for(int p = 0; p < input.pages.Count; p++)
		{
			ArmourBoxEntry page = input.pages[p];
			output.armourCrafting.pages[p] = new ArmourCraftingPageDefinition();
			output.armourCrafting.pages[p].name = page.name;
			output.armourCrafting.pages[p].entries = new ArmourCraftingEntryDefinition[4];
			for(int n = 0; n < 4; n++)
				output.armourCrafting.pages[p].entries[n] = ConvertEntry(page, n);
		}
	}

	private ArmourCraftingEntryDefinition ConvertEntry(ArmourBoxEntry page, int slot)
	{
		ArmourCraftingEntryDefinition def = new ArmourCraftingEntryDefinition();
		def.ingredients = new IngredientDefinition[page.requiredStacks[slot].Count];
		for(int i = 0; i < page.requiredStacks[slot].Count; i++)
		{
			def.ingredients[i] = ImportIngredient(page.requiredStacks[slot][i]);
		}
		def.output = ImportStackFromTypeName(page.armours[slot]);
		return def;
	}
}

public class GunBoxConverter : Converter<GunBoxType, WorkbenchDefinition>
{
	public static GunBoxConverter inst = new GunBoxConverter();
	protected override void DoConversion(GunBoxType input, WorkbenchDefinition output)
	{
		output.gunCrafting.FECostPerCraft = 0;
		output.gunCrafting.isActive = true;
		output.gunCrafting.pages = new GunCraftingPageDefinition[input.pages.Count];
		for(int p = 0; p < input.pages.Count; p++)
		{
			GunBoxPage page = input.pages[p];
			output.gunCrafting.pages[p] = new GunCraftingPageDefinition();

			List<GunCraftingEntryDefinition> outputEntries = new List<GunCraftingEntryDefinition>();
			for(int i = 0; i < page.entries.Count; i++)
			{
				GunBoxEntry entry = page.entries[i];
				CreateGunCraftingEntry(outputEntries, entry);
			}
			output.gunCrafting.pages[p].entries = outputEntries.ToArray();
		}
	}

	private void CreateGunCraftingEntry(List<GunCraftingEntryDefinition> entries, GunBoxEntry entry)
	{
		GunCraftingEntryDefinition output = new GunCraftingEntryDefinition();
		output.outputs = new ItemStackDefinition[] {
			ImportStackFromTypeName(entry.type)
		};
		output.ingredients = new IngredientDefinition[entry.requiredParts.Count];
		for(int n = 0; n < entry.requiredParts.Count; n++)
		{
			output.ingredients[n] = ImportIngredient(entry.requiredParts[n]);
		}

		if(entry is GunBoxEntryTopLevel withChildren)
		{
			for(int i = 0; i < withChildren.childEntries.Count; i++)
			{	
				CreateGunCraftingEntry(entries, withChildren.childEntries[i]);
			}
		}
		entries.Add(output);
	}
}

public class PlayerClassConverter : Converter<PlayerClass, ClassDefinition>
{
	public static PlayerClassConverter inst = new PlayerClassConverter();
	protected override void DoConversion(PlayerClass input, ClassDefinition output)
	{
		output.hat = ImportStack(input.hat);
		output.chest = ImportStack(input.chest);
		output.legs = ImportStack(input.legs);
		output.shoes = ImportStack(input.shoes);
		output.playerSkinOverride = input.playerSkinOverride;
		output.spawnOnEntity = input.horse ? "minecraft:horse" : "";
		output.startingItems = new ItemStackDefinition[input.startingItemStrings.Count];
		for(int i = 0; i < input.startingItemStrings.Count; i++)
		{
			output.startingItems[i] = ImportStack(input.startingItemStrings[i][0]);
		}
	}
}

public class TeamConverter : Converter<Team, TeamDefinition>
{
	public static TeamConverter inst = new TeamConverter();
	protected override void DoConversion(Team input, TeamDefinition output)
	{
		output.classes = input.classes.ToArray();
		output.flagColour = new ColourDefinition()
		{
			value = input.teamColour
		};
		output.textColour = input.textColour;
		output.hat = ImportStack(input.hat);
		output.chest = ImportStack(input.chest);
		output.legs = ImportStack(input.legs);
		output.shoes = ImportStack(input.shoes);
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
}

public class LoadoutConverter : Converter<LoadoutPool, LoadoutPoolDefinition>
{
	public static LoadoutConverter inst = new LoadoutConverter();
	protected override void DoConversion(LoadoutPool input, LoadoutPoolDefinition output)
	{
		output.maxLevel = input.maxLevel;
		output.xpForKill = input.XPForKill;
		output.xpForDeath = input.XPForDeath;
		output.xpForKillstreakBonus = input.XPForKillstreakBonus;
		output.defaultLoadouts = new LoadoutDefinition[input.defaultLoadouts.Length];
		for(int i = 0; i < input.defaultLoadouts.Length; i++)
		{
			output.defaultLoadouts[i] = new LoadoutDefinition()
			{
				slots = input.defaultLoadouts[i].slots,	
			};
		}
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
}










