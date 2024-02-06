using UnityEngine;
using UnityEngine.Serialization;
using static ResourceLocation;

[System.Serializable]
public class PartCraftingDefinition
{
	/*
	 * TEMP: To update old versions add " : ISerializationCallbackReceiver" and uncomment this
	[FormerlySerializedAs("partsByName")]
	public string[] _idArray;
	[FormerlySerializedAs("partsByTag")]
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

			craftableParts = new ItemCollectionDefinition()
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
	public int inputSlots = 8;
	[JsonField]
	public int outputSlots = 8;
	[JsonField]
[Tooltip("In seconds")]
	public float timePerCraft = 1.0f;
	[JsonField]
	public float FECostPerCraft = 0.0f;
	[JsonField]
	public ItemCollectionDefinition craftableParts = new ItemCollectionDefinition();
}
