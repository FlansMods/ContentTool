using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat
{
   /**
	 * The x, y and z positions of the seat within the plane in model co-ordinates
	 * x is forwards, y is up and z is left
	 */
	public int x, y, z;
	/**
	 * The id of this seat
	 */
	public int id;
	/**
	 * Limits for the look vector of the seat. Range is -360 to 360. Thus any range should lie in here without having to wrap
	 */
	public float minYaw = -360F, maxYaw = 360F;
	/**
	 * Limits for the look vector of the seat. Range is -90 to 90, but don't go beyond +/-89 or the view will mess up at the poles
	 */
	public float minPitch = -89F, maxPitch = 89F;
	/**
	 * The gun this seat requires. As of 1.6.2, seats and planes will require specific guns as opposed to being completely open to anything
	 */
	public string gunType;
	/**
	 * The name of the gun model this seat is connected to. Gun model names are specified in the model files
	 */
	public string gunName;
	/**
	 * The part of the driveable this seat is connected to.
	 */
	public string part;
	/**
	 * Auto assigned by driveable type. Indicates what ammo slot the gun should take from
	 */
	public int gunnerID;
	/**
	 * For turret mounted seats on tanks, the seat will be positioned differently according to this offset and the yaw of the turret
	 */
	public Vector3 rotatedOffset = new Vector3();
	/**
	 * Yaw/Pitch rotation speeds (Yaw/Pitch/z) where Z is ignored
	 */
	public Vector3 aimingSpeed = new Vector3(1f, 1f, 0f);
	/**
	 * Where the bullets come from
	 */
	public Vector3 gunOrigin = new Vector3();
	/**
	 * Legacy aiming mode
	 */
	public bool legacyAiming = false;
	/**
	 * Traverse Yaw before pitching
	 */
	public bool yawBeforePitch = false;
	/**
	 * Pitches gun at the last second
	 */
	public bool latePitch = true;
	
	/**
	 * Does the turret have traverse sounds?
	 */
	public bool traverseSounds = false;
	
	public string yawSound;
	public int yawSoundLength;
	public string pitchSound;
	public int pitchSoundLength;

	public Seat(string[] split)
	{
		id = int.Parse(split[1]);
		x = int.Parse(split[2]);
		y = int.Parse(split[3]);
		z = int.Parse(split[4]);
		gunOrigin = new Vector3(x, y, z);
		part = split[5];
		if(split.Length > 6)
		{
			minYaw = float.Parse(split[6]);
			maxYaw = float.Parse(split[7]);
			minPitch = float.Parse(split[8]);
			maxPitch = float.Parse(split[9]);
			if(split.Length > 10)
			{
				gunType = split[10];
				gunName = split[11];
			}
		}
	}

	public Seat(int dx, int dy, int dz)
	{
		id = 0;
		x = dx;
		y = dy;
		z = dz;
		part = "core";
	}

	public Seat(int dx, int dy, int dz, float y1, float y2, float p1, float p2)
	{
		id = 0;
		x = dx;
		y = dy;
		z = dz;
		part = "core";
		minYaw = y1;
		maxYaw = y2;
		minPitch = p1;
		maxPitch = p2;
	}
}
