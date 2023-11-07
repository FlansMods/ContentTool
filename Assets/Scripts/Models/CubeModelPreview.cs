using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CubeModelPreview : MinecraftModelPreview
{
	public CubeModel Cube { get { return Model as CubeModel; } }

	public override void RefreshGeometry()
	{
		Vector3[] corners = GenerateCubeCorners(Cube.Origin, Cube.Dimensions);
		BakeCubeToMesh(corners, Vector2Int.zero, Cube.Dimensions);
	}

	protected void BakeCubeToMesh(Vector3[] verts, Vector2Int boxUVOrigin, Vector3 boxUVDims)
	{
		Mesh.SetVertices(new Vector3[] {
				verts[0], verts[2], verts[3], verts[1],	// -z face
				verts[5], verts[7], verts[6], verts[4], // +z face
				verts[4], verts[6], verts[2], verts[0], // -x face
				verts[1], verts[3], verts[7], verts[5], // +x face
				verts[7], verts[3], verts[2], verts[6], // +y face
				verts[5], verts[4], verts[0], verts[1], // -y face
			});
		List<Vector2> uvs = new List<Vector2>();
		uvs.AddRange(GetUVS(EFace.north, boxUVDims, boxUVOrigin.x, boxUVOrigin.y));
		uvs.AddRange(GetUVS(EFace.south, boxUVDims, boxUVOrigin.x, boxUVOrigin.y));
		uvs.AddRange(GetUVS(EFace.west, boxUVDims, boxUVOrigin.x, boxUVOrigin.y));
		uvs.AddRange(GetUVS(EFace.east, boxUVDims, boxUVOrigin.x, boxUVOrigin.y));
		uvs.AddRange(GetUVS(EFace.up, boxUVDims, boxUVOrigin.x, boxUVOrigin.y));
		uvs.AddRange(GetUVS(EFace.down, boxUVDims, boxUVOrigin.x, boxUVOrigin.y));
		for (int i = 0; i < uvs.Count; i++)
		{
			uvs[i] = new Vector2(uvs[i].x / boxUVOrigin.x, 1.0f - uvs[i].y / boxUVOrigin.y);
		}
		Mesh.SetUVs(0, uvs);
		Mesh.SetTriangles(new int[] {
				0,1,2, 0,2,3,
				4,5,6, 4,6,7,
				8,9,10, 8,10,11,
				12,13,14, 12,14,15,
				16,17,18, 16,18,19,
				20,21,22, 20,22,23,
			}, 0);
		// TODO: Calculate from vertex positions
		Mesh.SetNormals(new Vector3[] {
				Vector3.back, Vector3.back, Vector3.back, Vector3.back,
				Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
				Vector3.left, Vector3.left, Vector3.left, Vector3.left,
				Vector3.right, Vector3.right, Vector3.right, Vector3.right,
				Vector3.up, Vector3.up, Vector3.up, Vector3.up,
				Vector3.down, Vector3.down, Vector3.down, Vector3.down,
			});
	}

	protected Vector3[] GenerateCubeCorners(Vector3 origin, Vector3 dims)
	{
		Vector3[] verts = new Vector3[8];
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				for (int z = 0; z < 2; z++)
				{
					int index = x + y * 2 + z * 4;
					verts[index] = origin;
					if (x == 1) verts[index].x += dims.x;
					if (y == 1) verts[index].y += dims.y;
					if (z == 1) verts[index].z += dims.z;
				}
			}
		}
		return verts;
	}

	public int[] GetCubeTriOrder()
	{
		return new int[] {
				0,2,3,  0,3,1, // -z 0, 2, 3, 1, 
				5,7,6,  5,6,4, // +z 5, 7, 6, 4,
				4,6,2,  4,2,0, // -x 4, 6, 2, 0,
				1,3,7,  1,7,5, // +x 1, 3, 7, 5,
				7,3,2,  7,2,6, // +y 7, 3, 2, 6,
				5,4,0,  5,0,1, // -y 5, 4, 0, 1 
			};
	}

	protected Vector2[] GetUVS(EFace face, Vector3 dims, int tu, int tv)
	{
		float x = Mathf.Ceil(dims.x);
		float y = Mathf.Ceil(dims.y);
		float z = Mathf.Ceil(dims.z);

		switch (face)
		{
			case EFace.north:
				return new Vector2[] {
					new Vector2(tu + x * 2 + z * 2, tv + y + z),
					new Vector2(tu + x * 2 + z * 2, tv + z),
					new Vector2(tu + x + z * 2, tv + z),
					new Vector2(tu + x + z * 2, tv + y + z),
				};
			case EFace.south:
				return new Vector2[] {
					new Vector2(tu + x + z, tv + y + z),
					new Vector2(tu + x + z, tv + z),
					new Vector2(tu + z, tv + z),
					new Vector2(tu + z, tv + y + z),
				};
			case EFace.west:
				return new Vector2[] {
					new Vector2(tu + z, tv + y + z),
					new Vector2(tu + z, tv + z),
					new Vector2(tu, tv + z),
					new Vector2(tu, tv + y + z)
				};
			case EFace.east:
				return new Vector2[] {
					new Vector2(tu + x + 2 * z, tv + y + z),
					new Vector2(tu + x + 2 * z, tv + z),
					new Vector2(tu + x + z, tv + z),
					new Vector2(tu + x + z, tv + y + z)
				};
			case EFace.up:
				return new Vector2[] {
					new Vector2(tu + x + z, tv + z),
					new Vector2(tu + x + z, tv),
					new Vector2(tu + z, tv),
					new Vector2(tu + z, tv + z)
				};
			case EFace.down:
				return new Vector2[] {
					new Vector2(tu + x * 2 + z, tv + z),
					new Vector2(tu + x + z, tv + z),
					new Vector2(tu + x + z, tv),
					new Vector2(tu + x * 2 + z, tv)
				};
			default:
				return new Vector2[4];
		}
	}
}
