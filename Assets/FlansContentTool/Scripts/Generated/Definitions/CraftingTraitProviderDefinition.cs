using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class CraftingTraitProviderDefinition : Element, ISerializationCallbackReceiver
{
	[JsonField(AssetPathHint = "traits/")]
	public ResourceLocation trait = InvalidLocation;
	[FormerlySerializedAs("trait")]
	[HideInInspector]
	public string _trait;
	[JsonField]
	public int level = 1;
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize() {
		if(trait == ResourceLocation.InvalidLocation)
			trait = new ResourceLocation(_trait);
	}
}
