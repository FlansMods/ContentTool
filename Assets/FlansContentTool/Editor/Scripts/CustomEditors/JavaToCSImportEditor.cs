using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(JavaToCSImport))]
public class JavaToCSImportEditor : Editor
{
	public override void OnInspectorGUI()
	{
		JavaToCSImport instance = (JavaToCSImport)target;
		if(instance != null)
		{
			if(GUILayout.Button("Scan Directory"))
			{
				string folder = EditorUtility.OpenFolderPanel("Select Definitions .java Root Folder", "", "");
				instance.AutoMappings.Clear();
				foreach(string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
				{
					if (file.Contains("Definition"))
					{
						if (file.Contains("Definitions"))
							continue;
						if (file.Contains("JsonDefinition"))
							continue;
						if (file.Contains("DefinitionParser"))
							continue;

						instance.AutoMappings.Add(file);
					}
					else
					{
						string fileName = file.Substring(file.LastIndexOfAny(Utils.SLASHES)+1);
						// Dodgy as heck, but kinda how my Enum naming works
						if(fileName.StartsWith("E") && fileName[1] == fileName.ToUpper()[1])
						{
							instance.AutoEnums.Add(file);
						}
					}

				}
			}

			GUILayout.Label($"Found {instance.AutoMappings.Count} files");

			if(GUILayout.Button("Process"))
			{
				instance.Process();
			}

			base.OnInspectorGUI();
		}
	}
}
