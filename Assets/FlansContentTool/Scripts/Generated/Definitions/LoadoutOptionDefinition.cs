using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LoadoutOptionDefinition : Element
{
	[JsonField]
	public EUnlockType unlockType = EUnlockType.Unlocked;
	[JsonField]
	public int unlockAtRank = 0;
	[JsonField]
	public string externalUnlockKey = "";
	[JsonField]
	public LoadoutItemModifierDefinition[] addItems = new LoadoutItemModifierDefinition[0];
	[JsonField]
	public LoadoutAttachmentModifierDefinition[] attachItems = new LoadoutAttachmentModifierDefinition[0];
	[JsonField]
	public LoadoutSkinModifierDefinition[] changeSkins = new LoadoutSkinModifierDefinition[0];
}
