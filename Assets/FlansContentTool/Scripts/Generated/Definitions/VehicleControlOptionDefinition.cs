using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class VehicleControlOptionDefinition : Element
{
	[JsonField]
	public string key = "default";
	[JsonField]
	public ResourceLocation controlScheme = InvalidLocation;
	[JsonField]
	public string modalCheck = "";
}
