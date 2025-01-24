using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class RedstoneOutputDefinition : Element
{
	[JsonField]
	[Tooltip("Leave as -1 to not use this field")]
    public int outputConstant = -1;
	[JsonField]
    public bool outputStackFullness = false;
}
