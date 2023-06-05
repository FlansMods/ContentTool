using UnityEngine;

[System.Serializable]
public class AttachmentDefinition : Definition
{
	[JsonField]
	public EAttachmentType attachmentType = EAttachmentType.Generic;
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public EMechaEffect[] mechaEffects = new EMechaEffect[0];
	[JsonField]
	public string mechaEffectFilter = "";
	[JsonField]
	public ActionDefinition[] primaryActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] secondaryActions = new ActionDefinition[0];
	[JsonField]
	public bool replacePrimaryAction = false;
	[JsonField]
	public bool replaceSecondaryAction = false;
	[JsonField]
	public EFireMode modeOverride = EFireMode.FullAuto;
	[JsonField]
	public bool overrideFireMode = false;
}
