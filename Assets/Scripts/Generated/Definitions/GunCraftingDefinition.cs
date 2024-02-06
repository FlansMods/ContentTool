using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static ResourceLocation;

[System.Serializable]
public class GunCraftingDefinition
{
	/* TEMP: To update old versions add " : ISerializationCallbackReceiver" and uncomment this
	
	[FormerlySerializedAs("craftsByName")]
	public string[] _idArray;
	[FormerlySerializedAs("craftsByTag")]
	public string[] _tagArray;
	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		if (_idArray != null && _tagArray != null)
		{
			ResourceLocation[] itemIDs = new ResourceLocation[_idArray.Length];
			ResourceLocation[] tagIDs = new ResourceLocation[_tagArray.Length];

			for (int i = 0; i < _idArray.Length; i++)
				itemIDs[i] = new ResourceLocation(_idArray[i]);
			for (int i = 0; i < _tagArray.Length; i++)
				tagIDs[i] = new ResourceLocation(_tagArray[i]);

			craftableGuns = new ItemCollectionDefinition()
			{
				itemIDFilters = itemIDs.Length > 0
					? new LocationFilterDefinition[1] {
					new LocationFilterDefinition() {
						filterType = EFilterType.Allow,
						matchResourceLocations = itemIDs,
					}
					}
					: new LocationFilterDefinition[0],
				itemTagFilters = tagIDs.Length > 0
					? new LocationFilterDefinition[1] {
					new LocationFilterDefinition() {
						filterType = EFilterType.Allow,
						matchResourceLocations = tagIDs,
					}
					}
					: new LocationFilterDefinition[0]
			};
		}
	}
	*/



	[JsonField]
	public bool isActive = false;
	[JsonField]
	public ItemCollectionDefinition craftableGuns = new ItemCollectionDefinition();
	[JsonField]
	public int maxSlots = 8;
	[JsonField]
	public int FECostPerCraft = 0;
}
