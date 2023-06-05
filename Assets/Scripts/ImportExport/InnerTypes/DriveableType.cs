using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilotGun : DriveablePosition
{
	public string type;
	
	public PilotGun(string[] split) : base(split)
	{
		type = split[5];
	}
}

public class CollisionBox
{
	public float x, y, z;
	public float w, h, d;
	public int health;
	public string part;
	
	public CollisionBox(int health, int x, int y, int z, int w, int h, int d)
	{
		this.health = health;
		this.x = x / 16F;
		this.y = y / 16F;
		this.z = z / 16F;
		this.w = w / 16F;
		this.h = h / 16F;
		this.d = d / 16F;
	}
	
	public double Max()
	{
		double xMax = Mathf.Max(x, x + w);
		double yMax = Mathf.Max(y, y + h);
		double zMax = Mathf.Max(z, z + d);
		return Mathf.Sqrt((float)(xMax * xMax + yMax * yMax + zMax * zMax));
	}
	
	/**
	 * @return The centre (in global co-ordinates)
	 */
	public Vector3 getCentre()
	{
		return new Vector3(x + w / 2F, y + h / 2F, z + d / 2F);
	}
}

public class CollisionShapeBox {
	public Vector3 pos, size;
	public Vector3 p1,p2,p3,p4,p5,p6,p7,p8;
	public string part;
	
	
	public CollisionShapeBox(Vector3 position, Vector3 boxsize, Vector3 p1mod, 
		Vector3 p2mod, Vector3 p3mod, Vector3 p4mod, Vector3 p5mod, 
		Vector3 p6mod, Vector3 p7mod, Vector3 p8mod, string driveablePart)
	{
		this.pos = new Vector3(position.x/16, -(position.y)/16 - (10F/16F), position.z/16);
		this.size = new Vector3(boxsize.x/16, boxsize.y/16, boxsize.z/16);
		this.p1 = new Vector3(p1mod.x/16, p1mod.y/16, p1mod.z/16);
		this.p2 = new Vector3(p2mod.x/16, p2mod.y/16, p2mod.z/16);
		this.p3 = new Vector3(p3mod.x/16, p3mod.y/16, p3mod.z/16);
		this.p4 = new Vector3(p4mod.x/16, p4mod.y/16, p4mod.z/16);
		this.p5 = new Vector3(p5mod.x/16, p5mod.y/16, p5mod.z/16);
		this.p6 = new Vector3(p6mod.x/16, p6mod.y/16, p6mod.z/16);
		this.p7 = new Vector3(p7mod.x/16, p7mod.y/16, p7mod.z/16);
		this.p8 = new Vector3(p8mod.x/16, p8mod.y/16, p8mod.z/16);
		this.part = driveablePart;
	}
}

public class DriveablePosition
{
	public static DriveablePosition Parse(string[] split)
	{
		//Its a gun with a type
		if(split.Length == 6)
		{
			return new PilotGun(split);
		}
		else if(split.Length == 5)
		{
			return new DriveablePosition(split);
		}
		return new DriveablePosition(new Vector3(), "core");
	}

	public Vector3 position;
	public string part;
	
	public DriveablePosition(Vector3 v, string p)
	{
		position = v;
		part = p;
	}
	
	public DriveablePosition(string[] split)
	{
		position = new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3])) / 16f;
		part = split[4];
	}
}

public class ShootPoint
{
	public DriveablePosition rootPos;
	public Vector3 offPos;
	
	
	public ShootPoint(DriveablePosition driverPos, Vector3 offsetPos)
	{
		rootPos = driverPos;
		offPos = offsetPos;
	}
}

public class ShootParticle
{
	public ShootParticle(string s, float x1, float y1, float z1)
	{
		x = x1;
		y = y1;
		z = z1;
		name = s;
	}
	
	float x = 0, y = 0, z = 0;
	string name;
}

