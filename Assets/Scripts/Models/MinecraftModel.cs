using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinecraftModel : ScriptableObject, IVerifiableAsset
{
	public List<NamedTexture> Textures = new List<NamedTexture>();
	public List<ItemTransform> Transforms = new List<ItemTransform>();

	public enum ItemTransformType
	{
		NONE,
		THIRD_PERSON_LEFT_HAND,
		THIRD_PERSON_RIGHT_HAND,
		FIRST_PERSON_LEFT_HAND,
		FIRST_PERSON_RIGHT_HAND,
		HEAD,
		GUI,
		GROUND,
		FIXED,
	}

	[System.Serializable]
	public class ItemTransform
	{
		public ItemTransformType Type = ItemTransformType.NONE;
		public Vector3 Position = Vector3.zero;
		public Quaternion Rotation = Quaternion.identity;
		public Vector3 Scale = Vector3.one;
	}

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

	public abstract bool IsUVMapSame(MinecraftModel other);
	public virtual void FixNamespaces() { }
    public virtual bool ExportToJson(QuickJSONBuilder builder)
	{
		using (builder.Indentation("display"))
		{
			using (builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(-90f, 0f, 0f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 1f, -3f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(0.55f, 0.55f, 0.55f)));
			}
			using (builder.Indentation("firstperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(0f, -135f, 25f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 4f, 2f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(1.7f, 1.7f, 1.7f)));
			}
		}
		return true;
	}
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
	public virtual void AddDefaultTransforms()
	{
		Transforms.Add(new ItemTransform());
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
		if(Transforms.Count == 0)
		{
			verifications.Add(Verification.Failure($"Model has no transforms", () => {
				AddDefaultTransforms();
			}));
		}
		else
		{
			verifications.Add(Verification.Success($"Model has {Transforms.Count} transforms"));
			List<ItemTransformType> foundTypes = new List<ItemTransformType>();
			for(int i = 0; i < Transforms.Count; i++)
			{
				if (foundTypes.Contains(Transforms[i].Type))
					verifications.Add(Verification.Failure($"Model has a duplicate transform for {Transforms[i].Type}"));
				else
					foundTypes.Add(Transforms[i].Type);

				if (Transforms[i].Scale.sqrMagnitude <= 0.00001f)
					verifications.Add(Verification.Neutral($"{Transforms[i].Type} transform has 0 scale. (This might be intentional)"));
			}
		}
	}
}
