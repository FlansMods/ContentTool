using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERepeatMode
{
	FullAuto,
	SemiAuto,
	BurstFire,
	Minigun,

	Toggle,
	WaitUntilNextAction,
}

public static class FireModes
{
	public static ERepeatMode Parse(string s)
    {
        s = s.ToLower();
        if (s.Equals("fullauto"))
            return ERepeatMode.FullAuto;
        if (s.Equals("minigun"))
            return ERepeatMode.Minigun;
		if (s.Equals("burst"))
			return ERepeatMode.BurstFire;
		if (s.Equals("toggle"))
			return ERepeatMode.Toggle;
		if (s.Equals("wait"))
			return ERepeatMode.WaitUntilNextAction;
		return ERepeatMode.SemiAuto;
    }
}