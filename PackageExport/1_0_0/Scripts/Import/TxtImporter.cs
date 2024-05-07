using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TxtImporter<T> where T : InfoType, new()
{
	public delegate void ParseFunc(string[] split, T d);
	public abstract void read(T obj, string[] split, TypeFile file);

	public T Import(TypeFile file)
	{
		T t = new T();
		t.shortName = file.name;

        preRead(file, t);
        for (;;)
        {
            string line = null;
            line = file.readLine();
            if (line == null)
                break;
            if (line.StartsWith("//"))
                continue;
            string[] split = line.Split(' ');
            if (split.Length < 2)
                continue;
            read(t, split, file);
        }
        postRead(file, t);

		return t;
	}

	/** Method for performing actions prior to reading the type file */
    public virtual void preRead(TypeFile file, T obj) {}
    /** Method for performing actions after reading the type file */
    public virtual void postRead(TypeFile file, T obj) {}

	protected string ConvertItemString(string str)
	{
		string[] split = str.Split(".");
		if(split.Length == 0)
			return "minecraft:air";
		
		string id = split[0];
		int damage = split.Length > 1 ? short.Parse(split[1]) : -1;
		int amount = 1;
		return ConvertItemString(id, amount, damage);
	}
	protected string ConvertItemString(string itemName, int amount, int damage)
	{
		if(damage >= 0)
			return $"{amount}x{itemName}.{damage}";
		return $"{amount}x{itemName}";
	}
	protected string ConvertPotionString(string[] split)
	{
		int potionID = int.Parse(split[1]);
		int duration = int.Parse(split[2]);
		int amplifier = int.Parse(split[3]);
		return $"{potionID}:{duration}:{amplifier}";
	}
	protected List<string> ConvertRecipeString(string[] split)
	{
		List<string> recipe = new List<string>();
		
		for(int i = 0; i < (split.Length - 2) / 2; i++)
		{
			if(split[i * 2 + 3].Contains("."))
				recipe.Add(ConvertItemString(split[i * 2 + 3].Split(".")[0], int.Parse(split[i * 2 + 2]), int.Parse(split[i * 2 + 3].Split(".")[1])));
			else
				recipe.Add(ConvertItemString(split[i * 2 + 3], int.Parse(split[i * 2 + 2]), 0));
		}
		
		return recipe;
	}
	protected Vector3 ParseVector(string input)
	{
		string noBrackets = input.Substring(1, input.Length - 1);
		string[] split = noBrackets.Split(",");
		return new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
	}
	protected bool KeyMatches(string[] split, string key)
	{
		return split != null && split.Length > 1 && key != null && split[0].ToLower().Equals(key.ToLower());
	}
	protected bool Read(string[] split, string key, bool currentValue)
	{
		if(KeyMatches(split, key) && split.Length == 2)
			return bool.Parse(split[1]);
		return currentValue;
	}
	protected int Read(string[] split, string key, int currentValue)
	{
		if(KeyMatches(split, key) && split.Length == 2)
			return int.Parse(split[1]);
		return currentValue;
	}
	protected float Read(string[] split, string key, float currentValue)
	{
		if(KeyMatches(split, key) && split.Length == 2)
			return float.Parse(split[1]);
		return currentValue;
	}
	protected double Read(string[] split, string key, double currentValue)
	{
		if(KeyMatches(split, key) && split.Length == 2)
			return double.Parse(split[1]);
		return currentValue;
	}
	protected string Read(string[] split, string key, string currentValue)
	{
		if(KeyMatches(split, key) && split.Length == 2)
			return split[1];
		return currentValue;
	}
}

public static class TxtImport
{
	public static InfoType Import(TypeFile file, EDefinitionType type)
	{
		switch(type)
		{
			case EDefinitionType.part: return PartTypeImporter.inst.Import(file);
			case EDefinitionType.bullet: return BulletTypeImporter.inst.Import(file);
			case EDefinitionType.attachment: return AttachmentTypeImporter.inst.Import(file);
			case EDefinitionType.grenade: return GrenadeTypeImporter.inst.Import(file);
			case EDefinitionType.gun: return GunTypeImporter.inst.Import(file);
			case EDefinitionType.aa: return AAGunTypeImporter.inst.Import(file);
			case EDefinitionType.vehicle: return VehicleTypeImporter.inst.Import(file);
			case EDefinitionType.plane: return PlaneTypeImporter.inst.Import(file);
			case EDefinitionType.mechaItem: return MechaItemTypeImporter.inst.Import(file);
			case EDefinitionType.mecha: return MechaTypeImporter.inst.Import(file);
			case EDefinitionType.tool: return ToolTypeImporter.inst.Import(file);
			case EDefinitionType.armour: return ArmourTypeImporter.inst.Import(file);
			case EDefinitionType.armourBox: return ArmourBoxTypeImporter.inst.Import(file);
			case EDefinitionType.box: return GunBoxTypeImporter.inst.Import(file);
			case EDefinitionType.playerClass: return PlayerClassTypeImporter.inst.Import(file);
			case EDefinitionType.team: return TeamTypeImporter.inst.Import(file);
			case EDefinitionType.itemHolder: return ItemHolderTypeImporter.inst.Import(file);
			case EDefinitionType.rewardBox: return RewardBoxTypeImporter.inst.Import(file);
			case EDefinitionType.loadout: return LoadoutPoolTypeImporter.inst.Import(file);

			default: return null;
		}
	}
}

public class InfoTypeImporter : TxtImporter<InfoType>
{
	public static InfoTypeImporter inst = new InfoTypeImporter();
	public override void read(InfoType obj, string[] split, TypeFile file)
	{
		if (split[0].Equals("Model"))
		{
			obj.modelString = split[1];
			if (obj.modelString.Contains("."))
			{
				obj.modelFolder = obj.modelString.Split('.')[0];
				obj.modelString = obj.modelString.Split('.')[1];
			}
		}
		else if (split[0].Equals("ModelScale"))
			obj.modelScale = float.Parse(split[1]);
		else if (split[0].Equals("Name"))
		{
			obj.longName = split[1];
			for (int i = 0; i < split.Length - 2; i++)
			{
				obj.longName = obj.longName + " " + split[i + 2];
			}
		}
		else if (split[0].Equals("Description"))
		{
			obj.description = split[1];
			for (int i = 0; i < split.Length - 2; i++)
			{
				obj.description = obj.description + " " + split[i + 2];
			}
		}
		else if (split[0].Equals("ShortName"))
		{
			obj.shortName = split[1];
		}
		else if (split[0].Equals("Icon"))
		{
			obj.iconPath = split[1];
		}
		else if(split[0].Equals("Texture"))
		{
			obj.texture = split[1];
		}
	}
}

public class GunTypeImporter : TxtImporter<GunType>
{
	public static GunTypeImporter inst = new GunTypeImporter();

	public override void read(GunType obj, string[] split, TypeFile file)
	{
		PaintableTypeImporter.inst.read(obj, split, file);
		if (split[0].Equals("Damage"))
			obj.damage = float.Parse(split[1]);
		else if (split[0].Equals("MeleeDamage"))
			obj.meleeDamage = float.Parse(split[1]);
		else if (split[0].Equals("CanForceReload"))
			obj.canForceReload = bool.Parse(split[1].ToLower());
		else if (split[0].Equals("ReloadTime"))
			obj.reloadTime = int.Parse(split[1]);
		else if (split[0].Equals("Recoil"))
			obj.recoil = int.Parse(split[1]);
		else if (split[0].Equals("Knockback"))
			obj.knockback = float.Parse(split[1]);
		else if (split[0].Equals("Accuracy") || split[0].Equals("Spread"))
			obj.bulletSpread = float.Parse(split[1]);
		else if (split[0].Equals("NumBullets"))
			obj.numBullets = int.Parse(split[1]);
		else if (split[0].Equals("ConsumeGunOnUse"))
			obj.consumeGunUponUse = bool.Parse(split[1]);
		else if (split[0].Equals("DropItemOnShoot"))
			obj.dropItemOnShoot = split[1];
		else if (split[0].Equals("NumBurstRounds"))
			obj.numBurstRounds = int.Parse(split[1]);
		else if (split[0].Equals("MinigunStartSpeed"))
			obj.minigunStartSpeed = float.Parse(split[1]);

		//Information
		else if (split[0].Equals("ShowAttachments"))
			obj.showAttachments = bool.Parse(split[1]);
		else if (split[0].Equals("ShowDamage"))
			obj.showDamage = bool.Parse(split[1]);
		else if (split[0].Equals("ShowRecoil"))
			obj.showRecoil = bool.Parse(split[1]);
		else if (split[0].Equals("ShowAccuracy"))
			obj.showSpread = bool.Parse(split[1]);
		else if (split[0].Equals("ShowReloadTime"))
			obj.showReloadTime = bool.Parse(split[1]);

		//Sounds
		else if (split[0].Equals("ShootDelay"))
			obj.shootDelay = float.Parse(split[1]);
		else if (split[0].Equals("SoundLength"))
			obj.shootSoundLength = int.Parse(split[1]);
		else if (split[0].Equals("DistortSound"))
			obj.distortSound = split[1].Equals("True");
		else if (split[0].Equals("ShootSound"))
		{
			obj.shootSound = split[1];
			//FlansMod.proxy.loadSound(contentPack, "guns", split[1]);
		}
		else if (split[0].Equals("ReloadSound"))
		{
			obj.reloadSound = split[1];
			//FlansMod.proxy.loadSound(contentPack, "guns", split[1]);
		}
		else if (split[0].Equals("IdleSound"))
		{
			obj.idleSound = split[1];
			//FlansMod.proxy.loadSound(contentPack, "guns", split[1]);
		}
		else if (split[0].Equals("IdleSoundLength"))
			obj.idleSoundLength = int.Parse(split[1]);
		else if (split[0].Equals("MeleeSound"))
		{
			obj.meleeSound = split[1];
			//FlansMod.proxy.loadSound(contentPack, "guns", split[1]);
		}

		//Looping sounds
		else if (split[0].Equals("WarmupSound"))
		{
			obj.warmupSound = split[1];
			//FlansMod.proxy.loadSound(contentPack, "guns", split[1]);
		}
		else if (split[0].Equals("WarmupSoundLength"))
			obj.warmupSoundLength = int.Parse(split[1]);
		else if (split[0].Equals("LoopedSound") || split[0].Equals("SpinSound"))
		{
			obj.loopedSound = split[1];
			obj.useLoopingSounds = true;
			//FlansMod.proxy.loadSound(contentPack, "guns", split[1]);
		}
		else if (split[0].Equals("LoopedSoundLength") || split[0].Equals("SpinSoundLength"))
			obj.loopedSoundLength = int.Parse(split[1]);
		else if (split[0].Equals("CooldownSound"))
		{
			obj.cooldownSound = split[1];
			//FlansMod.proxy.loadSound(contentPack, "guns", split[1]);
		}

		//Modes and zoom settings
		else if (split[0].Equals("Mode"))
			obj.mode = FireModes.Parse(split[1]);
		else if (split[0].Equals("Scope"))
		{
			obj.hasScopeOverlay = true;
			if (split[1].Equals("None"))
				obj.hasScopeOverlay = false;
			else obj.defaultScopeTexture = split[1];
		}
		else if (split[0].Equals("ZoomLevel"))
		{
			obj.zoomLevel = float.Parse(split[1]);
		}
		else if (split[0].Equals("FOVZoomLevel"))
		{
			obj.FOVFactor = float.Parse(split[1]);
		}
		//            else if (split[0].Equals("Deployable"))
		//          deployable = split[1].Equals("True");
		//          else if (FMLCommonHandler.instance().getSide().isClient() && deployable && split[0].Equals("DeployedModel"))
		//          {
		//              deployableModel = FlansMod.proxy.loadModel(split[1], shortName, ModelMG.class);
		//			}
		else if (split[0].Equals("Model"))
		{
			// MODEL PARSER!
			//model = FlansMod.proxy.loadModel(split[1], shortName, ModelGun.class);
		}
		else if (split[0].Equals("Texture"))
			obj.texture = split[1];
		//			else if(split[0].Equals("DeployedTexture"))
		//				deployableTexture = split[1];
		//			else if(split[0].Equals("StandBackDistance"))
		//				standBackDist = float.Parse(split[1]);
		//			else if(split[0].Equals("TopViewLimit"))
		//				topViewLimit = -float.Parse(split[1]);
		//			else if(split[0].Equals("BottomViewLimit"))
		//				bottomViewLimit = float.Parse(split[1]);
		//			else if(split[0].Equals("SideViewLimit"))
		//				sideViewLimit = float.Parse(split[1]);
		//			else if(split[0].Equals("PivotHeight"))
		//				pivotHeight = float.Parse(split[1]);
		else if (split[0].Equals("Ammo"))
		{
			obj.ammo.Add(split[1]);
		}
		else if (split[0].Equals("NumAmmoSlots") || split[0].Equals("NumAmmoItemsInGun") || split[0].Equals("LoadIntoGun"))
			obj.numAmmoItemsInGun = int.Parse(split[1]);
		else if (split[0].Equals("BulletSpeed"))
		{
			if (split[1].ToLower().Equals("instant"))
			{
				obj.bulletSpeed = 0.0f;
			}
			else obj.bulletSpeed = float.Parse(split[1]);

			if (obj.bulletSpeed > 3.0f)
			{
				obj.bulletSpeed = 0.0f;
			}
		}
		else if (split[0].Equals("CanShootUnderwater"))
			obj.canShootUnderwater = bool.Parse(split[1].ToLower());
		else if (split[0].Equals("OneHanded"))
			obj.oneHanded = bool.Parse(split[1].ToLower());

		//Player modifiers
		else if (split[0].Equals("MoveSpeedModifier") || split[0].Equals("Slowness"))
			obj.moveSpeedModifier = float.Parse(split[1]);
		else if (split[0].Equals("KnockbackReduction") || split[0].Equals("KnockbackModifier"))
			obj.knockbackModifier = float.Parse(split[1]);

		//Attachment settings
		else if (split[0].Equals("AllowAllAttachments"))
			obj.allowAllAttachments = bool.Parse(split[1].ToLower());
		else if (split[0].Equals("AllowAttachments"))
		{
			for (int i = 1; i < split.Length; i++)
			{
				obj.allowedAttachments.Add(split[i]);
			}
		}
		else if (split[0].Equals("AllowBarrelAttachments"))
			obj.allowBarrelAttachments = bool.Parse(split[1].ToLower());
		else if (split[0].Equals("AllowScopeAttachments"))
			obj.allowScopeAttachments = bool.Parse(split[1].ToLower());
		else if (split[0].Equals("AllowStockAttachments"))
			obj.allowStockAttachments = bool.Parse(split[1].ToLower());
		else if (split[0].Equals("AllowGripAttachments"))
			obj.allowGripAttachments = bool.Parse(split[1].ToLower());
		else if (split[0].Equals("NumGenericAttachmentSlots"))
			obj.numGenericAttachmentSlots = int.Parse(split[1]);

		//Shield settings
		else if (split[0].ToLower().Equals("shield"))
		{
			obj.shield = true;
			obj.shieldDamageAbsorption = float.Parse(split[1]);
			obj.shieldOrigin = new Vector3(float.Parse(split[2]) / 16F, float.Parse(split[3]) / 16F, float.Parse(split[4]) / 16F);
			obj.shieldDimensions = new Vector3(float.Parse(split[5]) / 16F, float.Parse(split[6]) / 16F, float.Parse(split[7]) / 16F);
		}
	}
}

