using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class InputDefinition : Element
{
	[JsonField]
	public EPlayerInput key = EPlayerInput.Jump;
	[JsonField]
	public ArticulationInputDefinition[] articulations = new ArticulationInputDefinition[0];
	[JsonField]
	public MountedGunInputDefinition[] guns = new MountedGunInputDefinition[0];
	[JsonField]
	public ArmInputDefinition[] arms = new ArmInputDefinition[0];
	[JsonField]
	[Tooltip("If true, each articulation/gun/driving control will be triggered in order. If false, one press = activate all")]
	public bool alternateInputs = false;
	[JsonField]
	public string switchVehicleMode = "";
}
