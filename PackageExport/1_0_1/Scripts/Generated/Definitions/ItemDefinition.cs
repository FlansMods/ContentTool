using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemDefinition : Element, ISerializationCallbackReceiver
{
	[JsonField]
	public int maxStackSize = 64;
	[JsonField(AssetPathHint = "tags/items/")]
	public ResourceLocation[] tags = new ResourceLocation[0];
	[FormerlySerializedAs("tags")]
	[HideInInspector]
	public string[] _tags;
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize() {
		if(tags.Length == 0) {
		tags = new ResourceLocation[_tags.Length];
		for(int i = 0; i < _tags.Length; i++)
			tags[i] = new ResourceLocation(_tags[i]);
		}
	}
}
