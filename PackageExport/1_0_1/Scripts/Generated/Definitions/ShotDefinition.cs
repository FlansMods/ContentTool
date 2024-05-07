using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class ShotDefinition : Element
{
	[JsonField]
	[Tooltip("Number of raycasts or bullet entities to create")]
	public int bulletCount = 1;
	[JsonField]
	public string[] breaksBlockTags = new string[0];
	[JsonField]
	public float penetrationPower = 1.0f;
	[JsonField]
	public ImpactDefinition impact = new ImpactDefinition();
}
