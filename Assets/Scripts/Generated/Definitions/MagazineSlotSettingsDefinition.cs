using UnityEngine;

[System.Serializable]
public class MagazineSlotSettingsDefinition
{
	[JsonField]
	public string[] matchByNames = new string[0];
	[JsonField]
	public string[] matchByTags = new string[0];
	[JsonField]
[Tooltip("How many upgrades are needed to perform a swap (not including the ones associated to the mags themselves)")]
	public int baseCostToSwap = 0;
}