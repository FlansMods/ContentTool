using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetRightClickActions
{
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
