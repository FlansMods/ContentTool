using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAGunType : InfoType
{
   /**
	 * The ammo types used by this gun
	 */
	public List<string> ammo = new List<string>();
	public int reloadTime;
	public int recoil = 5;
	public int accuracy;
	public int damage;
	public int shootDelay;
	public int numBarrels;
	public bool fireAlternately;
	public int health;
	public int gunnerX, gunnerY, gunnerZ;
	public string shootSound;
	public string reloadSound;
	public float topViewLimit = 75F;
	public float bottomViewLimit = 0F;
	public int[] barrelX, barrelY, barrelZ;
	/**
	 * Sentry mode. If target players is true then it either targets everyone on the other team, or everyone other than the owner when not playing with teams
	 */
	public bool targetMobs = false, targetPlayers = false, targetVehicles = false, targetPlanes = false, targetMechas = false;
	/**
	 * Targeting radius
	 */
	public float targetRange = 10F;
	/**
	 * If true, then all barrels share the same ammo slot
	 */
	public bool shareAmmo = false;
}
