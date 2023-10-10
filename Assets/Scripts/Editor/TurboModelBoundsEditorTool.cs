using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TurboModelBoundsEditorTool
{

}

[CustomEditor(typeof(TurboModelPreview))]
public class TurboModelEditor : Editor
{ 
	public override void OnInspectorGUI()
	{
		TurboModelPreview preview = (TurboModelPreview)target;
		if (preview == null)
			return;

		if (preview.Section == null)
		{
			GUILayout.Label("Invalid model! Please select one:");
			preview.PartName = GUILayout.TextField(preview.PartName);
			return;
		}

		preview.Section.partName = GUILayout.TextField(preview.Section.partName);

		int pieceToDelete = -1;
		int pieceToDuplicate = -1;
		for(int i = 0; i < preview.Section.pieces.Length; i++)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label($"{i}");
			if(GUILayout.Button("Delete"))
			{
				pieceToDelete = i;
			}
			if(GUILayout.Button("Duplicate"))
			{
				pieceToDuplicate = i;
			}
			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("Add"))
		{
			preview.AddChild();
		}

		if(pieceToDuplicate != -1)
		{
			preview.DuplicateChild(pieceToDuplicate);
		}

		if(pieceToDelete != -1)
		{
			preview.DeleteChild(pieceToDelete);
		}
	}
}