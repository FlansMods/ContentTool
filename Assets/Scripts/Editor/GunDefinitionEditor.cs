using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GunDefinition))]
public class GunDefinitionEditor : Editor
{
	private bool loopPreview = true;
	public ModelPreivewer Previewer = null;

	public void DebugRender()
	{
		if(Previewer == null)
		{
			Previewer = FindObjectOfType<ModelPreivewer>();
		}
	}
    public override void OnInspectorGUI()
	{
		
		GunDefinition gun = (GunDefinition)target;
		if(gun != null)
		{
			DebugRender();
			if(Previewer.Def != gun)
			{
				Previewer.SetDefinition(gun);
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Preview skin:");
			int selected = -1;
			string[] skinNames = new string[gun.paints.paintjobs.Length + 1];
			for(int i = 0; i < skinNames.Length - 1; i++)
			{
				skinNames[i] = gun.paints.paintjobs[i].textureName;
				if(skinNames[i].ToLower() == Previewer.Skin.ToLower())
				{
					selected = i;
				}
			}
			skinNames[skinNames.Length - 1] = gun.name;
			selected = EditorGUILayout.Popup(selected, skinNames);
			if(selected >= 0 && skinNames[selected] != Previewer.Skin)
			{
				Previewer.SetSkin(skinNames[selected]);
			}
			//Previewer.Anim = (AnimationDefinition)EditorGUILayout.ObjectField(Previewer.Anim, typeof(AnimationDefinition), false);
			GUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();

		base.OnInspectorGUI();

	}

	private void PlayPreview()
	{
		Previewer.Playing = true;
	}

	private void PausePreview()
	{
		Previewer.Playing = false;
	}
}
