using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

public class TurboRootNode : RootNode
{
	public Vector2Int UVMapSize = Vector2Int.zero;
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
		//SkinGenerator.CreateDefaultTexture(bakedMap, newSkinTexture);
		File.WriteAllBytes(fullPath, newSkinTexture.EncodeToPNG());
		AssetDatabase.Refresh();
		newSkinTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
		return new NamedTexture(newSkinName, newSkinTexture);
	}
	public NamedTexture CreateNewDefaultIcon()
	{
		Texture2D newSkinTexture = new Texture2D(UVMapSize.x, UVMapSize.y);
		return new NamedTexture();
	}

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

		bool anyChange = false;

		if (Textures.TextureListField(
			"Skins",
			this,
			() => { return CreateNewDefaultSkin(); },
			"textures/skins"))
			anyChange = true;
		if (Icons.TextureListField(
			"Icons",
			this,
			() => { return CreateNewDefaultIcon(); },
			"textures/item"))
			anyChange = true;

		if(anyChange)
		{
			EditorUtility.SetDirty(this);
		}

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

		List<EmptyNode> emptyNodes = new List<EmptyNode>(GetAllDescendantNodes<EmptyNode>());
		if(emptyNodes.Count > 0)
		{
			verifications.Add(Verification.Failure($"{emptyNodes.Count} EmptyNodes present in heirarchy", () => {

				string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
				using(var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
				{
					GameObject root = editingScope.prefabContentsRoot;
					int iterationCount = 0;
					EmptyNode empty = root.GetComponentInChildren<EmptyNode>();
					while(empty != null && iterationCount < 1000)
					{
						iterationCount++;
						if (empty.transform.parent != null)
						{
							for (int j = empty.transform.childCount - 1; j >= 0; j--)
							{
								empty.transform.GetChild(j).SetParent(empty.transform.parent, true);
							}
						}
						DestroyImmediate(empty.gameObject);
						empty = root.GetComponentInChildren<EmptyNode>();
					}
				}
			}));
		}

		List<SectionNode> sectionsWithTransforms = new List<SectionNode>();
		foreach(SectionNode section in GetAllDescendantNodes<SectionNode>())
			if (!section.IsIdentity)
				sectionsWithTransforms.Add(section);

		if(sectionsWithTransforms.Count > 0)
		{
			verifications.Add(Verification.Failure($"{sectionsWithTransforms.Count} SectionNodes with non-Identity transformations", () => {

				string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
				using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
				{
					GameObject root = editingScope.prefabContentsRoot;

					// We need to rebuild this list inside the prefab editing scope
					sectionsWithTransforms.Clear();
					foreach (SectionNode section in root.GetComponentsInChildren<SectionNode>())
					{
						if (!section.IsIdentity)
							sectionsWithTransforms.Add(section);
					}
					foreach(SectionNode section in sectionsWithTransforms)
					{
						section.transform.ZeroTransformButNotChildren();
					}
				}
			}));
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
