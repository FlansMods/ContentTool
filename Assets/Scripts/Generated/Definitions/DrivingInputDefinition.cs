using UnityEngine;

[System.Serializable]
public class DrivingInputDefinition
{
	[JsonField]
	public EDrivingControl control = EDrivingControl.Brake;
	[JsonField]
	public float force = 1.0f;
	[JsonField]
	public bool toggle = false;
}
