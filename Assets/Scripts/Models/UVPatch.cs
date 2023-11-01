using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using static UVMap;

[System.Serializable]
public abstract class UVPatch
{
	public abstract Vector2Int GetBoundingSize();
	public abstract bool IsEntirePiece();
	public abstract UVPatch GetSubPatch(string subpath);

	public abstract PixelPatch ConvertPixelsFromExistingPatch(PixelPatch other);
}
[System.Serializable]
public class BoxUVPatch : UVPatch
{
	public Vector3Int boxDims = Vector3Int.one;
	public override Vector2Int GetBoundingSize()
	{
		return new Vector2Int(boxDims.z * 2 + boxDims.x * 2, boxDims.z + boxDims.y);
	}
	public override bool IsEntirePiece() { return true; }
	public override UVPatch GetSubPatch(string subpath) { return null; }
	public override PixelPatch ConvertPixelsFromExistingPatch(PixelPatch src)
	{
		PixelPatch dst = new PixelPatch()
		{
			FromArea = this, 
			Pixels = new Color[GetBoundingSize().x * GetBoundingSize().y]
		};
		if(src.FromArea is BoxUVPatch srcPatch)
		{
			Vector3Int srcDim = srcPatch.boxDims;
			Vector3Int dstDim = boxDims;

			// Top face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z, 0, srcDim.x, srcDim.z, src,
				dstDim.z, 0, dstDim.x, dstDim.z, dst);
			// Bottom face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z + srcDim.x, 0, srcDim.x, srcDim.z, src,
				dstDim.z + dstDim.x, 0, dstDim.x, dstDim.z, dst);

			// East face
			PixelPatch.CopyResizingPxPatch(
				0, srcDim.z, srcDim.z, srcDim.y, src,
				0, dstDim.z, dstDim.z, dstDim.y, dst);
			// North face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z, srcDim.z, srcDim.x, srcDim.y, src,
				dstDim.z, dstDim.z, dstDim.x, dstDim.y, dst);
			// West face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z + srcDim.x, srcDim.z, srcDim.z, srcDim.y, src,
				dstDim.z + dstDim.x, dstDim.z, dstDim.z, dstDim.y, dst);
			// South face
			PixelPatch.CopyResizingPxPatch(
				srcDim.z + srcDim.x + srcDim.z, srcDim.z, srcDim.x, srcDim.y, src,
				dstDim.z + dstDim.x + dstDim.z, dstDim.z, dstDim.x, dstDim.y, dst);
		}
		return dst;
	}
	public override bool Equals(object other)
	{
		if (other is BoxUVPatch otherUV)
			return boxDims == otherUV.boxDims;
		return false;
	}
	public override int GetHashCode()
	{
		return boxDims.GetHashCode();
	}
}
[System.Serializable]
public class PixelPatch
{
	public UVPatch FromArea;
	public Color[] Pixels;

	public void SetPixel(int x, int y, Color color)
	{
		Pixels[x + y * FromArea.GetBoundingSize().x] = color;
	}
	public Color GetPixel(int x, int y)
	{
		return Pixels[x + y * FromArea.GetBoundingSize().x];
	}

	public Color[] GetPixels(int x, int y, int w, int h)
	{
		Color[] px = new Color[w * h];
		if (x >= 0 && y >= 0 && x + w < FromArea.GetBoundingSize().x && y + h < FromArea.GetBoundingSize().y)
			for (int i = 0; i < w; i++)
				for(int j = 0; j < h; j++)
				{
					px[i + j * w] = GetPixel(x + i, y + j);
				}
		return px;
	}

	public void SetPixels(int x, int y, int w, int h, Color[] px)
	{
		if (x >= 0 && y >= 0 && x + w < FromArea.GetBoundingSize().x && y + h < FromArea.GetBoundingSize().y)
			for (int i = 0; i < w; i++)
				for (int j = 0; j < h; j++)
				{
					SetPixel(x + i, y + j, px[i + j * w]);
				}
	}


	public static void CopyResizingPxPatch(int srcX, int srcY, int srcW, int srcH, PixelPatch srcPx,
											int dstX, int dstY, int dstW, int dstH, PixelPatch dstPx)
	{
		if (srcW <= 0 || srcH <= 0)
			return;
		for(int i = 0; i < dstW; i++)
		{
			for(int j = 0; j < dstH; j++)
			{
				dstPx.SetPixel(
					dstX + i,
					dstY + j,
					srcPx.GetPixel(
						srcX + Mathf.Min(i, srcW - 1),	// Cap the pixel coords to the edge of the src px
						srcY + Mathf.Min(j, srcH - 1)));
			}
		}
	}
}