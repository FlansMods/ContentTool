using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentPack : ScriptableObject
{
	public List<Definition> Content = new List<Definition>();
	public List<Model> Models = new List<Model>();
	public List<Texture2D> Textures = new List<Texture2D>();
	public List<AudioClip> Sounds = new List<AudioClip>();
	public List<Definition.LocalisedExtra> ExtraLocalisation = new List<Definition.LocalisedExtra>();
	public string ModName = "";

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

}