public class ParticleEmitter
{
	public string effectType;
	public int emitRate;
	public Vector3 origin;
	public Vector3 extents;
	public Vector3 velocity;
	public float minThrottle;
	public float maxThrottle;
	public string part;
	public float minHealth;
	public float maxHealth;
}

public enum EWeaponType
{
	MISSILE, BOMB, SHELL, MINE, GUN, NONE
}

public class DriveableType : PaintableType
{
	//Health and recipe
	/** Health of each driveable part */
	public Dictionary<string, CollisionBox> health = new Dictionary<string, CollisionBox>();
	/** Recipe parts associated to each driveable part */
	public Dictionary<string, string[]> partwiseRecipe = new Dictionary<string, string[]>();
	/** Recipe parts as one complete list */
	public List<string> driveableRecipe = new List<string>();
	
	//Ammo
	/** If true, then all ammo is accepted. Default is true to minimise backwards compatibility issues */
	public bool acceptAllAmmo = true;
	/** The list of bullet types that can be used in this driveable for the main gun (tank shells, plane bombs etc) */
	public List<string> ammo = new List<string>();
	
	//Harvesting variables
	/** If true, then this vehicle harvests blocks from the harvester hitbox and places them in the inventory */
	public bool harvestBlocks = false;
	/** What materials this harvester eats */
	public List<string> materialsHarvested = new List<string>();
	public bool collectHarvest = false;
	public bool dropHarvest = false;
	public Vector3 harvestBoxSize = new Vector3(0, 0, 0);
	public Vector3 harvestBoxPos = new Vector3(0, 0, 0);
	public int reloadSoundTick = 15214541;
	public float fallDamageFactor = 1.0F;
	
	//Weapon variables
	/** The weapon type assigned to left mouse */
	public EWeaponType primary = EWeaponType.NONE, secondary = EWeaponType.NONE;
	/** Whether to alternate weapons or fire all at once */
	public bool alternatePrimary = false, alternateSecondary = false;
	/** Delays. Can override gun delays */
	public int shootDelayPrimary = 1, shootDelaySecondary = 1;
	/** Firing modes for primary and secondary guns. Minigun also an option */
	public EFireMode modePrimary = EFireMode.FullAuto, modeSecondary = EFireMode.FullAuto;
	/** Damage modifiers, so that different vehicles firing the same weapons can do different damage */
	public int damageModifierPrimary = 1, damageModifierSecondary = 1;
	
	/** Positions of primary and secondary weapons */
	public List<ShootPoint> shootPointsPrimary = new List<ShootPoint>();
	public List<ShootPoint> shootPointsSecondary = new List<ShootPoint>();
	/** Pilot guns also have their own separate array so ammo handling can be done */
	public List<PilotGun> pilotGuns = new List<PilotGun>();
	
	/** Sounds */
	public string shootSoundPrimary, shootSoundSecondary;
	public int reloadTimePrimary = 0, reloadTimeSecondary = 0;
	public string reloadSoundPrimary = "", reloadSoundSecondary = "";
	public int placeTimePrimary = 5, placeTimeSecondary = 5;
	public string placeSoundPrimary = "", placeSoundSecondary = "";
	
	//Passengers
	/** The number of passengers, not including the pilot */
	public int numPassengers = 0;
	/** Seat objects for holding information about the position and gun setup of each seat */
	public Seat[] seats;
	/** Automatic counter used to setup ammo inventory for gunners */
	public int numPassengerGunners = 0;
	
	//Rendering variables
	/** Inventory sizes */
	public int numCargoSlots, numBombSlots, numMissileSlots;
	/** The fuel tank size */
	public int fuelTankSize = 100;
	
	//Rendering variables
	/** The yOffset of the model. Shouldn't be needed if you made your model properly */
	public float yOffset = 10F / 16F;
	/** Third person render distance */
	public float cameraDistance = 5F;
	
	//Particle system
	/** A list of ambient particle emitters on this vehicle */
	public List<ParticleEmitter> emitters = new List<ParticleEmitter>();
	// Shoot particles
	public float vehicleGunModelScale = 1f;
	
