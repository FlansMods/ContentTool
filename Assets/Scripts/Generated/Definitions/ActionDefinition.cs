using UnityEngine;

[System.Serializable]
public class ActionDefinition
{
	[JsonField]
	public EActionType actionType = EActionType.Animation;
	[JsonField]
	public bool canActUnderwater = true;
	[JsonField]
	public bool canActUnderOtherLiquid = false;
	[JsonField]
	public bool canBeOverriden = false;
	[JsonField]
	public SoundDefinition sound = new SoundDefinition();
	[JsonField]
	public float duration = 0.0f;
	[JsonField]
	public string itemStack = "";
	[JsonField]
	public EFireMode FireMode = EFireMode.FullAuto;
	[JsonField]
	public ShotDefinition shootStats = new ShotDefinition();
	[JsonField]
	public float fovFactor = 1.25f;
	[JsonField]
	public string scopeOverlay = "";
	[JsonField]
	public string anim = "";
	[JsonField]
	public float toolLevel = 1.0f;
	[JsonField]
	public float harvestSpeed = 1.0f;
	[JsonField]
	public float reach = 1.0f;
}
