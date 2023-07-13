using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/MaterialDefinition")]
public class MaterialDefinition : Definition
{
	[JsonField]
	public string[] matchItems = new string[0];
	[JsonField]
	public string[] matchTags = new string[0];
	[JsonField]
	public int craftingTier = 1;
	[JsonField]
	public EMaterialType materialType = EMaterialType.Misc;
}
