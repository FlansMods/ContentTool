using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class LoadoutChoiceDefinition : Element
{
	[JsonField]
	public string choiceName = "";
	[JsonField]
	public bool selectionMandatory = false;
	[JsonField]
	public LoadoutOptionDefinition[] options = new LoadoutOptionDefinition[0];
}
