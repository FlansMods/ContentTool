using UnityEngine;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/MagazineDefinition")]
public class MagazineDefinition : Definition
{
	[JsonField]
	public string[] tags = new string[0];
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public EAmmoLoadMode ammoLoadMode = EAmmoLoadMode.FullMag;
	[JsonField]
	public EAmmoConsumeMode ammoConsumeMode = EAmmoConsumeMode.RoundRobin;
	[JsonField]
[Tooltip("The number of Magazine Upgrade items needed to swap to this mag")]
	public int upgradeCost = 0;
	[JsonField]
	public int numRounds = 0;
	[JsonField]
[Tooltip("A performance optimisation, recommended if the mag size is 100 or more")]
	public bool allRoundsMustBeIdentical = true;
	[JsonField]
	public string[] matchBulletNames = new string[0];
	[JsonField]
	public string[] requiredBulletTags = new string[0];
	[JsonField]
	public string[] disallowedBulletTags = new string[0];
}
