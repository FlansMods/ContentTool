using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPartCategory
{
	COCKPIT, WING, ENGINE, PROPELLER, BAY, TAIL, WHEEL, CHASSIS, TURRET, FUEL, MISC
}

public class PartType : InfoType
{
    /**
	 * Category
	 */
	public EPartCategory category;
	/**
	 * Max stack size of item
	 */
	public int stackSize;
	/**
	 * (Engine) Multiplier applied to the thrust of the driveable
	 */
	public float engineSpeed = 1.0F;
	/**
	 * (Engine) Rate at which this engine consumes fuel
	 */
	public float fuelConsumption = 1.0F;
	/**
	 * (Fuel) The amount of fuel this fuel tank gives
	 */
	public int fuel = 0;
	/**
	 * The types of driveables that this engine works with. Used to designate some engines as mecha CPUs and whatnot
	 */
	public List<EDefinitionType> worksWith = new List<EDefinitionType>(new EDefinitionType[] { EDefinitionType.mecha, EDefinitionType.plane, EDefinitionType.vehicle});
	/**
	 * Let's just say you probably don't want to use this to power a mecha...
	 */
	public bool isAIChip = false;
	/**
	 * If set to false, then this engine will definitely not be the default for creatively spawned vehicles
	 */
	public bool canBeDefaultEngine = true;
	
	public List<string> partBoxRecipe = new List<string>();
	
	//------- RedstoneFlux -------
	/**
	 * If true, then this engine will draw from RedstoneFlux power source items such as power cubes. Otherwise it will draw from Flan's Mod fuel items
	 */
	public bool useRFPower = false;
	/**
	 * The power draw rate for RF (per tick)
	 */
	public int RFDrawRate = 1;
	//-----------------------------
}
