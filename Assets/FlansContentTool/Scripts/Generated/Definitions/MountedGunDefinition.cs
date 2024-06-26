using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class MountedGunDefinition : Element, ISerializationCallbackReceiver
{
	[JsonField]
	public Vector3 shootPointOffset = Vector3.zero;
	[JsonField]
	public ResourceLocation gun = InvalidLocation;
	[FormerlySerializedAs("gun")]
	[HideInInspector]
	public string _gun;
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize() {
		if(gun == ResourceLocation.InvalidLocation)
			gun = new ResourceLocation(_gun);
	}
}
