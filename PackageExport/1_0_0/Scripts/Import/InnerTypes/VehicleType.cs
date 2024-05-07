using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleType : DriveableType
{
	public float turnLeftModifier = 1F, turnRightModifier = 1F;
	public bool squashMobs = false;
	public bool fourWheelDrive = false;
	public bool rotateWheels = false;
	public bool tank = false;
	public bool hasDoor = false;
	public int trackLinkFix = 5;
	public bool flipLinkFix = false;
}