public class PaintableTypeImporter : TxtImporter<PaintableType>
{
	public static PaintableTypeImporter inst = new PaintableTypeImporter();

	public override void read(PaintableType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);

		if (split[0].ToLower().Equals("paintjob"))
		{
			obj.paintjobs.Add(new Paintjob(obj.nextPaintjobID++, split[1], split[2]));
		}
	}
}

public class BulletTypeImporter : TxtImporter<BulletType>
{
	public static BulletTypeImporter inst = new BulletTypeImporter();

	public override void read(BulletType obj, string[] split, TypeFile file)
	{
		ShootableTypeImporter.inst.read(obj, split, file);

		if(split[0].Equals("FlakParticles"))
			obj.flak = int.Parse(split[1]);
		else if(split[0].Equals("FlakParticleType"))
			obj.flakParticles = split[1];
		else if(split[0].Equals("SetEntitiesOnFire"))
			obj.setEntitiesOnFire = bool.Parse(split[1]);
		
		else if(split[0].Equals("HitSound")) 
			obj.hitSound = split[1]; 
		else if(split[0].Equals("HitSoundRange"))
			obj.hitSoundRange = float.Parse(split[1]);
		else if(split[0].Equals("Penetrates"))
			obj.penetratingPower = (bool.Parse(split[1].ToLower()) ? 1F : 0.25F);
		else if(split[0].Equals("Penetration") || split[0].Equals("PenetratingPower"))
			obj.penetratingPower = float.Parse(split[1]);
		
		else if(split[0].Equals("Bomb"))
			obj.weaponType = EWeaponType.BOMB;
		else if(split[0].Equals("Shell"))
			obj.weaponType = EWeaponType.SHELL;
		else if(split[0].Equals("Missile"))
			obj.weaponType = EWeaponType.MISSILE;
		else if(split[0].Equals("WeaponType"))
			obj.weaponType = System.Enum.Parse<EWeaponType>(split[1].ToUpper());
		
		else if(split[0].Equals("TrailTexture"))
			obj.trailTexture = split[1];
		
		else if(split[0].Equals("HasLight"))
			obj.hasLight = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("LockOnToDriveables"))
			obj.lockOnToPlanes = obj.lockOnToVehicles = obj.lockOnToMechas = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("LockOnToVehicles"))
			obj.lockOnToVehicles = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("LockOnToPlanes"))
			obj.lockOnToPlanes = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("LockOnToMechas"))
			obj.lockOnToMechas = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("LockOnToPlayers"))
			obj.lockOnToPlayers = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("LockOnToLivings"))
			obj.lockOnToLivings = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("MaxLockOnAngle"))
			obj.maxLockOnAngle = float.Parse(split[1]);
		else if(split[0].Equals("LockOnForce") || split[0].Equals("TurningForce"))
			obj.lockOnForce = float.Parse(split[1]);
		else if(split[0].Equals("PotionEffect"))
			obj.hitEffects.Add(ConvertPotionString(split));
	}
}

public class ShootableTypeImporter : TxtImporter<ShootableType>
{
	public static ShootableTypeImporter inst = new ShootableTypeImporter();

	public override void read(ShootableType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);

			//Item Stuff
		if(split[0].Equals("StackSize") || split[0].Equals("MaxStackSize"))
			obj.maxStackSize = int.Parse(split[1]);
		else if(split[0].Equals("DropItemOnShoot"))
			obj.dropItemOnShoot = split[1];
		else if(split[0].Equals("DropItemOnReload"))
			obj.dropItemOnReload = split[1];
		else if(split[0].Equals("DropItemOnHit"))
			obj.dropItemOnHit = split[1];
		else if(split[0].Equals("RoundsPerItem"))
			obj.roundsPerItem = int.Parse(split[1]);
		else if(split[0].Equals("NumBullets"))
			obj.numBullets = int.Parse(split[1]);
		else if(split[0].Equals("Accuracy") || split[0].Equals("Spread"))
			obj.bulletSpread = float.Parse(split[1]);
			
			//Physics
		else if(split[0].Equals("FallSpeed"))
			obj.fallSpeed = float.Parse(split[1]);
		else if(split[0].Equals("ThrowSpeed") || split[0].Equals("ShootSpeed"))
			obj.throwSpeed = float.Parse(split[1]);
		else if(split[0].Equals("HitBoxSize"))
			obj.hitBoxSize = float.Parse(split[1]);
			
			//Hit stuff
		else if(split[0].Equals("HitEntityDamage") || split[0].Equals("DamageVsLiving") || split[0].Equals("DamageVsPlayer"))
			obj.damageVsLiving = float.Parse(split[1]);
		else if(split[0].Equals("DamageVsVehicles"))
			obj.damageVsDriveable = float.Parse(split[1]);
		else if(split[0].Equals("Damage"))
		{
			obj.damageVsLiving = obj.damageVsDriveable = float.Parse(split[1]);
		}
		else if(split[0].Equals("BreaksGlass"))
			obj.breaksGlass = bool.Parse(split[1].ToLower());
			
			//Detonation conditions etc
		else if(split[0].Equals("Fuse"))
			obj.fuse = int.Parse(split[1]);
		else if(split[0].Equals("DespawnTime"))
			obj.despawnTime = int.Parse(split[1]);
		else if(split[0].Equals("ExplodeOnImpact") || split[0].Equals("DetonateOnImpact"))
			obj.explodeOnImpact = bool.Parse(split[1].ToLower());
			
			//Detonation
		else if(split[0].Equals("FireRadius") || split[0].Equals("Fire"))
			obj.fireRadius = float.Parse(split[1]);
		else if(split[0].Equals("ExplosionRadius") || split[0].Equals("Explosion"))
			obj.explosionRadius = float.Parse(split[1]);
		else if(split[0].Equals("ExplosionBreaksBlocks"))
			obj.explosionBreaksBlocks = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("DropItemOnDetonate"))
			obj.dropItemOnDetonate = split[1];
		else if(split[0].Equals("DetonateSound")) 
			obj.detonateSound = split[1]; 
		
		//Particles
		else if(split[0].Equals("TrailParticles") || split[0].Equals("SmokeTrail"))
			obj.trailParticles = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("TrailParticleType"))
			obj.trailParticleType = split[1];
	}
}

