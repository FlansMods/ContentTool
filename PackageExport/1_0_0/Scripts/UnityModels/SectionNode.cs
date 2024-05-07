using System.Collections.Generic;

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

	protected override void EditorUpdate()
	{
		base.EditorUpdate();
	}

	public override void GetVerifications(IVerificationLogger verifications)
	{
		base.GetVerifications(verifications);
		if(ParentNode is AttachPointNode apParent)
		{
			if (name != apParent.APName)
				verifications.Failure($"Section {name} is attached to AP {apParent.name}, which does not match",
					() =>
					{
						name = apParent.APName;
						return this;
					});
		}
		else if(ParentNode is TurboRootNode rootParent)
		{
			if (name != "body")
				verifications.Neutral($"Section {name} is attached to the RootNode, but is not called 'body'");
		}
		else
		{
			verifications.Failure($"Section {name} is not attached to an AP or Root node");
		}
	}
}
