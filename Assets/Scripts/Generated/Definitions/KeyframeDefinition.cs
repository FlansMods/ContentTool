using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class KeyframeDefinition : Element
{
	[JsonField]
	public string name = "";
	[JsonField]
	public PoseDefinition[] poses = new PoseDefinition[0];
	[JsonField]
	public string[] parents = new string[0];
}
