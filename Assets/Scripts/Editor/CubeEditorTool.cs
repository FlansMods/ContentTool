using Codice.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[EditorTool("Edit Cube Model Preview", typeof(CubeModelPreview))]
public class CubeEditorTool : MinecraftModelEditorTool<CubeModelPreview>
{
	private BoxBoundsHandle Handle = new BoxBoundsHandle();
	public override PrimitiveBoundsHandle BoundsHandle { get { return Handle; } }
	public override void CopyToHandle(CubeModelPreview cube)
	{
		Handle.center = cube.Cube.Center;
		Handle.size = cube.Cube.Dimensions;
	}
	public override void CopyFromHandle(CubeModelPreview cube)
	{
		cube.Cube.Origin = Handle.center - Handle.size * 0.5f;
		cube.Cube.Dimensions = Handle.size;
	}
}
