using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPlaneMode
{
	PLANE, VTOL, HELI
}

public class Propeller
{
	/**
	 * For crafting and plane destruction
	 */
	public string itemType;
	/**
	 * For rendering propellers. Refers to the position in the propellerModel array
	 */
	public int ID;
	/**
	 * Position of the propeller on the plane in model co-ordinates for thrust calculations
	 */
	public int x, y, z;
	/**
	 * Part of the plane it is connected to, for partial plane destruction purposes
	 */
	public string planePart;
	
	public Propeller(int i, int x, int y, int z, string part, string type)
	{
		ID = i;
		this.x = x;
		this.y = y;
		this.z = z;
		planePart = part;
		itemType = type;
	}
	
	public Vector3 getPosition()
	{
		return new Vector3(x / 16F, y / 16F, z / 16F);
	}
}

public class PlaneType : DriveableType
{
    /**
	 * What type of flying vehicle is this?
	 */
	public EPlaneMode mode = EPlaneMode.PLANE;
	/**
	 * Pitch modifiers
	 */
	public float lookDownModifier = 1F, lookUpModifier = 1F;
	/**
	 * Roll modifiers
	 */
	public float rollLeftModifier = 1F, rollRightModifier = 1F;
	/**
	 * Yaw modifiers
	 */
	public float turnLeftModifier = 1F, turnRightModifier = 1F;
	/**
	 * Co-efficient of lift which determines how the plane flies
	 */
	public float lift = 1F;
	
	/**
	 * The point at which bomb entities spawn
	 */
	public Vector3 bombPosition;
	/**
	 * The time in ticks between bullets fired by the nose / wing guns
	 */
	public int planeShootDelay;
	/**
	 * The time in ticks between bombs dropped
	 */
	public int planeBombDelay;
	
	/**
	 * The positions, parent parts and recipe items of the propellers, used to calculate forces and render the plane correctly
	 */
	public List<Propeller> propellers = new List<Propeller>();
	/**
	 * The positions, parent parts and recipe items of the helicopter propellers, used to calculate forces and render the plane correctly
	 */
	public List<Propeller> heliPropellers = new List<Propeller>(), heliTailPropellers = new List<Propeller>();
	
	/**
	 * Aesthetic features
	 */
	public bool hasGear = false, hasDoor = false, hasWing = false;
	/**
	 * Default pitch for when parked. Will implement better system soon
	 */
	public float restingPitch = 0F;
	
	/**
	 * Whether the player can access the inventory while in the air
	 */
	public bool invInflight = true;
}
