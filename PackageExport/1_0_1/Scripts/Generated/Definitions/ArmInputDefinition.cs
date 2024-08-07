using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ArmInputDefinition : Element
{
	[JsonField]
	public string armName = "default";
	[JsonField]
	public EArmInputType type = EArmInputType.SetPosition;
	[JsonField]
	public Vector3 position = Vector3.zero;
	[JsonField]
	public Vector3 euler = Vector3.zero;
	[JsonField]
	public float speed = 1.0f;
}
