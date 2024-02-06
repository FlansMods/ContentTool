using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class LocationFilterDefinition
{
	[JsonField]
	public EFilterType filterType = EFilterType.Allow;
	[JsonField]
	public ResourceLocation[] matchResourceLocations = new ResourceLocation[0];
}
