using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

[EditorTool("Move Corners", typeof(ShapeboxGeometryNode))]
public class ShapeboxCornersTool : MinecraftModelEditorTool<ShapeboxGeometryNode>
{
	private ShapeboxCornersHandle _Handle = new ShapeboxCornersHandle();
	public override PrimitiveBoundsHandle BoundsHandle { get { return _Handle; } }
	public override GUIContent toolbarIcon { get { return FlanCustomButtons.ShapeboxCornersToolButton; } }

	public void OnEnable()
	{
		FlanCustomButtons.ShapeboxCornersTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/shapebox_corners.png");
	}
	public void OnDisable()
	{
		FlanCustomButtons.ShapeboxCornersTexture = null;
	}

	public override void CopyFromHandle(ShapeboxGeometryNode shapebox)
	{
		if (shapebox == null)
			return;

		List<int> changedIndices = new List<int>();
		List<Vector3> changedOffsets = new List<Vector3>();

		for (int i = 0; i < 8; i++)
		{
			if (!shapebox.Offsets[i].Approximately(_Handle.Offsets[i]))
			{
				changedIndices.Add(i);
				changedOffsets.Add(_Handle.Offsets[i]);
			}
		}

		shapebox.ChangeOffsets(changedIndices, changedOffsets);
	}

	public override void CopyToHandle(ShapeboxGeometryNode shapebox)
	{
		if (shapebox == null)
			return;

		_Handle.SetOriginAndDims(Vector3.zero, shapebox.Dim);
		for (int i = 0; i < shapebox.Offsets.Length; i++)
			_Handle.Offsets[i] = shapebox.Offsets[i];
	}
}

