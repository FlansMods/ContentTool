using UnityEngine;
using static ResourceLocation;

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
	[Tooltip("These will be applied to this action if applicable")]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
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
