using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class HandlerDefinition : Element
{
	[JsonField]
	public EPlayerInput inputType = EPlayerInput.Fire1;
	[JsonField]
	public HandlerNodeDefinition[] nodes = new HandlerNodeDefinition[0];
}
