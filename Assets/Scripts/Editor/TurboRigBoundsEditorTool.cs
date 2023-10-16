using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TurboRigBoundsEditorTool
{
    
}

[CustomEditor(typeof(TurboRigPreview))]
public class TurboRigEditor : Editor
{
	public override void OnInspectorGUI()
	{
		TurboRigPreview preview = (TurboRigPreview)target;
		if (preview == null)
			return;

		if (preview.Rig == null)
		{
			GUILayout.Label("Invalid model!");
			return;
		}

		int sectionToDelete = -1;
		int sectionToDuplicate = -1;
		for (int i = 0; i < preview.Rig.Sections.Count; i++)
		{
			GUILayout.BeginHorizontal();
			string changedName = GUILayout.TextField(preview.Rig.Sections[i].PartName);
			if(changedName != preview.Rig.Sections[i].PartName)
			{
				Transform existing = preview.transform.Find(preview.Rig.Sections[i].PartName);
				if (existing != null)
					existing.name = changedName;
				preview.Rig.Sections[i].PartName = changedName;
			}
			if (GUILayout.Button("Delete"))
			{
				sectionToDelete = i;
			}
			if (GUILayout.Button("Duplicate"))
			{
				sectionToDuplicate = i;
			}
			GUILayout.EndHorizontal();
		}

		if (GUILayout.Button("Add"))
		{
			preview.AddSection();
		}

		if (sectionToDuplicate != -1)
		{
			preview.DuplicateSection(sectionToDuplicate);
		}

		if (sectionToDelete != -1)
		{
			preview.DeleteSection(sectionToDelete);
		}
	}
}