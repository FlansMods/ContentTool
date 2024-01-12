using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionNode : Node
{
	public string PartName { get { return name; } }
	public ETurboRenderMaterial Material = ETurboRenderMaterial.Cutout;


	public override bool SupportsMirror() { return true; }
	public override void Mirror(bool mirrorX, bool mirrorY, bool mirrorZ) 
	{
		foreach (Node node in ChildNodes)
			if (node.SupportsMirror())
				node.Mirror(mirrorX, mirrorY, mirrorZ);
	}

	public override void GetVerifications(List<Verification> verifications)
	{
		base.GetVerifications(verifications);
		if(ParentNode is AttachPointNode apParent)
		{
			if (name != apParent.APName)
				verifications.Add(Verification.Failure($"Section {name} is attached to AP {apParent.name}, which does not match",
					() => name = apParent.APName));
		}
		else if(ParentNode is RootNode rootParent)
		{
			if (name != "body")
				verifications.Add(Verification.Neutral($"Section {name} is attached to the RootNode, but is not called 'body'"));
		}
		else
		{
			verifications.Add(Verification.Failure($"Section {name} is not attached to an AP or Root node"));
		}
	}
}
