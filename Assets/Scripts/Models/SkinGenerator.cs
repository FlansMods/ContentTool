using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SkinGenerator
{
	public static void RemapSkins(UVMap from, UVMap to, List<Texture2D> textures)
	{
		for (int i = 0; i < textures.Count; i++)
		{
			if (textures[i] == null)
			{
				Debug.LogWarning($"Found null texture when trying to update UV map");
				continue;
			}

			try
			{
				// Cut our input texture into pieces
				// We have to store these in arrays because our input and output Texture2D might be the same
				Dictionary<string, PixelPatch> pxPatches = new Dictionary<string, PixelPatch>();
				foreach (BoxUVPlacement placement in from.PlacedBoxes)
				{
					RectInt area = placement.Bounds;
					if (area.xMin + area.width >= textures[i].width
					|| area.yMin + area.height >= textures[i].height)
						Debug.LogError($"Attempting to acquire invalid pixel patch of size {area} from texture of dimensions {textures[i].width}x{textures[i].height}");
					else
					{
						pxPatches.Add(placement.Key, new PixelPatch()
						{
							Size = placement.Patch.BoundingSize,
							Pixels = textures[i].GetPixels(area.xMin, area.yMin, area.width, area.height),
						});
					}
				}

				// Then re-stitch into the texture
				to.CalculateBounds();
				textures[i].Reinitialize(to.MaxSize.x, to.MaxSize.y);
				foreach (BoxUVPlacement placement in to.PlacedBoxes)
				{
					if (pxPatches.TryGetValue(placement.Key, out PixelPatch src))
					{
						BoxUVPlacement srcPlacement = from.GetPlacedPatch(placement.Key);
						if (srcPlacement != null)
						{
							PixelPatch newPatch = placement.Patch.ConvertPixelsFromExistingPatch(srcPlacement.Patch, src);
							RectInt bounds = placement.Bounds;
							textures[i].SetPixels(bounds.xMin, bounds.yMin, bounds.width, bounds.height, newPatch.Pixels);
						}
					}
					else
					{
						// TODO: Apply default debug box shapes
					}
				}
				textures[i].Apply();
				EditorUtility.SetDirty(textures[i]);
				AssetDatabase.SaveAssetIfDirty(textures[i]);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to stitch texture {textures[i]} due to '{e.Message}'");
			}
		}
	}

	public static void CreateDefaultTexture(UVMap map, Texture2D texture)
	{
		if (map == null)
			return;
		map.CalculateBounds();
		if (texture.width != map.MaxSize.x || texture.height != map.MaxSize.y)
			texture.Reinitialize(map.MaxSize.x, map.MaxSize.y);

		FillTexture(texture, 0, 0, texture.width, texture.height, Color.clear, Color.clear);
		foreach (BoxUVPlacement placement in map.PlacedBoxes)
		{
			int x = placement.Patch.BoxDims.x;
			int y = placement.Patch.BoxDims.y;
			int z = placement.Patch.BoxDims.z;

			int u = placement.Origin.x;
			int v = placement.Origin.y;

			FillTexture(texture, u + z, x, v + 0, z, Faces[0], Outlines[0]);
			FillTexture(texture, u + z + x, x, v + 0, z, Faces[1], Outlines[1]);
			FillTexture(texture, u + 0, z, v + z, y, Faces[2], Outlines[2]);
			FillTexture(texture, u + z, x, v + z, y, Faces[3], Outlines[3]);
			FillTexture(texture, u + z + x, z, v + z, y, Faces[4], Outlines[4]);
			FillTexture(texture, u + z + x + z, x, v + z, y, Faces[5], Outlines[5]);
		}
		texture.Apply();
	}

	private static void FillTexture(Texture2D texture, int x, int w, int y, int h, Color color, Color outline)
	{
		for (int i = x; i < x + w; i++)
			for (int j = y; j < y + h; j++)
			{
				if (i == x || i == x + w - 1 || j == y || j == y + h - 1)
					texture.SetPixel(i, texture.height - 1 - j, outline);
				else
					texture.SetPixel(i, texture.height - 1 - j, color);
			}
	}

	private static readonly Color[] Faces = new Color[]
	{
		new Color(0.25f, 0.5f, 1.0f),
		new Color(0.5f, 0.25f, 1.0f),
		new Color(1f, 0.5f, 0.25f),
		new Color(1f, 0.25f, 0.5f),
		new Color(0.25f, 1f, 0.5f),
		new Color(0.5f, 1f, 0.25f),
	};
	private static readonly Color[] Outlines = new Color[]
	{
		new Color(0.5f, 0.75f, 1.0f),
		new Color(0.75f, 0.5f, 1.0f),
		new Color(1f, 0.75f, 0.5f),
		new Color(1f, 0.5f, 0.75f),
		new Color(0.5f, 1f, 0.75f),
		new Color(0.75f, 1f, 0.5f),
	};
}
