using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class AbilityProviderDefinition
{
	[JsonField]
	public string ability = "";
	[JsonField]
	public int level = 1;
}
