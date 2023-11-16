using UnityEngine;

[System.Serializable]
public class HandlerDefinition
{
	[JsonField]
	public EPlayerInput inputType = EPlayerInput.Fire1;
	[JsonField]
	public HandlerNodeDefinition[] nodes = new HandlerNodeDefinition[0];
}
