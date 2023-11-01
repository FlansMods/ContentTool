using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Model;

[System.Serializable]
public class UVMap
{
	[System.Serializable]
	public class UVPlacement
    {
        public UVPatch Patch;
        public Vector2Int Origin;
        public RectInt GetBounds()
        {
            return new RectInt(Origin, Patch.GetBoundingSize());
        }

		public override bool Equals(object other)
		{
            if (other is UVPlacement otherUV)
                return Patch.Equals(otherUV.Patch) && Origin == otherUV.Origin;
            return false;
		}
		public override int GetHashCode()
		{
            return Patch.GetHashCode() ^ Origin.GetHashCode();
		}
        public static bool operator==(UVPlacement a, UVPlacement b)
        {
            return a.Origin == b.Origin && a.Equals(b.Patch);
        }
		public static bool operator !=(UVPlacement a, UVPlacement b)
		{
			return a.Origin != b.Origin || !a.Patch.Equals(b.Patch);
		}
	}

    public Vector2Int MaxSize = Vector2Int.one * 4;
    public Dictionary<string, UVPatch> PatchesToPlace = new Dictionary<string, UVPatch>();
    public Dictionary<string, UVPlacement> Placement = new Dictionary<string, UVPlacement>();

    public void Reset() 
    {
        PatchesToPlace.Clear();
        Placement.Clear();
    }

    public bool Solves(UVMap srcMap)
    {
        // Any defined placements MUST be present and identical
        foreach(var kvp in srcMap.Placement)
        {
            if (!Placement.TryGetValue(kvp.Key, out UVPlacement dstPlacement))
                return false;
            if (dstPlacement != kvp.Value)
                return false;
        }

        // But for our unplaced patches, we just need to know that the other map has them
        foreach(var kvp in srcMap.PatchesToPlace)
        {
            // Doesn't exist
            if (!Placement.TryGetValue(kvp.Key, out UVPlacement dstPlacement))
                return false;
            // Wrong size
            if (!dstPlacement.Patch.Equals(kvp.Value))
                return false;
        }

        return true;
    }

    public static UVMap GetPatchesFor(MinecraftModel model)
    {
		UVMap map = new UVMap();
		if (model != null)
			model.GenerateUVPatches(map.PatchesToPlace);
		return map;
	}

    public static UVMap ExportFromModel(MinecraftModel model)
    {
        UVMap map = new UVMap();
        if(model != null)
            model.ExportUVMap(map.Placement);
        map.CalculateBounds();
        return map;
    }

    public void CollectPatches(MinecraftModel model)
    {
        model.GenerateUVPatches(PatchesToPlace);
    }
    
    public bool HasUnplacedPatch(out string key)
    {
        if(PatchesToPlace.Count > 0)
        {
            foreach(var kvp in PatchesToPlace)
            {
                key = kvp.Key;
				return true; // Weird idea but okay, just iterate once to get the next
            }
        }
        key = null;
        return false;
    }

    public void AutoPlacePatch(string key)
    {
        if (PatchesToPlace.TryGetValue(key, out UVPatch patch))
        {
            Vector2Int uvSize = patch.GetBoundingSize();
			Queue<Vector2Int> possiblePositions = new Queue<Vector2Int>();
			possiblePositions.Enqueue(Vector2Int.zero);

			while (possiblePositions.Count > 0)
			{
				// Test this position for overlaps
				Vector2Int testPos = possiblePositions.Dequeue();
				Vector2Int nextTestPos = testPos;

                bool canPlace = !Overlaps(patch, testPos, ref nextTestPos);
                if(canPlace)
                {
                    Placement.Add(key, new UVPlacement()
                    {
                        Patch = patch,
                        Origin = testPos
                    });
                    PatchesToPlace.Remove(key);
					possiblePositions.Clear();
				}
                else
                {
					possiblePositions.Enqueue(new Vector2Int(testPos.x, nextTestPos.y));
					possiblePositions.Enqueue(new Vector2Int(nextTestPos.x, testPos.y));
				}
			}

            if(PatchesToPlace.ContainsKey(key))
            {
                Debug.LogError($"Failed to place UV patch {key}");
                // We HAVE TO remove this from the list, because otherwise it will be an infinite loop
                PatchesToPlace.Remove(key);
			}
		}
	}

    public bool TryPlacement(string key, Vector2Int position)
    {
        if(PatchesToPlace.TryGetValue(key, out UVPatch patch))
        {
            bool canPlace = !Overlaps(patch, position);
            if (canPlace)
            {
                PatchesToPlace.Remove(key);
                Placement.Add(key, new UVPlacement()
                {
                    Origin = position,
                    Patch = patch,
                });
                return true;
            }
		}
        return false;
    }

	public bool Overlaps(UVPatch patch, Vector2Int position, ref Vector2Int nextTestPos)
	{
		RectInt testBounds = new RectInt(position, patch.GetBoundingSize());
        bool anyOverlap = false;
		foreach (var kvp in Placement)
		{
			RectInt existingBounds = kvp.Value.GetBounds();
            if (existingBounds.Overlaps(testBounds))
            {
				nextTestPos.x = Mathf.Max(nextTestPos.x, existingBounds.max.x);
				nextTestPos.y = Mathf.Max(nextTestPos.y, existingBounds.max.y);
                anyOverlap = true;
			}
		}
		return anyOverlap;
	}
	public bool Overlaps(UVPatch patch, Vector2Int position)
    {
        RectInt testBounds = new RectInt(position, patch.GetBoundingSize());
        foreach(var kvp in Placement)
        {
            RectInt existingBounds = kvp.Value.GetBounds();
            if (existingBounds.Overlaps(testBounds))
                return true;
		}
        return false;
    }

    public void CalculateBounds()
    {
		MaxSize = Vector2Int.one * 4;
		foreach (var kvp in Placement)
        {
            RectInt rect = kvp.Value.GetBounds();
            int maxX = Mathf.NextPowerOfTwo(rect.x);
            int maxY = Mathf.NextPowerOfTwo(rect.y);

            if (maxX > MaxSize.x)
                MaxSize.x = maxX;
            if (maxY > MaxSize.y)
                MaxSize.y = maxY;
		}
    }

 

    

}
