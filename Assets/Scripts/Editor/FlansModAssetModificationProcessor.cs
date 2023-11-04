using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FlansModAssetModificationProcessor : AssetModificationProcessor
{
	public static string[] OnWillSaveAssets(string[] paths)
	{
		foreach (string path in paths)
		{
			if (path.Contains(".png"))
			{
				Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				if(tex != null)
				{
					File.WriteAllBytes(path, tex.EncodeToPNG());
					Debug.Log($"Wrote .png image to {path}");
				}
			}
		}
		return paths;
	}
}
