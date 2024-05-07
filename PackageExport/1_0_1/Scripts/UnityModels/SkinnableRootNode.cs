using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class SkinnableRootNode : RootNode
{
	public Vector2Int UVMapSize = Vector2Int.zero;
	public bool HasUVMap() { return UVMapSize != Vector2Int.zero; }
	public bool NeedsUVRemap()
	{
		if (!HasUVMap())
			return true;
		foreach (GeometryNode geomNode in GetAllDescendantNodes<GeometryNode>())
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
	public UVMap ToMap()
	{
		UVMap map = new UVMap();
		foreach (GeometryNode geomNode in GetAllDescendantNodes<GeometryNode>())
		{
			if (!HasUVMap() || !geomNode.IsUVMapCurrent())
				map.AddPatchForPlacement(geomNode.UVRequirements);
			else
				map.AddExistingPatchPlacement(geomNode.BakedUV.min, geomNode.UVRequirements);
		}
		return map;
	}
	public UVMap RunAutoUV()
	{
		UVMap map = ToMap();
		// Run the Auto-UV algorithm
		map.AutoPlacePatches();
		return map;
	}
	public bool ApplyAutoUV(out UVMap resultMap)
	{
		if (NeedsUVRemap())
		{
			UVMap map = RunAutoUV();

			foreach (GeometryNode geomNode in GetAllDescendantNodes<GeometryNode>())
			{
				BoxUVPlacement placement = map.GetPlacedPatch(geomNode.UniqueName);
				if (placement.Valid)
				{
					geomNode.BakedUV = placement.Bounds;
				}
			}
			UVMapSize = map.MaxSize;
			Debug.Log("Applied auto UV map");
			EditorUtility.SetDirty(this);
			resultMap = map;
			return true;
		}
		resultMap = null;
		return false;
	}
	public NamedTexture CreateNewDefaultSkin()
	{
		Texture2D newSkinTexture = new Texture2D(UVMapSize.x, UVMapSize.y);
		ResourceLocation modelLocation = this.GetLocation();
		string newSkinName = name;
		while (File.Exists($"Assets/Content Packs/{modelLocation.Namespace}/textures/skins/{newSkinName}.png"))
		{
			if (newSkinName.Contains("_new"))
				newSkinName += "_";
			else
				newSkinName += "_new";
		}
		string fullPath = $"Assets/Content Packs/{modelLocation.Namespace}/textures/skins/{newSkinName}.png";

		newSkinTexture.name = newSkinName;
		if (ApplyAutoUV(out UVMap resultMap))
		{
			SkinGenerator.CreateDefaultTexture(ToMap(), newSkinTexture);
		}
		File.WriteAllBytes(fullPath, newSkinTexture.EncodeToPNG());
		AssetDatabase.Refresh();
		newSkinTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
		return new NamedTexture(newSkinName, newSkinTexture);
	}



	// -----------------------------------------------------------------------------------
	#region Material caching
	// Not to be exported to Minecraft, but useful to have inside the prefab in Unity
	// -----------------------------------------------------------------------------------
	public List<Material> UnityMaterialCache = new List<Material>();
	public Material GetMaterial(string key, ETurboRenderMaterial renderType)
	{
		return GetMaterialForTexture(
			key,
			() => { return GetTextureOrDefault(key); },
			renderType);
	}
	public delegate Texture2D TextureCreateFunc();
	public Material GetMaterialForTexture(string key, TextureCreateFunc textureProvider, ETurboRenderMaterial renderType)
	{
		string fullKey = $"{key}_{renderType}";
		foreach (Material mat in UnityMaterialCache)
			if (mat != null)
				if (mat.name == fullKey)
					return mat;

		Texture2D tex = textureProvider();
		Material newMat = new Material(renderType.GetShader());
		newMat.name = fullKey;
		newMat.SetTexture("_MainTex", tex);
		newMat.EnableKeyword("_NORMALMAP");
		newMat.EnableKeyword("_DETAIL_MULX2");
		newMat.SetOverrideTag("RenderType", "Cutout");

#if UNITY_EDITOR
		// In Editor, we cache this material on disk
		string assetFolder = GetMaterialCacheFolder();
		string matPath = $"{assetFolder}/{name}_{fullKey}.mat";
		AssetDatabase.CreateAsset(newMat, matPath);
		// Now re-load from disk and link that verison of the material
		newMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
#endif

		UnityMaterialCache.Add(newMat);
		return newMat;
	}

	// Temp Editor textures
#if UNITY_EDITOR
	public Material GetTemporaryMaterial()
	{
		return GetMaterialForTexture("temp",
			GetOrCreateTemporaryTexture,
			ETurboRenderMaterial.Solid);
	}
	private Texture2D GetOrCreateTemporaryTexture()
	{

		UVMap map = RunAutoUV();

		string path = $"{GetMaterialCacheFolder()}/{name}_TempTex.png";
		Texture2D tempTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		if (tempTex == null)
		{
			tempTex = new Texture2D(256, 256);
			AssetDatabase.CreateAsset(tempTex, path);
			TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
			if (importer != null)
			{
				importer.isReadable = true;
			}
			importer.SaveAndReimport();
			tempTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		}
		return tempTex;
	}
	private string GetMaterialCacheFolder()
	{
		string assetFolder = "Assets/UnityMaterialCache";
		if (!Directory.Exists(assetFolder))
			Directory.CreateDirectory(assetFolder);
		return assetFolder;
	}
#endif

	public void SelectTexture(string key)
	{
		Texture2D tex = GetTextureOrDefault(key);

		foreach (ETurboRenderMaterial renderType in TurboRenderMaterials.Values)
		{
			List<Node> nodesForThisRenderType = new List<Node>();
			foreach (Node node in AllDescendantNodes)
			{
				if (node.NeedsMaterialType(renderType))
					nodesForThisRenderType.Add(node);
			}
			if (nodesForThisRenderType.Count > 0)
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

	protected override void EditorUpdate()
	{
		base.EditorUpdate();
		List<Mesh> childMeshes = new List<Mesh>();
		foreach (GeometryNode geom in GetAllDescendantNodes<GeometryNode>())
		{
			if (geom.TryGetComponent(out MeshFilter meshFilter))
			{
				if (meshFilter.sharedMesh != null)
				{
					if (!childMeshes.Contains(meshFilter.sharedMesh))
						childMeshes.Add(meshFilter.sharedMesh);
					else
						meshFilter.sharedMesh = null;
				}
			}

			if (!geom.IsUVMapCurrent())
			{

			}
		}
	}

	public override void GetVerifications(IVerificationLogger verifications)
	{
		base.GetVerifications(verifications);

		// Textures
		if (Textures == null || Textures.Count == 0)
			verifications.Neutral($"No skins present");

		if (Textures != null && Textures.Count > 0 && Textures[0].Key != "default")
			verifications.Failure($"Default texture is named incorrectly as {Textures[0].Key}",
			() =>
			{
				ApplyQuickFix((TurboRootNode _this) =>
				{
					_this.Textures[0].Key = "default";
				});
				return this;
			});


	}
}
