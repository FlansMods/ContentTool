using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Effectively a passthrough node. We need to display the animation in-Editor, but do not want it to alter the actual exported heirarchy
public class PreviewAnimNode : Node
{
	public override string GetFixedPrefix() { return "anim_"; }
	public string APName { get { return name.Substring("anim_".Length); } }

#if UNITY_EDITOR
	public override bool HideInHeirarchy() { return true; }
#endif
}
