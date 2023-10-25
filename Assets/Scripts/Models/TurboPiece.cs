using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Model;

[System.Serializable]
public class TurboPiece
{
	public int textureU, textureV;

	public Vector3 Pos = Vector3.zero;
	public Vector3 Dim = Vector3.one;
	public Vector3 Origin = Vector3.zero;
	public Vector3 Euler = Vector3.zero;

	// For shapeboxes
	public Vector3[] Offsets = new Vector3[8];

	public int NumOffsetVertices()
	{
		int numOffsets = 0;
		for (int i = 0; i < 8; i++)
			if (Offsets[i].sqrMagnitude > 0.00001f)
				numOffsets++;
		return numOffsets;
	}
	public bool IsBox()
	{
		for (int i = 0; i < 8; i++)
			if (Offsets[i].sqrMagnitude > 0.00001f)
				return false;
		return true;
	}

	public bool IsUVMapSame(TurboPiece other)
	{
		if (other.Dim != Dim)
			return false;
		if (other.textureU != textureU)
			return false;
		if (other.textureV != textureV)
			return false;

		return true;
	}

	public TurboPiece Copy()
	{
		return new TurboPiece()
		{
			textureU = this.textureU,
			textureV = this.textureV,
			Pos = this.Pos,
			Dim = this.Dim,
			Origin = this.Origin,
			Euler = this.Euler,
			Offsets = new Vector3[] {
					Offsets[0], Offsets[1], Offsets[2], Offsets[3],
					Offsets[4], Offsets[5], Offsets[6], Offsets[7],
				},
		};
	}

	public void DoMirror(bool bX, bool bY, bool bZ)
	{
		if (bX)
		{
			Pos.x = -Pos.x - Dim.x;
			Origin.x = -Origin.x;
		}
		if (bY)
		{
			Pos.y = -Pos.y - Dim.y;
			Origin.y = -Origin.y;
		}
		if (bZ)
		{
			Pos.z = -Pos.z - Dim.z;
			Origin.z = -Origin.z;
		}
		Offsets = JavaModelImporter.MirrorOffsets(Offsets, bX, bY, bZ);
	}

	public void GetBounds(out Vector3 min, out Vector3 max)
	{
		Vector3[] verts = GetVerts();
		min = Vector3.one * 1000f;
		max = Vector3.one * -1000f;
		for (int i = 0; i < verts.Length; i++)
		{
			verts[i] = Quaternion.Euler(Euler) * verts[i];
			verts[i] += Origin;
			min = Vector3.Min(min, verts[i]);
			max = Vector3.Max(max, verts[i]);
		}
	}

	public Vector2Int MinUV { get { return new Vector2Int(textureU, textureV); } }
	public Vector2Int MaxUV { get { return MinUV + BoxUVSize; } }
	public Vector2Int BoxUVSize { get { return GetBoxUVSize(); } }
	public Vector2Int GetBoxUVSize()
	{
		int x = Mathf.CeilToInt(Dim.x), y = Mathf.CeilToInt(Dim.y), z = Mathf.CeilToInt(Dim.z);
		return new Vector2Int(z + x + z + x, z + y);
	}

	public int[] GetIntUV(int u0, int v0, EFace face)
	{
		int x = Mathf.CeilToInt(Dim.x), y = Mathf.CeilToInt(Dim.y), z = Mathf.CeilToInt(Dim.z);
		switch (face)
		{
			case EFace.west: return new int[] { u0, v0 + z, u0 + z, v0 + z + y };
			case EFace.south: return new int[] { u0 + z, v0 + z, u0 + z + x, v0 + z + y };
			case EFace.east: return new int[] { u0 + z + x, v0 + z, u0 + z + x + z, v0 + z + y };
			case EFace.north: return new int[] { u0 + z + x + z, v0 + z, u0 + z + x + z + x, v0 + z + y };
			case EFace.down: return new int[] { u0 + z, v0, u0 + z + x, v0 + z };
			case EFace.up: return new int[] { u0 + z + x, v0, u0 + z + x + x, v0 + z };
			default: return new int[4];
		}
	}

