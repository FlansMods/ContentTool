using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EFireMode
{
	FullAuto,
	SemiAuto,
	BurstFire,
	Minigun,
}

public static class FireModes
{
	public static EFireMode Parse(string s)
    {
        s = s.ToLower();
        if (s.Equals("fullauto"))
            return EFireMode.FullAuto;
        if (s.Equals("minigun"))
            return EFireMode.Minigun;
        if (s.Equals("burst"))
            return EFireMode.BurstFire;
        return EFireMode.SemiAuto;
    }
}