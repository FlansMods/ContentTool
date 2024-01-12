using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

public class RootNode : Node
{
	public Vector2Int UVMapSize = Vector2Int.zero;
	public TextureList Textures = new TextureList();
	public TextureList Icons = new TextureList();
	public List<AnimationParameter> AnimationParameters = new List<AnimationParameter>();
	public FlanimationDefinition AnimationSet = null;



	public bool HasUVMap() { return UVMapSize != Vector2Int.zero; }
	public bool NeedsUVRemap() 
	{
		if (!HasUVMap())
			return true;
		foreach(GeometryNode geomNode in GetAllDescendantNodes<GeometryNode>())
		{
			if (!geomNode.IsUVMapCurrent())
				return true;
		}
		return false;
	}
	public void NumUVsToRemap(out int numToRemap, out int totalNum)
	{
		numToRemap = 0;
		totalNum = 0;
		foreach (GeometryNode geomNode in GetAllDescendantNodes<GeometryNode>())
		{
			totalNum++;
			if (!HasUVMap() || !geomNode.IsUVMapCurrent())
				numToRemap++;
		}
	}
	public void ApplyAutoUV()
	{
		
	}
	public NamedTexture CreateNewDefaultSkin()
	{
		Texture2D newSkinTexture = new Texture2D(UVMapSize.x, UVMapSize.y);
		//SkinGenerator.CreateDefaultTexture(bakedMap, newSkinTexture);


		/*
		string newSkinName = turboRig.name;
		if (newSkinName.EndsWith("_3d"))
			newSkinName = newSkinName.Substring(0, newSkinName.Length - 3);
		newSkinName = $"{newSkinName}";

		while (File.Exists($"Assets/Content Packs/{modelLocation.Namespace}/textures/skins/{newSkinName}.png"))
		{
			if (newSkinName.Contains("_new"))
				newSkinName += "_";
			else
				newSkinName += "_new";
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
		*/

		return new NamedTexture();
	}
	public NamedTexture CreateNewDefaultIcon()
	{
		Texture2D newSkinTexture = new Texture2D(UVMapSize.x, UVMapSize.y);
		return new NamedTexture();
	}

	public Texture2D GetTextureOrDefault(string key)
	{
		foreach (NamedTexture tex in Textures)
			if (tex.Key == key)
				return tex.Texture;
		return Texture2D.whiteTexture;
	}

	// -----------------------------------------------------------------------------------
	#region ItemTransforms
	// -----------------------------------------------------------------------------------
	public void AddDefaultTransforms()
	{

	}
	public bool TryGetItemTransform(ItemDisplayContext transformType, out ItemPoseNode node)
	{
		foreach (ItemPoseNode itemPoseNode in GetChildNodes<ItemPoseNode>())
			if (itemPoseNode.TransformType == transformType)
			{
				node = itemPoseNode;
				return true;
			}
		node = null;
		return false;
	}
	#endregion
	// -----------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------
	#region Operations
	// -----------------------------------------------------------------------------------
	public override bool SupportsTranslate() { return true; }
	public override void Translate(Vector3 deltaPos)
	{
		foreach (Node node in ChildNodes)
			if (node.SupportsTranslate())
				node.Translate(deltaPos);
	}
	public override bool SupportsRotate() { return true; }
	public override void Rotate(Vector3 deltaEuler)
	{
		foreach (Node node in ChildNodes)
			if (node.SupportsRotate())
				node.Rotate(deltaEuler);
	}
	public override bool SupportsMirror() { return true; }
	public override void Mirror(bool flipX, bool flipY, bool flipZ)
	{
		foreach (Node node in ChildNodes)
			if (node.SupportsMirror())
				node.Mirror(flipX, flipY, flipZ);
	}
	public override bool SupportsRename() { return false; }
	public override bool SupportsDelete() { return false; }
	public override bool SupportsDuplicate() { return false; }
	#endregion
	// -----------------------------------------------------------------------------------


	

	// -----------------------------------------------------------------------------------
	#region Material caching
	// Not to be exported to Minecraft, but useful to have inside the prefab in Unity
	// -----------------------------------------------------------------------------------
	public List<Material> UnityMaterialCache = new List<Material>();
	public Material GetMaterial(string key, ETurboRenderMaterial renderType)
	{
		string fullKey = $"{key}_{renderType}";
		foreach (Material mat in UnityMaterialCache)
			if(mat != null)
				if (mat.name == fullKey)
					return mat;

		Texture2D tex = GetTextureOrDefault(key);
		Material newMat = new Material(renderType.GetShader());
		newMat.name = fullKey;
		newMat.SetTexture("_MainTex", tex);
		newMat.EnableKeyword("_NORMALMAP");
		newMat.EnableKeyword("_DETAIL_MULX2");
		newMat.SetOverrideTag("RenderType", "Cutout");

#if UNITY_EDITOR
		// If this is inside a prefab, place the material in a subfolder adjacent
		string assetFolder = "Assets/UnityMaterialCache";
		if (!Directory.Exists(assetFolder))
			Directory.CreateDirectory(assetFolder);
		string matPath = $"{assetFolder}/{name}_{fullKey}.mat";
		AssetDatabase.CreateAsset(newMat, matPath);
		// Now re-load from disk and link that verison of the material
		newMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
#endif

		UnityMaterialCache.Add(newMat);
		return newMat;
	}


