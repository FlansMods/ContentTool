using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class PaintjobUnlockDefinition : Element
{
	[JsonField]
	public string forItem = "";
	[JsonField]
	public string name = "";
}
