using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Effectively a passthrough node. We need to display the animation in-Editor, but do not want it to alter the actual exported heirarchy
public class PreviewAnimNode : Node
{
	public AttachPointNode TargetAP = null;

	public override string GetFixedPrefix() { return "anim_"; }
	public string APName { get { return name.Substring("anim_".Length); } }

#if UNITY_EDITOR
	public override bool HideInHeirarchy() { return true; }
#endif



	public void SetPose(Vector3 pos, Vector3 euler, Vector3 scale)
	{
		SetPose(pos, Quaternion.Euler(euler), scale);
	}
	public void SetPose(Vector3 pos, Quaternion ori, Vector3 scale)
{
		if(TargetAP != null)
		{
			transform.localPosition = TargetAP.LocalOrigin + pos;
			transform.localRotation = Quaternion.Euler(TargetAP.LocalEuler) * ori;
			transform.localScale = new Vector3(
				TargetAP.LocalScale.x * scale.x,
				TargetAP.LocalScale.y * scale.y,
				TargetAP.LocalScale.z * scale.z);
		}
		else
		{
			transform.localPosition = pos;
			transform.localRotation = ori;
			transform.localScale = scale;
		}
	}
}
