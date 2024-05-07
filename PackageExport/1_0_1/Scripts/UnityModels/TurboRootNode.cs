using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TurboRootNode : SkinnableRootNode
{
	public List<AnimationParameter> AnimationParameters = new List<AnimationParameter>();
	public FlanimationDefinition AnimationSet = null;

	public override void AddDefaultTransforms()
	{
		GetOrCreateItemTransform(ItemDisplayContext.FIRST_PERSON_RIGHT_HAND, new Vector3(8f, -7f, -13f), new Vector3(0f, 0f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.FIRST_PERSON_LEFT_HAND, new Vector3(-8f, -7f, -13f), new Vector3(0f, 0f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.THIRD_PERSON_RIGHT_HAND, new Vector3(-1f, -2f, 0f), new Vector3(0f, -90f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.THIRD_PERSON_LEFT_HAND, new Vector3(-1f, -2f, 0f), new Vector3(0f, -90f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.HEAD, Vector3.zero, new Vector3(-90f, -90f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.GROUND, new Vector3(0f, 0.15f, 0f), new Vector3(0f, -90f, 0f), Vector3.one * 0.625f);
		GetOrCreateItemTransform(ItemDisplayContext.FIXED, new Vector3(6f, 6f, 6f), new Vector3(0f, -90f, 45f));
		GetOrCreateItemTransform(ItemDisplayContext.GUI, new Vector3(-0.001f, -0.02f, -0.001f), new Vector3(0f, 45f, 90f), Vector3.one * 1.185f);
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
				ApplyAutoUV(out UVMap discard);
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

	public void BakeOutSectionTransforms()
	{
		List<SectionNode> sectionsWithTransforms = new List<SectionNode>();
		foreach (SectionNode section in GetComponentsInChildren<SectionNode>())
		{
			if (!section.IsIdentity)
				sectionsWithTransforms.Add(section);
		}
		foreach (SectionNode section in sectionsWithTransforms)
		{
			section.transform.ZeroTransformButNotChildren();
		}
	}


	public override void GetVerifications(IVerificationLogger verifications)
	{
		base.GetVerifications(verifications);
		if (!HasUVMap())
		{
			NumUVsToRemap(out int numToRemap, out int totalNum);
			if (totalNum == 0)
			{
				verifications.Success("No UV map needed");
			}
			else if(numToRemap == totalNum)
			{
				verifications.Failure("UV map has not been calculated",
					() =>
					{
						ApplyAutoUV(out UVMap ignore);
						return this;
					});
			}
			else
			{
				verifications.Failure($"UV map has {numToRemap} missing pieces",
					() => { 
						ApplyAutoUV(out UVMap ignore);
						return this;
					});
			}
		}

		List<EmptyNode> emptyNodes = new List<EmptyNode>(GetAllDescendantNodes<EmptyNode>());
		if(emptyNodes.Count > 0)
		{
			verifications.Failure($"{emptyNodes.Count} EmptyNodes present in heirarchy", () => {

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
				return this;
			});
		}

		List<SectionNode> sectionsWithTransforms = new List<SectionNode>();
		foreach(SectionNode section in GetAllDescendantNodes<SectionNode>())
			if (!section.IsIdentity)
				sectionsWithTransforms.Add(section);

		if(sectionsWithTransforms.Count > 0)
		{
			verifications.Failure($"{sectionsWithTransforms.Count} SectionNodes with non-Identity transformations", () => {

				string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
				using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
				{
					GameObject root = editingScope.prefabContentsRoot;

					// We need to rebuild this list inside the prefab editing scope
					root.GetComponent<TurboRootNode>().BakeOutSectionTransforms();
				}
				return this;
			});
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
