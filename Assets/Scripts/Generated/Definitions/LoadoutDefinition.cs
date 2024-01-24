using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class LoadoutDefinition
{
	[JsonField]
	public string[] slots = new string[0];
}
