using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERepeatMode
{
	FullAuto,
	SemiAuto,
	BurstFire,
	Minigun,
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
        return ERepeatMode.SemiAuto;
    }
}