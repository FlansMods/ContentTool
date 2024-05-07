using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/ControlSchemeDefinition")]
public class ControlSchemeDefinition : Definition
{
	[JsonField]
	public EControlLogicType logicType = EControlLogicType.Car;
	[JsonField]
	public ControlSchemeAxisDefinition[] axes = new ControlSchemeAxisDefinition[0];
}
