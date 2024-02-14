using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class PaintjobUnlockDefinition : Element
{
	[JsonField]
	public string forItem = "";
	[JsonField]
	public string name = "";
}
