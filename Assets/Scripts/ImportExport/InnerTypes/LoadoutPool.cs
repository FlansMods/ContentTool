using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ELoadoutSlot
{
	primary,
	secondary,
	special,
	melee,
	armour,
}

public class PlayerLoadout
{
	public string[] slots = new string[8];
}

public class LoadoutEntry
{
	public int unlockLevel = 0;
	public bool available = false;
}

// Used in GUI only
public class LoadoutEntryPaintjob : LoadoutEntry
{
	public Paintjob paintjob = null;
}

// Used in the type file
public class LoadoutEntryInfoType : LoadoutEntry
{
	public string type = null;
	public List<string> extraItems = new List<string>(2);
}

public class LoadoutPool : InfoType
{
    public int maxLevel = 20;
	public int[] XPPerLevel;
	public int XPForKill = 10, XPForDeath = 5, XPForKillstreakBonus = 10;
	public List<LoadoutEntryInfoType>[] unlocks = new List<LoadoutEntryInfoType>[5];
	public PlayerLoadout[] defaultLoadouts = new PlayerLoadout[5];
	public string[] rewardBoxes = new string[3];
	public List<string>[] rewardsPerLevel;
	
	public int[] slotUnlockLevels = new int[]{0, 0, 5, 10, 20};
}
