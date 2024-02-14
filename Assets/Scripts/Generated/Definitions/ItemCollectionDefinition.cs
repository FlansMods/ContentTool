using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ItemCollectionDefinition : Element
{
	[JsonField]
	[Tooltip("ItemID inclusions/exclusions - to ignore itemID, leave empty")]
	public LocationFilterDefinition[] itemIDFilters = new LocationFilterDefinition[0];
	[JsonField]
	[Tooltip("ItemTag inclusions/exclusions - to ignore tags, leave empty")]
	public LocationFilterDefinition[] itemTagFilters = new LocationFilterDefinition[0];
	[JsonField]
	[Tooltip("Material inclusions/exclusions - to ignore materials, leave empty")]
	public MaterialFilterDefinition[] materialFilters = new MaterialFilterDefinition[0];
}
