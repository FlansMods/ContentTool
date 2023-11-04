using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class UVCalculator
{
	public static bool NeedsNewUVMap(MinecraftModel a, MinecraftModel b)
	{
		return !a.IsUVMapSame(b);
	}

	public static void GenerateNewUVMap(UVMap oldMap, UVMap newMap)
	{
		// Try to place each UV as it was in the old map, skipping conflicts
		foreach(var kvp in oldMap.Placements)
		{
			newMap.TryPlacement(kvp.Key, kvp.Value.Origin);
		}

		// Now the newMap should have as many old placements as possible
		// And we can place the remaining pieces where possible
		while(newMap.HasUnplacedPatch(out string key))
		{
			newMap.AutoPlacePatch(key);
		}

		newMap.CalculateBounds();
	}

	public static void UpdateSkins(UVMap from, UVMap to, List<Texture2D> textures)
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
				foreach (var kvp in from.Placements)
				{
					RectInt area = kvp.Value.GetBounds();
					if (area.xMin + area.width >= textures[i].width
					|| area.yMin + area.height >= textures[i].height)
						Debug.LogError($"Attempting to acquire invalid pixel patch of size {area} from texture of dimensions {textures[i].width}x{textures[i].height}");
					else
					{
						pxPatches.Add(kvp.Key, new PixelPatch()
						{
							FromArea = kvp.Value.Patch,
							Pixels = textures[i].GetPixels(area.xMin, area.yMin, area.width, area.height),
						});
					}
				}

				// Then re-stitch into the texture
				to.CalculateBounds();
				textures[i].Reinitialize(to.MaxSize.x, to.MaxSize.y);
				foreach (var kvp in to.Placements)
				{
					if (pxPatches.TryGetValue(kvp.Key, out PixelPatch src))
					{
						PixelPatch newPatch = kvp.Value.Patch.ConvertPixelsFromExistingPatch(src);
						RectInt bounds = kvp.Value.GetBounds();
						textures[i].SetPixels(bounds.xMin, bounds.yMin, bounds.width, bounds.height, newPatch.Pixels);
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
			catch(Exception e)
			{
				Debug.LogError($"Failed to stitch texture {textures[i]} due to '{e.Message}'");
			}
		}
	}

	public static bool StitchWithExistingUV(MinecraftModel model, MinecraftModelPreview preview, string skinName)
	{
		if (model is TurboRig rig && preview is TurboRigPreview rigPreview)
			return StitchWithExistingUV(rig, rigPreview, skinName);
		return false;
	}

	public static bool StitchWithExistingUV(TurboRig rig, TurboRigPreview preview, string skinName)
	{
		MinecraftModel.NamedTexture namedTexture = rig.GetOrCreateNamedTexture(skinName);
		if(namedTexture.Texture == null)
		{
			Vector2Int maxUV = rig.GetMaxUV();

			namedTexture.Texture = new Texture2D(
				Mathf.NextPowerOfTwo(maxUV.x), 
				Mathf.NextPowerOfTwo(maxUV.y));
		}

		return false;
	}
}
