using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class SequenceDefinition : Element
{
	[JsonField]
	public string name = "";
	[JsonField]
	public int ticks = 20;
	[JsonField]
	public SequenceEntryDefinition[] frames = new SequenceEntryDefinition[0];
}
