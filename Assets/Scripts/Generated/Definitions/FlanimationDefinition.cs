using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/FlanimationDefinition")]
public class FlanimationDefinition : Definition
{
	[JsonField]
	public KeyframeDefinition[] keyframes = new KeyframeDefinition[0];
	[JsonField]
	public SequenceDefinition[] sequences = new SequenceDefinition[0];
}
