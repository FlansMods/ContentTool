using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class RootNode : Node
{
	// -----------------------------------------------------------------------------------
	#region Textures and Icons
	// -----------------------------------------------------------------------------------
	public TextureList Textures = new TextureList();
	public TextureList Icons = new TextureList();

	public Texture2D GetTextureOrDefault(string key)
	{
		foreach (NamedTexture tex in Textures)
			if (tex.Key == key)
				return tex.Texture;
		return Texture2D.whiteTexture;
	}
	public void SwapTextureKeys(string keyA, string keyB)
	{
		for (int i = 0; i < Textures.Count; i++)
		{
			if (Textures[i].Key == keyA)
				Textures[i].Key = keyB;
			if (Textures[i].Key == keyB)
				Textures[i].Key = keyA;
		}
	}
	#endregion
	// -----------------------------------------------------------------------------------

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
	#region Material caching
	// Not to be exported to Minecraft, but useful to have inside the prefab in Unity
	// -----------------------------------------------------------------------------------
	public List<Material> UnityMaterialCache = new List<Material>();
	public Material GetMaterial(string key, ETurboRenderMaterial renderType)
	{
		string fullKey = $"{key}_{renderType}";
		foreach (Material mat in UnityMaterialCache)
			if (mat != null)
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
}
