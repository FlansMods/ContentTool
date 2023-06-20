using UnityEngine;

[System.Serializable]
public class KeyframeDefinition
{
	[JsonField]
	public string name = "";
	[JsonField]
	public PoseDefinition[] poses = new PoseDefinition[0];
	[JsonField]
	public string[] parents = new string[0];
}
