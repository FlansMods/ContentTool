using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DefinitionImporter))]
public class DefinitionImporterEditor : Editor
{	
	private bool hasDoneInit = false;

    public override void OnInspectorGUI()
	{
		DefinitionImporter instance = (DefinitionImporter)target;
		if(instance != null)
		{
			base.OnInspectorGUI();

			if(!hasDoneInit)
			{
				instance.CheckInit();
				hasDoneInit = true;
			}

			instance.ExportRoot = EditorGUILayout.DelayedTextField(instance.ExportRoot);
			if(GUILayout.Button("Choose Export Folder"))
			{
				instance.ExportRoot  = EditorUtility.OpenFolderPanel("Select resources root folder", "", "");
				
			}
			

			GUILayout.Label("Imported Packs");

			List<string> packNames = new List<string>();

			foreach(ContentPack pack in instance.Packs)
			{
				if(pack != null)
					packNames.Add(pack.name);
			}

			foreach(string packName in packNames)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"> {packName}");
				if(GUILayout.Button("Re-Import"))
					instance.ImportPack(packName);
				if(GUILayout.Button("Export"))
					instance.ExportPack(packName);
				GUILayout.EndHorizontal();
			}

			GUILayout.Label("Unimported Packs");
			
			foreach(string packName in instance.UnimportedPacks)
			{
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Import"))
				{
					instance.ImportPack(packName);
				}
				GUILayout.Label($"> {packName}");
				GUILayout.EndHorizontal();
			}

		}
	}
}
