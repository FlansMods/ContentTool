using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class MaterialFilterDefinition
{
	[JsonField]
	public EFilterType filterType = EFilterType.Allow;
	[JsonField]
	public EMaterialType[] materialTypes = new EMaterialType[0];
	[JsonField]
	public int minTier = 1;
	[JsonField]
	public int maxTier = 5;
}