public class DriveableTypeImporter : TxtImporter<DriveableType>
{
	public static DriveableTypeImporter inst = new DriveableTypeImporter();
	private Dictionary<string, ParseFunc> parsers = new Dictionary<string, ParseFunc>();
	public DriveableTypeImporter()
	{
		parsers = new Dictionary<string, ParseFunc>();
	// BASICS /////////////////////////////////////////////////////////////////////////////
		parsers.Add("MaxThrottle", (split, d) => d.maxThrottle = float.Parse(split[1]));
		parsers.Add("MaxNegativeThrottle", (split, d) => d.maxNegativeThrottle = float.Parse(split[1]));
		parsers.Add("Drag", (split, d) => d.drag = float.Parse(split[1]));
		parsers.Add("TurretOrigin", (split, d) =>
			d.turretOrigin = new Vector3(float.Parse(split[1]) / 16F,
				float.Parse(split[2]) / 16F,
				float.Parse(split[3]) / 16F));
		
		parsers.Add("TurretRotationSpeed", (split, d) => d.turretRotationSpeed = float.Parse(split[1]));
		parsers.Add("CanRoll", (split, d) => d.canRoll = bool.Parse(split[1]));
		parsers.Add("YOffset", (split, d) => d.yOffset = float.Parse(split[1]));
		parsers.Add("CameraDistance", (split, d) => d.cameraDistance = float.Parse(split[1]));
		
		// BOATS ////////////////////////////////////////////////////////////////////////////
		parsers.Add("PlaceableOnLand", (split, d) => d.placeableOnLand = bool.Parse(split[1]));
		parsers.Add("PlaceableOnWater", (split, d) => d.placeableOnWater = bool.Parse(split[1]));
		parsers.Add("FloatOnWater", (split, d) => d.floatOnWater = bool.Parse(split[1]));
		parsers.Add("Boat", (split, d) =>
		{
			d.placeableOnLand = false;
			d.placeableOnWater = true;
			d.floatOnWater = true;
			d.wheelStepHeight = 0F;
		});
		parsers.Add("Buoyancy", (split, d) => d.buoyancy = float.Parse(split[1]));
		
		// WHEELS ////////////////////////////////////////////////////////////////////////////////
		parsers.Add("Wheel", (split, d) =>
			d.wheelPositions[int.Parse(split[1])] = new DriveablePosition(new Vector3(
				float.Parse(split[2]) / 16F, float.Parse(split[3]) / 16F, float.Parse(split[4]) / 16F),
				split.Length > 5 ? split[5] : "coreWheel"));
		parsers.Add("WheelPosition", parsers.GetValueOrDefault("Wheel")); // Alt name
		parsers.Add("WheelRadius", (split, d) => d.wheelStepHeight = float.Parse(split[1]));
		parsers.Add("WheelStepHeight", parsers.GetValueOrDefault("WheelRadius")); // Alt name
		parsers.Add("WheelSpringStrength", (split, d) => d.wheelSpringStrength = float.Parse(split[1]));
		parsers.Add("SpringStrength", parsers.GetValueOrDefault("WheelSpringStrength")); // Alt name
		
		// HARVESTERS //////////////////////////////////////////////////////////////////////////////
		parsers.Add("Harvester", (split, d) => d.harvestBlocks = bool.Parse(split[1]));
		parsers.Add("HarvestMaterial", (split, d) => d.materialsHarvested.Add(split[1]));
		parsers.Add("HarvestToolType", (split, d) =>
		{
			switch(split[1])
			{
				case "Axe":
					d.materialsHarvested.Add("WOOD");
					d.materialsHarvested.Add("PLANTS");
					d.materialsHarvested.Add("VINE");
					break;
				case "Pickaxe":
				case "Drill":
					d.materialsHarvested.Add("IRON");
					d.materialsHarvested.Add("ANVIL");
					d.materialsHarvested.Add("ROCK");
					break;
				case "Spade":
				case "Shovel":
				case "Excavator":
					d.materialsHarvested.Add("GROUND");
					d.materialsHarvested.Add("GRASS");
					d.materialsHarvested.Add("SAND");
					d.materialsHarvested.Add("SNOW");
					d.materialsHarvested.Add("CLAY");
					break;
				case "Hoe":
				case "Combine":
					d.materialsHarvested.Add("PLANTS");
					d.materialsHarvested.Add("LEAVES");
					d.materialsHarvested.Add("VINE");
					d.materialsHarvested.Add("CACTUS");
					d.materialsHarvested.Add("GOURD");
					break;
			}
		});
		
		// CARGO ////////////////////////////////////////////////////////////////////////////////////////
		parsers.Add("CargoSlots", (split, d) => d.numCargoSlots = int.Parse(split[1]));
		parsers.Add("BombSlots", (split, d) => d.numBombSlots = int.Parse(split[1]));
		parsers.Add("MineSlots", parsers.GetValueOrDefault("BombSlots")); // Alt name
		parsers.Add("MissileSlots", (split, d) => d.numMissileSlots = int.Parse(split[1]));
		parsers.Add("ShellSlots", parsers.GetValueOrDefault("MissileSlots")); // Alt name
		parsers.Add("FuelTankSize", (split, d) => d.fuelTankSize = int.Parse(split[1]));
		parsers.Add("TrackFrames", (split, d) => d.animFrames = int.Parse(split[1]) - 1);
		parsers.Add("BulletDetection", (split, d) => d.bulletDetectionRadius = int.Parse(split[1]));
		
		// AMMO ////////////////////////////////////////////////////////////////////////////////////////
		parsers.Add("AddAmmo", (split, d) => d.ammo.Add(split[1]));
		parsers.Add("AllowAllAmmo", (split, d) => d.acceptAllAmmo = bool.Parse(split[1]));
		parsers.Add("AcceptAllAmmo", parsers.GetValueOrDefault("AllowAllAmmo")); // Alt name
		parsers.Add("Primary", (split, d) => d.primary = System.Enum.Parse<EWeaponType>(split[1].ToUpper()));
		parsers.Add("Secondary", (split, d) => d.secondary = System.Enum.Parse<EWeaponType>(split[1].ToUpper()));
		parsers.Add("ShootDelayPrimary", (split, d) => d.shootDelayPrimary = int.Parse(split[1]));
		parsers.Add("ShootDelaySecondary", (split, d) => d.shootDelaySecondary = int.Parse(split[1]));
		parsers.Add("DamageModifierPrimary", (split, d) => d.damageModifierPrimary = int.Parse(split[1]));
		parsers.Add("DamageModifierSecondary", (split, d) => d.damageModifierSecondary = int.Parse(split[1]));
		parsers.Add("AlternatePrimary", (split, d) => d.alternatePrimary = bool.Parse(split[1]));
		parsers.Add("AlternateSecondary", (split, d) => d.alternateSecondary = bool.Parse(split[1]));
		parsers.Add("ModePrimary", (split, d) => d.modePrimary = FireModes.Parse(split[1].ToUpper()));
		parsers.Add("ModeSecondary", (split, d) => d.modeSecondary = FireModes.Parse(split[1].ToUpper()));
		parsers.Add("ShootPointPrimary", (split, d) =>
		{
			DriveablePosition rootPos;
			Vector3 offPos;
			string[] gun;
			if(split.Length == 9)
			{
				gun = new string[]{split[0], split[1], split[2], split[3], split[4], split[5]};
				offPos = new Vector3(float.Parse(split[6]) / 16F,
					float.Parse(split[7]) / 16F,
					float.Parse(split[8]) / 16F);
			}
			else if(split.Length == 8)
			{
				gun = new string[]{split[0], split[1], split[2], split[3], split[4]};
				offPos = new Vector3(float.Parse(split[5]) / 16F,
					float.Parse(split[6]) / 16F,
					float.Parse(split[7]) / 16F);
			}
			else
			{
				gun = split;
				offPos = new Vector3(0, 0, 0);
			}
			rootPos = DriveablePosition.Parse(gun);
			ShootPoint sPoint = new ShootPoint(rootPos, offPos);
			d.shootPointsPrimary.Add(sPoint);
			if(rootPos is PilotGun)
				d.pilotGuns.Add((PilotGun)sPoint.rootPos);
		});
		parsers.Add("ShootPointSecondary", (split, d) =>
		{
			DriveablePosition rootPos;
			Vector3 offPos;
			string[] gun;
			if(split.Length == 9)
			{
				gun = new string[]{split[0], split[1], split[2], split[3], split[4], split[5]};
				offPos = new Vector3(float.Parse(split[6]) / 16F,
					float.Parse(split[7]) / 16F,
					float.Parse(split[8]) / 16F);
			}
			else if(split.Length == 8)
			{
				gun = new string[]{split[0], split[1], split[2], split[3], split[4]};
				offPos = new Vector3(float.Parse(split[5]) / 16F,
					float.Parse(split[6]) / 16F,
					float.Parse(split[7]) / 16F);
			}
			else
			{
				gun = split;
				offPos = new Vector3(0, 0, 0);
			}
			rootPos = DriveablePosition.Parse(gun);
			ShootPoint sPoint = new ShootPoint(rootPos, offPos);
			d.shootPointsSecondary.Add(sPoint);
			if(rootPos is PilotGun)
				d.pilotGuns.Add((PilotGun)sPoint.rootPos);
		});
		
		// BACKWARDS COMPATIBILITY ////////////////////////////////////////////////////////////
		parsers.Add("AddGun", (split, d) =>
		{
			d.secondary = EWeaponType.GUN;
			PilotGun pilotGun = (PilotGun)DriveablePosition.Parse(split);
			d.shootPointsSecondary.Add(new ShootPoint(pilotGun, new Vector3(0, 0, 0)));
			d.pilotGuns.Add(pilotGun);
			d.driveableRecipe.Add(pilotGun.type);
		});
		parsers.Add("BombPosition", (split, d) =>
		{
			d.primary = EWeaponType.BOMB;
			DriveablePosition pos = new DriveablePosition(new Vector3(
				float.Parse(split[1]) / 16F,
				float.Parse(split[2]) / 16F,
				float.Parse(split[3]) / 16F),
				"core");
			d.shootPointsPrimary.Add(new ShootPoint(pos, new Vector3(0, 0, 0)));
		});
		parsers.Add("BarrelPosition", (split, d) =>
		{
			d.primary = EWeaponType.SHELL;
			DriveablePosition pos = new DriveablePosition(new Vector3(
				float.Parse(split[1]) / 16F,
				float.Parse(split[2]) / 16F,
				float.Parse(split[3]) / 16F),
				"turret");
			d.shootPointsPrimary.Add(new ShootPoint(pos, new Vector3(0, 0, 0)));
		});
		parsers.Add("ShootDelay", (split, d) => d.shootDelaySecondary = int.Parse(split[1]));
		parsers.Add("ShellDelay", (split, d) => d.shootDelayPrimary = int.Parse(split[1]));
		parsers.Add("BombDelay", parsers.GetValueOrDefault("ShellDelay")); // Alt name
		
		// RECIPE ////////////////////////////////////////////////////////////////////////////////
		parsers.Add("AddRecipeParts", (split, d) =>
		{
			string[] stacks = new string[(split.Length - 2) / 2];
			for(int i = 0; i < (split.Length - 2) / 2; i++)
			{
				int amount = int.Parse(split[2 * i + 2]);
				bool damaged = split[2 * i + 3].Contains(".");
				string itemName = damaged ? split[2 * i + 3].Split(".")[0] : split[2 * i + 3];
				int damage = damaged ? int.Parse(split[2 * i + 3].Split(".")[1]) : 0;
				stacks[i] = ConvertItemString(itemName, amount, damage);
				d.driveableRecipe.Add(stacks[i]);
			}
			d.partwiseRecipe.Add(split[1], stacks);
		});
		parsers.Add("AddDye", (split, d) =>
		{
			int amount = int.Parse(split[1]);
			string dyeName = split[2];
			d.driveableRecipe.Add(ConvertItemString(dyeName, amount, 0));
		});
		
		// HEALTH & COLLISION //////////////////////////////////////////////////////
		parsers.Add("SetupPart", (split, d) =>
		{
			string part = split[1];
			CollisionBox box = new CollisionBox(int.Parse(split[2]),
				int.Parse(split[3]),
				int.Parse(split[4]),
				int.Parse(split[5]),
				int.Parse(split[6]),
				int.Parse(split[7]),
				int.Parse(split[8]));
			d.health[part] = box;
			double max = box.Max();
			if(max > d.hitboxRadius)
				d.hitboxRadius = max;
		});
		parsers.Add("CollisionPoint", (split, d) => d.collisionPoints.Add(new DriveablePosition(split)));
		parsers.Add("AddCollisionPoint", parsers.GetValueOrDefault("CollisionPoint")); // Alt name
		
		
		// PASSENGERS /////////////////////////////////////////////////////////////
		parsers.Add("Driver", (split, d) =>
		{
			if(split.Length > 4)
				d.seats[0] = new Seat(int.Parse(split[1]),
					int.Parse(split[2]),
					int.Parse(split[3]),
					float.Parse(split[4]),
					float.Parse(split[5]),
					float.Parse(split[6]),
					float.Parse(split[7]));
			else
				d.seats[0] = new Seat(int.Parse(split[1]),
					int.Parse(split[2]),
					int.Parse(split[3]));
		});
		parsers.Add("Pilot", parsers.GetValueOrDefault("Driver")); // Alt name
		parsers.Add("Passenger", (split, d) =>
		{
			Seat seat = new Seat(split);
			d.seats[seat.id] = seat;
			if(seat.gunType != null)
			{
				seat.gunnerID = d.numPassengerGunners++;
				d.driveableRecipe.Add(seat.gunType);
			}
		});
		parsers.Add("GunOrigin", (split, d) =>
		{
			if(d.seats[int.Parse(split[1])] == null)
				Debug.LogError(
					"GunOrigin line found in vehicle / mecha / plane file before Passenger line [" + d.shortName + "]");
			d.seats[int.Parse(split[1])].gunOrigin = new Vector3(float.Parse(split[2]) / 16F,
				float.Parse(split[3]) / 16F,
				float.Parse(split[4]) / 16F);
		});
		parsers.Add("RotatedDriverOffset", (split, d) =>
			d.seats[0].rotatedOffset = new Vector3(int.Parse(split[1]) / 16F,
				int.Parse(split[2]) / 16F,
				int.Parse(split[3]) / 16F));
		parsers.Add("DriverAimSpeed", (split, d) =>
			d.seats[0].aimingSpeed = new Vector3(float.Parse(split[1]),
				float.Parse(split[2]),
				float.Parse(split[3])));
		parsers.Add("RotatedPassengerOffset", (split, d) =>
			d.seats[int.Parse(split[1])].rotatedOffset = new Vector3(int.Parse(split[2]) / 16F,
				int.Parse(split[3]) / 16F,
				int.Parse(split[4]) / 16F));
		parsers.Add("PassengerAimSpeed", (split, d) =>
			d.seats[int.Parse(split[1])].aimingSpeed = new Vector3(float.Parse(split[2]),
				float.Parse(split[3]),
				float.Parse(split[4])));
		parsers.Add("DriverLegacyAiming", (split, d) => d.seats[0].legacyAiming = bool.Parse(split[1]));
		parsers.Add("PassengerLegacyAiming", (split, d) =>
			d.seats[int.Parse(split[1])].legacyAiming = bool.Parse(split[2]));
		parsers.Add("DriverYawBeforePitch", (split, d) => d.seats[0].yawBeforePitch = bool.Parse(split[1]));
		parsers.Add("PassengerYawBeforePitch", (split, d) =>
			d.seats[int.Parse(split[1])].yawBeforePitch = bool.Parse(split[2]));
		parsers.Add("DriverLatePitch", (split, d) => d.seats[0].latePitch = bool.Parse(split[1]));
		parsers.Add("PassengerLatePitch", (split, d) =>
			d.seats[int.Parse(split[1])].latePitch = bool.Parse(split[2]));
		
		
		// SOUNDS /////////////////////////////////////////////////////////////////////
		parsers.Add("DriverTraverseSounds", (split, d) => d.seats[0].traverseSounds = bool.Parse(split[1]));
		parsers.Add("PassengerTraverseSounds", (split, d) =>
			d.seats[int.Parse(split[1])].traverseSounds = bool.Parse(split[2]));
		
		parsers.Add("StartSoundLength", (split, d) => d.startSoundLength = int.Parse(split[1]));
		parsers.Add("EngineSoundLength", (split, d) => d.engineSoundLength = int.Parse(split[1]));
		parsers.Add("YawSoundLength", (split, d) => d.seats[0].yawSoundLength = int.Parse(split[1]));
		parsers.Add("PitchSoundLength", (split, d) => d.seats[0].pitchSoundLength = int.Parse(split[1]));
		parsers.Add("PassengerYawSoundLength", (split, d) =>
			d.seats[int.Parse(split[1])].yawSoundLength = int.Parse(split[2]));
		parsers.Add("PassengerPitchSoundLength",
			(split, d) => d.seats[int.Parse(split[1])].pitchSoundLength = int.Parse(split[2]));
		parsers.Add("StartSound", (split, d) => { d.startSound = split[1]; });
		parsers.Add("EngineSound", (split, d) => { d.engineSound = split[1]; });
		parsers.Add("YawSound", (split, d) => { d.seats[0].yawSound = split[1]; });
		parsers.Add("PitchSound", (split, d) => { d.seats[0].pitchSound = split[1]; });
		parsers.Add("PassengerYawSound", (split, d) => { d.seats[int.Parse(split[1])].yawSound = split[2]; });
		parsers.Add("PassengerPitchSound", (split, d) => { d.seats[int.Parse(split[1])].pitchSound = split[2]; });
		parsers.Add("ShootMainSound", (split, d) => { d.shootSoundPrimary = split[1]; });
		parsers.Add("ShootSoundPrimary", parsers.GetValueOrDefault("ShootMainSound")); // Alt name
		parsers.Add("ShellSound", parsers.GetValueOrDefault("ShootMainSound")); // Alt name
		parsers.Add("BombSound", parsers.GetValueOrDefault("ShootMainSound")); // Alt name
		parsers.Add("ShootSecondarySound", (split, d) => { d.shootSoundSecondary = split[1]; });
		parsers.Add("ShootSoundSecondary", parsers.GetValueOrDefault("ShootSecondarySound")); // Alt name
		
		// ICBM Mod Integration
		parsers.Add("OnRadar", (split, d) => d.onRadar = split[1] == "True");
		
		// PARTICLES & GRAPHICS /////////////////////////////////////////////////////////////
		parsers.Add("AddParticle", (split, d) =>
		{
			ParticleEmitter emitter = new ParticleEmitter();
			emitter.effectType = split[1];
			emitter.emitRate = int.Parse(split[2]);
			emitter.origin = ParseVector(split[3]);
			emitter.extents = ParseVector(split[4]);
			emitter.velocity = ParseVector(split[5]);
			emitter.minThrottle = float.Parse(split[6]);
			emitter.maxThrottle = float.Parse(split[7]);
			emitter.minHealth = float.Parse(split[8]);
			emitter.maxHealth = float.Parse(split[9]);
			emitter.part = split[10];
			//Scale from model coords to world coords
			emitter.origin *= (1.0f / 16.0f);
			emitter.extents *= (1.0f / 16.0f);
			emitter.velocity *= (1.0f / 16.0f);
			d.emitters.Add(emitter);
		});		
		
		// FM+ TODO
		
		parsers.Add("IsExplosionWhenDestroyed", (split, d) =>
			d.isExplosionWhenDestroyed = bool.Parse(split[1]));
		parsers.Add("VehicleGunModelScale", (split, d) => d.vehicleGunModelScale = float.Parse(split[1]));
		parsers.Add("VehicleGunReloadTick", (split, d) => d.reloadSoundTick = int.Parse(split[1]));
		parsers.Add("FallDamageFactor", (split, d) => d.fallDamageFactor = float.Parse(split[1]));
		parsers.Add("ClutchBrake", (split, d) => d.ClutchBrake = float.Parse(split[1]));
		parsers.Add("MaxThrottleInWater", (split, d) => d.maxThrottleInWater = float.Parse(split[1]));
		parsers.Add("TurretOriginOffset", (split, d) =>
			d.turretOriginOffset = new Vector3(
				float.Parse(split[1]) / 16F,
				float.Parse(split[2]) / 16F,
				float.Parse(split[3]) / 16F));
		parsers.Add("CollisionDamageEnable", (split, d) => d.collisionDamageEnable = bool.Parse(split[1]));
		parsers.Add("CollisionDamageThrottle", (split, d) => d.collisionDamageThrottle = float.Parse(split[1]));
		parsers.Add("CollisionDamageTimes", (split, d) => d.collisionDamageTimes = float.Parse(split[1]));
		parsers.Add("CanLockAngle", (split, d) => d.canLockOnAngle = int.Parse(split[1]));
		parsers.Add("LockOnSoundTime", (split, d) => d.lockOnSoundTime = int.Parse(split[1]));
		parsers.Add("LockOnToDriveables", (split, d) =>
			d.lockOnToPlanes = d.lockOnToVehicles = d.lockOnToMechas = bool.Parse(split[1].ToLower()));
		parsers.Add("LockOnToVehicles", (split, d) =>
			d.lockOnToVehicles = bool.Parse(split[1].ToLower()));
		parsers.Add("LockOnToPlanes", (split, d) => d.lockOnToPlanes = bool.Parse(split[1].ToLower()));
		parsers.Add("LockOnToMechas", (split, d) => d.lockOnToMechas = bool.Parse(split[1].ToLower()));
		parsers.Add("LockOnToPlayers", (split, d) => d.lockOnToPlayers = bool.Parse(split[1].ToLower()));
		parsers.Add("LockOnToLivings", (split, d) => d.lockOnToLivings = bool.Parse(split[1].ToLower()));
		parsers.Add("LockedOnSoundRange", (split, d) => d.lockedOnSoundRange = int.Parse(split[1]));
		parsers.Add("HasFlare", (split, d) => d.hasFlare = bool.Parse(split[1]));
		parsers.Add("FlareDelay", (split, d) =>
		{
			d.flareDelay = int.Parse(split[1]);
			if(d.flareDelay <= 0)
				d.flareDelay = 1;
		});
		parsers.Add("TimeFlareUsing", (split, d) =>
		{
			d.timeFlareUsing = int.Parse(split[1]);
			if(d.timeFlareUsing <= 0)
				d.timeFlareUsing = 1;
		});
		parsers.Add("PlaceableOnSponge", (split, d) => d.placeableOnSponge = bool.Parse(split[1]));
		parsers.Add("FloatOffset", (split, d) => d.floatOffset = float.Parse(split[1]));
		parsers.Add("CanMountEntity", (split, d) => d.canMountEntity = bool.Parse(split[1]));
		parsers.Add("CollectHarvest", (split, d) => d.collectHarvest = bool.Parse(split[1]));
		parsers.Add("DropHarvest", (split, d) => d.dropHarvest = bool.Parse(split[1]));
		parsers.Add("HarvestBox", (split, d) =>
		{
			d.harvestBoxSize = ParseVector(split[1]);
			d.harvestBoxPos = ParseVector(split[2]);
		});
		parsers.Add("PlaceTimePrimary", (split, d) => d.placeTimePrimary = int.Parse(split[1]));
		parsers.Add("PlaceTimeSecondary", (split, d) => d.placeTimeSecondary = int.Parse(split[1]));
		parsers.Add("ReloadTimePrimary", (split, d) => d.reloadTimePrimary = int.Parse(split[1]));
		parsers.Add("ReloadTimeSecondary", (split, d) => d.reloadTimeSecondary = int.Parse(split[1]));
		parsers.Add("BulletSpeed", (split, d) => d.bulletSpeed = float.Parse(split[1]));
		parsers.Add("BulletSpread", (split, d) => d.bulletSpread = float.Parse(split[1]));
		parsers.Add("RangingGun", (split, d) => d.rangingGun = bool.Parse(split[1]));
		parsers.Add("GunLength", (split, d) => d.gunLength = float.Parse(split[1]));
		parsers.Add("RecoilDistance", (split, d) => d.recoilDist = float.Parse(split[1]));
		parsers.Add("RecoilTime", (split, d) => d.recoilTime = float.Parse(split[1]));
		parsers.Add("EnableReloadTime", (split, d) => d.enableReloadTime = bool.Parse(split[1]));
		parsers.Add("ShootParticlesPrimary", (split, d) =>
			d.shootParticlesPrimary.Add(new ShootParticle(
				split[1],
				float.Parse(split[2]),
				float.Parse(split[3]),
				float.Parse(split[4]))));
		parsers.Add("ShootParticlesSecondary", (split, d) =>
			d.shootParticlesSecondary.Add(new ShootParticle(
				split[1],
				float.Parse(split[2]),
				float.Parse(split[3]),
				float.Parse(split[4]))));
		parsers.Add("SetPlayerInvisible", (split, d) =>
			d.setPlayerInvisible = bool.Parse(split[1].ToLower()));
		parsers.Add("IT1", (split, d) => d.IT1 = bool.Parse(split[1].ToLower()));
		parsers.Add("FixedPrimary", (split, d) => d.fixedPrimaryFire = bool.Parse(split[1].ToLower()));
		parsers.Add("PrimaryAngle", (split, d) =>
			d.primaryFireAngle = new Vector3(float.Parse(split[1]),
				float.Parse(split[2]),
				float.Parse(split[3])));
		parsers.Add("PlaceSoundPrimary", (split, d) => { d.placeSoundPrimary = split[1]; });
		parsers.Add("PlaceSoundSecondary", (split, d) => { d.placeSoundSecondary = split[1]; });
		parsers.Add("ReloadSoundPrimary", (split, d) => { d.reloadSoundPrimary = split[1]; });
		parsers.Add("ReloadSoundSecondary", (split, d) => { d.reloadSoundSecondary = split[1]; });
		parsers.Add("LockedOnSound", (split, d) => { d.lockedOnSound = split[1]; });
		parsers.Add("LockingOnSound", (split, d) => { d.lockingOnSound = split[1]; });
		parsers.Add("FlareSound", (split, d) => { d.flareSound = split[1]; });
		parsers.Add("FancyCollision", (split, d) => d.fancyCollision = bool.Parse(split[1]));
		parsers.Add("AddCollisionMesh", (split, d) =>
		{
			CollisionShapeBox box = new CollisionShapeBox(
				ParseVector(split[1]),
				ParseVector(split[2]),
				ParseVector(split[3]),
				ParseVector(split[4]),
				ParseVector(split[5]),
				ParseVector(split[6]),
				ParseVector(split[7]),
				ParseVector(split[8]),
				ParseVector(split[9]),
				ParseVector(split[10]),
				"core");
			d.collisionBox.Add(box);
		});
		parsers.Add("AddCollisionMeshRaw", (split, d) =>
		{
			Vector3 pos = new Vector3(float.Parse(split[1]),
				float.Parse(split[2]),
				float.Parse(split[3]));
			Vector3 size = new Vector3(float.Parse(split[4]),
				float.Parse(split[5]),
				float.Parse(split[6]));
			Vector3 p1 = new Vector3(float.Parse(split[8]),
				float.Parse(split[9]),
				float.Parse(split[10]));
			Vector3 p2 = new Vector3(float.Parse(split[11]),
				float.Parse(split[12]),
				float.Parse(split[13]));
			Vector3 p3 = new Vector3(float.Parse(split[14]),
				float.Parse(split[15]),
				float.Parse(split[16]));
			Vector3 p4 = new Vector3(float.Parse(split[17]),
				float.Parse(split[18]),
				float.Parse(split[19]));
			Vector3 p5 = new Vector3(float.Parse(split[20]),
				float.Parse(split[21]),
				float.Parse(split[22]));
			Vector3 p6 = new Vector3(float.Parse(split[23]),
				float.Parse(split[24]),
				float.Parse(split[25]));
			Vector3 p7 = new Vector3(float.Parse(split[26]),
				float.Parse(split[27]),
				float.Parse(split[28]));
			Vector3 p8 = new Vector3(float.Parse(split[29]),
				float.Parse(split[30]),
				float.Parse(split[31]));
			CollisionShapeBox box = new CollisionShapeBox(pos, size, p1, p2, p3, p4, p5, p6, p7, p8, "core");
			d.collisionBox.Add(box);
		});
		parsers.Add("AddTurretCollisionMesh", (split, d) =>
		{
			CollisionShapeBox box = new CollisionShapeBox(
				ParseVector(split[1]),
				ParseVector(split[2]),
				ParseVector(split[3]),
				ParseVector(split[4]),
				ParseVector(split[5]),
				ParseVector(split[6]),
				ParseVector(split[7]),
				ParseVector(split[8]),
				ParseVector(split[9]),
				ParseVector(split[10]),
				"core");
			d.collisionBox.Add(box);
		});
		parsers.Add("AddTurretCollisionMeshRaw", (split, d) =>
		{
			Vector3 pos = new Vector3(float.Parse(split[1]),
				float.Parse(split[2]),
				float.Parse(split[3]));
			Vector3 size = new Vector3(float.Parse(split[4]),
				float.Parse(split[5]),
				float.Parse(split[6]));
			Vector3 p1 = new Vector3(float.Parse(split[8]),
				float.Parse(split[9]),
				float.Parse(split[10]));
			Vector3 p2 = new Vector3(float.Parse(split[11]),
				float.Parse(split[12]),
				float.Parse(split[13]));
			Vector3 p3 = new Vector3(float.Parse(split[14]),
				float.Parse(split[15]),
				float.Parse(split[16]));
			Vector3 p4 = new Vector3(float.Parse(split[17]),
				float.Parse(split[18]),
				float.Parse(split[19]));
			Vector3 p5 = new Vector3(float.Parse(split[20]),
				float.Parse(split[21]),
				float.Parse(split[22]));
			Vector3 p6 = new Vector3(float.Parse(split[23]),
				float.Parse(split[24]),
				float.Parse(split[25]));
			Vector3 p7 = new Vector3(float.Parse(split[26]),
				float.Parse(split[27]),
				float.Parse(split[28]));
			Vector3 p8 = new Vector3(float.Parse(split[29]),
				float.Parse(split[30]),
				float.Parse(split[31]));
			CollisionShapeBox box = new CollisionShapeBox(pos, size, p1, p2, p3, p4, p5, p6, p7, p8, "turret");
			d.collisionBox.Add(box);
		});
		parsers.Add("LeftLinkPoint", (split, d) => d.leftTrackPoints.Add(ParseVector(split[1])));
		parsers.Add("RightLinkPoint", (split, d) => d.rightTrackPoints.Add(ParseVector(split[1])));
		parsers.Add("TrackLinkLength", (split, d) => d.trackLinkLength = float.Parse(split[1]));
		parsers.Add("RadarDetectableAltitude", (split, d) => d.radarDetectableAltitude = int.Parse(split[1]));
		parsers.Add("Stealth", (split, d) => d.stealth = split[1] == "True");
	}

