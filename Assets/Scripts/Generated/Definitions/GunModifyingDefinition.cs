using UnityEngine;

[System.Serializable]
public class GunModifyingDefinition
{
	[JsonField]
	public bool isActive = false;
	[JsonField]
	public string[] disallowedMods = new string[0];
	[JsonField]
	public string[] allowedMods = new string[0];
	[JsonField]
	public int FECostPerModify = 0;
}
