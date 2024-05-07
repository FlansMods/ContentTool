using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class PoseDefinition : Element
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
