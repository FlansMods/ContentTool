using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/GrenadeDefinition")]
public class GrenadeDefinition : Definition
{
	[JsonField]
	public ActionDefinition[] primaryActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] secondaryActions = new ActionDefinition[0];
	[JsonField]
	public float bounciness = 0.0f;
	[JsonField]
	public bool sticky = false;
	[JsonField]
	public bool canStickToThrower = false;
	[JsonField]
	public float livingProximityTrigger = -1f;
	[JsonField]
	public float vehicleProximityTrigger = -1f;
	[JsonField]
	public bool detonateWhenShot = false;
	[JsonField]
	public bool detonateWhenDamaged = false;
	[JsonField]
	public float fuse = -1.0f;
	[JsonField]
	public float spinForceX = 0.0f;
	[JsonField]
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
