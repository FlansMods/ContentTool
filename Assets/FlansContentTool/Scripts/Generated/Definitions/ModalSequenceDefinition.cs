using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ModalSequenceDefinition : Element
{
	[JsonField]
	public string modeName = "";
	[JsonField]
	public int ticksBetweenFrames = 20;
	[JsonField]
	public SequenceEntryDefinition[] frames = new SequenceEntryDefinition[0];
}
