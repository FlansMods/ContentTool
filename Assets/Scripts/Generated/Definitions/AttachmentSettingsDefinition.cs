using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class AttachmentSettingsDefinition : Element
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
