using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TurboModelPreview))]
public class TurboModelPreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboModelPreview preview = (TurboModelPreview)target;
		if (preview == null || preview.Parent == null || preview.Parent.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Parent.Rig)
			RigEditor = CreateEditor(preview.Parent.Rig);
		if (RigEditor is TurboRigEditor rigEditor)
		{
			rigEditor.InitialModellingNode(preview);
		}
	}
}


