using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[EditorTool("Resize Cube Bounds", typeof(TurboPiecePreview))]
public class TurboPieceBoundsEditorTool : MinecraftModelEditorTool<TurboPiecePreview>
{
	private ShapeboxBoundsHandle _Handle = new ShapeboxBoundsHandle();
	public override PrimitiveBoundsHandle BoundsHandle { get { return _Handle; } }
	public override GUIContent toolbarIcon => new GUIContent(ToolbarIcon);
	public Texture2D ToolbarIcon = null;
	public void OnEnable()
	{
		ToolbarIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/shapebox_bounds.png");
	}
	public void OnDisable()
	{
		ToolbarIcon = null;
	}

	public override void CopyFromHandle(TurboPiecePreview preview)
	{
		if(Changed(preview.Piece.Pos, _Handle.Origin)
		|| Changed(preview.Piece.Dim, _Handle.Dimensions))
			Undo.RecordObject(preview.GetComponentInParent<TurboRigPreview>().Rig, "Shapebox resize");
		
		preview.Piece.Pos = _Handle.Origin;
		preview.Piece.Dim = _Handle.Dimensions;
		preview.Refresh();
	}

	public override void CopyToHandle(TurboPiecePreview preview)
	{
		_Handle.SetOriginAndDims(preview.Piece.Pos, preview.Piece.Dim);
		_Handle.Offsets = preview.Piece.Offsets;
	}
}

[EditorTool("Move Corners", typeof(TurboPiecePreview))]
public class TurboPieceCornersEditorTool : MinecraftModelEditorTool<TurboPiecePreview>
{
	private ShapeboxCornersHandle _Handle = new ShapeboxCornersHandle();
	public override PrimitiveBoundsHandle BoundsHandle { get { return _Handle; } }
	public override GUIContent toolbarIcon => new GUIContent(ToolbarIcon);
	public Texture2D ToolbarIcon = null;
	public void OnEnable()
	{
		ToolbarIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/shapebox_corners.png");
	}
	public void OnDisable()
	{
		ToolbarIcon = null;
	}

	public override void CopyFromHandle(TurboPiecePreview preview)
	{
		if (Changed(preview.Piece.Offsets[0], _Handle.Offsets[0])
		|| Changed(preview.Piece.Offsets[1], _Handle.Offsets[1])
		|| Changed(preview.Piece.Offsets[2], _Handle.Offsets[2])
		|| Changed(preview.Piece.Offsets[3], _Handle.Offsets[3])
		|| Changed(preview.Piece.Offsets[4], _Handle.Offsets[4])
		|| Changed(preview.Piece.Offsets[5], _Handle.Offsets[5])
		|| Changed(preview.Piece.Offsets[6], _Handle.Offsets[6])
		|| Changed(preview.Piece.Offsets[7], _Handle.Offsets[7]))
		{
			Undo.RecordObject(preview.GetComponentInParent<TurboRigPreview>().Rig, "Shapebox corners");
		}

		for (int i = 0; i < preview.Piece.Offsets.Length; i++)
			preview.Piece.Offsets[i] = _Handle.Offsets[i];
		preview.Refresh();
	}

	public override void CopyToHandle(TurboPiecePreview preview)
	{
		_Handle.SetOriginAndDims(preview.Piece.Pos, preview.Piece.Dim);
		for (int i = 0; i < preview.Piece.Offsets.Length; i++)
			_Handle.Offsets[i] = preview.Piece.Offsets[i];
	}
}
