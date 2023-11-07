using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

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
	}

	// -------------------------------------------------------------------------------
	#region Unity Transform
	// -------------------------------------------------------------------------------
	protected void Update()
	{
		if (HasUnityTransformBeenChanged())
		{
			ModelEditingSystem.ApplyOperation(
				new TurboAttachPointMoveOperation(
					Parent.GetModel(),
					PartName,
					transform.localPosition,
					LockPartPositions,
					LockAttachPoints));
		}
		if (!ModelEditingSystem.ShouldSkipRefresh(Parent.GetModel(), PartName, 0))
			CopyToUnityTransform();
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
			Gizmos.DrawLine(ap.transform.position, transform.position);
		}
	}
}
