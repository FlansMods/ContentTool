using System.Collections;
using System.Collections.Generic;
using System.IO;
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

			// Model

			// Skins
			gun.Skin = SkinSelectorWithRecommendation("Default Skin:", gun.Skin, gun.name, "textures/skins");
			gun.Icon = SkinSelectorWithRecommendation("Default Icon:", gun.Icon, gun.name, "textures/items");

			if(GUILayout.Button("Detect Skins"))
			{
				AutoDetectSkins(gun);
			}
			/*
			for(int i = 0; i < gun.paints.paintjobs.Length; i++)
			{
				string skinName = gun.paints.paintjobs[i].textureName;
				Texture2D foundSkin = null;
				Texture2D foundIcon = null;
				foreach(Definition.AdditionalTexture additionalTexture in gun.AdditionalTextures)
				{
					if(additionalTexture.name == skinName)
					{
						string assetPath = AssetDatabase.GetAssetPath(additionalTexture.texture);
						if(assetPath.Contains("textures/skins"))
							foundSkin = additionalTexture.texture;
						if(assetPath.Contains("textures/items"))
							foundIcon = additionalTexture.texture;
					}
				}
				Texture2D selectedSkin = SkinSelector($"    Skin {i}:", foundSkin);
				Texture2D selectedIcon = SkinSelector($"    Icon {i}:", foundIcon);
				if(selectedSkin != foundSkin || selectedIcon != foundIcon)
				{
					if(selectedSkin != null && selectedIcon == null)
					{
						string iconPath = AssetDatabase.GetAssetPath(selectedSkin).Replace("/skins", "/items");
						selectedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
					}
					if(selectedSkin == null && selectedIcon != null)
					{
						string skinPath = AssetDatabase.GetAssetPath(selectedIcon).Replace("/items", "/skins");
						selectedSkin = AssetDatabase.LoadAssetAtPath<Texture2D>(skinPath);
					}

					gun.paints.paintjobs[i].textureName = selectedSkin.name;
					gun.AdditionalTextures.Add(new Definition.AdditionalTexture()
					{
						name = selectedSkin.name,
						texture = selectedSkin,
						icon = selectedIcon,
					});
				}
			}*/




		}



		EditorGUILayout.Space();

		base.OnInspectorGUI();

	}

	private void AutoDetectSkins(GunDefinition gun)
	{
		FileInfo gunPath = new FileInfo(AssetDatabase.GetAssetPath(gun));
		gun.AdditionalTextures.Clear();
		List<PaintjobDefinition> matches = new List<PaintjobDefinition>();
		DirectoryInfo skinsDir = new DirectoryInfo($"{gunPath.Directory}/../textures/skins");
		if(skinsDir.Exists)
		{
			foreach(FileInfo skinFile in skinsDir.EnumerateFiles())
			{
				if(skinFile.Extension != ".png")
					continue;
				if(!skinFile.Name.Contains($"{gun.name}_"))
					continue;

				FileInfo iconFile = new FileInfo($"{gunPath.Directory}/../textures/items/{skinFile.Name}");
				if(iconFile.Exists)
				{
					matches.Add(new PaintjobDefinition()
					{
						textureName = skinFile.Name.Split('.')[0],
					});
					gun.AdditionalTextures.Add(new Definition.AdditionalTexture()
					{
						name = skinFile.Name.Split('.')[0],
						texture = AssetDatabase.LoadAssetAtPath<Texture2D>(skinFile.FullName.Substring(skinFile.FullName.IndexOf("Assets\\"))),
						icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFile.FullName.Substring(iconFile.FullName.IndexOf("Assets\\")))
					});						
				}
			}
		}
		gun.paints.paintjobs = matches.ToArray();
	}

	private Texture2D SkinSelector(string label, Texture2D texture)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label);
		texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
		GUILayout.EndHorizontal();
		return texture;
	}

	private Texture2D SkinSelectorWithRecommendation(string label, Texture2D texture, string suggestedName, string onPath)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label);
		texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
		Texture2D suggestedAsset = null;
		foreach(string assetGUID in AssetDatabase.FindAssets(suggestedName))
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
			if(assetPath.Contains($"{suggestedName}.png") && assetPath.Contains(onPath))
			{
				suggestedAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
			}
		}
		if(suggestedAsset != null)
		{
			if(GUILayout.Button($"Use recommended match, {suggestedAsset.name}"))
				texture = suggestedAsset;
		}
		GUILayout.EndHorizontal();
		return texture;
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
