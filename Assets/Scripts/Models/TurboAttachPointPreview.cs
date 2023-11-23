using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

[ExecuteInEditMode]
public class TurboAttachPointPreview : MonoBehaviour
{
    public TurboRigPreview Parent { get { return GetComponentInParent<TurboRigPreview>(); } }
    public string PartName;
	public bool LockPartPositions = false;
	public bool LockAttachPoints = false;

	public void Compact_Editor_GUI()
	{
		// Attach Point setting
		string ap = Parent.Rig.GetAttachedTo(PartName);
		List<string> apNames = new List<string>(new string[] { "none" });
		Parent.Rig.GetSectionNames(apNames);
		int index = apNames.IndexOf(ap);
		int changedIndex = EditorGUILayout.Popup("Attached to:", index, apNames.ToArray());
		if (changedIndex != index)
		{
			ModelEditingSystem.ApplyOperation(
				new TurboAttachPointReparentOperation(
					Parent.GetModel(),
					PartName,
					apNames[changedIndex]));
		}

		// Attachment offset
		Vector3 offset = Parent.Rig.GetAttachmentOffset(PartName);
		Vector3 changedOffset = EditorGUILayout.Vector3Field("Offset:", offset);
		if (!Mathf.Approximately((offset - changedOffset).sqrMagnitude, 0f))
		{
			ModelEditingSystem.ApplyOperation(
					new TurboAttachPointMoveOperation(
						Parent.GetModel(),
						PartName,
						changedOffset,
						LockPartPositions,
						LockAttachPoints));
		}
		Vector3 euler = Parent.Rig.GetAttachmentEuler(PartName);
		Vector3 changedEuler = EditorGUILayout.Vector3Field("Euler:", euler);
		if (!Mathf.Approximately((euler - changedEuler).sqrMagnitude, 0f))
		{
			ModelEditingSystem.ApplyOperation(
					new TurboAttachPointRotateOperation(
						Parent.GetModel(),
						PartName,
						changedEuler,
						LockPartPositions,
						LockAttachPoints));
		}

	}

	// -------------------------------------------------------------------------------
	#region Unity Transform
	// -------------------------------------------------------------------------------

	protected virtual void OnEnable()
	{
		EditorApplication.update += EditorUpdate;
	}

	protected virtual void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
	}

	protected virtual void EditorUpdate()
	{
		if (Parent.Rig == null)
			return;

		//if (HasUnityTransformBeenChanged())
		//{
		//	ModelEditingSystem.ApplyOperation(
		//		new TurboAttachPointMoveOperation(
		//			Parent.GetModel(),
		//			PartName,
		//			transform.localPosition,
		//			LockPartPositions,
		//			LockAttachPoints));
		//}
		//if (!ModelEditingSystem.ShouldSkipRefresh(Parent.GetModel(), PartName, 0))
		//	CopyToUnityTransform();
	}
	private bool HasUnityTransformBeenChanged()
	{
		return !transform.localPosition.Approximately(Parent.Rig.GetAttachmentOffset(PartName));
	}
	public void CopyToUnityTransform()
	{
		transform.localPosition = Parent.Rig.GetAttachmentOffset(PartName);
	}
	public void UpdatePreviewFromModel()
	{
		Vector3 localPos = Parent.Rig.GetAttachmentOffset(PartName);
		transform.localPosition = localPos;

		// TODO: Reparent
	}
	#endregion
	// -------------------------------------------------------------------------------

	public void OnDrawGizmosSelected()
	{
		if (Parent == null || Parent.Rig == null)
			return;

		string attachedTo = Parent.Rig.GetAttachedTo(PartName);
		Vector3 offset = Parent.Rig.GetAttachmentOffset(PartName);
		TurboAttachPointPreview ap = Parent.GetAPPreview(attachedTo);
		if(ap != null)
		{
			Gizmos.color = Color.magenta;
			//Gizmos.DrawLine(ap.transform.position, transform.position);

			Gizmos.matrix = transform.localToWorldMatrix;

			switch(PartName)
			{
				case "eye_line":
					Gizmos.color = Color.cyan;
					Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
					Gizmos.DrawLine(-Vector3.right * 10f, Vector3.right * 100f);
					Gizmos.DrawSphere(-Vector3.right * 10f, 0.5f);
					break;
				case "shoot_origin":
					Gizmos.color = Color.red;
					Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
					Gizmos.DrawLine(Vector3.zero, Vector3.right * 10f);
					Gizmos.DrawCube(Vector3.zero + Vector3.right * 5f, Vector3.one * 0.7f);
					Gizmos.DrawCube(Vector3.zero + Vector3.right * 10f, Vector3.one * 1.2f);
					break;
				case "sights":
					Gizmos.color = Color.blue;
					Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
					Gizmos.DrawLine(Vector3.zero, Vector3.up * 5.0f);
					Gizmos.DrawWireCube(Vector3.up * 1.5f, new Vector3(6f, 2f, 2f));
					break;
				case "grip":
					Gizmos.color = Color.yellow;
					Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
					Gizmos.DrawLine(Vector3.zero, Vector3.down * 5.0f);
					Gizmos.DrawWireCube(Vector3.down * 0.5f, new Vector3(4f, 1f, 2f));
					Gizmos.DrawWireCube(Vector3.down * 3f, new Vector3(1.5f, 4f, 1.5f));
					break;
				case "barrel":
					Gizmos.color = Color.white;
					Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
					Gizmos.DrawLine(Vector3.zero, Vector3.right * 5.0f);
					Gizmos.DrawWireCube(Vector3.right * 2f, new Vector3(4f, 1f, 1f));
					break;
				default:
					Gizmos.color = Color.green;
					Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
					break;
			}
		}
	}
}
