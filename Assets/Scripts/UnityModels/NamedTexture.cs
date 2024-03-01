using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class NamedTexture : IVerifiableAsset, IModifyable, ICloneable<NamedTexture>
{
	public string Key = "default";
	public ResourceLocation Location = new ResourceLocation();
	public Texture2D Texture = null;

	public NamedTexture() { }
	public NamedTexture(string key) { Key = key; }
	public NamedTexture(string key, ResourceLocation resLoc, string subfolder = "")
	{
		Key = key;
		Location = resLoc;
		resLoc.TryLoad(out Texture, subfolder);
	}
	public NamedTexture(string key, Texture2D tex)
	{
		Key = key;
		Texture = tex;
		Location = tex.GetLocation();
	}

	public NamedTexture Clone()
	{
		return new NamedTexture()
		{
			Key = Key,
			Location = Location.Clone(),
			Texture = Texture,
		};
	}

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
					return null;
				}));
			}

	}
	public string GetName() { return Key; }
	public bool SupportsRename() { return true; }
	public bool SupportsDelete() { return true; }
	public bool SupportsDuplicate() { return true; }
	public bool SupportsPreview() { return true; }

	void IModifyable.Rename(Node parentNode, string newName)
	{
		Key = newName;
	}
	void IModifyable.Preview(Node parentNode)
	{
		if (parentNode is TurboRootNode root)
			root.SelectTexture(Key);
	}


	public override string ToString()
	{
		return $"'{Key}'={Location}";
	}

	
}