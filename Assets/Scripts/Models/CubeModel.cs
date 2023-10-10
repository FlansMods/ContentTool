using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/Cube")]
public class CubeModel : MinecraftModel
{
	public Vector3 Origin = Vector3.zero;
	public Vector3 Dimensions = Vector3.one;
	public Vector3 Center { get { return Origin + Dimensions * 0.5f; } }

	public ResourceLocation particle;

	public ResourceLocation top;
	public ResourceLocation north;
	public ResourceLocation east;
	public ResourceLocation south;
	public ResourceLocation west;
	public ResourceLocation bottom;

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", "block/cube");
		using (builder.Indentation("textures"))
		{
			builder.Current.Add("particle", particle.ResolveWithSubdir("block"));
			builder.Current.Add("up", top.ResolveWithSubdir("block"));
			builder.Current.Add("down", bottom.ResolveWithSubdir("block"));
			builder.Current.Add("north", north.ResolveWithSubdir("block"));
			builder.Current.Add("east", east.ResolveWithSubdir("block"));
			builder.Current.Add("south", south.ResolveWithSubdir("block"));
			builder.Current.Add("west", west.ResolveWithSubdir("block"));
		}
		return true;
	}

	public override bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", ID.ResolveWithSubdir("block"));
		using (builder.Indentation("display"))
		{
			using (builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(10f, -45f, 170f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 1.5f, -2.75f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(0.375f, 0.375f, 0.375f)));
			}
		}
		return true;
	}
}
