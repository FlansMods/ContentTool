using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetRightClickActions
{
    [MenuItem("Assets/Flan's Mod/Update To Nodes")]
    private static void ConvertToNodesApply()
    {
		foreach (string selectedGUID in Selection.assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
			string assetFolder = assetPath.Substring(0, assetPath.LastIndexOfAny(Utils.SLASHES));
			MinecraftModel model = AssetDatabase.LoadAssetAtPath<MinecraftModel>(assetPath);
			if (model != null)
			{
				RootNode newRootNode = null;
				if (model is TurboRig turboRig)
				{
					// Convert our model to a RootNode
					newRootNode = ConvertToNodes.FromTurboRig(turboRig);
					newRootNode.SelectTexture("default");
				}
				else if(model is ItemModel itemModel)
				{
					newRootNode = ConvertToNodes.FromItemModel(itemModel);
				}

				if(newRootNode != null)
				{
					GameObject prefab = PrefabUtility.SaveAsPrefabAsset(newRootNode.gameObject, assetPath.Replace(".asset", ".prefab"), out bool success);
					if (!success)
						Debug.LogError($"Failed to convert to nodes {model}");
					Object.DestroyImmediate(newRootNode.gameObject);
				}
			}
		}
	}
	[MenuItem("Assets/Flan's Mod/Update To Nodes", true)]
	private static bool ConvertToNodesValidation()
	{
		foreach (string selectedGUID in Selection.assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
			System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (!typeof(MinecraftModel).IsAssignableFrom(assetType))
			{
				return false;
			}
		}
        return true;
	}

	[MenuItem("Assets/Flan's Mod/Add to Scene View")]
    private static void CreateRig()
    {
        foreach (string selectedGUID in Selection.assetGUIDs)
        {
			string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
            MinecraftModel model = AssetDatabase.LoadAssetAtPath<MinecraftModel>(assetPath);
            if(model != null)
            {
				GameObject newGO = new GameObject(model.name);
				ModelEditingRig rig = newGO.AddComponent<ModelEditingRig>();
                rig.OpenModel(model);
			}
		}
	}
    [MenuItem("Assets/Flan's Mod/Add to Scene View", true)]
    private static bool CreateRigValidation()
    {
        foreach(string selectedGUID in Selection.assetGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
            if (!typeof(MinecraftModel).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(assetPath)))
            {
                return false;
            }
        }
        return true;
    }
}