using UnityEngine;

[System.Serializable]
public class LevelUpDefinition
{
	[JsonField]
	public PaintjobUnlockDefinition[] paintjobs = new PaintjobUnlockDefinition[0];
	[JsonField]
	public ItemUnlockDefinition[] items = new ItemUnlockDefinition[0];
	[JsonField]
	public int unlockSlot = -1;
	[JsonField]
	public int xpToLevel = 100;
}
