using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemModel))]
public class ItemModelEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Vanilla Item Icon Editor"); }

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		FlanStyles.HorizontalLine();
		GUILayout.Label("Item Settings", FlanStyles.BoldLabel);

		if (target is ItemModel itemModel)
		{
			ResourceLocation changedLocation = ResourceLocation.EditorObjectField(itemModel.IconLocation, itemModel.Icon, "textures/item");
			if (changedLocation != itemModel.IconLocation)
			{
				itemModel.IconLocation = changedLocation;
				itemModel.Icon = changedLocation.Load<Texture2D>();
				EditorUtility.SetDirty(itemModel);
			}

			ResourceLocation thisLocation = itemModel.GetLocation();
			bool differentNamespace = itemModel.IconLocation.Namespace != thisLocation.Namespace;
			bool differentName = 
				thisLocation.IDWithoutPrefixes().Contains("_default") 
				? thisLocation.IDWithoutPrefixes() != $"{itemModel.IconLocation.IDWithoutPrefixes()}_default_icon"
				: thisLocation.IDWithoutPrefixes() != $"{itemModel.IconLocation.IDWithoutPrefixes()}_icon";
			if (differentNamespace)
			{
				GUILayout.BeginHorizontal();
				GUIVerify.VerificationIcon(VerifyType.Neutral);
				GUILayout.Label($"Icon {itemModel.IconLocation} is from another content pack");
				GUILayout.EndHorizontal();
			}
			if (differentName)
			{
				GUILayout.BeginHorizontal();
				GUIVerify.VerificationIcon(VerifyType.Neutral);
				GUILayout.Label($"Icon {itemModel.IconLocation} does not match the name of this Icon model");
				GUILayout.EndHorizontal();
			}

			if (differentNamespace || differentName)
			{
				string check = thisLocation.IDWithoutPrefixes();
				if (check.EndsWith("_icon"))
					check = check.Substring(0, check.Length - 5);
				if (check.EndsWith("_default"))
					check = check.Substring(0, check.Length - 8);

				ResourceLocation betterLocation = new ResourceLocation(thisLocation.Namespace, $"textures/item/{check}");
				if(betterLocation.TryLoad(out Texture2D texture))
				{
					GUILayout.Label($"Possible better match found at {betterLocation}");
					EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
					if(GUILayout.Button("Set"))
					{
						itemModel.IconLocation = betterLocation;
						itemModel.Icon = texture;
						EditorUtility.SetDirty(itemModel);
					}
				}
			}
		}
	}
}
