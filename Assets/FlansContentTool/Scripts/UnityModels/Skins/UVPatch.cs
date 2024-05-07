using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public abstract class UVPatch
{
	public string Key = "";
	public bool Valid { get { return BoundingSize.sqrMagnitude > 0; } }
	
	public abstract Vector2Int BoundingSize { get; }
	public abstract PixelPatch ConvertPixelsFromExistingPatch(UVPatch srcPatch, PixelPatch srcPixels);
}
[System.Serializable]
public class BoxUVPatch : UVPatch
{
	public static readonly BoxUVPatch Invalid = new BoxUVPatch() { Key = "invalid", BoxDims = Vector3Int.zero };
	public Vector3Int BoxDims = Vector3Int.one;
	public override Vector2Int BoundingSize { get { return new Vector2Int(BoxDims.z * 2 + BoxDims.x * 2, BoxDims.z + BoxDims.y); } }
	public override PixelPatch ConvertPixelsFromExistingPatch(UVPatch srcPatch, PixelPatch srcPixels)
	{
		PixelPatch dst = new PixelPatch()
		{
			Size = BoundingSize,
			Pixels = new Color[BoundingSize.x * BoundingSize.y]
		};
		if(srcPatch is BoxUVPatch boxPatch)
		{
			Vector3Int srcDim = boxPatch.BoxDims;
			Vector3Int dstDim = BoxDims;

			// Top face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z, 0, srcDim.x, srcDim.z, srcPixels,
				dstDim.z, 0, dstDim.x, dstDim.z, dst);
			// Bottom face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z + srcDim.x, 0, srcDim.x, srcDim.z, srcPixels,
				dstDim.z + dstDim.x, 0, dstDim.x, dstDim.z, dst);

			// East face
			PixelPatch.CopyResizingPxPatch(
				0, srcDim.z, srcDim.z, srcDim.y, srcPixels,
				0, dstDim.z, dstDim.z, dstDim.y, dst);
			// North face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z, srcDim.z, srcDim.x, srcDim.y, srcPixels,
				dstDim.z, dstDim.z, dstDim.x, dstDim.y, dst);
			// West face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z + srcDim.x, srcDim.z, srcDim.z, srcDim.y, srcPixels,
				dstDim.z + dstDim.x, dstDim.z, dstDim.z, dstDim.y, dst);
			// South face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z + srcDim.x + srcDim.z, srcDim.z, srcDim.x, srcDim.y, srcPixels,
				dstDim.z + dstDim.x + dstDim.z, dstDim.z, dstDim.x, dstDim.y, dst);
		}
		return dst;
	}
	public BoxUVPatch Clone()
	{
		return new BoxUVPatch()
		{
			Key = Key,
			BoxDims = BoxDims,
		};
	}
	public override string ToString()
	{
		return $"{Key}=[{BoxDims}]";
	}
	public override bool Equals(object other)
	{
		if (other is BoxUVPatch otherUV)
			return BoxDims == otherUV.BoxDims;
		return false;
	}
	public override int GetHashCode()
	{
		return Key.GetHashCode() ^ BoxDims.GetHashCode();
	}
	public static bool operator ==(BoxUVPatch a, BoxUVPatch b)
	{
		return a.Key == b.Key && a.BoxDims == b.BoxDims;
	}
	public static bool operator !=(BoxUVPatch a, BoxUVPatch b)
	{
		return a.Key != b.Key || a.BoxDims != b.BoxDims;
	}
}