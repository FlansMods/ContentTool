using UnityEngine;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/WorkbenchDefinition")]
public class WorkbenchDefinition : Definition
{
	[JsonField]
	public string titleString = "workbench";
	[JsonField(AssetPathHint = "textures/gui/")]
	public ResourceLocation bannerTextureLocation = InvalidLocation;
	[JsonField]
	public GunCraftingDefinition gunCrafting = new GunCraftingDefinition();
	[JsonField]
	public PartCraftingDefinition partCrafting = new PartCraftingDefinition();
	[JsonField]
	public GunModifyingDefinition gunModifying = new GunModifyingDefinition();
	[JsonField]
	public EnergyBlockDefinition energy = new EnergyBlockDefinition();
	[JsonField]
	public ArmourCraftingDefinition armourCrafting = new ArmourCraftingDefinition();
	[JsonField]
	public ItemHoldingDefinition itemHolding = new ItemHoldingDefinition();
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public WorkbenchSideDefinition[] sides = new WorkbenchSideDefinition[0];
}
