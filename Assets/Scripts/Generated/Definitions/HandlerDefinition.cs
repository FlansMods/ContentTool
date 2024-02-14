using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class HandlerDefinition : Element
{
	[JsonField]
	public EPlayerInput inputType = EPlayerInput.Fire1;
	[JsonField]
	public HandlerNodeDefinition[] nodes = new HandlerNodeDefinition[0];
}
