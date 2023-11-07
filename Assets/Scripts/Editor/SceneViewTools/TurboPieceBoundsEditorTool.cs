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
		if (preview.Piece == null)
			return;

		if (Changed(preview.Piece.Pos, _Handle.Origin)
		|| Changed(preview.Piece.Dim, _Handle.Dimensions))
		{
			ModelEditingSystem.ApplyOperation(
				new TurboResizeBoxOperation(
					preview.GetModel(),
					preview.Parent.PartName,
					preview.PartIndex,
					new Bounds(_Handle.Origin + _Handle.Dimensions / 2, _Handle.Dimensions)));
		}
	}

	public override void CopyToHandle(TurboPiecePreview preview)
	{
		if(preview.Piece == null)
			return;

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
		if (preview.Piece == null)
			return;

		List<int> changedIndices = new List<int>();
		List<Vector3> changedOffsets = new List<Vector3>();

		for(int i = 0; i < 8; i++)
		{
			if(Changed(preview.Piece.Offsets[i], _Handle.Offsets[i]))
			{
				changedIndices.Add(i);
				changedOffsets.Add(_Handle.Offsets[i]);
			}
		}

		if(changedIndices.Count > 0)
		{
			ModelEditingSystem.ApplyOperation(
				new TurboEditOffsetsOperation(
					preview.GetModel(),
					preview.Parent.PartName,
					preview.PartIndex,
					changedIndices,
					changedOffsets));
		}
	}

	public override void CopyToHandle(TurboPiecePreview preview)
	{
		if (preview.Piece == null)
			return;

		_Handle.SetOriginAndDims(preview.Piece.Pos, preview.Piece.Dim);
		for (int i = 0; i < preview.Piece.Offsets.Length; i++)
			_Handle.Offsets[i] = preview.Piece.Offsets[i];
	}
}

