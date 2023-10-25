using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

		rig.Editor_Toolbox();

		EditorGUI.BeginDisabledGroup(rig.ModelOpenedForEdit == null);
		{
			EditorGUI.BeginChangeCheck();
			// --------------------------------------------------------------------------------
			#region Painting  Controls
			// --------------------------------------------------------------------------------
			rig.ApplySkin = GUILayout.Toggle(rig.ApplySkin, "Skins");
			EditorGUI.BeginDisabledGroup(!rig.ApplySkin);
			{
				if (rig.ModelOpenedForEdit != null)
				{
					for(int i = 0; i < rig.ModelOpenedForEdit.Textures.Count; i++)
					{
						MinecraftModel.NamedTexture tex = rig.ModelOpenedForEdit.Textures[i];
						GUILayout.BeginHorizontal(); 
						tex.Key = GUILayout.TextField(tex.Key);
						tex.Texture = (Texture2D)EditorGUILayout.ObjectField(tex.Texture, typeof(Texture2D), false);
						GUILayout.EndHorizontal();
						tex.Location = ResourceLocation.EditorField(tex.Location);
						
					}
				}
					
			}
			EditorGUI.EndDisabledGroup();
			#endregion
			// --------------------------------------------------------------------------------

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

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(rig.ModelOpenedForEdit);
			}
		}
		EditorGUI.EndDisabledGroup();






		base.OnInspectorGUI();


	}
}
