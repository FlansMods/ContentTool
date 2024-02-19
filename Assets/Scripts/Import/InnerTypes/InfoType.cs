using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoType
{
    public ContentPack contentPack;
    public string iconPath;

	public string longName;
    public string shortName;
    public string texture;
    public string modelString;
    public string modelFolder = "";
    public string description;
    public float modelScale = 1F;
	public Vector3 itemFrameOffset = Vector3.zero;
	public Vector3 thirdPersonOffset = Vector3.zero;

	public virtual void GetTextures(List<string> textureNames)
	{
		if(iconPath != null && iconPath.Length > 0)
			textureNames.Add($"textures/items/{iconPath}.png");
		if(texture != null && texture.Length > 0)
			textureNames.Add($"skins/{texture}.png");
	}
}