	public override void preRead(TypeFile file, DriveableType obj)
	{
		foreach(string line in file.lines)
		{
			if(line == null)
				break;
			if(line.StartsWith("//"))
				continue;
			string[] split = line.Split(" ");
			if(split.Length < 2)
				continue;
			
			if(split[0] == "Passengers")
			{
				
				obj.numPassengers = int.Parse(split[1]);
				obj.seats = new Seat[obj.numPassengers + 1];
			}			
			if(split[0] == "NumWheels")
			{
				obj.wheelPositions = new DriveablePosition[int.Parse(split[1])];
			}
		}

		if(obj.seats == null)
		{
			Debug.LogWarning($"Failed to find 'Passengers' line in {file.name}");
		}
		if(obj.wheelPositions == null || obj.wheelPositions.Length == 0)
		{
			Debug.LogWarning($"Failed to find 'NumWheels' line in {file.name}");
			obj.wheelPositions = new DriveablePosition[4];
		}
	}

	public override void read(DriveableType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);

		if(parsers.TryGetValue(split[0], out ParseFunc func))
			func(split, obj);
	}
}

public class PlaneTypeImporter : TxtImporter<PlaneType>
{
	public static PlaneTypeImporter inst = new PlaneTypeImporter();
	private Dictionary<string, ParseFunc> parsers = new Dictionary<string, ParseFunc>();
	public PlaneTypeImporter()
	{
		parsers = new Dictionary<string, ParseFunc>();
		// Plane / Heli Mode
		parsers.Add("Mode", (split, d) => d.mode = System.Enum.Parse<EPlaneMode>(split[1].ToUpper()));
		// Yaw modifiers
		parsers.Add("TurnLeftSpeed", (split, d) => d.turnLeftModifier = float.Parse(split[1]));
		parsers.Add("TurnRightSpeed", (split, d) => d.turnRightModifier = float.Parse(split[1]));
		// Pitch modifiers
		parsers.Add("LookUpSpeed", (split, d) => d.lookUpModifier = float.Parse(split[1]));
		parsers.Add("LookDownSpeed", (split, d) => d.lookDownModifier = float.Parse(split[1]));
		// Roll modifiers
		parsers.Add("RollLeftSpeed", (split, d) => d.rollLeftModifier = float.Parse(split[1]));
		parsers.Add("RollRightSpeed", (split, d) => d.rollRightModifier = float.Parse(split[1]));
		
		// Lift
		parsers.Add("Lift", (split, d) => d.lift = float.Parse(split[1]));
		// Armaments
		parsers.Add("ShootDelay", (split, d) => d.planeShootDelay = int.Parse(split[1]));
		parsers.Add("BombDelay", (split, d) => d.planeBombDelay = int.Parse(split[1]));
		
		// Propellers
		parsers.Add("Propeller", (split, d) => 
		{
			Propeller propeller = new Propeller(int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]), split[5], split[6]);
			d.propellers.Add(propeller);
			d.driveableRecipe.Add(propeller.itemType);
		});
		parsers.Add("HeliPropeller", (split, d) => 
		{
			Propeller propeller = new Propeller(int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]), split[5], split[6]);
			d.heliPropellers.Add(propeller);
			d.driveableRecipe.Add(propeller.itemType);
		});
		parsers.Add("HeliTailPropeller", (split, d) => 
		{
			Propeller propeller = new Propeller(int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]), split[5], split[6]);
			d.heliTailPropellers.Add(propeller);
			d.driveableRecipe.Add(propeller.itemType);
		});
		
		// Sound
		parsers.Add("PropSoundLength", (split, d) => d.engineSoundLength = int.Parse(split[1]));
		parsers.Add("PropSound", (split, d) => { d.engineSound = split[1]; });
		parsers.Add("ShootSound", (split, d) => { d.shootSoundPrimary = split[1]; });
		parsers.Add("BombSound", (split, d) => { d.shootSoundSecondary = split[1]; });
		
		// Aesthetics
		parsers.Add("HasGear", (split, d) => d.hasGear = bool.Parse(split[1]));
		parsers.Add("HasDoor", (split, d) => d.hasDoor = bool.Parse(split[1]));
		parsers.Add("HasWing", (split, d) => d.hasWing = bool.Parse(split[1]));
		parsers.Add("RestingPitch", (split, d) => d.restingPitch = float.Parse(split[1]));
		
		parsers.Add("InflightInventory", (split, d) => d.invInflight = bool.Parse(split[1]));
	}

	public override void preRead(TypeFile file, PlaneType obj)
	{
		DriveableTypeImporter.inst.preRead(file, obj);
	}

	public override void read(PlaneType obj, string[] split, TypeFile file)
	{
		DriveableTypeImporter.inst.read(obj, split, file);

		if(parsers.TryGetValue(split[0], out ParseFunc func))
			func(split, obj);
		//else
			//Debug.LogWarning($"Plane parser didn't understand {split[0]}");
	}
}

