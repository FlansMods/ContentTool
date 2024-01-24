using UnityEngine;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/RewardBoxDefinition")]
public class RewardBoxDefinition : Definition
{
	[JsonField]
	public PaintjobUnlockDefinition[] paintjobs = new PaintjobUnlockDefinition[0];
	[JsonField]
	public float legendaryChance = 0.05f;
	[JsonField]
	public float rareChance = 0.10f;
	[JsonField]
	public float uncommonChance = 0.35f;
	[JsonField]
	public float commonChance = 0.50f;
}
