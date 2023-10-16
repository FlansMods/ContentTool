using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UVCalculator
{
	public static bool AutoUV(MinecraftModel model, MinecraftModelPreview preview, string skinName)
	{
		if (model is TurboRig rig && preview is TurboRigPreview turboPreview)
			return AutoUVRig(rig, turboPreview, skinName);
		return false;
	}

	public static bool AutoUVRig(TurboRig rig, TurboRigPreview preview, string skinName)
	{
		Dictionary<TurboPiecePreview, RectInt> mappings = new Dictionary<TurboPiecePreview, RectInt>();
		Vector2Int maxBounds = new Vector2Int(16, 16);
		foreach (TurboModel section in rig.Sections)
		{
			TurboModelPreview modelPreview = preview.GetChild(section.PartName);
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
