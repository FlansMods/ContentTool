using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ShapeboxGeometryNode : BoxGeometryNode
{
	public Vector3[] Offsets = new Vector3[8];

	public override void Mirror(bool mirrorX, bool mirrorY, bool mirrorZ)
	{
		base.Mirror(mirrorX, mirrorY, mirrorZ);
		Offsets = JavaModelImporter.MirrorOffsets(Offsets, mirrorX, mirrorY, mirrorZ);
	}

	public virtual void ChangeOffsets(List<int> indices, List<Vector3> newOffsets)
	{
		if (indices.Count > 0)
		{
			Undo.RegisterCompleteObjectUndo(gameObject, $"Tweaked {indices.Count} ShapeboxGeometry corners in {name}");
			for (int i = 0; i < indices.Count; i++)
			{
				Offsets[indices[i]] = newOffsets[i];
			}
			EditorUtility.SetDirty(gameObject);
		}
	}

#if UNITY_EDITOR
	private static readonly int[] OldVertexOrder = new int[] {
		7, // V1 = +x, +y, +z
		6, // V2 = -x, +y, +z
		2, // V3 = -x, +y, -z
		3, // V4 = +x, +y, -z
		5, // V5 = +x, -y, +z
		4, // V6 = -x, -y, +z
		0, // V7 = -x, -y, -z
		1, // V8 = +x, -y, -z
	};

	private static readonly string[] VertexNames = new string[] {
		"Offset 1 (+x, +y, +z)",
		"Offset 2 (-x, +y, +z)",
		"Offset 3 (-x, +y, -z)",
		"Offset 4 (+x, +y, -z)",
		"Offset 5 (+x, -y, +z)",
		"Offset 6 (-x, -y, +z)",
		"Offset 7 (-x, -y, -z)",
		"Offset 8 (+x, -y, -z)",
	}; 
	public static bool ShouldShowVertex(int index)
	{
		switch (index)
		{
			case 0: return !X.HidePos && !Y.HidePos && !Z.HidePos;
			case 1: return !X.HideNeg && !Y.HidePos && !Z.HidePos;
			case 2: return !X.HideNeg && !Y.HidePos && !Z.HideNeg;
			case 3: return !X.HidePos && !Y.HidePos && !Z.HideNeg;
			case 4: return !X.HidePos && !Y.HideNeg && !Z.HidePos;
			case 5: return !X.HideNeg && !Y.HideNeg && !Z.HidePos;
			case 6: return !X.HideNeg && !Y.HideNeg && !Z.HideNeg;
			case 7: return !X.HidePos && !Y.HideNeg && !Z.HideNeg;
			default: return false;
		}
	}

	private static PairedToggle X;
	private static PairedToggle Y;
	private static PairedToggle Z;
	public static Vector3 groupSlider = Vector3.zero;

	private struct PairedToggle
	{
		public bool HidePos { get; private set; }
		public bool HideNeg { get; private set; }
		public void GUIField(string posLabel, string negLabel)
		{
			bool tickedPos = GUILayout.Toggle(!HidePos, posLabel);
			bool tickedNeg = GUILayout.Toggle(!HideNeg, negLabel);
			// The player made some sort of change
			if(tickedPos != HidePos || tickedNeg != HideNeg)
			{
				// If they tried to turn off both, we swap them
				if(!tickedPos && !tickedNeg)
				{
					HidePos = !HidePos;
					HideNeg = !HideNeg;
				}
				else
				{
					HidePos = !tickedPos;
					HideNeg = !tickedNeg;
				}
			}
		}
	}

	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		// Shapebox corner fields
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(FlanCustomButtons.ShapeboxCornersToolButton))
		{
			System.Type toolType = System.Type.GetType("ShapeboxCornersTool, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			if(toolType != null)
				UnityEditor.EditorTools.ToolManager.SetActiveTool(toolType);
		}
		GUILayout.Label("Filter Corners:", FlanStyles.BoldLabel);
		X.GUIField("+X", "-X");
		Y.GUIField("+Y", "-Y");
		Z.GUIField("+Z", "-Z");
		GUILayout.EndHorizontal();

		List<int> modifiedIndices = new List<int>();
		List<Vector3> modifedOffsets = new List<Vector3>();

		// The ultimate slider vec3 field
		Vector3 groupChanged = FlanStyles.CompactVector3Field("Move All", groupSlider);
		if(!groupChanged.Approximately(groupSlider))
		{
			groupSlider = groupChanged;
		}

		// When you let go with group slider stored, apply
		if (!groupSlider.Approximately(Vector3.zero) && !EditorGUIUtility.editingTextField)
		{
			for (int i = 0; i < 8; i++)
			{
				if (ShouldShowVertex(i))
				{
					int TransmutedVertexIndex = OldVertexOrder[i];
					modifiedIndices.Add(TransmutedVertexIndex);
					modifedOffsets.Add(Offsets[TransmutedVertexIndex] + groupSlider);
				}
			}
			groupSlider = Vector3.zero;
		}
		// Otherwise, check the normal sliders
		else
		{
			for (int i = 0; i < 8; i++)
			{
				EditorGUI.BeginDisabledGroup(!ShouldShowVertex(i));
				{
					int TransmutedVertexIndex = OldVertexOrder[i];
					Vector3 newOffset = FlanStyles.CompactVector3Field(VertexNames[i], Offsets[TransmutedVertexIndex]);
					if (!newOffset.Approximately(Offsets[TransmutedVertexIndex]))
					{
						modifiedIndices.Add(TransmutedVertexIndex);
						modifedOffsets.Add(newOffset);
					}
				}
				EditorGUI.EndDisabledGroup();
			}
		}
		ChangeOffsets(modifiedIndices, modifedOffsets);
	}
#endif

	public override Vector3[] GenerateVertsNoUV()
	{
		Vector3[] verts = new Vector3[8];
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				for (int z = 0; z < 2; z++)
				{
					int index = x + y * 2 + z * 4;
					verts[index] = Vector3.zero;
					if (x == 1) verts[index].x += Dim.x;
					if (y == 1) verts[index].y += Dim.y;
					if (z == 1) verts[index].z += Dim.z;

					verts[index] += Offsets[index];
				}
			}
		}
		return verts;
	}

	public void OnDrawGizmosSelected()
	{
		Vector3[] verts = GenerateVertsNoUV();
		Vector3 center = Vector3.zero;
		for (int i = 0; i < 8; i++)
			center += verts[i];
		center /= 8;

		for (int i = 0; i < 8; i++)
		{
			Vector3 outwardDir = (verts[i] - center).normalized;
			Gizmos.DrawLine(
				transform.TransformPoint(verts[i]),
				transform.TransformPoint(verts[i] + outwardDir * 1.0f));
			Gizmos.DrawIcon(transform.TransformPoint(verts[i] + outwardDir * 1.2f), $"Vertex_{OldVertexOrder[i] + 1}.png");
		}
	}
}
