using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Definition : ScriptableObject
{
	public Model Model;
	public Texture2D Skin;
	public Texture2D Icon;

	public class AdditionalTexture
	{
		public string name;
		public Texture2D texture;
	}

	public List<AdditionalTexture> AdditionalTextures = new List<AdditionalTexture>();
}
