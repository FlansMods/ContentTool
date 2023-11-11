using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ModelEditingSystem
{
    public static Queue<ModelEditOperation> Operations = new Queue<ModelEditOperation>();

    public static bool ApplyOperation(ModelEditOperation op)
    {
        if (ShouldApplyOperation_Internal(op))
        {
            Operations.Enqueue(op);

			//ApplyOperation_Internal(op);
            return true;
        }
        return false;
    }

    // This could be used to let you modify a Unity transform lots and then click apply
    public static bool ShouldSkipRefresh(MinecraftModel model, string partName, int pieceIndex)
    {
        return false;
    }

    public static void ApplyAllQueuedActions()
    {
        if (Operations.Count > 0)
            Debug.Log($"Applying {Operations.Count} queued modelling operations");
        while (Operations.Count > 0)
        {
            ModelEditOperation op = Operations.Dequeue();
            ApplyOperation_Internal(op);
        }
    }

	public static bool AppliedToAnyRig(MinecraftModel model)
    {
		foreach (ModelEditingRig rig in UnityEngine.Object.FindObjectsOfType<ModelEditingRig>())
            if (rig.ModelOpenedForEdit == model)
                return true;
        return false;
	}

	public static IEnumerable<ModelEditingRig> GetRigsPreviewing(MinecraftModel model)
    {
        foreach(ModelEditingRig rig in UnityEngine.Object.FindObjectsOfType<ModelEditingRig>())
        {
            if (rig.ModelOpenedForEdit == model)
                yield return rig;
        }
    }

    private static bool ShouldApplyOperation_Internal(ModelEditOperation op)
    {
		bool willInvalidateUV = op.WillInvalidateUVMap(op.Model.BakedUVMap);
        bool openInAnyRig = AppliedToAnyRig(op.Model);
		if (willInvalidateUV && !openInAnyRig)
        {
            return false; // TODO: Modal popup
        }
        return true;
	}

    private static void ApplyOperation_Internal(ModelEditOperation op)
    {
        bool willInvalidateUV = op.WillInvalidateUVMap(op.Model.BakedUVMap);
        bool appliedToAnyRig = false;
        foreach (ModelEditingRig rig in GetRigsPreviewing(op.Model))
        {
            appliedToAnyRig = true;
        }

		Undo.RegisterCompleteObjectUndo(op.Model, op.UndoMessage);
		

		op.ApplyToModel();

		if (willInvalidateUV)
		{
			UVMap newMap = new UVMap();
			op.Model.CollectUnplacedUVs(newMap.UnplacedBoxes);
			if (!op.Model.BakedUVMap.IsSolutionTo(newMap))
			{
				if (appliedToAnyRig)
				{
					// If this is being previewed, we need to apply a temporary texture and UV map
					newMap.AutoPlacePatches();
					foreach (ModelEditingRig rig in GetRigsPreviewing(op.Model))
					{
						rig.SetPreivewUVMap(newMap);
					}
				}
				// Otherwise, we are editing from the Project window, we should either warn or instantly apply the UV edits (dangerous)
				else
				{
                    // TODO: warn about texture invalidation
					newMap.AutoPlacePatches();
					op.Model.ApplyUVMap(newMap);
				}
			}
		}

		EditorUtility.SetDirty(op.Model);

        try
        {
            foreach (ModelEditingRig rig in GetRigsPreviewing(op.Model))
            {
                op.ApplyToPreview(rig.Preview);
            }
        }
        catch(Exception)
        {
            // We don't care so much about the previews
        }
	}
}
