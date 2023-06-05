using UnityEngine;

[System.Serializable]
public class AttachmentSettingsDefinition
{
	[JsonField]
	public string[] allowlist = new string[0];
	[JsonField]
	public bool allowAll = true;
	[JsonField]
	public int numAttachmentSlots = 0;
	[JsonField]
	public string attachToMesh = "body";
	[JsonField]
	public Vector3 attachPoint = Vector3.zero;
}
