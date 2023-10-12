using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Definition : ScriptableObject
{
	public Model Model;
	public Texture2D Skin;
	public Texture2D Icon;

	public Texture2D GetSkin(string name)
	{
		if (name == "default" || name.Length == 0)
			return Skin;
		foreach (AdditionalTexture tex in AdditionalTextures)
			if (Utils.ToLowerWithUnderscores(tex.name) == name)
				return tex.texture;
		return null;
	}
	public Texture2D GetIcon(string name)
	{
		if (name == "default" || name.Length == 0)
			return Icon;
		foreach (AdditionalTexture tex in AdditionalTextures)
			if (Utils.ToLowerWithUnderscores(tex.name) == name)
				return tex.icon;
		return null;
	}

	[System.Serializable]
	public class AdditionalTexture
	{
		public string name;
		public Texture2D texture;
		public Texture2D icon;
	}

	public List<AdditionalTexture> AdditionalTextures = new List<AdditionalTexture>();

	public enum ELang
	{
		en_us,
		en_gb,
		de_de,
		es_es,
		es_mx,
		fr_fr,
		fr_ca,
		it_it,
		ja_jp,
		ko_kr,
		pt_br,
		pt_pt,
		ru_ru,
		zh_cn,
		zh_tw,
		nl_nl,
		bg_bg,
		cs_cz,
		da_dk,
		el_gr,
		fi_fi,
		hu_hu,
		id_id,
		nb_no,
		pl_pl,
		sk_sk,
		sv_se,
		tr_tr,
		uk_ua,
	}

	[System.Serializable]
	public class LocalisedName
	{
		public ELang Lang = ELang.en_us;
		public string Name = "";
	}

	[System.Serializable]
	public class LocalisedExtra
	{
		public string Unlocalised = "";
		public ELang Lang = ELang.en_us;
		public string Localised = "";
	}

	public List<LocalisedName> LocalisedNames = new List<LocalisedName>();
	public List<LocalisedExtra> LocalisedExtras = new List<LocalisedExtra>();
}
