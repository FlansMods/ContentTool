using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public abstract class MinecraftModelEditorTool<T> : EditorTool where T : MinecraftModelPreview
{
	public bool Changed(Vector3 a, Vector3 b) { return !Mathf.Approximately(a.x, b.x) || !Mathf.Approximately(a.y, b.y) || !Mathf.Approximately(a.z, b.z); }

	public abstract PrimitiveBoundsHandle BoundsHandle { get; }
	public abstract void CopyToHandle(T shape);
	public abstract void CopyFromHandle(T shape);
	public override void OnToolGUI(EditorWindow window)
	{
		float oldSnap = EditorSnapSettings.scale;
		EditorSnapSettings.scale = 1.0f;
		foreach (var obj in targets)
		{
			if (obj is T modelPreview)
			{
				if (Mathf.Approximately(modelPreview.transform.lossyScale.sqrMagnitude, 0f))
					continue;
				using (new Handles.DrawingScope(Matrix4x4.TRS(modelPreview.transform.position, modelPreview.transform.rotation, Vector3.one)))
				{
					CopyToHandle(modelPreview);
					BoundsHandle.SetColor(Color.cyan);

					EditorGUI.BeginChangeCheck();
					if (BoundsHandle is MinecraftBoundsHandle mcBounds)
						mcBounds.DrawMinecraftHandle();
					else BoundsHandle.DrawHandle();
					if (EditorGUI.EndChangeCheck())
					{
						//Undo.RecordObject(modelPreview, string.Format("Modify {0}", ObjectNames.NicifyVariableName(target.GetType().Name)));
						CopyFromHandle(modelPreview);
						modelPreview.Refresh();
					}
				}
			}
		}
		EditorSnapSettings.scale = oldSnap;
	}
	private static Vector3 ShapeboxPointToHandleSpace(Transform shapeboxTransform, Vector3 pointInShapebox)
	{
		return Handles.inverseMatrix * (shapeboxTransform.localToWorldMatrix * pointInShapebox);
	}
	private static Vector3 HandleSpaceToShapeboxPoint(Transform shapeboxTransform, Vector3 handlePos)
	{
		return shapeboxTransform.localToWorldMatrix.inverse * (Handles.matrix * handlePos);
	}
}
