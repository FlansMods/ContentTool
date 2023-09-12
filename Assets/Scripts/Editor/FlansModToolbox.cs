using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FlansModToolbox : EditorWindow
{
    [MenuItem ("Flan's Mod/Toolbox")]
    public static void  ShowWindow () 
	{
        EditorWindow.GetWindow(typeof(FlansModToolbox));
    }

	private ModelPreivewer ModelPreviewerInst = null;
	private DefinitionImporter DefinitionImporter = null;

	private string SelectedContentPackName = "";

    void OnGUI()
	{
		if(ModelPreviewerInst == null)
		{
			ModelPreviewerInst = FindObjectOfType<ModelPreivewer>();
		}
		if(DefinitionImporter == null)
		{
			DefinitionImporter = FindObjectOfType<DefinitionImporter>();
		}

		if(DefinitionImporter != null)
		{
			// Pack selector
			GUILayout.Label("Select Content Pack");
			List<string> packNames = new List<string>();
			packNames.Add("None");
			int selectedPackIndex = 0;
			for(int i = 0; i < DefinitionImporter.Packs.Count; i++)
			{
				ContentPack pack = DefinitionImporter.Packs[i];
				packNames.Add(pack.ModName);
				if(pack.ModName == SelectedContentPackName)
				{
					selectedPackIndex = i + 1;
				}
			}
			selectedPackIndex = EditorGUILayout.Popup(selectedPackIndex, packNames.ToArray());
			SelectedContentPackName = selectedPackIndex == 0 ? "" : DefinitionImporter.Packs[selectedPackIndex - 1].ModName;
			GUILayout.Label(" ------------ ");
			// ---


			if(selectedPackIndex >= 1)
			{
				ContentPack pack = DefinitionImporter.Packs[selectedPackIndex - 1];
				
			}
		}

		if(ModelPreviewerInst != null)
		{
			GUILayout.Label("Test");
		}

    }
}
