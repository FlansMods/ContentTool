using UnityEngine;

[System.Serializable]
public class SoundLODDefinition
{
	[JsonField]
	public string sound = "";
	[JsonField]
	public float minDistance = 100f;
}
