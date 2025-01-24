using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class RedstoneResponseDefinition : Element
{
	[JsonField]
    public int minRedstoneLevel = 0;
	[JsonField]
    public int maxRedstoneLevel = 15;
	[JsonField]
    public bool allowIndirectPower = true;
	[JsonField]
    public EPlayerInput simulateInput = EPlayerInput.Fire1;
	[JsonField]
    public bool sendOnPressEvent = true;
	[JsonField]
    public bool sustainHeldEvent = false;
	[JsonField]
    public bool sendOnReleaseEvent = false;
}
