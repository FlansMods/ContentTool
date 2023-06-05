using UnityEngine;

[System.Serializable]
public class InputDefinition
{
	[JsonField]
	public EPlayerInput key = EPlayerInput.Jump;
	[JsonField]
	public ArticulationInputDefinition[] articulations = new ArticulationInputDefinition[0];
	[JsonField]
	public MountedGunInputDefinition[] guns = new MountedGunInputDefinition[0];
	[JsonField]
	public DrivingInputDefinition[] driving = new DrivingInputDefinition[0];
	[JsonField]
	public ArmInputDefinition[] arms = new ArmInputDefinition[0];
	[JsonField]
	public bool alternateInputs = false;
	[JsonField]
	public string switchVehicleMode = "";
}
