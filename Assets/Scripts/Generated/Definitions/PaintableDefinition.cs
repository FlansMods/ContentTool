using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class PaintableDefinition
{
	[JsonField]
	public PaintjobDefinition[] paintjobs = new PaintjobDefinition[0];
}
