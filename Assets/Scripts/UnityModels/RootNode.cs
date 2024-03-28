using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class RootNode : Node
{
	// -----------------------------------------------------------------------------------
	#region Textures and Icons
	// -----------------------------------------------------------------------------------
	public TextureList Textures = new TextureList();
	public TextureList Icons = new TextureList();

	public bool NeedsSkin() { return this is SkinnableRootNode; }
	protected virtual bool NeedsIcon() { return true; }

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
	#region Animations
	// -----------------------------------------------------------------------------------
	public FlanimationDefinition FlanimationDef;
	public AnimatorController UnityAnimController;
	public Animator UnityAnimator;

	public void SetAnimation(FlanimationDefinition flanimation)
	{
		
	}
	public void SetAnimation(AnimatorController unityAnim)
	{
		
	}
	#endregion
	// -----------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------
	#region ItemTransforms
	// -----------------------------------------------------------------------------------
	public virtual void AddDefaultTransforms()
	{
		GetOrCreateItemTransform(ItemDisplayContext.FIRST_PERSON_RIGHT_HAND, new Vector3(8f, -7f, -13f), new Vector3(0f, 90f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.FIRST_PERSON_LEFT_HAND, new Vector3(-8f, -7f, -13f), new Vector3(0f, 90f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.THIRD_PERSON_RIGHT_HAND, new Vector3(8f, 8f, 6f), new Vector3(0f, 90f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.THIRD_PERSON_LEFT_HAND, new Vector3(8f, 8f, 6f), new Vector3(0f, 90f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.HEAD, Vector3.zero, new Vector3(-90f, 0f, 0f));
		GetOrCreateItemTransform(ItemDisplayContext.GROUND, new Vector3(0f, 0.15f, 0f), Vector3.zero, Vector3.one * 0.55f);
		GetOrCreateItemTransform(ItemDisplayContext.FIXED, new Vector3(6f, 6f, 6f), new Vector3(0f, 0f, 45f));
		GetOrCreateItemTransform(ItemDisplayContext.GUI, new Vector3(-0.001f, -0.02f, -0.001f), new Vector3(0f, 45f, 90f), Vector3.one * 1.185f);
	}
	public ItemPoseNode GetOrCreateItemTransform(ItemDisplayContext transformType, Vector3 atPos, Vector3 withEuler)
	{
		return GetOrCreateItemTransform(transformType, atPos, withEuler, Vector3.one);
	}
	public ItemPoseNode GetOrCreateItemTransform(ItemDisplayContext transformType,
												 Vector3 atPos,
												 Vector3 withEuler,
												 Vector3 withScale)
	{
		foreach (ItemPoseNode itemPoseNode in GetChildNodes<ItemPoseNode>())
			if (itemPoseNode.TransformType == transformType)
				return itemPoseNode;

		GameObject poseGO = new GameObject($"pose_{transformType}");
		poseGO.transform.SetParent(transform);
		poseGO.transform.localPosition = atPos;
		poseGO.transform.localEulerAngles = withEuler;
		poseGO.transform.localScale = withScale;
		ItemPoseNode poseNode = poseGO.AddComponent<ItemPoseNode>();
		return poseNode;
	}
	public ItemPoseNode GetOrCreateItemTransform(ItemDisplayContext transformType)
	{
		foreach (ItemPoseNode itemPoseNode in GetChildNodes<ItemPoseNode>())
			if (itemPoseNode.TransformType == transformType)
				return itemPoseNode;

		GameObject poseGO = new GameObject($"pose_{transformType}");
		poseGO.transform.SetParentZero(transform);
		ItemPoseNode poseNode = poseGO.AddComponent<ItemPoseNode>();
		return poseNode;
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

	public override void GetVerifications(List<Verification> verifications)
	{
		base.GetVerifications(verifications);

		// Icons
		if (NeedsIcon())
		{
			if (Icons == null || Icons.Count == 0)
				verifications.Add(Verification.Neutral($"No icons present"));
			else if (Icons[0].Key != "default")
				verifications.Add(Verification.Failure($"Default icon is named incorrectly as {Icons[0].Key}",
				() =>
				{
					ApplyQuickFix((TurboRootNode _this) =>
					{
						_this.Icons[0].Key = "default";
					});
					return this;
				}));
		}

	}

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
		}
	}

	protected delegate void QuickFixFunction<T>(T _this);
	protected void ApplyQuickFix<T>(QuickFixFunction<T> quickFix)
	{
		if (this is T t)
		{
			string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
			using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
			{
				GameObject root = editingScope.prefabContentsRoot;
				T thisInsidePrefab = root.GetComponent<T>();
				if(thisInsidePrefab != null)
				{
					quickFix(thisInsidePrefab);
				}
				else
				{
					Debug.LogError($"Failed to apply quick-fix inside prefab scope on {name}");
				}
			}
		}
		else
		{
			Debug.LogError($"Quick-fix function has incorrect template type in {typeof(T)}");
		}
	}
}
