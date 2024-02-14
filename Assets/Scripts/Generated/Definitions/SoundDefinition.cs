using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class SoundDefinition : Element
{
	[JsonField(AssetPathHint = "sounds/")]
	public ResourceLocation sound = InvalidLocation;
	[JsonField]
	[Tooltip("In seconds")]
	public float length = 1f;
	[JsonField]
	public float minPitchMultiplier = 1f;
	[JsonField]
	public float maxPitchMultiplier = 1f;
	[JsonField]
	public float minVolume = 1f;
	[JsonField]
	public float maxVolume = 1f;
	[JsonField]
	public float maxRange = 100f;
	[JsonField]
	public SoundLODDefinition[] LODs = new SoundLODDefinition[0];
}
