using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MultiModel))]
public class MultiModelEditor : Editor
{
    public static readonly char[] SLASHES = new char[] { '/', '\\' };
	private static GUILayoutOption LABEL_COL_X = GUILayout.Width(90);
	private static GUILayoutOption DROPDOWN_COL_X = GUILayout.Width(140);


	public override void OnInspectorGUI()
    {
        if (target is MultiModel multi)
        {
            GUILayout.BeginHorizontal();
            FlanStyles.BigHeader("Rename All:");
            string rename = EditorGUILayout.DelayedTextField(target.name);
            GUILayout.EndHorizontal();
            if (rename != target.name)
            {
                string multiPath = AssetDatabase.GetAssetPath(target);
                string parentFolder = multiPath.Substring(0, multiPath.LastIndexOfAny(SLASHES));
                if (Directory.Exists($"{parentFolder}/{target.name}"))
                {
                    Directory.Move($"{parentFolder}/{target.name}", $"{parentFolder}/{rename}");
                    AssetDatabase.Refresh();
                    foreach (string file in Directory.EnumerateFiles($"{parentFolder}/{rename}"))
                    {
                        string fileName = file.Substring(file.LastIndexOfAny(SLASHES) + 1);
                        if (fileName.Contains(target.name))
                        {
                            AssetDatabase.RenameAsset($"{parentFolder}/{rename}/{fileName}", fileName.Replace(target.name, rename));
                            //File.Move($"{parentFolder}/{rename}/{fileName}",
                            //       $"{parentFolder}/{rename}/{fileName.Replace(target.name, rename)}");
                        }
                    }
                    AssetDatabase.DeleteAsset($"{parentFolder}/{target.name}");
                }
                AssetDatabase.RenameAsset(multiPath, rename);
                AssetDatabase.Refresh();
            }

			ModelSelector(multi, MinecraftModel.ItemTransformType.FIRST_PERSON_RIGHT_HAND, "First Person");
			ModelSelector(multi, MinecraftModel.ItemTransformType.THIRD_PERSON_RIGHT_HAND, "Third Person");
			ModelSelector(multi, MinecraftModel.ItemTransformType.HEAD, "On Head");
			ModelSelector(multi, MinecraftModel.ItemTransformType.GROUND, "On Ground");
			ModelSelector(multi, MinecraftModel.ItemTransformType.GUI, "In Inventory");
			ModelSelector(multi, MinecraftModel.ItemTransformType.FIXED, "Other");


			ResourceLocation thisLocation = multi.GetLocation();
            string thisName = thisLocation.IDWithoutPrefixes();
            string searchPath = $"Assets/Content Packs/{thisLocation.Namespace}/models/item/{thisName}";
            if (Directory.Exists(searchPath))
            {
                foreach (string file in Directory.EnumerateFiles(searchPath))
                {
                    if (file.Contains(thisName))
                    {
                        // TODO: Auto fixes for MultiModel
                    }
                }
            }
        }
    }

    private void ModelSelector(MultiModel multi, MinecraftModel.ItemTransformType transformType, string label)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label, LABEL_COL_X);
        MinecraftModel model = multi.GetModel(transformType);
        string modelName = model == null ? "" : model.name;

		// Step 1: See if there are some matches on disk
		ResourceLocation thisLocation = multi.GetLocation();
		string thisName = thisLocation.IDWithoutPrefixes();
		string searchPath = $"Assets/Content Packs/{thisLocation.Namespace}/models/item/{thisName}";
		List<string> selectableModelPaths = new List<string>();
        int selectedIndex = -1;
		if (Directory.Exists(searchPath))
		{
			foreach (string file in Directory.EnumerateFiles(searchPath))
			{
				string fileName = file.Substring(file.LastIndexOfAny(SLASHES) + 1);
                if (fileName.EndsWith(".meta"))
                    continue;
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
				if (fileName.Contains(thisName))
				{
                    if (fileName == modelName)
                        selectedIndex = selectableModelPaths.Count;
					selectableModelPaths.Add(fileName);
				}
			}
		}
        selectableModelPaths.Add("New 3D Model...");
		selectableModelPaths.Add("New Item Model... (switchable icons)");
		selectableModelPaths.Add("New Item Model... (one icon)");

		int changedIndex = EditorGUILayout.Popup(selectedIndex, selectableModelPaths.ToArray(), DROPDOWN_COL_X);
        if(changedIndex != selectedIndex)
        {
            if (!Directory.Exists(searchPath))
            {
                Directory.CreateDirectory(searchPath);
            }

			if (changedIndex < selectableModelPaths.Count - 3)
            {
                MinecraftModel selectedModel = AssetDatabase.LoadAssetAtPath<MinecraftModel>($"{searchPath}/{selectableModelPaths[changedIndex]}.asset");
                if(selectedModel != null)
                {
					multi.SetModel(transformType, selectedModel);
				}
            }
			else if(changedIndex == selectableModelPaths.Count - 1) // New item model no switcher
			{
				ItemModel itemModel = CreateInstance<ItemModel>();
                itemModel.AddDefaultTransforms();
				itemModel.name = $"{thisName}_default_icon";
                AssetDatabase.CreateAsset(itemModel, $"{searchPath}/{itemModel.name}.asset");
				multi.SetModel(transformType, itemModel);
			}
			else if(changedIndex == selectableModelPaths.Count - 2) // New item model with switcher
			{
				ItemModel itemModel = CreateInstance<ItemModel>();
                itemModel.AddDefaultTransforms();
				itemModel.name = $"{thisName}_default_icon";
				AssetDatabase.CreateAsset(itemModel, $"{searchPath}/{itemModel.name}.asset");

				SkinSwitcherModel skinSwitcherModel = CreateInstance<SkinSwitcherModel>();
                skinSwitcherModel.AddDefaultTransforms();
                skinSwitcherModel.DefaultModel = itemModel;
				skinSwitcherModel.name = $"{thisName}_icon";
				AssetDatabase.CreateAsset(skinSwitcherModel, $"{searchPath}/{skinSwitcherModel.name}.asset");
				multi.SetModel(transformType, skinSwitcherModel);
			}
            else if(changedIndex == selectableModelPaths.Count - 3) // New 3D model
            {
				TurboRig turboRig = CreateInstance<TurboRig>();
                turboRig.AddDefaultTransforms();
				turboRig.name = $"{thisName}_3d";
				AssetDatabase.CreateAsset(turboRig, $"{searchPath}/{turboRig.name}.asset");
                multi.SetModel(transformType, turboRig);
			}
			EditorUtility.SetDirty(multi);
		}
	    
        // Object field if that's what you prefer
		MinecraftModel changedModel = (MinecraftModel)EditorGUILayout.ObjectField(model, typeof(MinecraftModel), false);
        if(changedModel != model)
        {
            multi.SetModel(transformType, changedModel);
            EditorUtility.SetDirty(multi);
        }

        GUILayout.EndHorizontal();
	}
}
