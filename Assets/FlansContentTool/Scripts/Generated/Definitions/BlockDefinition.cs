using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class BlockDefinition : Element
{
	[JsonField]
    public bool full3D = false;
	[JsonField]
	[Tooltip("This is only used if full3D is false")]
    public VoxelShapeDefinition[] hitboxes = new VoxelShapeDefinition[0];
	[JsonField]
	[Tooltip("If empty, all facings are allowed")]
    public Direction[] allowedFacings = new Direction[0];
}
