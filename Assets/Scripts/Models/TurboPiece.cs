using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class TurboPiece
{
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

	public TurboPiece Copy()
	{
		return new TurboPiece()
		{
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

	public void Operation_DoMirror(bool bX, bool bY, bool bZ)
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

	// ---------------------------------------------------------------------------
	#region UV-Mapping information, (not actually stored in the TurboPiece)
	// ---------------------------------------------------------------------------
	public Vector2Int BoxUVSize
	{
		get
		{
			Vector3Int box = BoxUVDims;
			return new Vector2Int(box.z + box.x + box.z + box.x, box.z + box.y);
		}
	}
	public Vector3Int BoxUVDims
	{
		get
		{
			return new Vector3Int(
				Mathf.CeilToInt(Dim.x),
				Mathf.CeilToInt(Dim.y),
				Mathf.CeilToInt(Dim.z));
		}
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
	#endregion
	// ---------------------------------------------------------------------------

	public Vector3[] GetVerts()
	{
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
				}
			}
		}
		return verts;
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
