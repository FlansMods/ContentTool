using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeType : ShootableType
{
    //Misc
	/**
	 * The damage imparted by smacking someone over the head with this grenade
	 */
	public int meleeDamage = 1;
	
	//Throwing
	/**
	 * The delay between subsequent grenade throws
	 */
	public int throwDelay = 0;
	/**
	 * The sound to play upon throwing this grenade
	 */
	public string throwSound = "";
	/**
	 * The name of the item to drop (if any) when throwing the grenade
	 */
	public string dropItemOnThrow = null;
	/**
	 * Whether you can throw this grenade by right clicking
	 */
	public bool canThrow = true;
	
	//Physics
	/**
	 * Upon hitting a block or entity, the grenade will be deflected and its motion will be multiplied by this constant
	 */
	public float bounciness = 0.9F;
	/**
	 * Whether this grenade may pass through entities or blocks
	 */
	public bool penetratesEntities = false, penetratesBlocks = false;
	/**
	 * The sound to play upon bouncing off a surface
	 */
	public string bounceSound = "";
	/**
	 * Whether the grenade should stick to surfaces
	 */
	public bool sticky = false;
	/**
	 * If true, then the grenade will stick to the player that threw it. Used to make delayed self destruct weapons
	 */
	public bool stickToThrower = false;
	
	//Conditions for detonation
	/**
	 * If > 0 this will act like a mine and explode when a living entity comes within this radius of the grenade
	 */
	public float livingProximityTrigger = -1F;
	/**
	 * If > 0 this will act like a mine and explode when a driveable comes within this radius of the grenade
	 */
	public float driveableProximityTrigger = -1F;
	/**
	 * If true, then anything attacking this entity will detonate it
	 */
	public bool detonateWhenShot = false;
	/**
	 * If true, then this grenade can be detonated by any remote detonator tool
	 */
	public bool remote = false;
	/**
	 * How much damage to deal to the entity that triggered it
	 */
	public float damageToTriggerer = 0F;
	
	//Detonation
	/**
	 * Explosion damage vs various classes of entities
	 */
	public float explosionDamageVsLiving = 0F, explosionDamageVsDriveable = 0F;
	/**
	 * Detonation will not occur until after this time
	 */
	public int primeDelay = 0;
	
	//Aesthetics
	/**
	 * Particles given off in the detonation
	 */
	public int explodeParticles = 0;
	public string explodeParticleType = "largesmoke";
	/**
	 * Whether the grenade should spin when thrown. Generally false for mines or things that should lie flat
	 */
	public bool spinWhenThrown = true;
	
	//Smoke
	/**
	 * Time to remain after detonation
	 */
	public int smokeTime = 0;
	/**
	 * Particles given off after detonation
	 */
	public string smokeParticleType = "explode";
	/**
	 * The effects to be given to people coming too close
	 */
	public List<string> smokeEffects = new List<string>();
	/**
	 * The radius for smoke effects to take place in
	 */
	public float smokeRadius = 5F;
	
	//Deployed bag functionality
	/**
	 * If true, then right clicking this "grenade" will give the player health or buffs or ammo as defined below
	 */
	public bool isDeployableBag = false;
	/**
	 * The number of times players can use this bag before it runs out
	 */
	public int numUses = 1;
	/**
	 * The amount to heal the player using this bag
	 */
	public float healAmount = 0;
	/**
	 * The potion effects to apply to users of this bag
	 */
	public List<string> potionEffects = new List<string>();
	/**
	 * The number of clips to give to the player when using this bag
	 * When they right click with a gun, they will get this number of clips for that gun.
	 * They get the first ammo type, as listed in the gun type file
	 * The number of clips they get is multiplied by numBulletsInGun too
	 * TODO : Give guns a "can get ammo from bag" variable. Stops miniguns and such getting ammo
	 */
	public int numClips = 0;
}
