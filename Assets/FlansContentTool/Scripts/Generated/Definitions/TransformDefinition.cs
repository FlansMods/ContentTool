using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class TransformDefinition : Element
{
	[JsonField]
    public Vector3 position = new Vector3(0, 0, 0);
	[JsonField]
    public Vector3 eulerAngles = new Vector3(0, 0, 0);
}
