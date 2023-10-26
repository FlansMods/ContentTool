using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public class ContentPack : ScriptableObject, IVerifiableAsset
{
	private float RefreshEveryT = 10;
	private DateTime LastContentCheck = DateTime.Now;

	[SerializeField]
	private List<Definition> Content = new List<Definition>();
	[SerializeField]
	private List<MinecraftModel> Models = new List<MinecraftModel>();
	[SerializeField]
	private List<Texture2D> Textures = new List<Texture2D>();
	[SerializeField]
	private List<string> IDs = new List<string>();

	public List<AudioClip> Sounds = new List<AudioClip>();
	public List<Definition.LocalisedExtra> ExtraLocalisation = new List<Definition.LocalisedExtra>();
	public string ModName { get { return name; } }

	public void ForceRefreshAssets() 
	{
		Refresh(true);
	}
	public IEnumerable<Definition> AllContent
	{
		get
		{
			Refresh();
			foreach (Definition def in Content)
				yield return def;
		}
	}
	public IEnumerable<Texture2D> AllTextures
	{
		get
		{
			Refresh();
			foreach (Texture2D tex in Textures)
				yield return tex;
		}
	}
	public IEnumerable<string> AllIDs
	{
		get
		{
			Refresh();
			foreach (string id in IDs)
				yield return id;
		}
	}

	private void Refresh(bool force = false)
	{
		if (force || (DateTime.Now - LastContentCheck).TotalSeconds >= RefreshEveryT)
		{
			int count = Content.Count + Models.Count + Textures.Count;
			Content.Clear();
			Models.Clear();
			Textures.Clear();
			foreach (string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{name}/", "*.asset", SearchOption.AllDirectories))
			{
				if (assetPath.EndsWith($"{name}.asset"))
					continue;

				Definition def = AssetDatabase.LoadAssetAtPath<Definition>(assetPath);
				if (def != null)
					Content.Add(def);
				else
				{
					MinecraftModel model = AssetDatabase.LoadAssetAtPath<MinecraftModel>(assetPath);
					if (model != null)
						Models.Add(model);
					else
					{
						Debug.LogError($"Unknown asset type at {assetPath} in pack {name}");
					}
				}
			}

			foreach (string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{name}/", "*.png", SearchOption.AllDirectories))
			{
				Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
				if(texture != null)
					Textures.Add(texture);
				else
				{
					Debug.LogError($"Unable to load .png at {assetPath} in pack {name}");
				}
			}

			if(count != Content.Count + Models.Count + Textures.Count)
			{
				EditorUtility.SetDirty(this);
			}

			IDs.Clear();
			foreach (Definition def in Content)
				if (def.TryGetLocation(out ResourceLocation defLoc))
					IDs.Add(defLoc.ID);
			foreach (Texture2D tex in Textures)
				if (tex.TryGetLocation(out ResourceLocation texLoc))
					IDs.Add(texLoc.ID);
			foreach (MinecraftModel model in Models)
				if (model.TryGetLocation(out ResourceLocation modelLoc))
					IDs.Add(modelLoc.ID);
			LastContentCheck = DateTime.Now;
		}
	}

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

		List<string> referencedNamespaces = new List<string>();
		foreach (Definition def in Content)
		{
			ResourceLocation resLoc = def.GetLocation();
			if (resLoc.Namespace != ModName)
				verifications.Add(Verification.Failure($"Definition {def} is in {resLoc.Namespace} namespace, but referenced by mod {ModName}"));

			if (!referencedNamespaces.Contains(resLoc.Namespace))
				referencedNamespaces.Add(resLoc.Namespace);
		}

		if (referencedNamespaces.Count == 1)
			verifications.Add(Verification.Success("Pack has no dependencies"));
		else
		{
			foreach (string ns in referencedNamespaces)
				if (ns != ModName)
					verifications.Add(Verification.Neutral($"This pack depends on another pack: {ns}"));
		}
	}


}
