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
		FlanCustomButtons.BoxBoundsTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/shapebox_bounds.png");
	}
	public void OnDisable()
	{
		FlanCustomButtons.BoxBoundsTexture = null;
	}

	public override void CopyFromHandle(BoxGeometryNode boxNode)
	{
		if (boxNode == null)
			return;

		if (!boxNode.Pos.Approximately(_Handle.Origin))
			boxNode.Translate(_Handle.Origin);

		if (!boxNode.Dim.Approximately(_Handle.Dimensions))
			boxNode.Resize(_Handle.Dimensions);
	}

	public override void CopyToHandle(BoxGeometryNode boxNode)
	{
		if(boxNode == null)
			return;

		_Handle.SetOriginAndDims(Vector3.zero, boxNode.Dim);
		//_Handle.Offsets = preview.Piece.Offsets;
	}
}
