using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureExporter : AssetCopyExporter
{
	public static TextureExporter ITEMS = new TextureExporter("item", "textures/item");
	public static TextureExporter BLOCKS = new TextureExporter("block", "textures/block");
	public static TextureExporter SKINS = new TextureExporter("skins", "textures/skins");
	public static TextureExporter MAGS = new TextureExporter("mags", "textures/mags");
	public static TextureExporter GUI = new TextureExporter("gui", "textures/gui");
	public static TextureExporter MOB_EFFECT = new TextureExporter("mob_effect", "textures/mob_effect");
	public static TextureExporter ENTITY = new TextureExporter("entity", "textures/entity");

	public override string GetAssetExtension() { return "png"; }
	public override string GetOutputExtension() { return "png"; }
	private static System.Type TYPE_OF_TEXTURE = typeof(Texture);
	public override bool MatchesAssetType(System.Type type) { return TYPE_OF_TEXTURE.IsAssignableFrom(type); }
	public override string GetOutputFolder() { return OutputFolder; }
	private string OutputFolder;
	private string Match;

	public override bool MatchesAsset(Object asset) 
	{
		return asset.TryGetLocation(out ResourceLocation loc) && loc.GetPrefixes().Contains(Match);
	}

	public TextureExporter(string subfolder, string match)
	{
		OutputFolder = $"textures/{subfolder}";
		Match = match;
	}
}
