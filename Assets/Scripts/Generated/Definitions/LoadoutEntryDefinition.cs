using UnityEngine;

[System.Serializable]
public class LoadoutEntryDefinition
{
	[JsonField]
	public string paintjobName = "";
	[JsonField]
	public string[] itemUnlocks = new string[0];
}
