using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ImpactDefinition
{
	[JsonField]
	public ResourceLocation decal = InvalidLocation;
	[JsonField]
	public float damageToTarget = 1.0f;
	[JsonField]
	public float multiplierVsPlayers = 1.0f;
	[JsonField]
	public float multiplierVsVehicles = 1.0f;
	[JsonField]
	public float knockback = 0.0f;
	[JsonField]
	public string potionEffectOnTarget = "";
	[JsonField]
[Tooltip("The base amount of damage to apply to targets in the splash damage radius")]
	public float splashDamage = 0.0f;
	[JsonField]
[Tooltip("The radius within which to apply splash damage")]
	public float splashDamageRadius = 0.0f;
	[JsonField]
[Tooltip("The falloff rate of splash damage. 0=Full damage so long as they are in radius, 1=Scales to 0 at max radius")]
	public float splashDamageFalloff = 1.0f;
	[JsonField]
	public string potionEffectOnSplash = "";
	[JsonField]
	public float setFireToTarget = 0.0f;
	[JsonField]
	public float fireSpreadRadius = 0.0f;
	[JsonField]
	public float fireSpreadAmount = 0.0f;
	[JsonField]
	public float explosionRadius = 0.0f;
	[JsonField]
	public SoundDefinition[] hitSounds = new SoundDefinition[0];
	[JsonField]
[Tooltip("WIP, will be able to apply actions at the point of impact")]
	public ActionDefinition[] onImpactActions = new ActionDefinition[0];
}
