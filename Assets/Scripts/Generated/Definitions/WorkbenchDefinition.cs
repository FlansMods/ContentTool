using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/WorkbenchDefinition")]
public class WorkbenchDefinition : Definition
{
	[JsonField]
	public string titlestring = "workbench";
	[JsonField]
	public string bannerTextureLocation = "";
	[JsonField]
	public GunCraftingDefinition gunCrafting = new GunCraftingDefinition();
	[JsonField]
	public GunModifyingDefinition gunModifying = new GunModifyingDefinition();
	[JsonField]
	public EnergyBlockDefinition energy = new EnergyBlockDefinition();
	[JsonField]
	public ArmourCraftingDefinition armourCrafting = new ArmourCraftingDefinition();
	[JsonField]
	public ItemHoldingDefinition itemHolding = new ItemHoldingDefinition();
	[JsonField]
	public WorkbenchSideDefinition[] sides = new WorkbenchSideDefinition[0];
}
