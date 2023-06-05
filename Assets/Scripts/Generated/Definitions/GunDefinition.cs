using UnityEngine;

[System.Serializable]
public class GunDefinition : Definition
{
	[JsonField]
	public PaintableDefinition paints = new PaintableDefinition();
	[JsonField]
	public ReloadDefinition reload = new ReloadDefinition();
	[JsonField]
	public ActionDefinition[] primaryActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] secondaryActions = new ActionDefinition[0];
	[JsonField]
	public int numBullets = 0;
	[JsonField]
	public AttachmentSettingsDefinition barrelAttachments = new AttachmentSettingsDefinition();
	[JsonField]
	public AttachmentSettingsDefinition gripAttachments = new AttachmentSettingsDefinition();
	[JsonField]
	public AttachmentSettingsDefinition stockAttachments = new AttachmentSettingsDefinition();
	[JsonField]
	public AttachmentSettingsDefinition scopeAttachments = new AttachmentSettingsDefinition();
	[JsonField]
	public AttachmentSettingsDefinition genericAttachments = new AttachmentSettingsDefinition();
	[JsonField]
	public string[] modelParts = new string[0];
}