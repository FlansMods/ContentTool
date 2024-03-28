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

	public override void AddDefaultTransforms()
	{
		GetOrCreateItemTransform(ItemDisplayContext.THIRD_PERSON_RIGHT_HAND, new Vector3(0f, 1.25f, -2.5f), new Vector3(0f, 90f, -35f), Vector3.one * 0.85f);
		GetOrCreateItemTransform(ItemDisplayContext.THIRD_PERSON_LEFT_HAND, new Vector3(0f, 1.25f, -2.5f), new Vector3(0f, 90f, -35f), Vector3.one * 0.85f);
		GetOrCreateItemTransform(ItemDisplayContext.FIRST_PERSON_RIGHT_HAND, new Vector3(0f, 4f, 2f), new Vector3(0f, -45f, 25f), Vector3.one * 0.85f);
		GetOrCreateItemTransform(ItemDisplayContext.FIRST_PERSON_LEFT_HAND, new Vector3(0f, 4f, 2f), new Vector3(0f, -45f, 25f), Vector3.one * 0.85f);
	}

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

		if (Icons.Count == 0)
		{
			verifications.Add(Verification.Failure($"VanillaIconRootNode ({name}) does not have any icons!",
				() =>
				{
					ApplyQuickFix((VanillaIconRootNode _this) =>
					{
						_this.Icons.Add(new NamedTexture("default"));
					});
					return this;
				}));
		}
		else if (Icons.Count > 1)
		{
			verifications.Add(Verification.Failure($"VanillaIconRootNode ({name}) has multiple icons. Switching is not supported!",
				() =>
				{
					ApplyQuickFix((VanillaIconRootNode _this) =>
					{
						while (_this.Icons.Count > 1)
							_this.Icons.RemoveAt(1);
					});
					return this;
				}));
		}
		// So you have exactly one icon, what's up with it
		else
		{
			if (Icons[0].Key != "default")
				verifications.Add(Verification.Failure($"VanillaIconRootNode ({name}) icon is not keyed as 'default'",
					() =>
					{
						ApplyQuickFix((VanillaIconRootNode _this) =>
						{
							_this.Icons[0].Key = "default";
						});
						return this;
					}));

			if (Icons[0].Location == ResourceLocation.InvalidLocation)
			{
				// See if we can find a good replacement
				if (this.GetLocation().TryLoad(out Texture2D match, "textures/item"))
				{
					verifications.Add(Verification.Neutral($"VanillaIconRootNode ({name}) icon is unset, but a match was found",
						() =>
						{
							ApplyQuickFix((VanillaIconRootNode _this) =>
							{
								_this.Icons[0] = new NamedTexture("default", match);
							});
							return this;
						}));
				}
				// Otherwise, we don't know what to do, so just warn about it
				else
					verifications.Add(Verification.Neutral($"VanillaIconRootNode ({name}) icon is unset"));
			}

		}
	}

	// Unity doesn't use Gizmos.matrix for the Gizmos.DrawGUITexture, oof
	//public void OnDrawGizmosSelected()
	//{
	//	//Gizmos.matrix = transform.localToWorldMatrix;
	//	if (Icons != null && Icons.Count > 0 && Icons[0].Texture != null)
	//		Gizmos.DrawGUITexture(new Rect(10, 10, 20, 20), Icons[0].Texture);
	//}
}
