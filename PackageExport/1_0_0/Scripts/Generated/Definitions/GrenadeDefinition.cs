using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/GrenadeDefinition")]
public class GrenadeDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	[Tooltip("Most likely a throw action")]
	public ActionDefinition[] primaryActions = new ActionDefinition[0];
	[JsonField]
	[Tooltip("Could be a cook, drop, detonate etc")]
	public ActionDefinition[] secondaryActions = new ActionDefinition[0];
	[JsonField]
	public float bounciness = 0.0f;
	[JsonField]
	public bool sticky = false;
	[JsonField]
	public bool canStickToThrower = false;
	[JsonField]
	[Tooltip("Keep this <= 0 if you don't want a proximity trigger")]
	public float livingProximityTrigger = -1f;
	[JsonField]
	[Tooltip("Keep this <= 0 if you don't want a proximity trigger")]
	public float vehicleProximityTrigger = -1f;
	[JsonField]
	public bool detonateWhenShot = false;
	[JsonField]
	public bool detonateWhenDamaged = false;
	[JsonField]
	[Tooltip("Keep this <= 0 for no fuse")]
	public float fuse = -1.0f;
	[JsonField]
	[Tooltip("This will spin like a frisbee")]
	public float spinForceX = 0.0f;
	[JsonField]
	[Tooltip("This will tumble forwards")]
	public float spinForceY = 0.0f;
	[JsonField]
	public ImpactDefinition impact = new ImpactDefinition();
	[JsonField]
	public float lifetimeAfterDetonation = 0.0f;
	[JsonField]
	public string[] smokeParticles = new string[0];
	[JsonField]
	public string[] effectsToApplyInSmoke = new string[0];
	[JsonField]
	public float smokeRadius = 0.0f;
}
