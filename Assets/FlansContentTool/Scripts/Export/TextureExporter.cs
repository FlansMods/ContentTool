using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureExporter : AssetCopyExporter
{
	public static TextureExporter ITEMS = new TextureExporter("items");
	public static TextureExporter BLOCKS = new TextureExporter("blocks");
	public static TextureExporter SKINS = new TextureExporter("skins");

	public override string GetAssetExtension() { return "png"; }
	public override string GetOutputExtension() { return "png"; }
	private static System.Type TYPE_OF_TEXTURE = typeof(Texture);
	public override bool MatchesAssetType(System.Type type) { return TYPE_OF_TEXTURE.IsAssignableFrom(type); }
	public override string GetOutputFolder() { return OutputFolder; }
	private string OutputFolder;


	public override bool MatchesAsset(Object asset) 
	{
		return asset.TryGetLocation(out ResourceLocation loc) && loc.GetPrefixes().Contains(OutputFolder);
	}

	public TextureExporter(string subfolder)
	{
		OutputFolder = $"textures/{subfolder}";
	}
}
