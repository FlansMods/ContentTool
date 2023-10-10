using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CubeModelPreview : MinecraftModelPreview
{
	public CubeModel Cube { get { return Model as CubeModel; } }

	public override void GenerateMesh()
	{
		Vector3[] corners = GenerateCubeCorners(Cube.Origin, Cube.Dimensions);
		BakeCubeToMesh(corners, Vector2Int.zero, Cube.Dimensions);
	}
}
