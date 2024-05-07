using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechaType : DriveableType
{
    /**
	 * Movement modifiers
	 */
	public float turnLeftModifier = 1F, turnRightModifier = 1F, moveSpeed = 1F;
	/**
	 * If true, this will crush any living entity under the wheels
	 */
	public bool squashMobs = false;
	/**
	 * How many blocks can be stepped up when walking
	 */
	public int stepHeight = 0;
	/**
	 * Jump Height (set 0 for no jump)
	 */
	public float jumpHeight = 1F;
	public float jumpVelocity = 1F;
	/**
	 * Speed of Rotation
	 */
	public float rotateSpeed = 10F;
	/**
	 * Origin of the mecha arms
	 */
	public Vector3 leftArmOrigin, rightArmOrigin;
	/**
	 * Length of the mecha arms and legs
	 */
	public float armLength = 1F, legLength = 1F, RearlegLength = 1f, FrontlegLength = 1F, LegTrans = 0F, RearLegTrans = 0F, FrontLegTrans = 0F;
	/**
	 * The amount to scale the held items / tools by when rendering
	 */
	public float heldItemScale = 1F;
	/**
	 * Height and Width of the world collision box
	 */
	public float height = 3F, width = 2F;
	/**
	 * The height of chassis above the ground; for use when legs are gone
	 */
	public float chassisHeight = 1F;
	
	/**
	 * The default reach of tools. Tools can multiply this base reach as they wish
	 */
	public float reach = 10F;
	
	//Falling
	/**
	 * Whether the mecha damages blocks when falling. Can be overriden by upgrades
	 */
	public bool damageBlocksFromFalling = true;
	/**
	 * The size of explosion to cause, per fall damage
	 */
	public float blockDamageFromFalling = 1F;
	
	/**
	 * Whether the mecha takes fall damage. Can be overriden by upgrades
	 */
	public bool takeFallDamage = true;
	/**
	 * How much fall damage the mecha takes by default
	 */
	public float fallDamageMultiplier = 1F;
	
	/**
	 * Leg Swing Limit
	 */
	public float legSwingLimit = 2F;

	// Limiting head turning
	public bool limitHeadTurn = false;
	public float limitHeadTurnValue = 90F;

	// Speed of Leg movement
	public float legSwingTime = 5;

	// Upper/Lower Arm Limit
	public float upperArmLimit = 90;
	public float lowerArmLimit = 90;

	// Modifier for Weapons in Hand
	public float leftHandModifierX = 0;
	public float leftHandModifierY = 0;
	public float leftHandModifierZ = 0;
	public float rightHandModifierX = 0;
	public float rightHandModifierY = 0;
	public float rightHandModifierZ = 0;
}
