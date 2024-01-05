using System;
using System.Collections;
using System.Collections.Generic;
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

[System.Serializable]
public class ModifiableList<T> : List<T> where T : IModifyable, ICloneable<T>, new()
{
	private static FlanStyles.FoldoutTree Tree = new FlanStyles.FoldoutTree();
	public delegate void GUIFunc(T entry);
	public delegate T CreateFunc();

	#if UNITY_EDITOR
	public void ListField(string label, Node parentNode, GUIFunc entryFunc, CreateFunc createFunc = null)
	{
		// Header
		GUILayout.BeginHorizontal();
		bool listFoldout = Tree.Foldout(new GUIContent(label), label);
		GUILayout.FlexibleSpace();
		GUILayout.Label($"[{Count}]");
		if (createFunc != null)
			if (GUILayout.Button(FlanStyles.AddEntry))
			{
				Add(createFunc.Invoke());
				EditorUtility.SetDirty(parentNode);
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
						entry.Rename(parentNode, newName);
				}
				else GUILayout.Label(entry.GetName());
				// Space so the buttons are right-aligned
				GUILayout.FlexibleSpace();
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
				// And a verifications summary box
				if (entry is IVerifiableAsset verifiable)
				{
					List<Verification> verifications = new List<Verification>();
					verifiable.GetVerifications(verifications);
					GUIVerify.VerificationIcon(verifications);
				}
				GUILayout.EndHorizontal();

				if (foldout)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 16), GUILayout.ExpandHeight(true));

					GUILayout.BeginVertical();
					// -------------------
					FlanStyles.ThinLine();
					entryFunc.Invoke(entry);
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
			}
			if (indexToDelete != -1)
			{
				this[indexToDelete].PreDelete(parentNode);
				RemoveAt(indexToDelete);
			}
		}
	}
	#endif
}

[ExecuteInEditMode]
public abstract class Node : MonoBehaviour
{
	public static PositionSnapSetting PosSnap = PositionSnapSetting.One;
	public static RotationSnapSetting RotSnap = RotationSnapSetting.Deg22_5;



	public Vector3 LocalOrigin { get { return transform.localPosition; } set { transform.localPosition = value; } }
    public Vector3 LocalEuler { get { return transform.localEulerAngles; } set { transform.localEulerAngles = value; } }
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
				return parentNode;
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

	#endregion
	// ---------------------------------------------------------------------------


	// ---------------------------------------------------------------------------
	#region Texturing
	// ---------------------------------------------------------------------------
	public virtual bool NeedsMaterialType(ETurboRenderMaterial renderType) { return false; }
	public virtual void ApplyMaterial(ETurboRenderMaterial renderType, Material mat) { }


	#endregion
	// ---------------------------------------------------------------------------

}
