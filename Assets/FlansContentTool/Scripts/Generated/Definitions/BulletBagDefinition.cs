using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/BulletBagDefinition")]
public class BulletBagDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public ItemCollectionDefinition bulletFilters = new ItemCollectionDefinition();
	[JsonField]
	public int slotCount = 1;
	[JsonField]
	public int maxStackSize = 64;
}
