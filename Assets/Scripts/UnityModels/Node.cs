using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public interface IModifyable
{
	string GetName();
	bool SupportsRename();
	void Rename(Node parentNode, string newName) { }

	bool SupportsPreview();
	void Preview(Node parentNode) { }

	bool SupportsDelete();
	void PreDelete(Node parentNode) { }

	bool SupportsDuplicate();
	void PreDuplicate(Node parentNode) { }
	void PostDuplicate(Node parentNode) { }
}
public interface ICloneable<T>
{
	T Clone();
}

[Serializable]
public class TextureList : ModifiableList<NamedTexture>
{
	private static Vector2 TexturePreviewScroller = Vector2.zero;
	public bool TextureListField(string label, Node parentNode, CreateFunc createFunc, string folderHint = "")
	{
		return ListField(label, parentNode, (entry) =>
		{
			bool anyChange = false;

			// Add a texture field
			ResourceLocation changedTextureLocation = ResourceLocation.EditorObjectField(entry.Location, entry.Texture, folderHint);
			if (changedTextureLocation != entry.Location)
			{
				entry.Location = changedTextureLocation;
				entry.Texture = changedTextureLocation.Load<Texture2D>();
				anyChange = true;
			}
			GUILayout.BeginHorizontal();
			GUILayout.Label("Key: ");
			string changedEntry = EditorGUILayout.DelayedTextField(entry.Key);
			if(changedEntry != entry.Key)
			{
				entry.Key = changedEntry;
				anyChange = true;
			}
			GUILayout.EndHorizontal();

			if (entry.Texture != null)
			{
				TexturePreviewScroller = GUILayout.BeginScrollView(TexturePreviewScroller, GUILayout.ExpandHeight(false));
				FlanStyles.RenderTextureAutoWidth(entry.Texture);
				GUILayout.EndScrollView();
			}

			return anyChange;
		},
		createFunc);
	}
}

[Serializable]
public class ModifiableList<T> where T : IModifyable, ICloneable<T>, new()
{
	private static FlanStyles.FoldoutTree Tree = new FlanStyles.FoldoutTree();

	// -------------------------------------------------------------------------
	// Unity serialization doesn't like extending the List<T>, so we wrap it >.>
	public List<T> List = new List<T>();
	public int Count { get { return List.Count; } }
	public void Insert(int index, T t) { List.Insert(index, t); }
	public void RemoveAt(int index) { List.RemoveAt(index); }
	public IEnumerator<T> GetEnumerator() { return List.GetEnumerator(); }
	public void Add(T t) { List.Add(t); }
	public void AddRange(IEnumerable<T> range) { List.AddRange(range); }
	public T this[int i] {
		get { return List[i]; }
		set { List[i] = value; }
	}
	// -------------------------------------------------------------------------

	public delegate bool GUIFunc(T entry);
	public delegate T CreateFunc();

	#if UNITY_EDITOR
	public bool ListField(string label, Node parentNode, GUIFunc entryFunc, CreateFunc createFunc = null)
	{
		bool anyChange = false;

		// Header
		GUILayout.BeginHorizontal();
		bool listFoldout = Tree.Foldout(new GUIContent(label), label);
		GUILayout.FlexibleSpace();
		GUILayout.Label($"[{Count}]");
		if (createFunc != null)
			if (GUILayout.Button(FlanStyles.AddEntry))
			{
				Add(createFunc.Invoke());
				anyChange = true;
			}
		GUILayout.EndHorizontal();

		
		if (listFoldout)
		{
			// Entries
			int indexToDelete = -1, indexToDuplicate = -1;
			for (int i = 0; i < Count; i++)
			{
				T entry = this[i];

				// Header bar with quick buttons
				GUILayout.BeginHorizontal();
				// Foldout is the most important thing here
				bool foldout = Tree.Foldout(new GUIContent(entry.GetName()), $"{label}/{entry.GetName()}");
				// Name is either a text field or a label, depending on whether editable
				if (entry.SupportsRename())
				{
					string newName = EditorGUILayout.DelayedTextField(entry.GetName());
					if (newName != entry.GetName())
					{
						Tree.ForceExpand($"{label}/{newName}");
						entry.Rename(parentNode, newName);
						anyChange = true;
					}
				}
				else GUILayout.Label(entry.GetName());
				// Space so the buttons are right-aligned
				GUILayout.FlexibleSpace();
				// And a verifications summary box
				if (entry is IVerifiableAsset verifiable)
				{
					GUIVerify.CachedVerificationsIcon(verifiable);
				}
				// Preview button
				if (entry.SupportsPreview())
					if (GUILayout.Button(FlanStyles.ApplyPose))
						entry.Preview(parentNode);
				// Duplicate button
				if (entry.SupportsDuplicate())
					if (GUILayout.Button(FlanStyles.DuplicateEntry))
						indexToDuplicate = i;
				// Delete button
				if (entry.SupportsDelete())
					if (GUILayout.Button(FlanStyles.DeleteEntry))
						indexToDelete = i;
				
				GUILayout.EndHorizontal();

				if (foldout)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 16), GUILayout.ExpandHeight(true));

					GUILayout.BeginVertical();
					// -------------------
					FlanStyles.ThinLine();
					if(entryFunc.Invoke(entry))
						anyChange = true;
					FlanStyles.ThinLine();
					// -------------------
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
			}