public class VehicleTypeImporter : TxtImporter<VehicleType>
{
	public static VehicleTypeImporter inst = new VehicleTypeImporter();
	private Dictionary<string, ParseFunc> parsers = new Dictionary<string, ParseFunc>();
	public VehicleTypeImporter()
	{
		parsers = new Dictionary<string, ParseFunc>();
		parsers.Add("SquashMobs", (split, d) => d.squashMobs = bool.Parse(split[1]));
		parsers.Add("FourWheelDrive", (split, d) => d.fourWheelDrive = bool.Parse(split[1]));
		parsers.Add("Tank", (split, d) => d.tank = bool.Parse(split[1]));
		parsers.Add("TankMode", (split, d) => d.tank = bool.Parse(split[1]));
		parsers.Add("HasDoor", (split, d) => d.hasDoor = bool.Parse(split[1]));
		parsers.Add("RotateWheels", (split, d) => d.rotateWheels = bool.Parse(split[1]));
		parsers.Add("TurnLeftSpeed", (split, d) => d.turnLeftModifier = float.Parse(split[1]));
		parsers.Add("TurnRightSpeed", (split, d) => d.turnRightModifier = float.Parse(split[1]));
		parsers.Add("ShootDelay", (split, d) => d.shootDelaySecondary = int.Parse(split[1]));
		parsers.Add("ShellDelay", (split, d) => d.shootDelayPrimary = int.Parse(split[1]));
		parsers.Add("ShellSound", (split, d) => { d.shootSoundSecondary = split[1]; });
		parsers.Add("ShootSound", (split, d) => { d.shootSoundPrimary = split[1]; });
	}


	public override void preRead(TypeFile file, VehicleType obj)
	{
		DriveableTypeImporter.inst.preRead(file, obj);
	}

	public override void read(VehicleType obj, string[] split, TypeFile file)
	{
		DriveableTypeImporter.inst.read(obj, split, file);

		if(parsers.TryGetValue(split[0], out ParseFunc func))
			func(split, obj);
	}
}

