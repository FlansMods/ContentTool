using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClass : InfoType
{
    public List<string[]> startingItemStrings = new List<string[]>();
	public bool horse = false;
	public string playerSkinOverride = "";
	/**
	 * Override armour. If this is set, then it will override the team armour
	 */
	public string hat, chest, legs, shoes;
}
