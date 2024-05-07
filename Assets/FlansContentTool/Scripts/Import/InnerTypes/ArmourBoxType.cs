using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourBoxEntry
{
	public string shortName;
	public string name = "";
	public string[] armours;
	public List<string>[] requiredStacks;
	
	public ArmourBoxEntry(string s, string s1)
	{
		shortName = s;
		name = s1;
		
		//Prep arrays and lists
		armours = new string[4];
		requiredStacks = new List<string>[4];
		for(int i = 0; i < 4; i++)
			requiredStacks[i] = new List<string>();
	}
}

public class ArmourBoxType : BoxType
{
	public List<ArmourBoxEntry> pages = new List<ArmourBoxEntry>();
}
