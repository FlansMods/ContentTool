using UnityEngine;

[System.Serializable]
public class ModeDefinition
{
	[JsonField]
	public string key = "mode";
	[JsonField]
	public string[] values = new string[] { "on", "off" };
}
