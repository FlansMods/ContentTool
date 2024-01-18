using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class VanillaIconRootNode : RootNode
{
	// -----------------------------------------------------------------------------------
	#region Operations
	// -----------------------------------------------------------------------------------
	public override bool SupportsTranslate() { return false; }
	public override bool SupportsRotate() { return false; }
	public override bool SupportsMirror() { return false; }
	public override bool SupportsRename() { return false; }
	public override bool SupportsDelete() { return false; }
	public override bool SupportsDuplicate() { return false; }
	#endregion
	// -----------------------------------------------------------------------------------

	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		Icons.TextureListField(
			"Icons",
			this,
			() => { return CreateNewIcon(); },
			"textures/item");
	}
	public NamedTexture CreateNewIcon()
	{
		Texture2D newIconTexture = new Texture2D(16, 16);
		ResourceLocation location = this.GetLocation();
		string fullPath = $"Assets/Content Packs/{location.Namespace}/textures/item/{location.ID}.png";
		if (!File.Exists(fullPath))
		{
			File.WriteAllBytes(fullPath, newIconTexture.EncodeToPNG());
			AssetDatabase.Refresh();
		}
		newIconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
		return new NamedTexture("default", newIconTexture);
	}
}
