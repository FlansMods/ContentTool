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
	public override GUIContent toolbarIcon
	{
		get
		{
			GUIContent guiContent = new GUIContent(ToolbarIcon);
			guiContent.tooltip = "Resize Cube";
			return guiContent;
		}
	}
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
