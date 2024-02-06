using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETurboRenderMaterial
{
	Solid,
	Cutout,
	Transparent,
	Emissive,
}

public static class TurboRenderMaterials
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
}