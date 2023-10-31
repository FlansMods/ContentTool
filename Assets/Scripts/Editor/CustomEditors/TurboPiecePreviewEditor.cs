using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TurboPiecePreview))]
public class TurboPiecePreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboPiecePreview preview = (TurboPiecePreview)target;
		if (preview == null || preview.Parent == null || preview.Parent.Parent == null || preview.Parent.Parent.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Parent.Parent.Rig)
			RigEditor = CreateEditor(preview.Parent.Parent.Rig);
		if (RigEditor is TurboRigEditor rigEditor)
		{
			rigEditor.InitialModellingNode(preview);
		}
	}
}
