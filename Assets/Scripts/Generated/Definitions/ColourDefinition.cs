using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ColourDefinition : Element
{
	[JsonField]
	public float alpha = 1.0f;
	[JsonField]
	public float red = 1.0f;
	[JsonField]
	public float green = 1.0f;
	[JsonField]
	public float blue = 1.0f;
}
