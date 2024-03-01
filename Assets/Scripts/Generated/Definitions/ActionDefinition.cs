using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ActionDefinition : Element
{
	[JsonField]
	public EActionType actionType = EActionType.Invalid;
	[JsonField]
	[Tooltip("In seconds")]
	public float duration = 0.0f;
	[JsonField]
	public SoundDefinition[] sounds = new SoundDefinition[0];
	[JsonField]
	public string itemStack = "";
	[JsonField]
	public string scopeOverlay = "";
	[JsonField]
	public string anim = "";
}