			if (indexToDuplicate != -1)
			{
				this[indexToDuplicate].PreDuplicate(parentNode);
				T newEntry = this[indexToDuplicate].Clone();
				Insert(indexToDuplicate + 1, newEntry);
				newEntry.PostDuplicate(parentNode);
				anyChange = true;
			}
			if (indexToDelete != -1)
			{
				this[indexToDelete].PreDelete(parentNode);
				RemoveAt(indexToDelete);
				anyChange = true;
			}
		}

		return anyChange;
	}
	#endif
}

[ExecuteInEditMode]
public abstract class Node : MonoBehaviour, IVerifiableAsset, IVerifiableContainer
{
	public static PositionSnapSetting PosSnap = PositionSnapSetting.One;
	public static RotationSnapSetting RotSnap = RotationSnapSetting.Deg22_5;

	// TODO: If you MUST, change these. But better to fix the root cause
	public Vector3 ExportOrigin { get { return LocalOrigin; } }
	// TODO: Re-apply Proto's fix
	// float xRot = piece.Euler.x;
	// if (xRot< 180) xRot *= -1;
	public Vector3 ExportEuler { get { return LocalEuler; } }
	public Vector3 ExportScale { get { return LocalScale; } }

	public bool IsIdentity { get {
			return transform.localPosition.Approximately(Vector3.zero)
			&& transform.localEulerAngles.Approximately(Vector3.zero)
			&& transform.localScale.Approximately(Vector3.one);
		}
	}



	public Vector3 LocalOrigin { get { return transform.localPosition; } set { transform.localPosition = value; } }
	public Vector3 LocalEuler { get { return transform.localEulerAngles; } set { transform.localEulerAngles = value; } }
	public Vector3 LocalScale { get { return transform.localScale; } set { transform.localScale = value; } }
	// Minecraft Forward is -z
	public Vector3 Forward { get { return -transform.forward; } }
	public Vector3 Right { get { return transform.right; } }
	public Vector3 Up { get { return transform.up; } }

#if UNITY_EDITOR
	protected void OnEnable()
	{
		EditorApplication.update += EditorUpdate;
	}
	protected void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
	}
	protected virtual void EditorUpdate()
	{

	}
	public virtual bool HasCompactEditorGUI() { return false; }
	public virtual void CompactEditorGUI()
	{

	}
	public virtual bool HideInHeirarchy() { return false; }
