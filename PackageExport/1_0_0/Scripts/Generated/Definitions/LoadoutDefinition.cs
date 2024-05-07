using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LoadoutDefinition : Element
{
	[JsonField]
	public string[] slots = new string[0];
}
