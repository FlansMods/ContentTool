using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class UVCalculator
{
	public static bool IsSolutionTo(this UVMap solutionMap, UVMap testMap)
	{
		// Any defined placements MUST be present and identical
		foreach (BoxUVPlacement testPlacement in testMap.PlacedBoxes)
		{
			if (!solutionMap.HasPlacedPatchMatching(testPlacement))
				return false;
		}

		// But for our unplaced patches, we just need to know that the other map has them
		foreach (BoxUVPatch testPatch in testMap.UnplacedBoxes)
		{
			if (!solutionMap.HasConsideredUnplacedPatch(testPatch))
				return false;
		}

		return true;
	}

	public static void ManuallyPlace(this UVMap map, BoxUVPatch patch, Vector2Int pos, bool allowConflict)
	{
		if(allowConflict || CanManuallyPlace(map, patch, pos))
		{
			map.PlacedBoxes.Add(new BoxUVPlacement()
			{
				Patch = patch,
				Origin = pos,
			});
		}
	}

	public static bool CanManuallyPlace(this UVMap map, BoxUVPatch patch, Vector2Int pos)
	{
		return !map.OverlapTest(pos, patch);
	}

	public static void AutoPlacePatches(this UVMap map)
	{
		while(map.TryPopUnplacedPatch(out BoxUVPatch patch))
		{
			AutoPlacePatch(map, patch);
		}
	}

	public static void AutoPlacePatch(this UVMap map, BoxUVPatch patch)
	{
		Vector2Int pos = GetAutoPlacementForPatch(map, patch);
		map.PlacedBoxes.Add(new BoxUVPlacement()
		{
			Patch = patch,
			Origin = pos,
		});
	}

	public static Vector2Int GetAutoPlacementForPatch(this UVMap map, BoxUVPatch patch)
	{
		Queue<Vector2Int> possiblePositions = new Queue<Vector2Int>();
		possiblePositions.Enqueue(Vector2Int.zero);

		while (possiblePositions.Count > 0)
		{
			// Test this position for overlaps
			Vector2Int testPos = possiblePositions.Dequeue();

			int hintPosX = testPos.x, hintPosY = testPos.y;
			bool canPlace = !map.OverlapTestWithHints(testPos, patch, ref hintPosX, ref hintPosY);
			if(canPlace)
			{
				return testPos;
			}
			else
			{
				possiblePositions.Enqueue(new Vector2Int(testPos.x, hintPosY));
				possiblePositions.Enqueue(new Vector2Int(hintPosX, testPos.y));
			}
		}

		Debug.LogError($"Failed to place UV patch {patch.Key}");
		return Vector2Int.zero;
	}
}
