using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ControlSchemeAxisDefinition : Element
{
	[JsonField]
	public EVehicleAxis axisType = EVehicleAxis.Accelerator;
	[JsonField]
	public EAxisBehaviourType axisBehaviour = EAxisBehaviourType.SliderWithRestPosition;
	[JsonField]
	public float minValue = -1.0f;
	[JsonField]
	public float maxValue = 1.0f;
	[JsonField]
	public float startingPosition = 0.0f;
	[JsonField]
	[Tooltip("Used if you select SliderWithRestPosition")]
	public float restingPosition = 0.0f;
	[JsonField]
	[Tooltip("Used if you select a Notched slider")]
	public float[] notches = new float[0];
}
