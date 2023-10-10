using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContentPack))]
public class ContentPackEditor : Editor
{
    public override void OnInspectorGUI()
	{
		ContentPack pack = (ContentPack)target;
		if(pack != null)
		{
			if(GUILayout.Button("Auto-Add"))
			{
				pack.Content.Clear();
				foreach(string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{pack.name}/", "*.asset", SearchOption.AllDirectories))
				{
					if (assetPath.EndsWith($"{pack.name}.asset"))
						continue;
				//}
				//foreach(string assetGUID in AssetDatabase.FindAssets($"dir:{pack.name} t:Definition"))
				//{
				//	string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
					Definition def = AssetDatabase.LoadAssetAtPath<Definition>(assetPath);
					if(def != null)
					{
						pack.Content.Add(def);
					}
					else
					{
						Debug.LogError($"Failed to load asset at path {assetPath}");
					}
				}
			}

			if(GUILayout.Button("Export"))
			{
				DefinitionImporter importExport = FindObjectOfType<DefinitionImporter>();
				if(importExport != null)
				{
					importExport.ExportPack(pack.name);
				}
			}
		}
		base.OnInspectorGUI();


	}
}
