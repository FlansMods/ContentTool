using System.Collections;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public abstract class MinecraftModelEditorTool<TNodeType> : EditorTool where TNodeType : Node
{
	public bool Changed(Vector3 a, Vector3 b) { return !Mathf.Approximately(a.x, b.x) || !Mathf.Approximately(a.y, b.y) || !Mathf.Approximately(a.z, b.z); }


	public Vector3? ToolStartedAtLocalPos = null;


	public abstract PrimitiveBoundsHandle BoundsHandle { get; }
	public abstract void CopyToHandle(TNodeType shape);
	public abstract void CopyFromHandle(TNodeType shape);
	public override void OnToolGUI(EditorWindow window)
	{
		float oldSnap = EditorSnapSettings.scale;
		EditorSnapSettings.scale = 1.0f;
		foreach (var obj in targets)
		{
			if (obj is TNodeType node)
			{
				if(ToolStartedAtLocalPos == null)
				{
					ToolStartedAtLocalPos = node.LocalOrigin;
				}

				if (Mathf.Approximately(node.transform.lossyScale.sqrMagnitude, 0f))
					continue;

				using (new Handles.DrawingScope(Matrix4x4.TRS(
					node.transform.parent.TransformPoint(ToolStartedAtLocalPos.Value), 
					node.transform.rotation, 
					node.transform.lossyScale)))
				{
					Handles.DrawDottedLine(Vector3.zero, node.LocalOrigin - ToolStartedAtLocalPos.Value, 0.1f);

					CopyToHandle(node);
					BoundsHandle.SetColor(Color.cyan);

					EditorGUI.BeginChangeCheck();
					if (BoundsHandle is MinecraftBoundsHandle mcBounds)
						mcBounds.DrawMinecraftHandle();
					else BoundsHandle.DrawHandle();
					if (EditorGUI.EndChangeCheck())
					{
						CopyFromHandle(node);
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
