using UnityEngine;
using UnityEngine.Serialization;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/PartDefinition")]
public class PartDefinition : Definition
{
	[JsonField]
	public bool canPlaceInMachiningTable = false;
	[JsonField]
	public bool canPlaceInModificationTable = false;
	[JsonField]
	public string[] compatiblityTags = new string[] { "mecha", "groundVehicle", "plane" };
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public AbilityProviderDefinition[] abilities = new AbilityProviderDefinition[0];
	[JsonField]
	public EngineDefinition engine = new EngineDefinition();

	/* TEMP: To update old versions add " : ISerializationCallbackReceiver" and uncomment this
	
	[FormerlySerializedAs("material")]
	public string _temp;
	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		material = new ResourceLocation(_temp);
	}
	*/

	[JsonField]
	public ResourceLocation material = InvalidLocation;
}
