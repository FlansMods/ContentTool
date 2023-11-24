using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/Item")]
public class ItemModel : MinecraftModel
{
	public ResourceLocation IconLocation;
	public Texture2D Icon;
	public List<Variant> Variants;
	[System.Serializable]
	public class Variant
	{
		public string Conditional = "CustomModelData";
		public string Value = "1";
		public ResourceLocation IconLocation;
		public Texture2D Icon;
	}

	public override void GetVerifications(List<Verification> verifications)
	{
		base.GetVerifications(verifications);

		ResourceLocation thisLocation = this.GetLocation();
		bool differentNamespace = IconLocation.Namespace != thisLocation.Namespace;
		bool differentName = IconLocation.IDWithoutPrefixes() != thisLocation.IDWithoutPrefixes();
		//if (differentNamespace)
		//{
		//	verifications.Add(Verification.Neutral($"Icon {IconLocation} is from another content pack"));
		//}
		//if (differentName)
		//{
		//	verifications.Add(Verification.Neutral($"Icon {IconLocation} does not match the name of this Icon model"));
		//}
		if (differentNamespace || differentName)
		{
			string check = thisLocation.IDWithoutPrefixes();
			if (check.EndsWith("_icon"))
				check = check.Substring(0, check.Length - 5);
			if (check.EndsWith("_default"))
				check = check.Substring(0, check.Length - 8);

			ResourceLocation betterLocation = new ResourceLocation(thisLocation.Namespace, $"textures/item/{check}");
			if (betterLocation.TryLoad(out Texture2D texture))
			{
				verifications.Add(Verification.Neutral(
					$"Possible better match found at {betterLocation}",
					() =>
					{
						IconLocation = betterLocation;
						Icon = texture;
					}));
			}
		}
	}

	public Texture2D GetIcon()
	{
		if(Icon == null && IconLocation != null)
		{
			Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
				$"Assets/Content Packs/{IconLocation.Namespace}/textures/items/{IconLocation.ID}.png");
		}
		return Icon;
	}

	public override void AddDefaultTransforms()
	{
		Transforms.Add(new ItemTransform()
		{
			Type = ItemTransformType.THIRD_PERSON_RIGHT_HAND,
			Position = new Vector3(0f, 1f, -3f),
			Rotation = Quaternion.Euler(-90f, 0f, 0f),
			Scale = Vector3.one * 0.55f,
		});
		Transforms.Add(new ItemTransform()
		{
			Type = ItemTransformType.FIRST_PERSON_RIGHT_HAND,
			Position = new Vector3(0f, 4f, 2f),
			Rotation = Quaternion.Euler(0f, -135f, 25f),
			Scale = Vector3.one * 1.7f,
		});
	}

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", $"item/generated");
		using (builder.Indentation("textures"))
		{
			builder.Current.Add("layer0", IconLocation.ExportAsTexturePath());
		}
		return base.ExportToJson(builder);
	}
}
