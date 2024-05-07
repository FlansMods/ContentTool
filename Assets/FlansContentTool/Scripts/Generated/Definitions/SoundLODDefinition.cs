using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class SoundLODDefinition : Element, ISerializationCallbackReceiver
{
	[JsonField(AssetPathHint = "sounds/")]
	public ResourceLocation sound = InvalidLocation;
	[FormerlySerializedAs("sound")]
	[HideInInspector]
	public string _sound;
	[JsonField]
	public float minDistance = 100f;
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize() {
		if(sound == ResourceLocation.InvalidLocation)
			sound = new ResourceLocation(_sound);
	}
}
