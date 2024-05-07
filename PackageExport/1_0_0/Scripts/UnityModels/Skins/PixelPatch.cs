using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PixelPatch
{
	public Vector2Int Size;
	public Color[] Pixels;

	public void SetPixel(int x, int y, Color color)
	{
		Pixels[x + y * Size.x] = color;
	}
	public Color GetPixel(int x, int y)
	{
		return Pixels[x + y * Size.x];
	}

	public Color[] GetPixels(int x, int y, int w, int h)
	{
		Color[] px = new Color[w * h];
		if (x >= 0 && y >= 0 && x + w < Size.x && y + h < Size.y)
			for (int i = 0; i < w; i++)
				for (int j = 0; j < h; j++)
				{
					px[i + j * w] = GetPixel(x + i, y + j);
				}
		return px;
	}

	public void SetPixels(int x, int y, int w, int h, Color[] px)
	{
		if (x >= 0 && y >= 0 && x + w < Size.x && y + h < Size.y)
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
		for (int i = 0; i < dstW; i++)
		{
			for (int j = 0; j < dstH; j++)
			{
				dstPx.SetPixel(
					dstX + i,
					dstY + j,
					srcPx.GetPixel(
						srcX + Mathf.Min(i, srcW - 1),  // Cap the pixel coords to the edge of the src px
						srcY + Mathf.Min(j, srcH - 1)));
			}
		}
	}
}