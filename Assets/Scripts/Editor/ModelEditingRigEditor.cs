using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModelEditingRig))]
public class ModelEditingRigEditor : Editor
{
	public override void OnInspectorGUI()
	{
		ModelEditingRig rig = (ModelEditingRig)target;
		if (rig == null)
			return;

		if (rig.ModelOpenedForEdit != null)
		{
			GUILayout.Label($"Editing {rig.ModelOpenedForEdit.name}");
		}
		else
		{
			GUILayout.Label("No model selected");
		}

		Object chosenObject = EditorGUILayout.ObjectField(rig.ModelOpenedForEdit, typeof(MinecraftModel), false);
		if (chosenObject != rig.ModelOpenedForEdit)
		{
			if (rig.ModelOpenedForEdit != null)
				rig.DiscardChanges();
			
			if (chosenObject == null)
			{
				//rig.SaveAndCloseModel();
			}
			else if (chosenObject is MinecraftModel mcModel)
			{
				rig.OpenModel(mcModel);
				Debug.Log("Switching to model " + mcModel);
			}
			else
			{
				Debug.LogWarning("Could not switch to object " + chosenObject);
			}
		}
		//if (GUILayout.Button("Open Model..."))
		//{
		//	Object chosenObject = EditorGUILayout.ObjectField(rig.ModelOpenedForEdit, typeof(MinecraftModel), false);
		//	if(chosenObject != rig.ModelOpenedForEdit
		//	&& chosenObject is MinecraftModel mcModel)
		//	{
		//		rig.OpenModel(mcModel);
		//	}
		//}
		EditorGUI.BeginDisabledGroup(rig.WorkingCopy == null);
		{
			if (GUILayout.Button("Save"))
				rig.SaveChanges();
			if (GUILayout.Button("Discard"))
				rig.DiscardChanges();

		}
		EditorGUI.EndDisabledGroup();

		// --------------------------------------------------------------------------------
		#region Animation Controls
		// --------------------------------------------------------------------------------
		rig.ApplyAnimation = GUILayout.Toggle(rig.ApplyAnimation, "Animations");
		EditorGUI.BeginDisabledGroup(!rig.ApplyAnimation);
		{
			
		}
		EditorGUI.EndDisabledGroup();
		#endregion
		// --------------------------------------------------------------------------------







		base.OnInspectorGUI();


	}
}
