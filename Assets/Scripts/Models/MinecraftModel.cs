using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinecraftModel : ScriptableObject, IVerifiableAsset
{


	[System.Serializable]
	public class NamedTexture : IVerifiableAsset
	{
		public string Key;
		public ResourceLocation Location;
		public Texture2D Texture;

		public void GetVerifications(List<Verification> verifications)
		{
			if (Texture == null)
				verifications.Add(Verification.Failure("Texture is null"));
			if (Key == null || Key.Length == 0)
				verifications.Add(Verification.Failure("Key is empty"));
		}
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
	public List<NamedTexture> Textures = new List<NamedTexture>();

	public abstract bool IsUVMapSame(MinecraftModel other);
	public virtual void FixNamespaces() { }
    public abstract bool ExportToJson(QuickJSONBuilder builder);
	public virtual IEnumerable<MinecraftModel> GetChildren() { yield break; }

	public void BuildExportTree(ExportTree tree)
	{
		tree.Asset = this;
		tree.AssetRelativeExportPath = this.GetLocation().GetPrefixes();
		foreach (MinecraftModel model in GetChildren())
		{
			ExportTree childBranch = new ExportTree();
			model.BuildExportTree(childBranch);
			tree.Children.Add(childBranch);
		}
	}

	public virtual void GetVerifications(List<Verification> verifications)
	{
		List<Texture2D> textureRefs = new List<Texture2D>();
		foreach(NamedTexture texture in Textures)
		{
			if(texture.Texture == null)
			{
				verifications.Add(Verification.Neutral($"Texture missing at {texture.Location}, could be from another mod"));
			}
			else
			{
				if(textureRefs.Contains(texture.Texture))
					verifications.Add(Verification.Neutral($"Texture {texture.Texture} referenced twice in model"));
				else
					textureRefs.Add(texture.Texture);
				verifications.Add(Verification.Success($"Texture {texture.Location} located"));
			}
		}
	}
}
