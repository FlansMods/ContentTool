using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public class ContentPack : ScriptableObject, IVerifiableAsset, IVerifiableContainer
{
	private float RefreshEveryT = 10;
	private DateTime LastContentCheck = DateTime.Now;

	[SerializeField]
	private List<Definition> Content = new List<Definition>();
	[SerializeField]
	private List<RootNode> Models = new List<RootNode>();
	[SerializeField]
	private List<Texture2D> Textures = new List<Texture2D>();
	[SerializeField]
	private List<string> IDs = new List<string>();
	[SerializeField]
	private List<ScriptableObject> OutdatedContent = new List<ScriptableObject>();
	[SerializeField]
	private List<SoundEventList> Sounds = new List<SoundEventList>();
	public List<Definition.LocalisedExtra> ExtraLocalisation = new List<Definition.LocalisedExtra>();
	public string ModName { get { return name; } }

	public void ForceRefreshAssets() 
	{
		Refresh(true);
	}
	public int ContentCount { get { Refresh(); return Content.Count; } }
	public IEnumerable<Definition> AllContent
	{
		get
		{
			Refresh();
			foreach (Definition def in Content)
				if(def != null)
					yield return def;
		}
	}
	public int TextureCount { get { Refresh(); return Textures.Count; } }
	public IEnumerable<Texture2D> AllTextures
	{
		get
		{
			Refresh();
			foreach (Texture2D tex in Textures)
				if(tex != null)
					yield return tex;
		}
	}
	public int ModelCount { get { Refresh(); return Models.Count; } }
	public IEnumerable<RootNode> AllModels
	{
		get
		{
			Refresh();
			foreach (RootNode model in Models)
				if(model != null)
					yield return model;
		}
	}
	public int SoundCount { get { Refresh(); return Sounds.Count; } }
	public IEnumerable<SoundEventEntry> AllSounds
	{
		get
		{
			Refresh();
			foreach (SoundEventList list in Sounds)
				foreach(SoundEventEntry entry in list.SoundEvents)
					if(entry != null)
						yield return entry;
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
	public IEnumerable<string> IDsWithPrefix(string prefix)
	{
		Refresh();
		foreach (string id in IDs)
			if(id.StartsWith(prefix))
				yield return id.Substring(prefix.Length);
	}
	public Dictionary<ENewDefinitionType, List<Definition>> GetSortedContent()
	{
		Refresh();
		Dictionary<ENewDefinitionType, List<Definition>> content = new Dictionary<ENewDefinitionType, List<Definition>>();
		foreach(Definition def in Content)
		{
			ENewDefinitionType type = DefinitionTypes.GetFromObject(def);
			if (!content.ContainsKey(type))
				content.Add(type, new List<Definition>());
			content[type].Add(def);
		}
		return content;
	}

	private void Refresh(bool force = false)
	{
		if (Content == null || Models == null || Textures == null || Sounds == null)
		{
			Content = new List<Definition>();
			Models = new List<RootNode>();
			Textures = new List<Texture2D>();
			Sounds = new List<SoundEventList>();
			force = true;
		}
			

		if (force || (DateTime.Now - LastContentCheck).TotalSeconds >= RefreshEveryT)
		{
			int count = Content.Count + Models.Count + Textures.Count;
			Content.Clear();
			Models.Clear();
			Textures.Clear();
			Sounds.Clear();
			if (Directory.Exists($"Assets/Content Packs/{name}/"))
			{
				foreach (string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{name}/", "*.asset", SearchOption.AllDirectories))
				{
					if (assetPath.EndsWith($"{name}.asset"))
						continue;

					Definition def = AssetDatabase.LoadAssetAtPath<Definition>(assetPath);
					if (def != null)
						Content.Add(def);
					else
					{
						SoundEventList soundList = AssetDatabase.LoadAssetAtPath<SoundEventList>(assetPath);
						if (soundList != null)
						{
							Sounds.Add(soundList);
						}
						else
						{
							ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
							if (asset != null)
							{
								OutdatedContent.Add(asset);
								Debug.LogWarning($"Outdated asset type ({asset.GetType()}) at '{assetPath}' in pack '{name}'");
							}
							else
							{
								Debug.LogError($"Unknown asset type at '{assetPath}' in pack '{name}'");
							}
						}
					}
				}
				foreach (string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{name}/", "*.png", SearchOption.AllDirectories))
				{
					Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (texture != null)
						Textures.Add(texture);
					else
					{
						Debug.LogError($"Unable to load .png at {assetPath} in pack {name}");
					}
				}
				foreach (string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{name}/", "*.prefab", SearchOption.AllDirectories))
				{
					RootNode model = AssetDatabase.LoadAssetAtPath<RootNode>(assetPath);
					if (model != null)
						Models.Add(model);
					else
					{
						Debug.LogError($"Unknown prefab type at {assetPath} in pack {name}");
					}

				}

					//foreach (string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{name}/", "*.ogg", SearchOption.AllDirectories))
					//{
					//	AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
					//	if (clip != null)
					//		Sounds.Add(clip);
					//	else
					//		Debug.LogError($"Unable to load .ogg at {assetPath} in pack {name}");
					//}
					//foreach (string assetPath in Directory.EnumerateFiles($"Assets/Content Packs/{name}/", "*.mp3", SearchOption.AllDirectories))
					//{
					//	AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
					//	if (clip != null)
					//		Sounds.Add(clip);
					//	else
					//		Debug.LogError($"Unable to load .mp3 at {assetPath} in pack {name}");
					//}
				}

			if (count != Content.Count + Models.Count + Textures.Count)
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
			foreach (RootNode model in Models)
				if (model.TryGetLocation(out ResourceLocation modelLoc))
					IDs.Add(modelLoc.ID);
			foreach (SoundEventList list in Sounds)
			{
				foreach (SoundEventEntry entry in list.SoundEvents)
					IDs.Add($"sounds/{entry.Key}");
			}
			LastContentCheck = DateTime.Now;
		}
	}

	public int GetContentCount(ENewDefinitionType defType)
	{ 
		int count = 0;
		Refresh();
		foreach (Definition def in Content)
		{
			if (DefinitionTypes.GetFromObject(def) == defType)
				count++;
		}
		return count;
	}

	public IEnumerable<TDefType> GetContent<TDefType>() where TDefType : Definition
	{
		Refresh();
		foreach (Definition def in Content)
		{
			if (def is TDefType tDef)
				yield return tDef;
		}
	}

	public bool TryGetContent(string shortName, out Definition result)
	{
		shortName = Utils.ToLowerWithUnderscores(shortName);
		if (Content != null)
			foreach (Definition def in Content)
				if (def != null)
					if (def.name == shortName)
					{
						result = def;
						return true;
					}
		result = null;
		return false;
	}

	public bool HasContent(string shortName)
	{
		return TryGetContent(shortName, out Definition ignore);
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
					return this;
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

	public UnityEngine.Object GetUnityObject()
	{
		return this;
	}

	public IEnumerable<IVerifiableAsset> GetAssets()
	{
		Refresh();
		foreach (Definition def in Content)
			yield return def;
		foreach (RootNode model in Models)
			yield return model;
		//foreach (SoundEventList soundList in Sounds)
		//	foreach (SoundEventEntry sound in soundList.SoundEvents)
		//		yield return sound;
	}
}
