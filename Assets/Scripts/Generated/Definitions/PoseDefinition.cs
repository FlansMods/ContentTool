using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class PoseDefinition
{
	[JsonField]
	public string applyTo = "";
	[JsonField]
	public VecWithOverride position = new VecWithOverride();
	[JsonField]
	public VecWithOverride rotation = new VecWithOverride();
	[JsonField]
	public Vector3 scale = new Vector3(1f, 1f, 1f);
}
