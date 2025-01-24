using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/TeamDefinition")]
public class TeamDefinition : Definition, ISerializationCallbackReceiver
{
	[JsonField]
	public ResourceLocation[] loadouts = new ResourceLocation[0];
	[FormerlySerializedAs("loadouts")]
	[HideInInspector]
	public string[] _loadouts;
	[JsonField]
	public ColourDefinition flagColour = new ColourDefinition();
	[JsonField]
	public char textColour = 'f';
	[JsonField]
	public ItemStackDefinition hat = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition chest = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition legs = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition shoes = new ItemStackDefinition();
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize() {
		if(loadouts.Length == 0) {
		loadouts = new ResourceLocation[_loadouts.Length];
		for(int i = 0; i < _loadouts.Length; i++)
			loadouts[i] = new ResourceLocation(_loadouts[i]);
		}
	}
}
