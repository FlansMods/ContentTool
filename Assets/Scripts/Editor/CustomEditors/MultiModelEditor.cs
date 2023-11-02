using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MultiModel))]
public class MultiModelEditor : Editor
{
    private static readonly char[] SLASHES = new char[] { '/', '\\' };

	public override void OnInspectorGUI()
    {
        if (target is MultiModel multi)
        {
            GUILayout.BeginHorizontal();
            FlanStyles.BigHeader("Rename All:");
            string rename = EditorGUILayout.DelayedTextField(target.name);
            GUILayout.EndHorizontal();
            if(rename != target.name)
            {
                string multiPath = AssetDatabase.GetAssetPath(target);
				string parentFolder = multiPath.Substring(0, multiPath.LastIndexOfAny(SLASHES));
				if (Directory.Exists($"{parentFolder}/{target.name}"))
                {
                    Directory.Move($"{parentFolder}/{target.name}", $"{parentFolder}/{rename}");
					AssetDatabase.Refresh();
					foreach (string file in Directory.EnumerateFiles($"{parentFolder}/{rename}"))
                    {
                        string fileName = file.Substring(file.LastIndexOfAny(SLASHES) +1);
                        if(fileName.Contains(target.name))
                        {
                            AssetDatabase.RenameAsset($"{parentFolder}/{rename}/{fileName}", fileName.Replace(target.name, rename));
                            //File.Move($"{parentFolder}/{rename}/{fileName}",
                            //       $"{parentFolder}/{rename}/{fileName.Replace(target.name, rename)}");
                        }
					}
					AssetDatabase.DeleteAsset($"{parentFolder}/{target.name}");
				}
				AssetDatabase.RenameAsset(multiPath, rename);
                AssetDatabase.Refresh();
            }
        }
        base.OnInspectorGUI();
    }
}
