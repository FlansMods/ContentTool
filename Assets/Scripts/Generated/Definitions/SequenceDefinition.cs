using UnityEngine;

[System.Serializable]
public class SequenceDefinition
{
	[JsonField]
	public string name = "";
	[JsonField]
	public int ticks = 20;
	[JsonField]
	public SequenceEntryDefinition[] frames = new SequenceEntryDefinition[0];
}
