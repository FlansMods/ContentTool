using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TurboAttachPointPreview))]
public class TurboAttachPointPreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboAttachPointPreview preview = (TurboAttachPointPreview)target;
		if (preview == null || preview.Parent == null || preview.Parent.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Parent.Rig)
			RigEditor = CreateEditor(preview.Parent.Rig);
		if (RigEditor is TurboRigEditor rigEditor)
		{
			rigEditor.AttachPointNode(preview);
		}

		preview.LockPartPositions = GUILayout.Toggle(preview.LockPartPositions, "Lock Positions (pieces will stay still by altering their origins)");
		preview.LockAttachPoints = GUILayout.Toggle(preview.LockAttachPoints, "Lock Attach Points (if other APs are attached to this one, they are fixed)");
	}
}
