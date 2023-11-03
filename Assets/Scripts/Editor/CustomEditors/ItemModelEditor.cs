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
			}
		}
	}
}
