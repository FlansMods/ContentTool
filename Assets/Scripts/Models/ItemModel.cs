using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/Item")]
public class ItemModel : MinecraftModel
{
	public ResourceLocation icon;

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", $"item/generated");
		using (builder.Indentation("textures"))
		{
			builder.Current.Add("layer0", icon.ResolveWithSubdir("item"));
		}
		using (builder.Indentation("display"))
		{
			using (builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(-90f, 0f, 0f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 1f, -3f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(0.55f, 0.55f, 0.55f)));
			}
			using (builder.Indentation("firstperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(0f, -135f, 25f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 4f, 2f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(1.7f, 1.7f, 1.7f)));
			}
		}
		return true;
	}


	public override bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		return false;
	}
}
