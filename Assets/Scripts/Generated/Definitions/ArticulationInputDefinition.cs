using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ArticulationInputDefinition : Element
{
	[JsonField]
	public string partName = "";
	[JsonField]
	public EArticulationInputType type = EArticulationInputType.CycleKeyframes;
	[JsonField]
	[Tooltip("Used with the SpecificKeyframe type. Index based on the ArticulatedPart definition")]
	public int keyframeIndex = 0;
	[JsonField]
	[Tooltip("Used with the ApplyMotion type. Pushes in this direction if within the bounds of the part")]
	public Vector3 motion = Vector3.zero;
	[JsonField]
	[Tooltip("Used with the ApplyMotion type. Rotates in this direction (euler angles) if within the bounds of the part")]
	public Vector3 rotationalMotion = Vector3.zero;
	[JsonField]
	public float speed = 1.0f;
}
