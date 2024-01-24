using UnityEngine;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/AttachmentDefinition")]
public class AttachmentDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public EAttachmentType attachmentType = EAttachmentType.Generic;
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public EMechaEffect[] mechaEffects = new EMechaEffect[0];
	[JsonField]
	public string mechaEffectFilter = "";
	[JsonField]
	public HandlerDefinition[] handlerOverrides = new HandlerDefinition[0];
	[JsonField]
	public ActionGroupDefinition[] actionOverrides = new ActionGroupDefinition[0];
	[JsonField]
	public ReloadDefinition[] reloadOverrides = new ReloadDefinition[0];
	[JsonField]
	public AbilityProviderDefinition[] abilities = new AbilityProviderDefinition[0];
	[JsonField]
	public ERepeatMode modeOverride = ERepeatMode.FullAuto;
	[JsonField]
	public bool overrideFireMode = false;
}
