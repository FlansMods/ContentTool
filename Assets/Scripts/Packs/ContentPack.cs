using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using static Definition;

public class ContentPack : ScriptableObject, IVerifiableAsset
{
	public List<Definition> Content = new List<Definition>();
	public List<MinecraftModel> Models = new List<MinecraftModel>();
	public List<Texture2D> Textures = new List<Texture2D>();
	public List<AudioClip> Sounds = new List<AudioClip>();
	public List<Definition.LocalisedExtra> ExtraLocalisation = new List<Definition.LocalisedExtra>();
	public string ModName { get { return name; } }

	public bool HasContent(string shortName)
	{
		shortName = Utils.ToLowerWithUnderscores(shortName);
		if(Content != null)
			foreach(Definition def in Content)
				if(def != null)
					if(def.name == shortName)
						return true;
		return false;
	}

	public virtual void GetVerifications(List<Verification> verifications)
	{
		if(ModName != Utils.ToLowerWithUnderscores(ModName))
		{
			verifications.Add(Verification.Failure(
				$"Pack {ModName} does not have a Minecraft-compliant name",
				() => 
				{ 
					name = Utils.ToLowerWithUnderscores(name); 
				})
			);
		}

		foreach(Definition def in Content)
		{
			ResourceLocation resLoc = def.GetLocation();
			if (resLoc.Namespace != ModName)
				verifications.Add(Verification.Failure($"Definition {def} is in {resLoc.Namespace} namespace, but referenced by mod {ModName}"));
		}
	}


}
