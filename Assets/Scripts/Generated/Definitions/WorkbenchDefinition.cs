using UnityEngine;

[System.Serializable]
public class WorkbenchDefinition : Definition
{
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
