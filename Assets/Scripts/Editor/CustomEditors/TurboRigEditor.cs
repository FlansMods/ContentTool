using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TurboRig))]
public class TurboRigEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Turbo Rig Editor"); }

	private List<int> ExpandedTextures = new List<int>();
	private Vector2 TexturePreviewScroller = Vector2.zero;
	protected override void TexturingTabImpl(MinecraftModel mcModel)
	{
		UVMapField("Baked UV Map", mcModel.BakedUVMap);
		ModelEditingRig rig = RigSelector(mcModel);
		if (rig != null)
		{
			if (rig.TemporaryUVMap != null)
			{
				UVMapField("Temporary UV Map", rig.TemporaryUVMap);
				if (GUILayout.Button("Apply"))
				{

				}
				if (rig.DebugTexture != null)
				{
					RenderTextureAutoWidth(rig.DebugTexture);
				}
			}
		}
	

		ResourceLocation modelLocation = mcModel.GetLocation();
		List<string> existingTextures = new List<string>();
		if (mcModel is TurboRig turboRig)
		{
			//EditorGUI.BeginDisabledGroup(rig == null);
			//if(GUILayout.Button("Re-apply UVs to Preview"))
			//{
			//	rig.SelectSkin(rig.SelectedSkin);
			//}
			//EditorGUI.EndDisabledGroup();

			// Draw a box for each texture
			int indexToDelete = -1;
			int indexToDuplicate = -1;
			for (int i = 0; i < turboRig.Textures.Count; i++)
			{
				MinecraftModel.NamedTexture texture = turboRig.Textures[i];
				existingTextures.Add(texture.Location.IDWithoutPrefixes());
				List<Verification> verifications = new List<Verification>();
				texture.GetVerifications(verifications);
				if (texture.Location.Namespace != modelLocation.Namespace)
					verifications.Add(Verification.Neutral($"Texture {texture} is from another pack"));

				bool oldExpanded = ExpandedTextures.Contains(i);

				GUILayout.BeginHorizontal();
				bool newExpanded = EditorGUILayout.Foldout(oldExpanded, GUIContent.none);
				if (oldExpanded && !newExpanded)
					ExpandedTextures.Remove(i);
				else if (newExpanded && !oldExpanded)
					ExpandedTextures.Add(i);
				GUILayout.Label($"[{i}] {texture.Key}", GUILayout.Width(200));

				if (!newExpanded)
				{
					EditorGUI.BeginDisabledGroup(rig == null);
					if(GUILayout.Button(FlanStyles.ApplyPose))
					{
						rig.SelectSkin(texture.Key);
					}
					EditorGUI.EndDisabledGroup();
					if(GUILayout.Button(FlanStyles.DuplicateEntry))
					{
						indexToDuplicate = i;
					}
					if (GUILayout.Button(FlanStyles.DeleteEntry))
					{
						indexToDelete = i;
					}
					GUIVerify.VerificationIcon(verifications);
					ResourceLocation changedTextureLocation = ResourceLocation.EditorObjectField(texture.Location, texture.Texture, "textures/skins");
					if (changedTextureLocation != texture.Location)
					{
						texture.Location = changedTextureLocation;
						texture.Texture = changedTextureLocation.Load<Texture2D>();
					}
				}
				GUILayout.FlexibleSpace();

				GUILayout.EndHorizontal();

				if (newExpanded)
				{
					GUILayout.BeginHorizontal();

					GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 15));

					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Key: ");
					texture.Key = GUILayout.TextField(texture.Key);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Texture: ");
					ResourceLocation changedTextureLocation = ResourceLocation.EditorObjectField(texture.Location, texture.Texture, "textures/skins");
					if (changedTextureLocation != texture.Location)
					{
						texture.Location = changedTextureLocation;
						texture.Texture = changedTextureLocation.Load<Texture2D>();
					}
					GUILayout.EndHorizontal();

					if (texture.Texture != null)
					{
						TexturePreviewScroller = GUILayout.BeginScrollView(TexturePreviewScroller, GUILayout.ExpandHeight(false));
						RenderTextureAutoWidth(texture.Texture);
						GUILayout.EndScrollView();
					}
					GUIVerify.VerificationsBox(verifications);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(FlanStyles.DuplicateEntry))
					{
						indexToDuplicate = i;
					}
					if (GUILayout.Button(FlanStyles.DeleteEntry))
					{
						indexToDelete = i;
					}
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();

					GUILayout.EndHorizontal();
				}
			}

			if(indexToDelete != -1)
			{
				turboRig.Textures.RemoveAt(indexToDelete);
				EditorUtility.SetDirty(turboRig);
			}
			if(indexToDuplicate != -1)
			{
				MinecraftModel.NamedTexture existing = turboRig.Textures[indexToDuplicate];
				MinecraftModel.NamedTexture clone = new MinecraftModel.NamedTexture()
				{
					Key = $"{existing.Key}-",
					Location = existing.Location.Clone(),
					Texture = existing.Texture,
				};
				turboRig.Textures.Add(clone);
				EditorUtility.SetDirty(turboRig);
			}

			// And a row for "add new"
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(FlanStyles.AddEntry, GUILayout.Width(32)))
			{
				string newSkinName = turboRig.name;
				if (newSkinName.EndsWith("_3d"))
					newSkinName = newSkinName.Substring(0, newSkinName.Length - 3);
				newSkinName = $"{newSkinName}_new";

				while (File.Exists($"Assets/Content Packs/{modelLocation.Namespace}/textures/skins/{newSkinName}.png"))
				{
					newSkinName += "_";
				}
				string fullPath = $"Assets/Content Packs/{modelLocation.Namespace}/textures/skins/{newSkinName}.png";

				UVMap bakedMap = turboRig.BakedUVMap;
				Texture2D newSkinTexture = new Texture2D(bakedMap.MaxSize.x, bakedMap.MaxSize.y);
				newSkinTexture.name = newSkinName;
				SkinGenerator.CreateDefaultTexture(bakedMap, newSkinTexture);
				//AssetDatabase.CreateAsset(newSkinTexture, fullPath);
				File.WriteAllBytes(fullPath, newSkinTexture.EncodeToPNG());
				AssetDatabase.Refresh();
				newSkinTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
				turboRig.Textures.Add(new MinecraftModel.NamedTexture()
				{
					Key = newSkinName,
					Location = newSkinTexture.GetLocation(),
					Texture = newSkinTexture,
				});
				EditorUtility.SetDirty(turboRig);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			// Also, auto buttons for fixing up skins
			string pathToCheck = $"Assets/Content Packs/{modelLocation.Namespace}/textures/skins/";
			string modelName = modelLocation.IDWithoutPrefixes();
			if (modelName.EndsWith("_3d"))
				modelName = modelName.Substring(0, modelName.Length - 3);
			if(Directory.Exists(pathToCheck))
			{
				foreach(string file in Directory.EnumerateFiles(pathToCheck))
				{
					if(file.Contains(modelName))
					{
						string textureName = file.Substring(file.LastIndexOfAny(SLASHES) + 1);
						textureName = textureName.Substring(0, textureName.LastIndexOf('.'));
						if (!existingTextures.Contains(textureName))
						{
							ResourceLocation textureLoc = new ResourceLocation(modelLocation.Namespace, $"textures/skins/{textureName}");
							if (textureLoc.TryLoad(out Texture2D texture))
							{
								GUILayout.Label($"Found possible matching skin {textureLoc}");
								EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
								if(GUILayout.Button("Add"))
								{
									turboRig.Textures.Add(new MinecraftModel.NamedTexture()
									{
										Key = textureName,
										Location = textureLoc,
										Texture = texture,
									});
									EditorUtility.SetDirty(turboRig);
								}
							}
						}
					}
				}
			}
		}
	}

	private void RenderTextureAutoWidth(Texture texture)
	{
		if (MinecraftModelPreview.TextureZoomLevel == 0)
		{
			float scale = (float)(Screen.width - 10) / texture.width;
			GUILayout.Label(GUIContent.none,
							GUILayout.Width(texture.width * scale),
							GUILayout.Height(texture.height * scale));
		}
		else
		{
			GUILayout.Label(GUIContent.none,
							GUILayout.Width(texture.width * MinecraftModelPreview.TextureZoomLevel),
							GUILayout.Height(texture.height * MinecraftModelPreview.TextureZoomLevel));
		}
		GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
	}
}
