using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Paint Model", typeof(MinecraftModelPreview))]
public class ColourPaintingTool : MinecraftModelPaintingTool
{
	public override void OnEnable()
	{
		base.OnEnable();
		BrushTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/paint_color.png");
	}
	public override void OnDisable()
	{
		base.OnDisable();
		BrushTexture = null;
	}
	protected override void DrawBrush()
	{

	}
	protected override void OnMouseDown() { }
	protected override void OnMouseHeld() { }
	protected override void OnMouseUp() { }
}

public abstract class MinecraftModelPaintingTool : EditorTool
{
	private static readonly int TOOL_HASH = "MinecraftModelPaintingTool".GetHashCode();
	public override GUIContent toolbarIcon => new GUIContent(ToolbarIcon);
	private Texture2D ToolbarIcon = null;
	protected Texture2D BrushTexture = null;
	public virtual void OnEnable()
	{
		ToolbarIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/model_painting.png");
	}
	public virtual void OnDisable()
	{
		ToolbarIcon = null;
	}
	protected abstract void DrawBrush();
	protected abstract void OnMouseDown();
	protected abstract void OnMouseHeld();
	protected abstract void OnMouseUp();

	private MinecraftModelPreview CurrentlyPainting = null;
	public override void OnToolGUI(EditorWindow window)
	{
		CurrentlyPainting = null;
		if (window is SceneView sceneView && target != null)
		{
			MinecraftModelPreview modelTarget = null;
			if (target is MinecraftModelPreview)
				modelTarget = target as MinecraftModelPreview;
			else if (target is GameObject go)
				modelTarget = go.GetComponent<MinecraftModelPreview>();

			if (modelTarget != null)
			{
				



			}
			else Debug.LogError($"Couldn't find a MinecraftModelPreview component on {target}");


			Event e = Event.current;
			int controlID = GUIUtility.GetControlID(TOOL_HASH, FocusType.Passive);

			// 1. Raycast all Paintable surfaces




			// 2. Render a "brush" overlay, like the terrain tool
			DrawBrush();

			// 3. Apply the event, using the current paint operation to them
			switch (e.type)
			{
				case EventType.Layout:
					HandleUtility.AddDefaultControl(controlID);
					break;
				case EventType.MouseMove:
					if(modelTarget != null)
						HandleUtility.Repaint();
					break;
				case EventType.MouseDown: // Start using this tool
					if(GUIUtility.hotControl == controlID
					&& e.button == 0
					&& !e.alt)
					{
						HandleUtility.AddDefaultControl(controlID);
						if(HandleUtility.nearestControl == controlID)
						{
							GUIUtility.hotControl = controlID;
							OnMouseDown();
							e.Use();
						}
					}
					break;
				case EventType.MouseDrag:
					if((GUIUtility.hotControl == controlID || GUIUtility.hotControl == 0)
					&& !e.alt
					&& e.button == 0)
					{
						HandleUtility.AddDefaultControl(controlID);
						if (HandleUtility.nearestControl == controlID)
						{
							OnMouseHeld();
							e.Use();
						}
					}
					break;
				case EventType.MouseUp:
					if(GUIUtility.hotControl == controlID)
					{
						GUIUtility.hotControl = 0;
						OnMouseUp();
						e.Use();
					}
					break;
			}
		}
		else Debug.LogError($"Can't use MinecraftModelPaintingTool on {target}");
	}

	private void DrawPaintColourBrush()
	{
		
	}
}
