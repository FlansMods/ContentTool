using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/NpcDefinition")]
public class NpcDefinition : Definition
{
	[JsonField]
	public VoiceLineDefinition[] voiceLines = new VoiceLineDefinition[0];
	[JsonField]
	public ItemStackDefinition hat = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition chest = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition legs = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition shoes = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition mainHand = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition offHand = new ItemStackDefinition();
	[JsonField]
	public ENpcActionType[] validActions = new ENpcActionType[0];
	[JsonField]
	public float cooldownSecondsFriendly = 120;
	[JsonField]
	public float cooldownSecondsHostile = 300;
	[JsonField]
[Tooltip("If set to 0, this NPC will not be considered a merchant")]
	public int maxMerchantLevel = 0;
	[JsonField]
	public int[] xpPerMerchantLevel = new int[0];
	[JsonField]
	public MerchantOfferDefinition[] offers = new MerchantOfferDefinition[0];
}
