using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/MaterialDefinition")]
public class MaterialDefinition : Definition
{
	[JsonField]
	public MaterialSourceDefinition[] sources = new MaterialSourceDefinition[0];
	[JsonField]
	public int craftingTier = 1;
	[JsonField]
	public EMaterialType materialType = EMaterialType.Misc;
}
