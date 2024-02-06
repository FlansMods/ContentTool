using UnityEngine;
using UnityEngine.Serialization;
using static ResourceLocation;

[System.Serializable]
public class GunModifyingDefinition
{
/* TEMP: To update old versions add " : ISerializationCallbackReceiver" and uncomment this
	
	[FormerlySerializedAs("disallowedMods")]
	public string[] _tempArray;
	[FormerlySerializedAs("allowedMods")]
	public string[] _tempArray2;
	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		disallowedMods = new ResourceLocation[_tempArray.Length];
		for(int i = 0; i < _tempArray.Length; i++)
		{
			disallowedMods[i] = new ResourceLocation(_tempArray[i]);
		}
		allowedMods = new ResourceLocation[_tempArray2.Length];
		for (int i = 0; i < _tempArray2.Length; i++)
		{
			allowedMods[i] = new ResourceLocation(_tempArray2[i]);
		}
	}
	*/

	[JsonField]
	public bool isActive = false;
	[JsonField]
[Tooltip("Disallows certain mods, but only if size > 0")]
	public ResourceLocation[] disallowedMods = new ResourceLocation[0];
	[JsonField]
[Tooltip("Allows only certain mods if set. If size == 0, nothing will be applied")]
	public ResourceLocation[] allowedMods = new ResourceLocation[0];
	[JsonField]
	public int FECostPerModify = 0;
}
