using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class SoundDefinition : Element, ISerializationCallbackReceiver
{
	[JsonField(AssetPathHint = "sounds/")]
	public ResourceLocation sound = InvalidLocation;
	[FormerlySerializedAs("sound")]
	[HideInInspector]
	public string _sound;
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
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize() {
		if(sound == ResourceLocation.InvalidLocation)
			sound = new ResourceLocation(_sound);
	}
}
