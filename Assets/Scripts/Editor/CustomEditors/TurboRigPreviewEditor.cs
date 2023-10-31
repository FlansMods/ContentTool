using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TurboRigPreview))]
public class TurboRigPreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboRigPreview preview = (TurboRigPreview)target;
		if (preview == null || preview.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Rig)
			RigEditor = CreateEditor(preview.Rig);
		if (RigEditor != null)
			RigEditor.OnInspectorGUI();
	}
}