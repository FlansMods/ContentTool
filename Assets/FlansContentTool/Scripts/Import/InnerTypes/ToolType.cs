using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolType : InfoType
{
    /**
	 * Boolean switches that decide whether the tool should heal players and / or driveables
	 */
	public bool healPlayers = false, healDriveables = false;
	/**
	 * The amount to heal per use (one use per click)
	 */
	public int healAmount = 0;
	/**
	 * The amount of uses the tool has. 0 means infinite
	 */
	public int toolLife = 0;
	/**
	 * If true, the tool will destroy itself when finished. Disable this for rechargeable tools
	 */
	public bool destroyOnEmpty = true;
	/**
	 * The items required to be added (shapelessly) to recharge the tool
	 */
	public List<string> rechargeRecipe = new List<string>();
	/**
	 * Not yet implemented. For making tools chargeable with IC2 EU
	 */
	public int EUPerCharge = 0;
	/**
	 * If true, then this tool will deploy a parachute upon use (and consume itself)
	 */
	public bool parachute = false;
	/**
	 * If true, then this will detonate the least recently placed remote explosive
	 */
	public bool remote = false;
	/**
	 * If > 0, then the player can eat this and recover this much hunger
	 */
	public int foodness = 0;
}
