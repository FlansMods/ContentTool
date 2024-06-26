using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/VehicleDefinition")]
public class VehicleDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public VehiclePartDefinition[] parts = new VehiclePartDefinition[0];
	[JsonField]
	public VehiclePhysicsDefinition physics = new VehiclePhysicsDefinition();
	[JsonField]
	public EngineDefinition defaultEngine = new EngineDefinition();
	[JsonField]
	public bool useAABBCollisionOnly = false;
	[JsonField]
	public Vector3 aabbSize = Vector3.zero;
	[JsonField]
	public int CargoSlots = 0;
	[JsonField]
	public bool CanAccessMenusWhileMoving = true;
}
