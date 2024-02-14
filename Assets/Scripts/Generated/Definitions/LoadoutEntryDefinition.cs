using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class LoadoutEntryDefinition : Element
{
	[JsonField]
	public string paintjobName = "";
	[JsonField]
	public string[] itemUnlocks = new string[0];
}
