using UnityEngine;
using UnityEngine.Serialization;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/MagazineDefinition")]
public class MagazineDefinition : Definition
{
/* TEMP: To update old versions add " : ISerializationCallbackReceiver" and uncomment this
	
	[FormerlySerializedAs("matchBulletNames")]
	public string[] _idArray;
	[FormerlySerializedAs("requiredBulletTags")]
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

			matchingBullets = new ItemCollectionDefinition()
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
	public string[] tags = new string[0];
	[JsonField]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public EAmmoLoadMode ammoLoadMode = EAmmoLoadMode.FullMag;
	[JsonField]
	public EAmmoConsumeMode ammoConsumeMode = EAmmoConsumeMode.RoundRobin;
	[JsonField]
[Tooltip("The number of Magazine Upgrade items needed to swap to this mag")]
	public int upgradeCost = 0;
	[JsonField]
	public int numRounds = 0;
	[JsonField]
[Tooltip("A performance optimisation, recommended if the mag size is 100 or more")]
	public bool allRoundsMustBeIdentical = true;
	[JsonField]
	public ItemCollectionDefinition matchingBullets = new ItemCollectionDefinition();
}
