using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMechaItemType
{
	upgrade, tool, armUpgrade, legUpgrade, headUpgrade, shoulderUpgrade, feetUpgrade, hipsUpgrade, nothing
}

public enum EMechaToolType
{
	pickaxe, axe, shovel, shears, sword
}

public class MechaItemType : InfoType
{
  /**
	 * The type of item
	 */
	public EMechaItemType type;
	/**
	 * If this is a tool, then what type of tool is this? Axe? Pick?
	 */
	public EMechaToolType function = EMechaToolType.sword;
	/**
	 * How quickly this tool works
	 */
	public float speed = 1F;
	/**
	 * The maximum block hardness you can break with this tool
	 */
	public float toolHardness = 1F;
	/**
	 * This is multiplied by the mecha reach to calculate the total reach
	 */
	public float reach = 1F;
	/**
	 * This makes the mecha float towards the surface when underwater, because apparently people prefer limited functionality
	 */
	public bool floater = false;
	/**
	 * This allows an upgrade to affect the mecha's move speed
	 */
	public float speedMultiplier = 1F;
	/**
	 * This allows upgrades to reduce incoming damage
	 */
	public float damageResistance = 0F;
	/**
	 * This allows a sound to be played upon use (RocketPack only for the moment)
	 */
	public string soundEffect = "";
	public string detectSound = "";
	public float soundTime = 0;
	public int energyShield = 0;
	public int lightLevel = 0;
	/**
	 * The following are a ton of upgrade flags and modifiers. The mecha will iterate over all upgrades in its
	 * inventory multiplying multipliers and looking for true booleans in order to decide if things should happen
	 * or what certain values should take
	 */
	public bool stopMechaFallDamage = false, 
		forceBlockFallDamage = false, 
		vacuumItems = false, 
		refineIron = false, 
		autoCoal = false, 
		autoRepair = false, 
		rocketPack = false, 
		diamondDetect = false, 
		infiniteAmmo = false, 
		forceDark = false, 
		wasteCompact = false, 
		flameBurst = false;
	
	/**
	 * The drop rate of these items are multiplied by this float. They stack between items too.
	 * Once dropRate has been calculated, each block then gives floor(dropRate) items with a
	 * dropRate - floor(dropRate) chance of getting one more
	 */
	public float fortuneDiamond = 1F, 
		fortuneRedstone = 1F, 
		fortuneCoal = 1F,
		fortuneEmerald = 1F, 
		fortuneIron = 1F;
	
	/**
	 * The power of any attached jet pack is multiplied by this float
	 */
	public float rocketPower = 1F;
	
}
