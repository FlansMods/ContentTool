using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static ResourceLocation;

[System.Serializable]
public class AbilityProviderDefinition
{
/* TEMP: To update old versions add " : ISerializationCallbackReceiver" and uncomment this
	
	[FormerlySerializedAs("ability")]
	public string _ability;
	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		ability = new ResourceLocation(_ability);
	}
	*/

	[JsonField]
	public ResourceLocation ability = InvalidLocation;
	[JsonField]
	public int level = 1;
}
