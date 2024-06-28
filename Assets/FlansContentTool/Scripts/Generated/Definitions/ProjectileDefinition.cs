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
	[Tooltip("How quickly a projectile rotates to face the direction of travel")]
	public float turnRate = 0.5f;
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
