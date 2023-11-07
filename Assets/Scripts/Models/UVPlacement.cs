using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class UVPlacement<TPatchType> where TPatchType : UVPatch
{
	public Vector2Int Origin;
	public TPatchType Patch;
	public string Key { get { return Patch.Key; } }
	public bool Valid { get { return Patch.Valid; } }
	public RectInt Bounds { get { return new RectInt(Origin, Patch.BoundingSize); } }

	public override string ToString()
	{
		return $"{Patch} @ [{Origin}]";
	}
	public override bool Equals(object other)
	{
		if (other is UVPlacement<TPatchType> otherUV)
			return Patch.Equals(otherUV.Patch) && Origin.Equals(otherUV.Origin);
		return false;
	}
	public override int GetHashCode()
	{
		return Patch.GetHashCode() ^ Origin.GetHashCode();
	}
	public static bool operator ==(UVPlacement<TPatchType> a, UVPlacement<TPatchType> b)
	{
		return a.Origin == b.Origin && a.Patch == b.Patch;
	}
	public static bool operator !=(UVPlacement<TPatchType> a, UVPlacement<TPatchType> b)
	{
		return a.Origin != b.Origin || a.Patch != b.Patch;
	}
}
[System.Serializable]
public class BoxUVPlacement : UVPlacement<BoxUVPatch>
{
	public static readonly BoxUVPlacement Invalid = new BoxUVPlacement() { Patch = BoxUVPatch.Invalid, Origin = Vector2Int.zero };
	public BoxUVPlacement Clone()
	{
		return new BoxUVPlacement()
		{
			Origin = Origin,
			Patch = Patch.Clone(),
		};
	}
}
