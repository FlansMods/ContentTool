using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class MaterialSourceDefinition : Element
{
	[JsonField]
	public string[] matchItems = new string[0];
	[JsonField]
	public string[] matchTags = new string[0];
	[JsonField]
	[Tooltip("For ref, nugget = 1, ingot = 9, block = 81")]
	public int count = 1;
	[JsonField]
	public EMaterialIconType icon = EMaterialIconType.ingot;
}
