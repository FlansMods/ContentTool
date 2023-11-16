using UnityEngine;

[System.Serializable]
public class HandlerNodeDefinition
{
	[JsonField]
	public string actionGroupToTrigger = "";
	[JsonField]
[Tooltip("[TODO] If non-empty, this will check to see if the gun is in the specified mode. If you start with '!', it will check for not being in that mode")]
	public string modalCheck = "";
	[JsonField]
	public bool deferToAttachment = false;
	[JsonField]
	public EAttachmentType attachmentType = EAttachmentType.Generic;
	[JsonField]
	public int attachmentIndex = 0;
	[JsonField]
	public bool andContinueEvaluating = false;
}
