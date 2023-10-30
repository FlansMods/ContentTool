using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurboAttachPointPreview : MonoBehaviour
{
    public TurboRigPreview Parent { get { return GetComponentInParent<TurboRigPreview>(); } }
    public string PartName;
	public bool LockPartPositions = false;
	public bool LockAttachPoints = false;

	public void OnDrawGizmosSelected()
	{
		if (Parent == null)
			return;

		string attachedTo = Parent.Rig.GetAttachedTo(PartName);
		Vector3 offset = Parent.Rig.GetAttachmentOffset(PartName);
		Transform apTransform = Parent.GetOrCreateAPTransform(attachedTo);
		if(apTransform != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(apTransform.position, transform.position);
		}
	}
}
