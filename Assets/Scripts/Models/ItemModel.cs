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
		// TODO: Check for skins
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

	public override bool IsUVMapSame(MinecraftModel other)
	{
		return other is ItemModel;
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
			builder.Current.Add("layer0", IconLocation.ResolveWithSubdir("item"));
		}
		return base.ExportToJson(builder);
	}

	public override void GenerateUVPatches(Dictionary<string, UVPatch> patches)
	{

	}
	public override void ExportUVMap(Dictionary<string, UVMap.UVPlacement> placements)
	{

	}
}
