using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyNode : Node
{
	public override bool SupportsMirror() { return true; }
	public override void Mirror(bool mirrorX, bool mirrorY, bool mirrorZ)
	{
		transform.localPosition = new Vector3(
			transform.localPosition.x * (mirrorX ? -1f : 1f),
			transform.localPosition.y * (mirrorY ? -1f : 1f),
			transform.localPosition.z * (mirrorZ ? -1f : 1f));



		foreach (Node node in ChildNodes)
			if (node.SupportsMirror())
				node.Mirror(mirrorX, mirrorY, mirrorZ);
	}
}
