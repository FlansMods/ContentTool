using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/PartDefinition")]
public class PartDefinition : Definition, ISerializationCallbackReceiver
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
	public CraftingTraitProviderDefinition[] traits = new CraftingTraitProviderDefinition[0];
	[JsonField]
	public EngineDefinition engine = new EngineDefinition();
	[JsonField(AssetPathHint = "materials/")]
	public ResourceLocation material = InvalidLocation;
	[FormerlySerializedAs("material")]
	[HideInInspector]
	public string _material;
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize() {
		if(material == ResourceLocation.InvalidLocation)
			material = new ResourceLocation(_material);
	}
}
