using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumExtensions
{
	public static readonly ETurboRenderMaterial[] Values = new ETurboRenderMaterial[] {
	ETurboRenderMaterial.Solid,
	ETurboRenderMaterial.Cutout,
	ETurboRenderMaterial.Transparent,
	ETurboRenderMaterial.Emissive,
	};

	public static readonly Shader SolidShader = Shader.Find("Standard");

	public static Shader GetShader(this ETurboRenderMaterial renderType)
	{
		return SolidShader;
	}

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
