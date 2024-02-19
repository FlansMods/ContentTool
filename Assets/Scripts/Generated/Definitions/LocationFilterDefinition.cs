using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LocationFilterDefinition : Element
{
	[JsonField]
	public EFilterType filterType = EFilterType.Allow;
	[JsonField]
	public ResourceLocation[] matchResourceLocations = new ResourceLocation[0];
}
