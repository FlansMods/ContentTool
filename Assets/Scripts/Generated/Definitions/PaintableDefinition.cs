using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class PaintableDefinition : Element
{
	[JsonField]
	public PaintjobDefinition[] paintjobs = new PaintjobDefinition[0];
}
