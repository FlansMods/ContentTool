using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class PaintableDefinition : Element
{
	[JsonField]
	public PaintjobDefinition[] paintjobs = new PaintjobDefinition[0];
}
