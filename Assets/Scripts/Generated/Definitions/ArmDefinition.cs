using UnityEngine;

[System.Serializable]
public class ArmDefinition
{
	[JsonField]
	public string name = "default";
	[JsonField]
	public string attachedTo = "body";
	[JsonField]
	public bool right = false;
	[JsonField]
	public Vector3 origin = Vector3.zero;
	[JsonField]
	public float armLength = 1.0f;
	[JsonField]
	public bool hasHoldingSlot = false;
	[JsonField]
	public int numUpgradeSlots = 0;
	[JsonField]
	public bool canFireGuns = false;
	[JsonField]
	public bool canUseMechaTools = false;
	[JsonField]
	public float heldItemScale = 1.0f; // Should be in the model?
	[JsonField]
[Tooltip("How far this hand can reach when using tools")]
	public float reach = 10.0f;
}