	public void SelectTexture(string key)
	{
		Texture2D tex = GetTextureOrDefault(key);

		foreach(ETurboRenderMaterial renderType in TurboRenderMaterials.Values)
		{
			List<Node> nodesForThisRenderType = new List<Node>();
			foreach (Node node in AllDescendantNodes)
			{
				if (node.NeedsMaterialType(renderType))
					nodesForThisRenderType.Add(node);
			}
			if(nodesForThisRenderType.Count > 0)
			{
				Material mat = GetMaterial(key, renderType);
				foreach (Node node in nodesForThisRenderType)
				{
					node.ApplyMaterial(renderType, mat);
				}
			}
		}		
	}
	#endregion
	// -----------------------------------------------------------------------------------





#if UNITY_EDITOR
	private static Vector2 TexturePreviewScroller = Vector2.zero;
	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		GUILayout.BeginHorizontal();
		GUILayout.Label($"UV Size:[{UVMapSize.x},{UVMapSize.y}]");
		if(NeedsUVRemap())
		{
			NumUVsToRemap(out int numToRemap, out int totalNum);
			GUILayout.Label($"{numToRemap}/{totalNum} UVs need remapping");
			if(GUILayout.Button("Auto"))
			{
				ApplyAutoUV();
			}
		}
		GUILayout.EndHorizontal();

		FlanimationDefinition changedAnimSet = (FlanimationDefinition)EditorGUILayout.ObjectField(AnimationSet, typeof(FlanimationDefinition), true);
		if(changedAnimSet != AnimationSet)
		{
			Undo.RecordObject(this, $"Selected anim set {changedAnimSet.name}");
			AnimationSet = changedAnimSet;
			EditorUtility.SetDirty(this);
		}

		Textures.ListField("Skins", this, (entry) =>
		{
			// Add a texture field
			ResourceLocation changedTextureLocation = ResourceLocation.EditorObjectField(entry.Location, entry.Texture, "textures/skins");
			if (changedTextureLocation != entry.Location)
			{
				entry.Location = changedTextureLocation;
				entry.Texture = changedTextureLocation.Load<Texture2D>();
			}
			GUILayout.BeginHorizontal();
			GUILayout.Label("Key: ");
			entry.Key = EditorGUILayout.DelayedTextField(entry.Key);
			GUILayout.EndHorizontal();

			if (entry.Texture != null)
			{
				TexturePreviewScroller = GUILayout.BeginScrollView(TexturePreviewScroller, GUILayout.ExpandHeight(false));
				FlanStyles.RenderTextureAutoWidth(entry.Texture);
				GUILayout.EndScrollView();
			}
		},
		() => {
			return CreateNewDefaultSkin();
		});

		Icons.ListField("Icons", this, 
			(entry) => {
				// Add a texture field
				ResourceLocation changedTextureLocation = ResourceLocation.EditorObjectField(entry.Location, entry.Texture, "textures/skins");
				if (changedTextureLocation != entry.Location)
				{
					entry.Location = changedTextureLocation;
					entry.Texture = changedTextureLocation.Load<Texture2D>();
				}
				GUILayout.BeginHorizontal();
				GUILayout.Label("Key: ");
				entry.Key = EditorGUILayout.DelayedTextField(entry.Key);
				GUILayout.EndHorizontal();

				if (entry.Texture != null)
				{
					TexturePreviewScroller = GUILayout.BeginScrollView(TexturePreviewScroller, GUILayout.ExpandHeight(false));
					FlanStyles.RenderTextureAutoWidth(entry.Texture);
					GUILayout.EndScrollView();
				}
			},
			() => {
				return CreateNewDefaultIcon();
			});

	}
#endif


	public override void GetVerifications(List<Verification> verifications)
	{
		base.GetVerifications(verifications);
		if (!HasUVMap())
		{
			NumUVsToRemap(out int numToRemap, out int totalNum);
			if (totalNum == 0)
			{
				verifications.Add(Verification.Success("No UV map needed"));
			}
			else if(numToRemap == totalNum)
			{
				verifications.Add(Verification.Failure("UV map has not been calculated",
					() => { ApplyAutoUV(); }));
			}
			else
			{
				verifications.Add(Verification.Failure($"UV map has {numToRemap} missing pieces",
						() => { ApplyAutoUV(); }));
			}
		}

		// TODO: Suggested content lookups
	}

	public override IEnumerable<IVerifiableAsset> GetAssets()
	{
		foreach (IVerifiableAsset baseAsset in base.GetAssets())
			yield return baseAsset;
		foreach (NamedTexture icon in Icons)
			yield return icon;
		foreach (NamedTexture texture in Textures)
			yield return texture;
	}
}