public class AttachmentTypeImporter : TxtImporter<AttachmentType>
{
	public static AttachmentTypeImporter inst = new AttachmentTypeImporter();

	public override void read(AttachmentType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);

		if (split[0].Equals("AttachmentType"))
		{
			if (split[1].Equals("barrel")) obj.type = EAttachmentType.Barrel;
			else if (split[1].Equals("grip")) obj.type = EAttachmentType.Grip;
			else if (split[1].Equals("sights")) obj.type = EAttachmentType.Sights;
			else if (split[1].Equals("sight")) obj.type = EAttachmentType.Sights;
			else if (split[1].Equals("scope")) obj.type = EAttachmentType.Sights;
			else if (split[1].Equals("stock")) obj.type = EAttachmentType.Stock;
			else obj.type = EAttachmentType.Generic;
		}
			
//            else if (split[0].Equals("Model"))
//                model = FlansMod.proxy.loadModel(split[1], shortName, ModelAttachment.class);
		
		else if(split[0].Equals("Silencer"))
			obj.silencer = bool.Parse(split[1].ToLower());
		
		//Flashlight settings
		else if(split[0].Equals("Flashlight"))
			obj.flashlight = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("FlashlightRange"))
			obj.flashlightRange = float.Parse(split[1]);
		else if(split[0].Equals("FlashlightStrength"))
			obj.flashlightStrength = int.Parse(split[1]);
		//Mode override
		else if(split[0].Equals("ModeOverride"))
			obj.modeOverride = FireModes.Parse(split[1]);
		
		//Multipliers
		else if(split[0].Equals("MeleeDamageMultiplier"))
			obj.meleeDamageMultiplier = float.Parse(split[1]);
		else if(split[0].Equals("DamageMultiplier"))
			obj.damageMultiplier = float.Parse(split[1]);
		else if(split[0].Equals("SpreadMultiplier"))
			obj.spreadMultiplier = float.Parse(split[1]);
		else if(split[0].Equals("RecoilMultiplier"))
			obj.recoilMultiplier = float.Parse(split[1]);
		else if(split[0].Equals("BulletSpeedMultiplier"))
			obj.bulletSpeedMultiplier = float.Parse(split[1]);
		else if(split[0].Equals("ReloadTimeMultiplier"))
			obj.reloadTimeMultiplier = float.Parse(split[1]);
		//Scope Variables
		else if(split[0].Equals("ZoomLevel"))
			obj.zoomLevel = float.Parse(split[1]);
		else if(split[0].Equals("FOVZoomLevel"))
			obj.FOVZoomLevel = float.Parse(split[1]);
		else if (split[0].Equals("ZoomOverlay"))
		{
			obj.hasScopeOverlay = true;
			if (split[1].Equals("None"))
				obj.hasScopeOverlay = false;
			else obj.zoomOverlay = split[1];
		}
	}
}

public class RewardBoxTypeImporter : TxtImporter<RewardBox>
{
	public static RewardBoxTypeImporter inst = new RewardBoxTypeImporter();
	public override void read(RewardBox obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		if(KeyMatches(split, "AddPaintjob"))
		{
			obj.paintjobs.Add(split[2]);
		}
		else if(KeyMatches(split, "RarityWeight"))
		{
			EPaintjobRarity rarity = System.Enum.Parse<EPaintjobRarity>(split[1].ToUpper());
			float weight = float.Parse(split[2]);
			obj.weightPerRarity[(int)rarity] = weight;
		}
	}
}

public class TeamTypeImporter : TxtImporter<Team>
{
	public static TeamTypeImporter inst = new TeamTypeImporter();
	public override void read(Team obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		if(split[0].Equals("TeamColour"))
		{
			obj.teamColour = (int.Parse(split[1]) << 16) + ((int.Parse(split[2])) << 8) + ((int.Parse(split[3])));
		}
		if(split[0].Equals("TextColour"))
		{
			if(split[1].Equals("Black"))
				obj.textColour = '0';
			if(split[1].Equals("Blue"))
				obj.textColour = '1';
			if(split[1].Equals("Green"))
				obj.textColour = '2';
			if(split[1].Equals("Aqua"))
				obj.textColour = '3';
			if(split[1].Equals("Red"))
				obj.textColour = '4';
			if(split[1].Equals("Purple"))
				obj.textColour = '5';
			if(split[1].Equals("Orange"))
				obj.textColour = '6';
			if(split[1].Equals("LGrey"))
				obj.textColour = '7';
			if(split[1].Equals("Grey"))
				obj.textColour = '8';
			if(split[1].Equals("LBlue"))
				obj.textColour = '9';
			if(split[1].Equals("LGreen"))
				obj.textColour = 'a';
			if(split[1].Equals("LAqua"))
				obj.textColour = 'b';
			if(split[1].Equals("Red"))
				obj.textColour = 'c';
			if(split[1].Equals("Pink"))
				obj.textColour = 'd';
			if(split[1].Equals("Yellow"))
				obj.textColour = 'e';
			if(split[1].Equals("White"))
				obj.textColour = 'f';
		}
		if(split[0] == "Hat" || split[0] == "Helmet")
		{
			obj.hat = split[1];
		}
		if(split[0] == "Chest" || split[0] == "Top")
		{
			obj.chest = split[1];
		}
		if(split[0] == "Legs" || split[0] == "Bottom")
		{
			obj.legs = split[1];
		}
		if(split[0] == "Shoes" || split[0] == "Boots")
		{
			obj.shoes = split[1];
		}
		if(split[0] == "AddDefaultClass" || split[0] == "AddClass")
		{
			obj.classes.Add(split[1]);
		}
	}
}

public class PlayerClassTypeImporter : TxtImporter<PlayerClass>
{
	public static PlayerClassTypeImporter inst = new PlayerClassTypeImporter();
	public override void read(PlayerClass obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		if(split[0].Equals("AddItem"))
		{
			obj.startingItemStrings.Add(split);
		}
		if(split[0].Equals("SkinOverride"))
			obj.playerSkinOverride = split[1];
		if(split[0].Equals("Hat") || split[0].Equals("Helmet"))
		{
			obj.hat = split[1];
		}
		if(split[0].Equals("Chest") || split[0].Equals("Top"))
		{
			obj.chest = split[1];
		}
		if(split[0].Equals("Legs") || split[0].Equals("Bottom"))
		{
			obj.legs = split[1];
		}
		if(split[0].Equals("Shoes") || split[0].Equals("Boots"))
		{
			obj.shoes = split[1];
		}
	}
}

public class LoadoutPoolTypeImporter : TxtImporter<LoadoutPool>
{
	public static LoadoutPoolTypeImporter inst = new LoadoutPoolTypeImporter();
	public override void read(LoadoutPool obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		obj.XPForKill = Read(split, "XPForKill", obj.XPForKill);
		obj.XPForDeath = Read(split, "XPForDeath", obj.XPForDeath);
		obj.XPForKillstreakBonus = Read(split, "XPForKillstreakBonus", obj.XPForKillstreakBonus);
		
		if(KeyMatches(split, "MaxLevel"))
		{
			obj.maxLevel = int.Parse(split[1]);
			obj.XPPerLevel = new int[obj.maxLevel];
			obj.rewardsPerLevel = new List<string>[obj.maxLevel];
			for(int i = 0; i < obj.maxLevel; i++)
			{
				obj.XPPerLevel[i] = 10 * i;
				obj.rewardsPerLevel[i] = new List<string>();
			}
		}
		else if(KeyMatches(split, "XPPerLevel"))
		{
			for(int i = 0; i < obj.maxLevel; i++)
			{
				if(i + 1 < split.Length)
				{
					obj.XPPerLevel[i] = int.Parse(split[i + 1]);
				}
			}
		}
		else if(ParseLoadoutEntry("AddPrimary", ELoadoutSlot.primary, split, obj))
		{
		}
		else if(ParseLoadoutEntry("AddSecondary", ELoadoutSlot.secondary, split, obj))
		{
		}
		else if(ParseLoadoutEntry("AddSpecial", ELoadoutSlot.special, split, obj))
		{
		}
		else if(ParseLoadoutEntry("AddMelee", ELoadoutSlot.melee, split, obj))
		{
		}
		else if(ParseLoadoutEntry("AddArmour", ELoadoutSlot.armour, split, obj))
		{
		}
		else if(KeyMatches(split, "SlotUnlockLevels"))
		{
			for(int i = 0; i < 5; i++)
			{
				obj.slotUnlockLevels[i] = int.Parse(split[i + 1]);
			}
		}
		else if(KeyMatches(split, "DefaultLoadout"))
		{
			int index = int.Parse(split[1]) - 1;
			
			foreach(int i in System.Enum.GetValues(typeof(ELoadoutSlot)))
			{
				if(2 + i < split.Length)
				{
					if(obj.defaultLoadouts[index] == null)
						obj.defaultLoadouts[index] = new PlayerLoadout();
					obj.defaultLoadouts[index].slots[i] = split[2 + i];
				}
			}
		}
		else if(KeyMatches(split, "AddRewardBox"))
		{
			bool slotAvailable = false;
			for(int i = 0; i < 3; i++)
			{
				if(obj.rewardBoxes[i] == null)
				{
					obj.rewardBoxes[i] = split[1];
					slotAvailable = true;
					break;
				}
			}
			Debug.Assert(slotAvailable, "Trying to insert more than 3 reward box types. No support for this yet");
		}
		else if(KeyMatches(split, "AddReward"))
		{
			bool found = false;
			for(int i = 0; i < 3; i++)
			{
				if(split[1] == obj.rewardBoxes[i])
					found = true;
			}
			if(!found)
			{
				Debug.Assert(false, "Trying to give player reward box invalid for this loadout pool");
			}
			else
			{
				obj.rewardsPerLevel[int.Parse(split[2]) - 1].Add(split[1]);
			}
		}
	}
	private bool ParseLoadoutEntry(string keyword, ELoadoutSlot slot, string[] split, LoadoutPool obj)
	{
		if(KeyMatches(split, keyword))
		{
			LoadoutEntryInfoType entry = new LoadoutEntryInfoType();
			
			entry.type = split[1];
			entry.unlockLevel = int.Parse(split[2]);
			int numAdditionalItems = (split.Length - 3) / 2;
			for(int i = 0; i < numAdditionalItems; i++)
			{
				string stack = ConvertItemString(split[2 * i + 3], int.Parse(split[2 * i + 4]), 0);
				if(stack != null)
					entry.extraItems.Add(stack);
			}
			
			if(entry.type != null)
			{
				if(obj.unlocks[(int)slot] == null)
					obj.unlocks[(int)slot] = new List<LoadoutEntryInfoType>();
				obj.unlocks[(int)slot].Add(entry);
				return true;
			}
		}
		
		return false;
	}
}

public class PartTypeImporter : TxtImporter<PartType>
{
	public static PartTypeImporter inst = new PartTypeImporter();
	public override void read(PartType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		if(split[0].Equals("Category"))
			obj.category = System.Enum.Parse<EPartCategory>(split[1].ToUpper());
		else if(split[0].Equals("StackSize"))
			obj.stackSize = int.Parse(split[1]);
		else if(split[0].Equals("EngineSpeed"))
			obj.engineSpeed = float.Parse(split[1]);
		else if(split[0].Equals("FuelConsumption"))
			obj.fuelConsumption = float.Parse(split[1]);
		else if(split[0].Equals("Fuel"))
			obj.fuel = int.Parse(split[1]);
			//Recipe
		else if(split[0].Equals("PartBoxRecipe"))
		{
			string[] stacks = new string[(split.Length - 2) / 2];
			for(int i = 0; i < (split.Length - 2) / 2; i++)
			{
				int amount = int.Parse(split[2 * i + 2]);
				bool damaged = split[2 * i + 3].Contains(".");
				string itemName = damaged ? split[2 * i + 3].Split(".")[0] : split[2 * i + 3];
				int damage = damaged ? int.Parse(split[2 * i + 3].Split(".")[1]) : 0;
				stacks[i] = ConvertItemString(itemName, amount, damage);
			}
			obj.partBoxRecipe.AddRange(stacks);
		}
		else if(split[0].Equals("WorksWith"))
		{
			obj.worksWith = new List<EDefinitionType>();
			for(int i = 0; i < split.Length - 1; i++)
			{
				obj.worksWith.Add(System.Enum.Parse<EDefinitionType>(split[i + 1].TrimEnd('s')));
			}
		}
		
		//------- RedstoneFlux -------
		else if(split[0].Equals("UseRF") || split[0].Equals("UseRFPower"))
			obj.useRFPower = bool.Parse(split[1]);
		else if(split[0].Equals("RFDrawRate"))
			obj.RFDrawRate = int.Parse(split[1]);
			//-----------------------------
		
		else if(split[0].Equals("IsAIChip"))
			obj.isAIChip = bool.Parse(split[1]);
		else if(split[0].Equals("CanBeDefaultEngine"))
			obj.canBeDefaultEngine = bool.Parse(split[1]);
	}
}

