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

	public override void GetVerifications(List<Verification> verifications)
	{
		base.GetVerifications(verifications);

		if(Icons.Count == 0)
		{
			verifications.Add(Verification.Failure($"VanillaIconRootNode ({name}) does not have any icons!",
				() => {
					ApplyQuickFix((VanillaIconRootNode _this) => {
						_this.Icons.Add(new NamedTexture("default"));
					});
				}));
		}
		else if(Icons.Count > 1)
		{
			verifications.Add(Verification.Failure($"VanillaIconRootNode ({name}) has multiple icons. Switching is not supported!",
				() => {
					ApplyQuickFix((VanillaIconRootNode _this) => {
						while (_this.Icons.Count > 1)
							_this.Icons.RemoveAt(1);
					});
				}));
		}
		// So you have exactly one icon, what's up with it
		else
		{
			if (Icons[0].Key != "default")
				verifications.Add(Verification.Failure($"VanillaIconRootNode ({name}) icon is not keyed as 'default'",
					() => { 
						ApplyQuickFix((VanillaIconRootNode _this) => { 
							_this.Icons[0].Key = "default"; 
						}); 
					}));

			if(Icons[0].Location == ResourceLocation.InvalidLocation)
			{
				// See if we can find a good replacement
				if(this.GetLocation().TryLoad(out Texture2D match, "textures/item"))
				{
					verifications.Add(Verification.Neutral($"VanillaIconRootNode ({name}) icon is unset, but a match was found",
						() => { 
							ApplyQuickFix((VanillaIconRootNode _this) => { 
								_this.Icons[0] = new NamedTexture("default", match); 
							}); 
						}));
				}
				// Otherwise, we don't know what to do, so just warn about it
				else
					verifications.Add(Verification.Neutral($"VanillaIconRootNode ({name}) icon is unset"));
			}
				
		}
	}

	
}