	public Vector2[] GetUVS(EFace face, int tu, int tv)
	{
		float x = Mathf.Ceil(Dim.x);
		float y = Mathf.Ceil(Dim.y);
		float z = Mathf.Ceil(Dim.z);

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

	public Vector3[] GetVerts()
	{
		Quaternion xRotation = Quaternion.Euler(Euler);
		Vector3[] verts = new Vector3[8];
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				for (int z = 0; z < 2; z++)
				{
					int index = x + y * 2 + z * 4;
					verts[index] = Pos;
					if (x == 1) verts[index].x += Dim.x;
					if (y == 1) verts[index].y += Dim.y;
					if (z == 1) verts[index].z += Dim.z;

					verts[index] += Offsets[index];

					// These get applied in the Unity transform
					//verts[index] = xRotation * verts[index];
					//verts[index] += Origin;
				}
			}
		}

		return verts;
	}

	public static readonly int[] NON_UV_TRIS = new int[] {
		0,2,3,  0,3,1, // -z 0, 2, 3, 1, 
		5,7,6,  5,6,4, // +z 5, 7, 6, 4,
		4,6,2,  4,2,0, // -x 4, 6, 2, 0,
		1,3,7,  1,7,5, // +x 1, 3, 7, 5,
		7,3,2,  7,2,6, // +y 7, 3, 2, 6,
		5,4,0,  5,0,1, // -y 5, 4, 0, 1 
	};
	public int[] GetTris() { return NON_UV_TRIS; }

	public static readonly int[] VERTS_WITH_UV = new int[] {
		0, 2, 3, 1,	 // -z face
		5, 7, 6, 4,	 // +z face
		4, 6, 2, 0,	 // -x face
		1, 3, 7, 5,	 // +x face
		7, 3, 2, 6,	 // +y face
		5, 4, 0, 1,	 // -y face
	};
	public Vector3[] GenerateVertsForUV(Vector3[] inputVerts)
	{
		return new Vector3[] {
			inputVerts[0], inputVerts[2], inputVerts[3], inputVerts[1],	// -z face
			inputVerts[5], inputVerts[7], inputVerts[6], inputVerts[4], // +z face
			inputVerts[4], inputVerts[6], inputVerts[2], inputVerts[0], // -x face
			inputVerts[1], inputVerts[3], inputVerts[7], inputVerts[5], // +x face
			inputVerts[7], inputVerts[3], inputVerts[2], inputVerts[6], // +y face
			inputVerts[5], inputVerts[4], inputVerts[0], inputVerts[1], // -y face
		};
	}
	public static readonly int[] TRIS_WITH_UV = new int[] {
		0,1,2, 0,2,3,
		4,5,6, 4,6,7,
		8,9,10, 8,10,11,
		12,13,14, 12,14,15,
		16,17,18, 16,18,19,
		20,21,22, 20,22,23,
	};
	public int[] GenerateTrisForUV() { return TRIS_WITH_UV; }
	public static readonly Vector3[] NORMALS_WITH_UV = new Vector3[] {
		Vector3.back, Vector3.back, Vector3.back, Vector3.back,
		Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
		Vector3.left, Vector3.left, Vector3.left, Vector3.left,
		Vector3.right, Vector3.right, Vector3.right, Vector3.right,
		Vector3.up, Vector3.up, Vector3.up, Vector3.up,
		Vector3.down, Vector3.down, Vector3.down, Vector3.down,
	};
	public Vector3[] GenerateNormalsForUV() { return NORMALS_WITH_UV; }


	public void ExportToMesh(Mesh mesh, float textureX, float textureY)
	{
		Vector3[] v = GetVerts();
		mesh.SetVertices(GenerateVertsForUV(v));
		List<Vector2> uvs = new List<Vector2>();
		uvs.AddRange(GetUVS(EFace.north, 0, 0));
		uvs.AddRange(GetUVS(EFace.south, 0, 0));
		uvs.AddRange(GetUVS(EFace.west, 0, 0));
		uvs.AddRange(GetUVS(EFace.east, 0, 0));
		uvs.AddRange(GetUVS(EFace.up, 0, 0));
		uvs.AddRange(GetUVS(EFace.down, 0, 0));
		for (int i = 0; i < uvs.Count; i++)
		{
			uvs[i] = new Vector2(uvs[i].x / textureX, (textureY - uvs[i].y) / textureY);
		}
		mesh.SetUVs(0, uvs);
		mesh.SetTriangles(GenerateTrisForUV(), 0);
		mesh.SetNormals(GenerateNormalsForUV());
	}


	public bool ExportToJson(QuickJSONBuilder builder)
	{
		return false;
	}
	public bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		return false;
	}

}
