using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class MaterialFilterDefinition : Element
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
