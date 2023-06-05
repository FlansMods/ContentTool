using UnityEngine;

[System.Serializable]
public class LoadoutPoolDefinition : Definition
{
	[JsonField]
	public int maxLevel = 20;
	[JsonField]
	public int xpForKill = 10;
	[JsonField]
	public int xpForDeath = 5;
	[JsonField]
	public int xpForKillstreakBonus = 10;
	[JsonField]
	public int xpForAssist = 5;
	[JsonField]
	public int xpForMultikill = 10;
	[JsonField]
	public LoadoutDefinition[] defaultLoadouts = new LoadoutDefinition[0];
	[JsonField]
	public string[] availableRewardBoxes = new string[0];
	[JsonField]
	public LevelUpDefinition[] levelUps = new LevelUpDefinition[0];
}