	public double hitboxRadius = 0d;
	

	
	public List<ShootParticle> shootParticlesPrimary = new List<ShootParticle>();
	public List<ShootParticle> shootParticlesSecondary = new List<ShootParticle>();
	
	//Movement variables
	/** Generic movement modifiers, no longer repeated for plane and vehicle */
	public float maxThrottle = 1F, maxNegativeThrottle = 0F;
	public float ClutchBrake = 0F;
	/** The origin of the tank turret */
	public Vector3 turretOrigin = new Vector3();
	public Vector3 turretOriginOffset = new Vector3();
	
	/** Wheel positions */
	public DriveablePosition[] wheelPositions = new DriveablePosition[0];
	/** Strength of springs connecting car to wheels */
	public float wheelSpringStrength = 0.5F;
	/** The wheel radius for onGround checks */
	public float wheelStepHeight = 1.0F;
	/** Whether or not the vehicle rolls */
	public bool canRoll = true;
	/**
	 *
	 */
	public float turretRotationSpeed = 2.5F;
	
	/** Collision points for block based collisions */
	public List<DriveablePosition> collisionPoints = new List<DriveablePosition>();
	
	/** Coefficient of drag */
	public float drag = 1F;
	
	//Boat Stuff
	/** If true, then the vehicles wheels float on water */
	public bool floatOnWater = false;
	/** Defines where you can place this vehicle */
	public bool placeableOnLand = true, placeableOnWater = false, placeableOnSponge = false;
	/** The upwards force to apply to the vehicle per wheel when on water */
	public float buoyancy = 0.0165F;
	public float floatOffset = 0;
	
	/** The radius within which to check for bullets */
	public float bulletDetectionRadius = 5F;
	
	/** Plane is shown on ICBM Radar and engaged by AA Guns */
	public bool onRadar = false;
	
	/** Track animation frames */
	public int animFrames = 2;
	
	/** Sounds */
	public string startSound = "";
	public int startSoundLength;
	public string engineSound = "";
	public int engineSoundLength;
	
	public bool collisionDamageEnable = false;
	public float collisionDamageThrottle = 0;
	public float collisionDamageTimes = 0;
	
	public bool enableReloadTime = false;
	
	public bool canMountEntity = false;
	
	public float bulletSpread = 0F;
	public float bulletSpeed = 3F;
	public bool rangingGun = false;
	
	public bool isExplosionWhenDestroyed = false;
	
	public string lockedOnSound = "";
	
	public int canLockOnAngle = 10;
	public int lockOnSoundTime = 60;
	public int lockedOnSoundRange = 5;
	public string lockingOnSound = "";
	
	public bool lockOnToPlanes = false, lockOnToVehicles = false, lockOnToMechas = false, lockOnToPlayers = false, lockOnToLivings = false;
	
	//flares
	public bool hasFlare = false;
	public int flareDelay = 20 * 10;
	public string flareSound = "";
	public int timeFlareUsing = 1;
	
	// radar (for mapwriter)
	/**
	 * The height of the entity that can be detected by radar.<br> -1 = It does not detect.<br>
	 */
	public int radarDetectableAltitude = -1;
	public bool stealth = false;
	
	/** Barrel Recoil stuff */
	public float recoilDist = 5F;
	public float recoilTime = 5F;
	
	/** more nonsense */
	public bool fixedPrimaryFire = false;
	public Vector3 primaryFireAngle = new Vector3(0, 0, 0);
	
	/** backwards compatibility attempt */
	public float gunLength = 0;
	
	
	public bool setPlayerInvisible = false;
	
	public float maxThrottleInWater = 0.5F;
	
	public List<Vector3> leftTrackPoints = new List<Vector3>();
	public List<Vector3> rightTrackPoints = new List<Vector3>();
	public float trackLinkLength = 0;
	
	/** activator bool for IT-1 reloads */
	public bool IT1 = false;
	
	public List<CollisionShapeBox> collisionBox = new List<CollisionShapeBox>();
	public bool fancyCollision = false;
}
