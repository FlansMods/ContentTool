using UnityEngine;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/GunDefinition")]
public class GunDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public PaintableDefinition paints = new PaintableDefinition();
	[JsonField]
[Tooltip("For each key input this gun should accept, you need a handler")]
	public HandlerDefinition[] inputHandlers = new HandlerDefinition[0];
	[JsonField]
[Tooltip("The possible actions of this gun")]
	public ActionGroupDefinition[] actionGroups = new ActionGroupDefinition[0];
	[JsonField]
[Tooltip("Defines which magazine options there are")]
	public MagazineSlotSettingsDefinition[] magazines = new MagazineSlotSettingsDefinition[0];
	[JsonField]
	public ReloadDefinition[] reloads = new ReloadDefinition[0];
	[JsonField]
	public ModeDefinition[] modes = new ModeDefinition[0];
	[JsonField]
	public SoundDefinition[] loopingSounds = new SoundDefinition[0];
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
	[JsonField]
	public string animationSet = "";
}
