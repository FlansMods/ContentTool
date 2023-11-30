using UnityEngine;

[System.Serializable]
public class AttachmentSettingsDefinition
{
	[JsonField]
	public string[] matchNames = new string[0];
	[JsonField]
	public string[] matchTags = new string[0];
	[JsonField]
	public int numAttachmentSlots = 0;
	[JsonField]
	public bool hideDefaultMesh = true;
}
