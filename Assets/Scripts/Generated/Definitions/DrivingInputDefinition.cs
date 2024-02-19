using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class DrivingInputDefinition : Element
{
	[JsonField]
	public EDrivingControl control = EDrivingControl.Brake;
	[JsonField]
	public float force = 1.0f;
	[JsonField]
	public bool toggle = false;
}
