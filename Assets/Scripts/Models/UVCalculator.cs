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
		foreach(var kvp in oldMap.Placement)
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

			// Cut our input texture into pieces
			// We have to store these in arrays because our input and output Texture2D might be the same
			Dictionary<string, PixelPatch> pxPatches = new Dictionary<string, PixelPatch>();
			foreach(var kvp in from.Placement)
			{
				RectInt area = kvp.Value.GetBounds();
				pxPatches.Add(kvp.Key, new PixelPatch() {
					FromArea = kvp.Value.Patch,
					Pixels = textures[i].GetPixels(area.xMin, area.yMin, area.width, area.height),
				});
			}

			// Then re-stitch into the texture
			textures[i].Reinitialize(to.MaxSize.x, to.MaxSize.y);
			foreach(var kvp in to.Placement)
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
		}
	}

	public static bool StitchWithExistingUV(MinecraftModel model, MinecraftModelPreview preview, string skinName)
	{
		if (model is TurboRig rig && preview is TurboRigPreview rigPreview)
			return StitchWithExistingUV(rig, rigPreview, skinName);
		return false;
		
	}

	public static bool AutoUV(MinecraftModel model, MinecraftModelPreview preview, string skinName)
	{
		if (model is TurboRig rig && preview is TurboRigPreview turboPreview)
			return AutoUVRig(rig, turboPreview, skinName);
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

	public static bool AutoUVRig(TurboRig rig, TurboRigPreview preview, string skinName)
	{
		Dictionary<TurboPiecePreview, RectInt> mappings = new Dictionary<TurboPiecePreview, RectInt>();
		Vector2Int maxBounds = new Vector2Int(16, 16);
		foreach (TurboModel section in rig.Sections)
		{
			TurboModelPreview modelPreview = preview.GetAndUpdateChild(section.PartName);
			if(modelPreview == null)
			{
				Debug.LogError($"Section {section.PartName} did not have a valid preview");
				return false;
			}
			for(int i = 0; i < section.Pieces.Count; i++)
			{
				TurboPiece piece = section.Pieces[i];
				TurboPiecePreview piecePreview = modelPreview.GetChild(i);
				if (piecePreview == null)
				{
					Debug.LogError($"Piece {i} did not have a valid preview");
					return false;
				}

				Vector2Int uvSize = piece.GetBoxUVSize();

				// Then try and place it
				Queue<Vector2Int> possiblePositions = new Queue<Vector2Int>();
				possiblePositions.Enqueue(Vector2Int.zero);

				while(possiblePositions.Count > 0)
				{
					// Test this position for overlaps
					Vector2Int testPos = possiblePositions.Dequeue();
					Vector2Int nextTestPos = testPos;
					RectInt testRect = new RectInt(testPos, uvSize);
					bool anyOverlap = false;
					foreach(RectInt overlapTest in mappings.Values)
					{
						if(overlapTest.Overlaps(testRect))
						{
							// Keep bumping these bounds outwards. We want to definitely move far enough in x/y
							nextTestPos.x = Mathf.Max(nextTestPos.x, overlapTest.max.x);
							nextTestPos.y = Mathf.Max(nextTestPos.y, overlapTest.max.y);
							anyOverlap = true;
						}
					}
					if(anyOverlap)
					{
						possiblePositions.Enqueue(new Vector2Int(testPos.x, nextTestPos.y));
						possiblePositions.Enqueue(new Vector2Int(nextTestPos.x, testPos.y));
					}
					else
					{
						// When we select a space, make sure the texture is big enough to include it
						while (testRect.max.x > maxBounds.x)
							maxBounds.x *= 2;
						while (testRect.max.y > maxBounds.y)
							maxBounds.y *= 2;

						mappings.Add(piecePreview, testRect);
						possiblePositions.Clear();	
					}
				}

			}
		}

		// UV map has been calculated, create the texture
		MinecraftModel.NamedTexture namedTexture = rig.GetOrCreateNamedTexture(skinName);
		namedTexture.Texture = new Texture2D(maxBounds.x, maxBounds.y);
		foreach(var kvp in mappings)
		{
			Texture2D tempTex = kvp.Key.GetTemporaryTexture();
			for(int i = 0; i < kvp.Value.width; i++)
				for(int j = 0; j < kvp.Value.height; j++)
				{
					namedTexture.Texture.SetPixel(
						kvp.Value.min.x + i, 
						kvp.Value.min.y + j,
						tempTex.GetPixel(i, j));
				}
		}
		namedTexture.Texture.Apply();
		return true;
	}

}
