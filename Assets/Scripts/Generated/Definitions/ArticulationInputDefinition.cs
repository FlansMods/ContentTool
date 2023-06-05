using UnityEngine;

[System.Serializable]
public class ArticulationInputDefinition
{
	[JsonField]
	public string partName = "";
	[JsonField]
	public EArticulationInputType type = EArticulationInputType.CycleKeyframes;
	[JsonField]
	public int keyframeIndex = 0;
	[JsonField]
	public Vector3 motion = Vector3.zero;
	[JsonField]
	public Vector3 rotationalMotion = Vector3.zero;
	[JsonField]
	public float speed = 1.0f;
}