public class GrenadeTypeImporter : TxtImporter<GrenadeType>
{
	public static GrenadeTypeImporter inst = new GrenadeTypeImporter();
	public override void read(GrenadeType obj, string[] split, TypeFile file)
	{
		ShootableTypeImporter.inst.read(obj, split, file);
		if(split[0].Equals("MeleeDamage"))
			obj.meleeDamage = int.Parse(split[1]);
			
			//Grenade Throwing
		else if(split[0].Equals("ThrowDelay"))
			obj.throwDelay = int.Parse(split[1]);
		else if(split[0].Equals("ThrowSound"))
			obj.throwSound = split[1];
		else if(split[0].Equals("DropItemOnThrow"))
			obj.dropItemOnThrow = split[1];
		else if(split[0].Equals("CanThrow"))
			obj.canThrow = bool.Parse(split[1]);
			
			//Grenade Physics
		else if(split[0].Equals("Bounciness"))
			obj.bounciness = float.Parse(split[1]);
		else if(split[0].Equals("PenetratesEntities"))
			obj.penetratesEntities = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("PenetratesBlocks"))
			obj.penetratesBlocks = bool.Parse(split[1].ToLower());
		
		else if(split[0].Equals("BounceSound"))
			obj.bounceSound = split[1];
		else if(split[0].Equals("Sticky"))
			obj.sticky = bool.Parse(split[1]);
		else if(split[0].Equals("LivingProximityTrigger"))
			obj.livingProximityTrigger = float.Parse(split[1]);
		else if(split[0].Equals("VehicleProximityTrigger"))
			obj.driveableProximityTrigger = float.Parse(split[1]);
		else if(split[0].Equals("DamageToTriggerer"))
			obj.damageToTriggerer = float.Parse(split[1]);
		else if(split[0].Equals("DetonateWhenShot"))
			obj.detonateWhenShot = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("PrimeDelay") || split[0].Equals("TriggerDelay"))
			obj.primeDelay = int.Parse(split[1]);
		
		else if(split[0].Equals("StickToThrower"))
			obj.stickToThrower = bool.Parse(split[1]);
		
		else if(split[0].Equals("ExplosionDamageVsLiving"))
			obj.explosionDamageVsLiving = float.Parse(split[1]);
		else if(split[0].Equals("ExplosionDamageVsDrivable"))
			obj.explosionDamageVsDriveable = float.Parse(split[1]);
		
		
		else if(split[0].Equals("NumExplodeParticles"))
			obj.explodeParticles = int.Parse(split[1]);
		else if(split[0].Equals("ExplodeParticles"))
			obj.explodeParticleType = split[1];
		else if(split[0].Equals("SmokeTime"))
			obj.smokeTime = int.Parse(split[1]);
		else if(split[0].Equals("SmokeParticles"))
			obj.smokeParticleType = split[1];
		else if(split[0].Equals("SmokeEffect"))
			obj.smokeEffects.Add(ConvertPotionString(split));
		else if(split[0].Equals("SmokeRadius"))
			obj.smokeRadius = float.Parse(split[1]);
		else if(split[0].Equals("SpinWhenThrown"))
			obj.spinWhenThrown = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("Remote"))
			obj.remote = bool.Parse(split[1].ToLower());
			
			//Deployable Bag Stuff
		else if(split[0].Equals("DeployableBag"))
			obj.isDeployableBag = true;
		else if(split[0].Equals("NumUses"))
			obj.numUses = int.Parse(split[1]);
		else if(split[0].Equals("HealAmount"))
			obj.healAmount = float.Parse(split[1]);
		else if(split[0].Equals("AddPotionEffect") || split[0].Equals("PotionEffect"))
			obj.potionEffects.Add(ConvertPotionString(split));
		else if(split[0].Equals("NumClips"))
			obj.numClips = int.Parse(split[1]);
	}
}

public class AAGunTypeImporter : TxtImporter<AAGunType>
{
	public static AAGunTypeImporter inst = new AAGunTypeImporter();
	public override void read(AAGunType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		obj.damage = Read(split, "Damage", obj.damage);
		obj.reloadTime = Read(split, "ReloadTime", obj.reloadTime);
		obj.recoil = Read(split, "Recoil", obj.recoil);
		obj.accuracy = Read(split, "Accuracy", obj.accuracy);
		obj.shootDelay = Read(split, "ShootDelay", obj.shootDelay);
		obj.fireAlternately = Read(split, "FireAlternately", obj.fireAlternately);
		obj.health = Read(split, "Health", obj.health);
		obj.topViewLimit = Read(split, "TopViewLimit", obj.topViewLimit);
		obj.bottomViewLimit = Read(split, "BottomViewLimit", obj.bottomViewLimit);
		obj.targetMobs = Read(split, "TargetMobs", obj.targetMobs);
		obj.targetPlayers = Read(split, "TargetPlayers", obj.targetPlayers);
		obj.targetVehicles = Read(split, "TargetVehicles", obj.targetVehicles);
		obj.targetPlanes = Read(split, "TargetPlanes", obj.targetPlanes);
		obj.targetMechas = Read(split, "TargetMechas", obj.targetMechas);
		obj.shareAmmo = Read(split, "ShareAmmo", obj.shareAmmo);
		obj.targetRange = Read(split, "TargetRange", obj.targetRange);
		obj.bottomViewLimit = Read(split, "BottomViewLimit", obj.bottomViewLimit);
		
		if(split[0].Equals("TargetDriveables"))
			obj.targetMechas = obj.targetPlanes = obj.targetVehicles = bool.Parse(split[1]);
		
		if(split[0].Equals("ShootSound"))
		{
			obj.shootSound = split[1];
		}
		if(split[0].Equals("ReloadSound"))
		{
			obj.reloadSound = split[1];
		}
		if(split[0].Equals("NumBarrels"))
		{
			obj.numBarrels = int.Parse(split[1]);
			obj.barrelX = new int[obj.numBarrels];
			obj.barrelY = new int[obj.numBarrels];
			obj.barrelZ = new int[obj.numBarrels];
		}
		if(split[0].Equals("Barrel"))
		{
			int id = int.Parse(split[1]);
			obj.barrelX[id] = int.Parse(split[2]);
			obj.barrelY[id] = int.Parse(split[3]);
			obj.barrelZ[id] = int.Parse(split[4]);
		}
		if(split[0].Equals("Health"))
		{
			obj.health = int.Parse(split[1]);
		}
		if(split[0].Equals("Ammo"))
		{
			obj.ammo.Add(split[1]);
		}
		if(split[0].Equals("GunnerPos"))
		{
			obj.gunnerX = int.Parse(split[1]);
			obj.gunnerY = int.Parse(split[2]);
			obj.gunnerZ = int.Parse(split[3]);
		}
	}
}

public class MechaItemTypeImporter : TxtImporter<MechaItemType>
{
	public static MechaItemTypeImporter inst = new MechaItemTypeImporter();
	public override void read(MechaItemType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		if(split[0].Equals("Type"))
			obj.type = System.Enum.Parse<EMechaItemType>(split[1]);
		if(split[0].Equals("ToolType"))
			obj.function = System.Enum.Parse<EMechaToolType>(split[1]);
		if(split[0].Equals("Speed"))
			obj.speed = float.Parse(split[1]);
		if(split[0].Equals("ToolHardness"))
			obj.toolHardness = float.Parse(split[1]);
		if(split[0].Equals("Reach"))
			obj.reach = float.Parse(split[1]);
		
		/** The following are the upgrade booleans and multipliers, which
			*  are alphabetised. Mess with the order at your peril*/
		
		if(split[0].Equals("AutoFuel"))
			obj.autoCoal = bool.Parse(split[1].ToLower());
		if(split[0].Equals("Armour"))
			obj.damageResistance = float.Parse(split[1]);
		if(split[0].Equals("CoalMultiplier"))
			obj.fortuneCoal = float.Parse(split[1]);
		if(split[0].Equals("DetectSound"))
			obj.detectSound = split[1];
		if(split[0].Equals("DiamondDetect"))
			obj.diamondDetect = bool.Parse(split[1].ToLower());
		if(split[0].Equals("DiamondMultiplier"))
			obj.fortuneDiamond = float.Parse(split[1]);
		if(split[0].Equals("EmeraldMultiplier"))
			obj.fortuneEmerald = float.Parse(split[1]);
		if(split[0].Equals("FlameBurst"))
			obj.flameBurst = bool.Parse(split[1].ToLower());
		if(split[0].Equals("Floatation"))
			obj.floater = bool.Parse(split[1].ToLower());
		if(split[0].Equals("ForceBlockFallDamage"))
			obj.forceBlockFallDamage = bool.Parse(split[1].ToLower());
		if(split[0].Equals("ForceDark"))
			obj.forceDark = bool.Parse(split[1].ToLower());
		if(split[0].Equals("InfiniteAmmo"))
			obj.infiniteAmmo = bool.Parse(split[1].ToLower());
		if(split[0].Equals("IronMultiplier"))
			obj.fortuneIron = float.Parse(split[1]);
		if(split[0].Equals("IronRefine"))
			obj.refineIron = bool.Parse(split[1].ToLower());
		if(split[0].Equals("ItemVacuum"))
			obj.vacuumItems = bool.Parse(split[1].ToLower());
		if(split[0].Equals("LightLevel"))
			obj.lightLevel = int.Parse(split[1]);
		if(split[0].Equals("Nanorepair"))
			obj.autoRepair = bool.Parse(split[1].ToLower());
		if(split[0].Equals("RedstoneMultiplier"))
			obj.fortuneRedstone = float.Parse(split[1]);
		if(split[0].Equals("RocketPack"))
			obj.rocketPack = bool.Parse(split[1].ToLower());
		if(split[0].Equals("RocketPower"))
			obj.rocketPower = float.Parse(split[1]);
		if(split[0].Equals("SoundEffect"))
			obj.soundEffect = split[1];
		if(split[0].Equals("SoundTime"))
			obj.soundTime = float.Parse(split[1]);
		if(split[0].Equals("SpeedMultiplier"))
			obj.speedMultiplier = float.Parse(split[1]);
		if(split[0].Equals("StopMechaFallDamage"))
			obj.stopMechaFallDamage = bool.Parse(split[1].ToLower());
		if(split[0].Equals("WasteCompact"))
			obj.wasteCompact = bool.Parse(split[1].ToLower());
	}
}

public class MechaTypeImporter : TxtImporter<MechaType>
{
	public static MechaTypeImporter inst = new MechaTypeImporter();
	public override void preRead(TypeFile file, MechaType obj)
	{
		DriveableTypeImporter.inst.preRead(file, obj);
	}

