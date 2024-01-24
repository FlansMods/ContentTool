using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemDefinition
{
	[JsonField]
	public int maxStackSize = 64;
	[JsonField]
	public string[] tags = new string[0];
}
