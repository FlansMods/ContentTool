using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Paint Model", typeof(MinecraftModelPreview))]
public class MinecraftModelPaintingTool : EditorTool
{
	public override GUIContent toolbarIcon => new GUIContent(ToolbarIcon);
	public Texture2D ToolbarIcon = null;
	public void OnEnable()
	{
		ToolbarIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/model_painting.png");
	}
	public void OnDisable()
	{
		ToolbarIcon = null;
	}
}
