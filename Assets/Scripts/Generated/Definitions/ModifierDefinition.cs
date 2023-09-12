using UnityEngine;

[System.Serializable]
public class ModifierDefinition
{
	[JsonField]
	public string Stat = "";
	[JsonField]
	public string Filter = "";
	[JsonField]
[Tooltip("Additive modifiers are applied first")]
	public float Add = 0.0f;
	[JsonField]
[Tooltip("All multiplys are applied after all adds, so notably a 0x multiplier will always 0 the stat")]
	public float Multiply = 1.0f;
	[JsonField]
[Tooltip("For non-numeric values, such as enums, this is a simple override")]
	public string SetValue = "";
	[JsonField]
	public bool ApplyToPrimary = true;
	[JsonField]
	public bool ApplyToSecondary = false;
}
