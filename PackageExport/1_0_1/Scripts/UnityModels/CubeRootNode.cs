using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CubeRootNode : RootNode
{
	// -----------------------------------------------------------------------------------
	#region Operations
	// -----------------------------------------------------------------------------------
	public override bool SupportsTranslate() { return false; }
	public override bool SupportsRotate() { return false; }
	public override bool SupportsMirror() { return true; }
	public override void Mirror(bool flipX, bool flipY, bool flipZ)
	{
		if (flipX)
			SwapTextureKeys("east", "west");
		if (flipY)
			SwapTextureKeys("up", "down");
		if (flipZ)
			SwapTextureKeys("north", "south");
		foreach (Node node in ChildNodes)
			if (node.SupportsMirror())
				node.Mirror(flipX, flipY, flipZ);
	}
	public override bool SupportsRename() { return false; }
	public override bool SupportsDelete() { return false; }
	public override bool SupportsDuplicate() { return false; }
	#endregion
	// -----------------------------------------------------------------------------------

	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		Textures.TextureListField(
			"Faces",
			this,
			() => { return CreateNewFace(); },
			"textures/block");
	}
	private static readonly string[] faceKeys = new string[] {
		"particle", "north", "east", "west", "south", "up", "down"
	};
	public NamedTexture CreateNewFace()
	{
		Texture2D newSkinTexture = new Texture2D(16, 16);

		ResourceLocation location = this.GetLocation();
		string fullPath = $"Assets/Content Packs/{location.Namespace}/textures/block/{location.ID}_{faceKeys[0]}.png";
		int tryIndex = 0;
		while (File.Exists(fullPath) && tryIndex < faceKeys.Length - 1)
		{
			tryIndex++;
			fullPath = $"Assets/Content Packs/{location.Namespace}/textures/block/{location.ID}_{faceKeys[tryIndex]}.png";
		}
		File.WriteAllBytes(fullPath, newSkinTexture.EncodeToPNG());
		AssetDatabase.Refresh();
		newSkinTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
		return new NamedTexture(faceKeys[tryIndex], newSkinTexture);
	}
}
