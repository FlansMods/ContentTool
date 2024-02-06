using UnityEngine;
using UnityEngine.Serialization;
using static ResourceLocation;

[System.Serializable]
public class ItemDefinition
{
/* TEMP: To update old versions add " : ISerializationCallbackReceiver" and uncomment this
	
	[FormerlySerializedAs("tags")]
	public string[] _tempArray;
	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		tags = new ResourceLocation[_tempArray.Length];
		for (int i = 0; i < _tempArray.Length; i++)
		{
			tags[i] = new ResourceLocation(_tempArray[i]);
		}
	}
	*/

	[JsonField]
	public int maxStackSize = 64;
	[JsonField]
	public ResourceLocation[] tags = new ResourceLocation[0];
}
