using log4net.Util;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[EditorTool("Resize Box Bounds", typeof(BoxGeometryNode))]
public class BoxBoundsTool : MinecraftModelEditorTool<BoxGeometryNode>
{
	private ShapeboxBoundsHandle _Handle = new ShapeboxBoundsHandle();
	public override PrimitiveBoundsHandle BoundsHandle { get { return _Handle; } }
	public override GUIContent toolbarIcon { get { return FlanCustomButtons.BoxBoundsToolButton; } }

	public void OnEnable()
	{
		FlanCustomButtons.BoxBoundsTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Resources/shapebox_bounds.png");
	}
	public void OnDisable()
	{
		FlanCustomButtons.BoxBoundsTexture = null;
	}



	public override void CopyFromHandle(BoxGeometryNode boxNode)
	{
		if (boxNode == null)
			return;

		Vector3 toolOrigin = ToolStartedAtLocalPos.Value;

		Vector3 minLocalSpace = _Handle.Origin + toolOrigin;
		Vector3 dimLocalSpace = _Handle.Dimensions;

		if (!minLocalSpace.Approximately(boxNode.LocalOrigin))
		{
			Vector3 delta = minLocalSpace - boxNode.LocalOrigin;
			boxNode.Resize(boxNode.LocalOrigin + delta, boxNode.Dim - delta);
		}

		if (!boxNode.Dim.Approximately(dimLocalSpace))
		{
			boxNode.Resize(dimLocalSpace);
		}		
	}

	public override void CopyToHandle(BoxGeometryNode boxNode)
	{
		if(boxNode == null)
			return;

		Vector3 toolOrigin = ToolStartedAtLocalPos.Value;

		_Handle.SetOriginAndDims(boxNode.LocalOrigin - toolOrigin, boxNode.Dim);
	}
}
