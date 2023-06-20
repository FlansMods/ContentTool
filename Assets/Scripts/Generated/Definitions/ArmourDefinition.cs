using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/ArmourDefinition")]
public class ArmourDefinition : Definition
{
	[JsonField]
	public EArmourSlot slot = EArmourSlot.HEAD;
	[JsonField]
	public int maxDurability = 128;
	[JsonField]
	public int toughness = 1;
	[JsonField]
	public int enchantability = 0;
	[JsonField]
	public int damageReduction = 0;
	[JsonField]
	public string armourTextureName = "";
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public bool nightVision = false;
	[JsonField]
	public string screenOverlay = "";
	[JsonField]
	public string[] immunities = new string[0];
}
