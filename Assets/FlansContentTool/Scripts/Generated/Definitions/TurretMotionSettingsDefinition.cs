using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class TurretMotionSettingsDefinition : Element
{
	[JsonField]
    public ETurretMotionType motionType = ETurretMotionType.Static;
	[JsonField]
    public float minYaw = 0.0f;
	[JsonField]
    public float maxYaw = 0.0f;
	[JsonField]
    public float minPitch = 0.0f;
	[JsonField]
    public float maxPitch = 0.0f;
	[JsonField]
    public float minRoll = 0.0f;
	[JsonField]
    public float maxRoll = 0.0f;
	[JsonField]
	[Tooltip("For use with LookAtEntity or LookAtBlock")]
    public LocationFilterDefinition[] idFilters = new LocationFilterDefinition[0];
	[JsonField]
	[Tooltip("For use with CycleStates or CycleContinuous")]
    public TransformDefinition[] poses = new TransformDefinition[0];
	[JsonField]
	[Tooltip("How long to wait in each state in a Cycle")]
    public float cycleStatesWaitSeconds = 0.0f;
	[JsonField]
    public float cycleTransitionTimeSeconds = 0.0f;
}
