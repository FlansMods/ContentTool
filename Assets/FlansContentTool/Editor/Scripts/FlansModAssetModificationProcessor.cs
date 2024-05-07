using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FlansModAssetModificationProcessor : AssetPostprocessor
{
	private void OnPreprocessAsset()
	{
		string path = assetImporter.assetPath;
		if(path.EndsWith(".asset"))
		{
			List<string> lines = new List<string>(File.ReadAllLines(path));
			for (int i = 0; i < lines.Count; i++)
			{
				if(lines[i].EndsWith("itemSettings:"))
				{

				}
			}
		}
		Definition def = AssetDatabase.LoadAssetAtPath<Definition>(path);
		if(def != null)
		{
			
		}

		{
			ModelImporter modelImporter = assetImporter as ModelImporter;
			if (modelImporter != null)
			{
				if (!assetPath.Contains("@"))
					modelImporter.importAnimation = false;
				modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
			}
		}
	}

	public static string[] OnWillSaveAssets(string[] paths)
	{
		GUIVerify.InvalidateCaches();

		for(int i = paths.Length - 1; i >= 0; i--)
		{
			string path = paths[i];

			Definition def = AssetDatabase.LoadAssetAtPath<Definition>(path);

			/*
			System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);


			if(typeof(TurboRootNode).IsAssignableFrom(assetType))
			{
				TurboRootNode model = AssetDatabase.LoadAssetAtPath<TurboRootNode>(path);
				if (model.NeedsUVRemap())
				{
					if (EditorUtility.DisplayDialog("Bake Skin Modifications?", $"Saving this model ({model.name}) will change the UV map. As a result, the current {model.Textures.Count} skins assigned will be remapped from the old map to the new map. This process is not lossless and you may want to backup your textures first", "Save", "Skip"))
					{
						List<Texture2D> textures = new List<Texture2D>();
						foreach (NamedTexture namedTexture in model.Textures)
						{
							textures.Add(namedTexture.Texture);
						}

						//UVMap existingMap = new UVMap();
						//foreach (GeometryNode geomNode in model.GetAllDescendantNodes<GeometryNode>())
						//{
						//	BoxUVPatch patch = new BoxUVPatch(geomNode.BoxUVBounds);
						//	if (geomNode.IsUVMapCurrent())
						//		existingMap.AddExistingPatchPlacement(new BoxUVPlacement()
						//		{
						//			Key = $"{geomNode.GetInstanceID()}",
						//		});
						//}

						SkinGenerator.RemapSkins(model, textures);

						foreach (Texture2D texture in textures)
						{
							string texturePath = AssetDatabase.GetAssetPath(texture);
							EditorUtility.SetDirty(texture);
							File.WriteAllBytes(texturePath, texture.EncodeToPNG());
							Debug.Log($"Wrote .png image to {texturePath}");
							AssetDatabase.ImportAsset(texturePath);
						}
						//model.BakedUVMap = rig.TemporaryUVMap;
						EditorUtility.SetDirty(model);
						//rig.TemporaryUVMap = null;
					}
					else Debug.Log($"Did not save {model.name} Model asset due to user prompt");
				}
			}*/
		}
		return paths;
	}
}