	public override void read(MechaType obj, string[] split, TypeFile file)
	{
		DriveableTypeImporter.inst.read(obj, split, file);
		//Movement modifiers
		if(split[0].Equals("TurnLeftSpeed"))
			obj.turnLeftModifier = float.Parse(split[1]);
		if(split[0].Equals("TurnRightSpeed"))
			obj.turnRightModifier = float.Parse(split[1]);
		if(split[0].Equals("MoveSpeed"))
			obj.moveSpeed = float.Parse(split[1]);
		if(split[0].Equals("SquashMobs"))
			obj.squashMobs = bool.Parse(split[1].ToLower());
		if(split[0].Equals("StepHeight"))
			obj.stepHeight = int.Parse(split[1]);
		if(split[0].Equals("JumpHeight"))
		{
			obj.jumpHeight = float.Parse(split[1]);
			obj.jumpVelocity = (float)Mathf.Sqrt(Mathf.Abs(9.81F * (obj.jumpHeight + 0.2F) / 200F));
		}
		if(split[0].Equals("RotateSpeed"))
			obj.rotateSpeed = float.Parse(split[1]);
		
		if(split[0].Equals("LeftArmOrigin"))
			obj.leftArmOrigin = new Vector3(float.Parse(split[1]) / 16F, float.Parse(split[2]) / 16F, float.Parse(split[3]) / 16F);
		if(split[0].Equals("RightArmOrigin"))
			obj.rightArmOrigin = new Vector3(float.Parse(split[1]) / 16F, float.Parse(split[2]) / 16F, float.Parse(split[3]) / 16F);
		if(split[0].Equals("ArmLength"))
			obj.armLength = float.Parse(split[1]) / 16F;
		if(split[0].Equals("LegLength"))
			obj.legLength = float.Parse(split[1]) / 16F;
		if(split[0].Equals("LegTrans"))
			obj.LegTrans = float.Parse(split[1]) / 16F;
		if(split[0].Equals("RearLegLength"))
			obj.RearlegLength = float.Parse(split[1]) / 16F;
		if(split[0].Equals("FrontLegLength"))
			obj.FrontlegLength = float.Parse(split[1]) / 16F;
		if(split[0].Equals("RearLegTrans"))
			obj.RearLegTrans = float.Parse(split[1]) / 16F;
		if(split[0].Equals("FrontLegTrans"))
			obj.FrontLegTrans = float.Parse(split[1]) / 16F;
		if(split[0].Equals("HeldItemScale"))
			obj.heldItemScale = float.Parse(split[1]);
		if(split[0].Equals("Height"))
			obj.height = (float.Parse(split[1]) / 16F);
		if(split[0].Equals("Width"))
			obj.width = (float.Parse(split[1]) / 16F);
		if(split[0].Equals("ChassisHeight"))
			obj.chassisHeight = (int.Parse(split[1])) / 16F;
		if(split[0].Equals("FallDamageMultiplier"))
			obj.fallDamageMultiplier = float.Parse(split[1]);
		if(split[0].Equals("BlockDamageFromFalling"))
			obj.blockDamageFromFalling = float.Parse(split[1]);
		if(split[0].Equals("Reach"))
			obj.reach = float.Parse(split[1]);
		if(split[0].Equals("TakeFallDamage"))
			obj.takeFallDamage = bool.Parse(split[1].ToLower());
		if(split[0].Equals("DamageBlocksFromFalling"))
			obj.damageBlocksFromFalling = bool.Parse(split[1].ToLower());
		if(split[0].Equals("LegSwingLimit"))
			obj.legSwingLimit = float.Parse(split[1]);
		if(split[0].Equals("LimitHeadTurn"))
		{
			obj.limitHeadTurn = bool.Parse(split[1].ToLower());
			obj.limitHeadTurnValue = float.Parse(split[2]);
		}
		if(split[0].Equals("LegSwingTime"))
			obj.legSwingTime = float.Parse(split[1]);
		if(split[0].Equals("UpperArmLimit"))
			obj.upperArmLimit = float.Parse(split[1]);
		if(split[0].Equals("LowerArmLimit"))
			obj.lowerArmLimit = float.Parse(split[1]);
		if(split[0].Equals("LeftHandModifier"))
		{
			obj.leftHandModifierX = float.Parse(split[1]) / 16F;
			obj.leftHandModifierY = float.Parse(split[2]) / 16F;
			obj.leftHandModifierZ = float.Parse(split[3]) / 16F;
		}
		if(split[0].Equals("RightHandModifier"))
		{
			obj.rightHandModifierX = float.Parse(split[1]) / 16F;
			obj.rightHandModifierY = float.Parse(split[2]) / 16F;
			obj.rightHandModifierZ = float.Parse(split[3]) / 16F;
		}
	}
}

public class ToolTypeImporter : TxtImporter<ToolType>
{
	public static ToolTypeImporter inst = new ToolTypeImporter();
	public override void read(ToolType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		if(split[0].Equals("Parachute"))
			obj.parachute = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("ExplosiveRemote"))
			obj.remote = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("Heal") || split[0].Equals("HealPlayers"))
			obj.healPlayers = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("Repair") || split[0].Equals("RepairVehicles"))
			obj.healDriveables = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("HealAmount") || split[0].Equals("RepairAmount"))
			obj.healAmount = int.Parse(split[1]);
		else if(split[0].Equals("ToolLife") || split[0].Equals("ToolUses"))
			obj.toolLife = int.Parse(split[1]);
		else if(split[0].Equals("EUPerCharge"))
			obj.EUPerCharge = int.Parse(split[1]);
		else if(split[0].Equals("RechargeRecipe"))
		{
			for(int i = 0; i < (split.Length - 1) / 2; i++)
			{
				int amount = int.Parse(split[2 * i + 1]);
				bool damaged = split[2 * i + 2].Contains(".");
				string itemName = damaged ? split[2 * i + 2].Split(".")[0] : split[2 * i + 2];
				int damage = damaged ? int.Parse(split[2 * i + 2].Split(".")[1]) : 0;
				obj.rechargeRecipe.Add(ConvertItemString(itemName, amount, damage));
			}
		}
		else if(split[0].Equals("DestroyOnEmpty"))
			obj.destroyOnEmpty = bool.Parse(split[1].ToLower());
		else if(split[0].Equals("Food") || split[0].Equals("Foodness"))
			obj.foodness = int.Parse(split[1]);
	}
}

public class ArmourTypeImporter : TxtImporter<ArmourType>
{
	public static ArmourTypeImporter inst = new ArmourTypeImporter();
	public override void read(ArmourType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		if(split[0].Equals("Type"))
		{
			if(split[1].Equals("Hat") || split[1].Equals("Helmet"))
				obj.type = 0;
			if(split[1].Equals("Chest") || split[1].Equals("Body"))
				obj.type = 1;
			if(split[1].Equals("Legs") || split[1].Equals("Pants"))
				obj.type = 2;
			if(split[1].Equals("Shoes") || split[1].Equals("Boots"))
				obj.type = 3;
		}
		
		obj.defence = Read(split, "DamageReduction", obj.defence);
		obj.defence = Read(split, "Defence", obj.defence);
		obj.moveSpeedModifier = Read(split, "MoveSpeedModifier", obj.moveSpeedModifier);
		obj.moveSpeedModifier = Read(split, "Slowness", obj.moveSpeedModifier);
		obj.jumpModifier = Read(split, "JumpModifier", obj.jumpModifier);
		obj.knockbackModifier = Read(split, "KnockbackReduction", obj.knockbackModifier);
		obj.knockbackModifier = Read(split, "KnockbackModifier", obj.knockbackModifier);
		obj.nightVision = Read(split, "NightVision", obj.nightVision);
		obj.negateFallDamage = Read(split, "NegateFallDamage", obj.negateFallDamage);
		obj.overlay = Read(split, "Overlay", obj.overlay);
		obj.smokeProtection = Read(split, "SmokeProtection", obj.smokeProtection);
		obj.armourTextureName = Read(split, "ArmourTexture", obj.armourTextureName);
		obj.armourTextureName = Read(split, "ArmorTexture", obj.armourTextureName);
		obj.Enchantability = Read(split, "Enchantability", obj.Enchantability);
		obj.Toughness = Read(split, "Toughness", obj.Toughness);
		obj.Durability = Read(split, "Durability", obj.Durability);
		obj.DamageReductionAmount = Read(split, "DamageReductionAmount", obj.DamageReductionAmount);
	}
}

public class BoxTypeImporter : TxtImporter<BoxType>
{
	public static BoxTypeImporter inst = new BoxTypeImporter();
	public override void read(BoxType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		obj.topTexturePath = Read(split, "TopTexture", obj.topTexturePath);
		obj.bottomTexturePath = Read(split, "BottomTexture", obj.bottomTexturePath);
		obj.sideTexturePath = Read(split, "SideTexture", obj.sideTexturePath);
	}
}

public class ArmourBoxTypeImporter : TxtImporter<ArmourBoxType>
{
	public static ArmourBoxTypeImporter inst = new ArmourBoxTypeImporter();
	public override void read(ArmourBoxType obj, string[] split, TypeFile file)
	{
		BoxTypeImporter.inst.read(obj, split, file);
		if(split[0].ToLower().Equals("addarmour") || split[0].ToLower().Equals("addarmor"))
		{
			string name = split[2];
			for(int i = 3; i < split.Length; i++)
				name = name + " " + split[i];
			ArmourBoxEntry entry = new ArmourBoxEntry(split[1], name);
			//Read the next 4 lines for each armour piece
			for(int i = 0; i < 4; i++)
			{
				string line = null;
				line = file.readLine();
				if(line == null)
					continue;
				if(line.StartsWith("//"))
				{
					i--;
					continue;
				}
				string[] lineSplit = line.Split(" ");
				entry.armours[i] = lineSplit[0];
				for(int j = 0; j < (lineSplit.Length - 1) / 2; j++)
				{
					string stack = "";
					if(lineSplit[j * 2 + 1].Contains("."))
						stack = ConvertItemString(lineSplit[j * 2 + 1].Split(".")[0], int.Parse(lineSplit[j * 2 + 2]), int.Parse(lineSplit[j * 2 + 1].Split(".")[1]));
					else
						stack = ConvertItemString(lineSplit[j * 2 + 1], int.Parse(lineSplit[j * 2 + 2]), 0);
					
					if(stack != null && stack.Length > 0)
						entry.requiredStacks[i].Add(stack);
				}
			}
			
			obj.pages.Add(entry);
		}
	}
}

public class GunBoxTypeImporter : TxtImporter<GunBoxType>
{
	public static GunBoxTypeImporter inst = new GunBoxTypeImporter();
	public override void read(GunBoxType obj, string[] split, TypeFile file)
	{
		BoxTypeImporter.inst.read(obj, split, file);

		//Sets the current page of the reader.
		if(split[0].Equals("SetPage"))
		{
			string pageName = split[1];
			for(int i = 2; i < split.Length; i++)
				pageName += " " + split[i];
			if(!obj.pagesByTitle.ContainsKey(pageName))
			{
				obj.currentPage = new GunBoxPage(pageName);
				obj.pagesByTitle.Add(pageName, obj.currentPage);
			}
			obj.pages.Add(obj.currentPage);
			
		}
		//Add an info type at the top level.
		else if(split[0].Equals("AddGun") || split[0].Equals("AddType"))
		{
			if (obj.currentPage == null)
			{
				obj.currentPage = new GunBoxPage("default");
				obj.pagesByTitle.Add("default", obj.currentPage);
			}
			obj.currentPage.addNewEntry(split[1], ConvertRecipeString(split));
		}
		//Add a subtype (such as ammo) to the current top level InfoType
		else if(split[0].Equals("AddAmmo") || split[0].Equals("AddAltType") || split[0].Equals("AddAltAmmo") || split[0].Equals("AddAlternateAmmo"))
		{
			obj.currentPage.addAmmoToCurrentEntry(split[1], ConvertRecipeString(split));
		}
	}
}

public class ItemHolderTypeImporter : TxtImporter<ItemHolderType>
{
	public static ItemHolderTypeImporter inst = new ItemHolderTypeImporter();
	public override void read(ItemHolderType obj, string[] split, TypeFile file)
	{
		InfoTypeImporter.inst.read(obj, split, file);
		// Nothing here
	}
}