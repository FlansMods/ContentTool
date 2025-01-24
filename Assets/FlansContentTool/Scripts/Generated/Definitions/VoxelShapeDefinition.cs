using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class VoxelShapeDefinition : Element
{
	[JsonField]
    public Vector3 min = new Vector3(0, 0, 0);
	[JsonField]
    public Vector3 max = new Vector3(1, 1, 1);
}
