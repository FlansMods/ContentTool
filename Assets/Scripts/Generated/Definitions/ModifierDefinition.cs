using UnityEngine;

[System.Serializable]
public class ModifierDefinition
{
	[JsonField]
	public string Stat = "";
	[JsonField]
	public string Filter = "";
	[JsonField]
	public float Add = 0.0f;
	[JsonField]
	public float Multiply = 1.0f;
	[JsonField]
	public string SetValue = "";
	[JsonField]
	public bool ApplyToPrimary = true;
	[JsonField]
	public bool ApplyToSecondary = false;
}
