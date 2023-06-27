using UnityEngine;

[System.Serializable]
public class ImpactDefinition
{
	[JsonField]
	public string decal = "";
	[JsonField]
	public float damageToTarget = 8.0f;
	[JsonField]
	public float multiplierVsPlayers = 1.0f;
	[JsonField]
	public float multiplierVsVehicles = 1.0f;
	[JsonField]
	public float knockback = 0.0f;
	[JsonField]
	public float splashDamageRadius = 0.0f;
	[JsonField]
	public float splashDamageFalloff = 1.0f;
	[JsonField]
	public float setFireToTarget = 0.0f;
	[JsonField]
	public float fireSpreadRadius = 0.0f;
	[JsonField]
	public float fireSpreadAmount = 0.0f;
	[JsonField]
	public SoundDefinition[] hitSounds = new SoundDefinition[0];
}
