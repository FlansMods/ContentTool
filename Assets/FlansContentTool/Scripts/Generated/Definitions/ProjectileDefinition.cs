using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ProjectileDefinition : Element
{
	[JsonField]
	[Tooltip("Number of bullet entities to create")]
	public int shotCount = 1;
	[JsonField]
	[Tooltip("The radius within which to apply splash effects. If 0, any Impacts on splash won't trigger")]
	public float splashRadius = 0.0f;
	[JsonField]
	[Tooltip("Impact settings. You probably want at least a ShotPosition, or ShotEntity and ShotBlock")]
	public ImpactDefinition[] impacts = new ImpactDefinition[0];
	[JsonField]
	[Tooltip("The speed (blocks/s) with which this projectile leaves the gun")]
	public float launchSpeed = 3.0f;
	[JsonField]
	[Tooltip("Acceleration of the projectile in blocks/s per tick")]
	public float acceleration = 0.0f;
	[JsonField]
	[Tooltip("The maximum linear speed of the projectile")]
	public float maxSpeed = 3.0f;
	[JsonField]
	[Tooltip("How much does gravity affect this? 0=Not at all, 1=Regular")]
	public float gravityFactor = 1.0f;
	[JsonField]
	public EProjectileResponseType responseToBlock = EProjectileResponseType.Bounce;
	[JsonField]
	public EProjectileResponseType responseToEntity = EProjectileResponseType.Detonate;
	[JsonField]
	public EProjectileResponseType responseToVehicle = EProjectileResponseType.Detonate;
	[JsonField]
	[Tooltip("If set to a non-zero amount, this projectile will have a fuse timer, in seconds")]
	public float fuseTime = 0.0f;
	[JsonField]
	[Tooltip("The guidance mode of the projectile. none/beam_riding/lock_on/lock_on_proportional")]
	public string guidanceType = "none";
	[JsonField]
	[Tooltip("The size of the cone in which the projectile can lock onto a new target")]
	public float lockRange = 128f;
	[JsonField]
	[Tooltip("The size of the cone in which the projectile can lock onto a new target")]
	public float lockCone = 15f;
	[JsonField]
	[Tooltip("The size of the cone in which the projectile can track a locked-on target")]
	public float trackCone = 70f;
	[JsonField]
	[Tooltip("The size of the cone in which the projectile can track a locked-on target")]
	public float lockTime = 20f;
	[JsonField]
	[Tooltip("How quickly a projectile rotates to face the direction of travel")]
	public float turnRate = 0.1f;
	[JsonField]
	[Tooltip("Altitude gained above target during top attack mode")]
	public float topAttackHeight = 10;
	[JsonField]
	[Tooltip("Lateral distance where missile will directly track target during top attack mode")]
	public float topAttackRange = 10;
	[JsonField]
	[Tooltip("What types of entity this can target")]
	public string[] targetTypes = new string[0];
	[JsonField]
	[Tooltip("Percent speed loss per tick (1/20s)")]
	public float dragInAir = 0.01f;
	[JsonField]
	public string airParticles = "";
	[JsonField]
	[Tooltip("Percent speed loss per tick (1/20s)")]
	public float dragInWater = 0.2f;
	[JsonField]
	public string waterParticles = "";
	[JsonField]
	public float timeBetweenTrailParticles = 0.25f;
}
