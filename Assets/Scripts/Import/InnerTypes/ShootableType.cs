using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableType : PaintableType
{
    /**
	 * Whether trail particles are given off
	 */
	public bool trailParticles = false;
	/**
	 * Trail particles given off by this while being thrown
	 */
	public string trailParticleType = "smoke";
	
	//Item Stuff
	/**
	 * The maximum number of grenades that can be stacked together
	 */
	public int maxStackSize = 1;
	/**
	 * Items dropped on various events
	 */
	public string dropItemOnReload = null, dropItemOnShoot = null, dropItemOnHit = null;
	/**
	 * The number of rounds fired by a gun per item
	 */
	public int roundsPerItem = 1;
	/**
	 * The number of bullet entities to create per round
	 */
	public int numBullets = 1;
	/**
	 * Bullet spread multiplier to be applied to gun's bullet spread
	 */
	public float bulletSpread = 1F;
	
	//Physics and Stuff
	/**
	 * The speed at which the grenade should fall
	 */
	public float fallSpeed = 1.0F;
	/**
	 * The speed at which to throw the grenade. 0 will just drop it on the floor
	 */
	public float throwSpeed = 1.0F;
	/**
	 * Hit box size
	 */
	public float hitBoxSize = 0.5F;
	
	//Damage to hit entities
	/**
	 * Amount of damage to impart upon various entities
	 */
	public float damageVsLiving = 1, damageVsDriveable = 1;
	/**
	 * Whether this grenade will break glass when thrown against it
	 */
	public bool breaksGlass = false;
	
	//Detonation Conditions
	/**
	 * If 0, then the grenade will last until some other detonation condition is met, else the grenade will detonate after this time (in ticks)
	 */
	public int fuse = 0;
	/**
	 * After this time the grenade will despawn quietly. 0 means no despawn time
	 */
	public int despawnTime = 0;
	/**
	 * If true, then this will explode upon hitting something
	 */
	public bool explodeOnImpact = false;
	
	//Detonation Stuff
	/**
	 * The radius in which to spread fire
	 */
	public float fireRadius = 0F;
	/**
	 * The radius of explosion upon detonation
	 */
	public float explosionRadius = 0F;
	/**
	 * Whether the explosion can destroy blocks
	 */
	public bool explosionBreaksBlocks = true;
	/**
	 * The name of the item to drop upon detonating
	 */
	public string dropItemOnDetonate = null;
	/**
	 * Sound to play upon detonation
	 */
	public string detonateSound = "";
}
