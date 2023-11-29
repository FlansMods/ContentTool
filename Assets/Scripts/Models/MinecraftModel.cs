using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinecraftModel : ScriptableObject, IVerifiableAsset
{
	public List<NamedTexture> Textures = new List<NamedTexture>();
	public List<ItemTransform> Transforms = new List<ItemTransform>();
	public UVMap BakedUVMap = null;

	public void ApplyUVMap(UVMap newMap)
	{
		BakedUVMap = newMap;
	}

	// ------------------------------------------------------------------------------
	#region Transforms
	// ------------------------------------------------------------------------------
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

		public string GetOutputKey()
		{
			switch(Type)
			{
				case ItemTransformType.THIRD_PERSON_RIGHT_HAND:
					return "thirdperson_righthand";
				case ItemTransformType.THIRD_PERSON_LEFT_HAND:
					return "thirdperson_lefthand";
				case ItemTransformType.FIRST_PERSON_RIGHT_HAND:
					return "firstperson_righthand";
				case ItemTransformType.FIRST_PERSON_LEFT_HAND:
					return "firstperson_lefthand";
				case ItemTransformType.HEAD:
					return "head";
				case ItemTransformType.GUI:
					return "gui";
				case ItemTransformType.GROUND:
					return "ground";
				case ItemTransformType.FIXED:
					return "fixed";
				default: return null;
			}
		}
	}

	public virtual void AddDefaultTransforms()
	{
		Transforms.Add(new ItemTransform());
	}
	#endregion
	// ------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------
	#region Textures
	// ------------------------------------------------------------------------------
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
			if (Key != "default")
				if (Key != Location.IDWithoutPrefixes())
				{
					verifications.Add(Verification.Neutral($"Key for skin does not match the skin name",
					() => {
						Key = Location.IDWithoutPrefixes();
					}));
				}
			
		}
		public override string ToString()
		{
			return $"'{Key}'={Location}";
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
	#endregion
	// ------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------
	#region UV Mapping
	// ------------------------------------------------------------------------------
	public virtual void CollectUnplacedUVs(List<BoxUVPatch> unplacedPatches)
	{
		
	}

	public void OnResizeUV(string key, UVPatch newPatch)
	{
		//CurrentUVMap
	}
	#endregion
	// ------------------------------------------------------------------------------


	public virtual void FixNamespaces() { }
	public virtual string WriteRawJson() { return ""; }
	public virtual bool ExportToJson(QuickJSONBuilder builder)
	{
		using (builder.Indentation("display"))
		{
			foreach(ItemTransform trans in Transforms)
			{
				string outputKey = trans.GetOutputKey();
				if (outputKey != null)
				{
					using (builder.Indentation(outputKey))
					{
						builder.Current.Add("rotation", JSONHelpers.ToJSON(trans.Rotation.eulerAngles));
						builder.Current.Add("translation", JSONHelpers.ToJSON(trans.Position));
						builder.Current.Add("scale", JSONHelpers.ToJSON(trans.Scale));
					}
				}
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

	public virtual void GetVerifications(List<Verification> verifications)
	{
		List<Texture2D> textureRefs = new List<Texture2D>();
		if (Textures != null)
		{
			foreach (NamedTexture texture in Textures)
			{
				if (texture.Texture == null)
				{
					verifications.Add(Verification.Neutral($"Texture missing at {texture.Location}, could be from another mod"));
				}
				else
				{
					if (textureRefs.Contains(texture.Texture))
						verifications.Add(Verification.Neutral($"Texture {texture.Texture} referenced twice in model"));
					else
						textureRefs.Add(texture.Texture);
					verifications.Add(Verification.Success($"Texture {texture.Location} located"));
				}
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
			//verifications.Add(Verification.Success($"Model has {Transforms.Count} transforms"));
			List<ItemTransformType> foundTypes = new List<ItemTransformType>();
			for(int i = 0; i < Transforms.Count; i++)
			{
				if (foundTypes.Contains(Transforms[i].Type))
					verifications.Add(Verification.Failure($"Model has a duplicate transform for {Transforms[i].Type.ToNiceString()}"));
				else
					foundTypes.Add(Transforms[i].Type);

				if (Transforms[i].Scale.sqrMagnitude <= 0.00001f)
					verifications.Add(Verification.Neutral($"{Transforms[i].Type.ToNiceString()} transform has 0 scale. (This might be intentional)"));
			}
		}
	}
}

public static class ItemTransformTypes
{
	public static string ToNiceString(this MinecraftModel.ItemTransformType type)
	{
		switch (type)
		{
			case MinecraftModel.ItemTransformType.NONE: return "None";
			case MinecraftModel.ItemTransformType.THIRD_PERSON_LEFT_HAND: return "Third Person, Left Hand";
			case MinecraftModel.ItemTransformType.THIRD_PERSON_RIGHT_HAND: return "Third Person, Right Hand";
			case MinecraftModel.ItemTransformType.FIRST_PERSON_LEFT_HAND: return "First Person, Left Hand";
			case MinecraftModel.ItemTransformType.FIRST_PERSON_RIGHT_HAND: return "First Person, Right Hand";
			case MinecraftModel.ItemTransformType.HEAD: return "Head";
			case MinecraftModel.ItemTransformType.GUI: return "GUI";
			case MinecraftModel.ItemTransformType.GROUND: return "Ground";
			case MinecraftModel.ItemTransformType.FIXED: return "Fixed";
			default: return "Unknown";
		}
	}
}