#endif

	public RootNode Root { get { return GetParentOfType<RootNode>(); } }
	public TNodeType GetParentOfType<TNodeType>() where TNodeType : Node
	{
		if (this is TNodeType tNode)
			return tNode;
		if (ParentNode != null)
			return ParentNode.GetParentOfType<TNodeType>();
		return null;
	}
	public Node ParentNode
	{
		get
		{
			if (transform.parent != null && transform.parent.TryGetComponent(out Node parentNode))
			{
				while (parentNode != null && parentNode.HideInHeirarchy())
				{
					parentNode = parentNode.ParentNode;
				}
				return parentNode;
			}
			return null;
		}
	}
	public IEnumerable<Node> ChildNodes
	{
		get
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				if (transform.GetChild(i).TryGetComponent(out Node childNode))
					yield return childNode;
			}
		}
	}
	public IEnumerable<TNodeType> GetChildNodes<TNodeType>() where TNodeType : Node
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			if (transform.GetChild(i).TryGetComponent(out TNodeType childNode))
				yield return childNode;
		}
	}
	public IEnumerable<Node> AllDescendantNodes
	{
		get
		{
			return GetComponentsInChildren<Node>();
		}
	}
	public IEnumerable<TNodeType> GetAllDescendantNodes<TNodeType>() where TNodeType : Node
	{
		return GetComponentsInChildren<TNodeType>();
	}
	public bool TryFindChild<TNodeType>(string nodeName, out TNodeType resultNode) where TNodeType : Node
	{
		foreach (TNodeType node in GetChildNodes<TNodeType>())
			if (node.name == nodeName)
			{
				resultNode = node;
				return true;
			}
		resultNode = null;
		return false;
	}
	public TNodeType FindChild<TNodeType>(string nodeName) where TNodeType : Node
	{
		foreach (TNodeType node in GetChildNodes<TNodeType>())
			if (node.name == nodeName)
				return node;
		return null;
	}
	public bool TryFindDescendant<TNodeType>(string nodeName, out TNodeType resultNode) where TNodeType : Node
	{
		foreach (TNodeType node in GetAllDescendantNodes<TNodeType>())
			if (node.name == nodeName)
			{
				resultNode = node;
				return true;
			}
		resultNode = null;
		return false;
	}
	public TNodeType FindDescendant<TNodeType>(string nodeName) where TNodeType : Node
	{
		foreach (TNodeType node in GetAllDescendantNodes<TNodeType>())
			if (node.name == nodeName)
				return node;
		return null;
	}

	// ---------------------------------------------------------------------------
	#region Operations
	// ---------------------------------------------------------------------------
	// Default available
	public virtual bool SupportsDuplicate() { return ParentNode != null; }
	public virtual void Duplicate()
	{
		Transform copy = Instantiate(transform, transform.parent);
		Undo.RegisterCreatedObjectUndo(copy.gameObject, $"Duplicated {name} and created {copy.name}");
		EditorUtility.SetDirty(copy.gameObject);
	}
	public virtual bool SupportsDelete() { return ParentNode != null; }
	public virtual void Delete()
	{
		Undo.DestroyObjectImmediate(gameObject);
	}
	public virtual bool SupportsTranslate() { return true; }
	public virtual void Translate(Vector3 deltaPos)
	{
		deltaPos = PosSnap.Snap(deltaPos);
		if (!deltaPos.Approximately(Vector3.zero))
		{
			Undo.RegisterCompleteObjectUndo(gameObject, $"Offset {name} by {deltaPos}");
			transform.localPosition += deltaPos;
			EditorUtility.SetDirty(gameObject);
		}
	}
	public virtual bool SupportsRotate() { return true; }
	public virtual void Rotate(Vector3 deltaEuler)
	{
		deltaEuler = RotSnap.SnapEulers(deltaEuler);
		if (!deltaEuler.Approximately(Vector3.zero))
		{
			Undo.RegisterCompleteObjectUndo(gameObject, $"Added euler[{deltaEuler}] to {name}");
			transform.localEulerAngles += deltaEuler;
			EditorUtility.SetDirty(gameObject);
		}
	}
	public virtual string GetFixedPrefix() { return ""; }
	public virtual bool SupportsRename() { return true; }
	public virtual void Rename(string newName)
	{
		Undo.RegisterCompleteObjectUndo(gameObject, $"Renamed {name} to {newName}");
		name = newName;
		EditorUtility.SetDirty(gameObject);
	}

	// Default unavailable
	public virtual bool SupportsMirror() { return false; }
	public virtual void Mirror(bool mirrorX, bool mirrorY, bool mirrorZ) { }
	public virtual bool SupportsPreview() { return false; }
	public virtual void Preview() { }

	#endregion
	// ---------------------------------------------------------------------------



	// ---------------------------------------------------------------------------
	#region Texturing
	// ---------------------------------------------------------------------------
	public virtual bool NeedsMaterialType(ETurboRenderMaterial renderType) { return false; }
	public virtual void ApplyMaterial(ETurboRenderMaterial renderType, Material mat) { }
	#endregion
	// ---------------------------------------------------------------------------




	// ---------------------------------------------------------------------------
	#region Verifications
	// ---------------------------------------------------------------------------
	public UnityEngine.Object GetUnityObject() { return this; }
	public virtual void GetVerifications(List<Verification> verifications)
	{
		if (Root == null)
			verifications.Add(Verification.Failure("Node without a RootNode"));
	}
	public virtual IEnumerable<IVerifiableAsset> GetAssets()
	{
		foreach (Node child in ChildNodes)
			yield return child;
	}
	#endregion
	// ---------------------------------------------------------------------------

}
