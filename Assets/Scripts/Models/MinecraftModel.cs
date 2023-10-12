using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinecraftModel : ScriptableObject
{
	[System.Serializable]
	public class NamedTexture
	{
		public string Key;
		public ResourceLocation Location;
		public Texture2D Texture;
	}

	public void AddTexture(string key, string modID, Texture2D texture)
	{
		if (texture != null)
		{
			NamedTexture namedTex = new NamedTexture()
			{
				Key = key,
				Location = new ResourceLocation(modID, texture.name),
				Texture = texture,
			};
			Textures.Add(namedTex);
		}
	}

	public NamedTexture GetOrCreateNamedTexture(string name)
	{
		foreach (NamedTexture tex in Textures)
			if (tex.Key == name)
				return tex;

		NamedTexture defaultTex = GetDefaultTexture();
		NamedTexture newTex = new NamedTexture()
		{
			Key = name,
			Location = defaultTex == null ? new ResourceLocation() : new ResourceLocation(defaultTex.Location.Namespace, name),
			Texture = null
		};
		Textures.Add(newTex);
		return newTex;
	}

	public NamedTexture GetNamedTexture(string name)
	{
		foreach (NamedTexture tex in Textures)
			if (tex.Key == name)
				return tex;
		return GetDefaultTexture();
	}

	public NamedTexture GetDefaultTexture()
	{
		foreach (NamedTexture tex in Textures)
			if (tex.Key == "default")
				return tex;
		if (Textures.Count > 0)
			return Textures[0];
		return null;
	}

	public ResourceLocation ID = new ResourceLocation();
	public List<NamedTexture> Textures = new List<NamedTexture>();

    public virtual void FixNamespaces() { }
    public abstract bool ExportToJson(QuickJSONBuilder builder);
    public abstract bool ExportInventoryVariantToJson(QuickJSONBuilder builder);
}
