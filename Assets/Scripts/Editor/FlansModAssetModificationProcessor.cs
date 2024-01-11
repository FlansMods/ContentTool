using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FlansModAssetModificationProcessor : AssetModificationProcessor
{

	public static string[] OnWillSaveAssets(string[] paths)
	{
		GUIVerify.InvalidateCaches();

		for(int i = paths.Length - 1; i >= 0; i--)
		{
			string path = paths[i];
			System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
			if(typeof(MinecraftModel).IsAssignableFrom(assetType))
			{
				MinecraftModel model = AssetDatabase.LoadAssetAtPath<MinecraftModel>(path);
				if(ModelEditingSystem.AppliedToAnyRig(model))
				{
					foreach(ModelEditingRig rig in ModelEditingSystem.GetRigsPreviewing(model))
					{
						if(rig.TemporaryUVMap != null)
						{
							if (EditorUtility.DisplayDialog("Bake Skin Modifications?", $"Saving this model ({model.name}) will change the UV map. As a result, the current {model.Textures.Count} skins assigned will be remapped from the old map to the new map. This process is not lossless and you may want to backup your textures first", "Save", "Skip"))
							{
								List<Texture2D> textures = new List<Texture2D>();
								foreach (MinecraftModel.NamedTexture namedTexture in model.Textures)
								{
									textures.Add(namedTexture.Texture);
								}
								SkinGenerator.RemapSkins(model.BakedUVMap, rig.TemporaryUVMap, textures);
								foreach (Texture2D texture in textures)
								{
									string texturePath = AssetDatabase.GetAssetPath(texture);
									EditorUtility.SetDirty(texture);
									File.WriteAllBytes(texturePath, texture.EncodeToPNG());
									Debug.Log($"Wrote .png image to {texturePath}");
									AssetDatabase.ImportAsset(texturePath);
								}
								model.BakedUVMap = rig.TemporaryUVMap;
								EditorUtility.SetDirty(model);
								rig.TemporaryUVMap = null;
							}
							else Debug.Log($"Did not save {model.name} Model asset due to user prompt");
						}
					}
				}
			}
		}
		return paths;
	}
}
