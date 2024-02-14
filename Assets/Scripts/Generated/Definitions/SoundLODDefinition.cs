using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class SoundLODDefinition : Element
{
	[JsonField(AssetPathHint = "sounds/")]
	public ResourceLocation sound = InvalidLocation;
	[JsonField]
	public float minDistance = 100f;
}
