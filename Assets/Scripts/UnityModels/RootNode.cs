using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RootNode : Node
{
	public Vector2Int UVMapSize = Vector2Int.zero;
	public ModifiableList<NamedTexture> Textures = new ModifiableList<NamedTexture>();
	public ModifiableList<NamedTexture> Icons = new ModifiableList<NamedTexture>();
	public List<AnimationParameter> AnimationParameters = new List<AnimationParameter>();

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

	public Texture2D GetTextureOrDefault(string key)
	{
		foreach (NamedTexture tex in Textures)
			if (tex.Key == key)
				return tex.Texture;
		return Texture2D.whiteTexture;
	}

	// TODO: This could have some fancy code that detects if we are in a PrefabStage
	public override bool SupportsTranslate() { return false; }
	public override bool SupportsRotate() { return false; }
	public override bool SupportsRename() { return false; }
	public override bool SupportsDelete() { return false; }
	public override bool SupportsDuplicate() { return false; }

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

	private Dictionary<string, Material> Materials = new Dictionary<string, Material>();
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
				string matKey = $"{key}_{renderType}";
				if (!Materials.TryGetValue(matKey, out Material mat))
				{
					mat = new Material(renderType.GetShader());
					mat.name = matKey;
					mat.SetTexture("_MainTex", tex);
					mat.EnableKeyword("_NORMALMAP");
					mat.EnableKeyword("_DETAIL_MULX2");
					mat.SetOverrideTag("RenderType", "Cutout");
					Materials.Add(matKey, mat);
				}

				foreach (Node node in nodesForThisRenderType)
				{
					node.ApplyMaterial(renderType, mat);
				}
			}
		}		
	}

#if UNITY_EDITOR
	private static Vector2 TexturePreviewScroller = Vector2.zero;
	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		GUILayout.Label($"UV Size:[{UVMapSize.x},{UVMapSize.y}]");

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
				Texture2D newSkinTexture = new Texture2D(UVMapSize.x, UVMapSize.y);
				return new NamedTexture();
			});
	}
#endif
}
