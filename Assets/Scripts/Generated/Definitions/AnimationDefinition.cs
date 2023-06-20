using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/AnimationDefinition")]
public class AnimationDefinition : Definition
{
	[JsonField]
	public KeyframeDefinition[] keyframes = new KeyframeDefinition[0];
	[JsonField]
	public SequenceDefinition[] sequences = new SequenceDefinition[0];
}
