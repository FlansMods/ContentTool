using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UVMap
{
    public List<BoxUVPatch> UnplacedBoxes = new List<BoxUVPatch>();
    public List<BoxUVPlacement> PlacedBoxes = new List<BoxUVPlacement>();
    public Vector2Int MaxSize = Vector2Int.one * 4;

    public void Clear() 
    {
		UnplacedBoxes.Clear();
		PlacedBoxes.Clear();
        MaxSize = Vector2Int.one * 4;
    }

	public UVMap Clone()
	{
		UVMap clone = new UVMap();
		foreach (BoxUVPatch unplaced in UnplacedBoxes)
			clone.UnplacedBoxes.Add(unplaced.Clone());
		foreach (BoxUVPlacement placed in PlacedBoxes)
			clone.PlacedBoxes.Add(placed.Clone());
		clone.MaxSize = MaxSize;
		return clone;
	}

	public void CalculateBounds()
	{
		MaxSize = Vector2Int.one * 4;
		foreach (BoxUVPlacement placement in PlacedBoxes)
		{
			RectInt rect = placement.Bounds;
			int maxX = Mathf.NextPowerOfTwo(rect.xMax);
			int maxY = Mathf.NextPowerOfTwo(rect.yMax);

			if (maxX > MaxSize.x)
				MaxSize.x = maxX;
			if (maxY > MaxSize.y)
				MaxSize.y = maxY;
		}
	}

	// --------------------------------------------------------------------------
	#region Adding Patches and/or Placements
	// --------------------------------------------------------------------------
	public void AddPatchForPlacement(BoxUVPatch patch)
	{
		UnplacedBoxes.Add(patch);
	}
	public void SetUVPlacement(string key, Vector2Int uvCoords)
	{
		BoxUVPlacement existingPlacement = GetPlacedPatch(key);
		BoxUVPatch existingUnplacedPatch = GetUnplacedPatch(key);
		if (existingPlacement.Valid)
			existingPlacement.Origin = uvCoords;
		else if(existingUnplacedPatch.Valid)
		{
			PlacedBoxes.Add(new BoxUVPlacement()
			{
				Origin = uvCoords,
				Patch = existingUnplacedPatch,
			});
			UnplacedBoxes.Remove(existingUnplacedPatch);
		}
		else
		{
			PlacedBoxes.Add(new BoxUVPlacement()
			{
				Origin = uvCoords,
				Patch = new BoxUVPatch()
				{
					Key = key,
					BoxDims = Vector3Int.zero,
				}
			});
		}
	}
	public void SetBoxSize(string key, Vector3Int boxSize)
	{
		BoxUVPlacement existingPlacement = GetPlacedPatch(key);
		if (existingPlacement != null)
			existingPlacement.Patch.BoxDims = boxSize;
		else
		{
			UnplacedBoxes.Add(new BoxUVPatch()
			{
				Key = key,
				BoxDims = boxSize,
			});
		}
	}
	public void AddExistingPatchPlacement(BoxUVPlacement placement)
	{
		PlacedBoxes.Add(placement);
	}
	#endregion
	// --------------------------------------------------------------------------

	// --------------------------------------------------------------------------
	#region Getting Patches and/or Placements
	// --------------------------------------------------------------------------
	public BoxUVPatch GetUnplacedPatch(string key)
    {
        foreach (BoxUVPatch patch in UnplacedBoxes)
            if (patch.Key == key)
                return patch;
        return BoxUVPatch.Invalid;
	}
	public BoxUVPlacement GetPlacedPatch(string key)
	{
		foreach (BoxUVPlacement placement in PlacedBoxes)
			if (placement.Key == key)
				return placement;
		return BoxUVPlacement.Invalid;
	}
    public BoxUVPatch GetAnyPatch(string key)
    {
        BoxUVPatch unplaced = GetUnplacedPatch(key);
        if (unplaced.Valid)
            return unplaced;
        BoxUVPlacement placed = GetPlacedPatch(key);
        if (placed.Valid)
            return placed.Patch;
        return BoxUVPatch.Invalid;
	}
    public bool HasPlacedPatch(string key)
    {
		foreach (BoxUVPlacement placement in PlacedBoxes)
			if (placement.Key == key)
				return true;
		return false;
	}
	public bool HasPlacedPatchMatching(BoxUVPlacement otherPlacement)
	{
		foreach (BoxUVPlacement placement in PlacedBoxes)
			if (placement == otherPlacement)
				return true;
		return false;
	}
    public bool HasPlacedPatchMatching(BoxUVPatch otherPatch)
    {
		foreach (BoxUVPlacement placement in PlacedBoxes)
			if (placement.Patch == otherPatch)
				return true;
        return false;
	}
    public bool HasUnplacedPatchMatching(BoxUVPatch otherPatch)
    {
		foreach (BoxUVPlacement placement in PlacedBoxes)
			if (placement.Patch == otherPatch)
				return true;
        return false;
	}
    public bool HasConsideredUnplacedPatch(BoxUVPatch otherPatch)
    {
        return HasUnplacedPatchMatching(otherPatch) || HasPlacedPatchMatching(otherPatch);
	}
	public bool HasAnyUnplacedPatches()
	{
		return UnplacedBoxes.Count > 0;
	}
	public bool TryPopUnplacedPatch(out BoxUVPatch patch)
	{
		if (UnplacedBoxes.Count > 0)
		{
			patch = UnplacedBoxes[UnplacedBoxes.Count - 1];
			UnplacedBoxes.RemoveAt(UnplacedBoxes.Count - 1);
			return true;
		}
		patch = BoxUVPatch.Invalid;
		return false;
	}
	#endregion
	// --------------------------------------------------------------------------

	// --------------------------------------------------------------------------
	#region Overlap Tests
	// --------------------------------------------------------------------------
	public bool OverlapTest(Vector2Int coords, BoxUVPatch testPatch) { return OverlapTest(new RectInt(coords, testPatch.BoundingSize)); }
	public bool OverlapTest(BoxUVPlacement testPlacement) { return OverlapTest(testPlacement.Bounds); }
	public bool OverlapTest(RectInt testRect)
	{
		foreach (BoxUVPlacement placement in PlacedBoxes)
			if (placement.Bounds.Overlaps(testRect))
				return true;
		return false;
	}
	public bool OverlapTestWithHints(Vector2Int coords, BoxUVPatch testPatch, ref int hintPosX, ref int hintPosY) { return OverlapTestWithHints(new RectInt(coords, testPatch.BoundingSize), ref hintPosX, ref hintPosY); }
	public bool OverlapTestWithHints(BoxUVPlacement testPlacement, ref int hintPosX, ref int hintPosY) { return OverlapTestWithHints(testPlacement.Bounds, ref hintPosX, ref hintPosY); }
	public bool OverlapTestWithHints(RectInt testRect, ref int hintPosX, ref int hintPosY)
	{
		bool anyOverlap = false;
		hintPosX = testRect.xMin;
		hintPosY = testRect.yMin;
		foreach (BoxUVPlacement placement in PlacedBoxes)
			if (placement.Bounds.Overlaps(testRect))
			{
				anyOverlap = true;
				hintPosX = Mathf.Max(hintPosX, placement.Bounds.xMax);
				hintPosY = Mathf.Max(hintPosY, placement.Bounds.yMax);
			}
		return anyOverlap;
	}
	#endregion
	// --------------------------------------------------------------------------

	

    
}
