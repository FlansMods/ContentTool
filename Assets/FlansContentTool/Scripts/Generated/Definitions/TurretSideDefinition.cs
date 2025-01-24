using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class TurretSideDefinition : Element
{
	[JsonField]
    public Direction side = Direction.NORTH;
	[JsonField]
    public RedstoneResponseDefinition[] redstoneResponses = new RedstoneResponseDefinition[0];
	[JsonField]
    public RedstoneOutputDefinition[] redstoneOutputs = new RedstoneOutputDefinition[0];
}
