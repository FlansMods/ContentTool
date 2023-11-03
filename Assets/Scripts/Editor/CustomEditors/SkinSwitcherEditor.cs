using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkinSwitcherModel))]
public class SkinSwitcherEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Vanilla Item Icon Editor"); }

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		FlanStyles.HorizontalLine();
		GUILayout.Label("Skin Switcher Settings", FlanStyles.BoldLabel);

		if (target is SkinSwitcherModel skinSwitcher)
		{
			ResourceLocation thisLocation = skinSwitcher.GetLocation();
			string searchName = skinSwitcher.name;
			if (searchName.EndsWith("_icon"))
				searchName = searchName.Substring(0, searchName.Length - 5);

			List<string> existingNames = new List<string>();
			int indexToRemove = -1;
			for(int i = 0; i < skinSwitcher.Models.Count; i++)
			{
				MinecraftModel model = skinSwitcher.Models[i];
				ResourceLocation location = skinSwitcher.Models[i].GetLocation();
				string existingName = location.IDWithoutPrefixes();
				if (existingName.EndsWith("_icon"))
					existingName = existingName.Substring(0, existingName.Length - 5);
				if (existingName.EndsWith("_default"))
					existingName = existingName.Substring(0, existingName.Length - 8);
				existingNames.Add(existingName);
				ResourceLocation changedLocation = ResourceLocation.EditorObjectField(location, model, "models/item");
				if (changedLocation != location)
				{
					skinSwitcher.Models[i] = changedLocation.Load<MinecraftModel>();
					EditorUtility.SetDirty(skinSwitcher);
				}

				if(!existingName.Contains(searchName))
				{
					GUILayout.BeginHorizontal();
					GUIVerify.VerificationIcon(VerifyType.Neutral);
					GUILayout.Label($"Skin {existingName} does not seem to match {searchName}");
					if(GUILayout.Button("Remove"))
					{
						indexToRemove = i;
					}
					GUILayout.EndHorizontal();
				}
			}

			if(indexToRemove != -1)
			{
				skinSwitcher.Models.RemoveAt(indexToRemove);
				EditorUtility.SetDirty(skinSwitcher);
			}

	

			string searchPath = $"Assets/Content Packs/{thisLocation.Namespace}/textures/item";
			if (Directory.Exists(searchPath))
			{
				foreach (string file in Directory.EnumerateFiles(searchPath))
				{
					if (file.Contains(searchName))
					{
						string actualFileName = file.Substring(file.LastIndexOfAny(SLASHES) + 1);
						actualFileName = actualFileName.Substring(0, actualFileName.LastIndexOf('.'));
						if (!existingNames.Contains(actualFileName))
						{
							ResourceLocation iconLoc = new ResourceLocation(thisLocation.Namespace, $"textures/item/{actualFileName}");
							if (iconLoc.TryLoad(out Texture2D icon))
							{
								GUILayout.Label($"Found possible additional icon {actualFileName} at {file}");
								EditorGUILayout.ObjectField(icon, typeof(Texture2D), false);
								if (GUILayout.Button("Add"))
								{
									ItemModel newModel = CreateInstance<ItemModel>();
									newModel.IconLocation = iconLoc;
									newModel.Icon = icon;
									if (actualFileName == searchName)
										actualFileName += "_default";
									AssetDatabase.CreateAsset(newModel, $"Assets/Content Packs/{thisLocation.Namespace}/models/item/{searchName}/{actualFileName}_icon.asset");
									skinSwitcher.Models.Add(newModel);
									EditorUtility.SetDirty(skinSwitcher);
								}
							}
						}
					}
				}
			}
		}
	}
}
