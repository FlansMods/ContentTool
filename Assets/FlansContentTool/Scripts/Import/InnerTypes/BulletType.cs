using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletType : ShootableType
{
 /**
	 * The number of flak particles to spawn upon exploding
	 */
	public int flak = 0;
	/**
	 * The type of flak particles to spawn
	 */
	public string flakParticles = "largesmoke";
	
	/**
	 * If true then this bullet will burn entites it hits
	 */
	public bool setEntitiesOnFire = false;
	
	/**
	 * Exclusively for driveable usage. Replaces old isBomb and isShell booleans with something more flexible
	 */
	public EWeaponType weaponType = EWeaponType.NONE;
	
	public string hitSound;
	public float hitSoundRange = 50;
	
	public bool hasLight = false;
	public float penetratingPower = 1F;
	/**
	 * Lock on variables. If true, then the bullet will search for a target at the moment it is fired
	 */
	public bool lockOnToPlanes = false, lockOnToVehicles = false, lockOnToMechas = false, lockOnToPlayers = false, lockOnToLivings = false;
	/**
	 * Lock on maximum angle for finding a target
	 */
	public float maxLockOnAngle = 45F;
	/**
	 * Lock on force that pulls the bullet towards its prey
	 */
	public float lockOnForce = 1F;
	
	public string trailTexture = "defaultBulletTrail";
	
	public List<string> hitEffects = new List<string>();
}
