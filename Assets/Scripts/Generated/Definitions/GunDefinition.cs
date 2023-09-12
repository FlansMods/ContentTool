using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/GunDefinition")]
public class GunDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public PaintableDefinition paints = new PaintableDefinition();
	[JsonField]
	public ReloadDefinition primaryReload = new ReloadDefinition();
	[JsonField]
	public ReloadDefinition secondaryReload = new ReloadDefinition();
	[JsonField]
[Tooltip("Actions on the primary mouse button, this is where a shoot action normally goes")]
	public ActionGroupDefinition primary = new ActionGroupDefinition();
	[JsonField]
[Tooltip("Actions on the alternate mouse button, like scopes")]
	public ActionGroupDefinition secondary = new ActionGroupDefinition();
	[JsonField]
[Tooltip("Actions to trigger when pressing the 'Look At' key")]
	public ActionGroupDefinition lookAt = new ActionGroupDefinition();
	[JsonField]
[Tooltip("Defines which magazine options there are for the primary shoot action")]
	public MagazineSlotSettingsDefinition primaryMagazines = new MagazineSlotSettingsDefinition();
	[JsonField]
[Tooltip("If there is a secondary slot, defines which magazines are applicable")]
	public MagazineSlotSettingsDefinition secondaryMagazines = new MagazineSlotSettingsDefinition();
	[JsonField]
	public ActionDefinition[] startSpinUpActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] reachMaxSpinActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] startSpinDownActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] reachZeroSpinActions = new ActionDefinition[0];
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
