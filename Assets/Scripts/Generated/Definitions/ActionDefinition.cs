using UnityEngine;

[System.Serializable]
public class ActionDefinition
{
	[JsonField]
	public EActionType actionType = EActionType.Invalid;
	[JsonField]
	public bool canActUnderwater = true;
	[JsonField]
	public bool canActUnderOtherLiquid = false;
	[JsonField]
	public bool canBeOverriden = false;
	[JsonField]
	public bool twoHanded = false;
	[JsonField]
	public ERepeatMode repeatMode = ERepeatMode.SemiAuto;
	[JsonField]
	public float repeatDelay = 0.0f;
	[JsonField]
	public int repeatCount = 0;
	[JsonField]
	public float spinUpDuration = 1.0f;
	[JsonField]
	public float duration = 0.0f;
	[JsonField]
	public SoundDefinition[] sounds = new SoundDefinition[0];
	[JsonField]
	public string itemStack = "";
	[JsonField]
	public ShotDefinition[] shootStats = new ShotDefinition[0];
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
