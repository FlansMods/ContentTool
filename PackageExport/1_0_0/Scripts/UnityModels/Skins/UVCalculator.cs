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
		BoxUVPlacement placement = new BoxUVPlacement()
		{
			Patch = patch,
			Origin = pos,
		};
		map.PlacedBoxes.Add(placement);
		if (placement.Bounds.xMax > map.MaxSize.x)
			map.MaxSize.x = Mathf.NextPowerOfTwo(placement.Bounds.xMax);
		if (placement.Bounds.yMax > map.MaxSize.y)
			map.MaxSize.y = Mathf.NextPowerOfTwo(placement.Bounds.yMax);
	}

	public static Vector2Int GetAutoPlacementForPatch(this UVMap map, BoxUVPatch patch)
	{
		Queue<Vector2Int> possiblePositions = new Queue<Vector2Int>();
		possiblePositions.Enqueue(Vector2Int.zero);
		Queue<Vector2Int> expandingPositions = new Queue<Vector2Int>();

		while (possiblePositions.Count > 0 || expandingPositions.Count > 0)
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
				if (hintPosX + patch.BoundingSize.x > map.MaxSize.x)
					expandingPositions.Enqueue(new Vector2Int(hintPosX, testPos.y));
				else
					possiblePositions.Enqueue(new Vector2Int(hintPosX, testPos.y));

				if (hintPosY + patch.BoundingSize.y > map.MaxSize.y)
					expandingPositions.Enqueue(new Vector2Int(testPos.x, hintPosY));
				else
					possiblePositions.Enqueue(new Vector2Int(testPos.x, hintPosY));
			}

			// No possibilities left, move on to expanding possibilities
			if (possiblePositions.Count == 0 && expandingPositions.Count > 0)
				possiblePositions.Enqueue(expandingPositions.Dequeue());
		}

		Debug.LogError($"Failed to place UV patch {patch.Key}");
		return Vector2Int.zero;
	}
}
