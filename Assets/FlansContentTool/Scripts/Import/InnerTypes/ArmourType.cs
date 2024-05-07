using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourType : InfoType
{
    /**
	 * 0 = Helmet, 1 = Chestplate, 2 = Legs, 3 = Shoes
	 */
	public int type;
	/**
	 * The amount of damage to absorb. From 0 to 1. Stacks additively between armour pieces
	 */
	public double defence;
	
	public int DamageReductionAmount;
	// < 0 durability = infinite
	public int Durability;
	// Armour toughness, like diamond
	public int Toughness;
	// Enchantability, optional
	public int Enchantability = 10;
	
	/**
	 * The name for the armour texture. Texture path/name is assets/flansmod/armor/<armourTextureName>_1.png or _2 for legs
	 */
	public string armourTextureName;
	/**
	 * Modifiers for various player stats
	 */
	public float moveSpeedModifier = 1F, knockbackModifier = 0.2F, jumpModifier = 1F;
	/**
	 * If true, then the player gets a night vision buff every couple of seconds
	 */
	public bool nightVision = false;
	/**
	 * The overlay to display when using this helmet. Textures are pulled from the scopes directory
	 */
	public string overlay = null;
	/**
	 * If true, then smoke effects from grenades will have no effect on players wearing this
	 */
	public bool smokeProtection = false;
	/**
	 * If ture, the player will not receive fall damage
	 */
	public bool negateFallDamage = false;
}
