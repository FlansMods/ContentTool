using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/MultiModel")]
public class MultiModel : MinecraftModel
{
	public MinecraftModel FirstPersonModel;
	public MinecraftModel ThirdPersonModel;
	public MinecraftModel HeadModel;
	public MinecraftModel GroundModel;
	public MinecraftModel FixedModel;
	public MinecraftModel GUIModel;

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("loader", "flansmod:multimodel");
		builder.Current.Add("first_person_model", FirstPersonModel.GetLocation().ToString());
		builder.Current.Add("third_person_model", ThirdPersonModel.GetLocation().ToString());
		builder.Current.Add("head_model", HeadModel.GetLocation().ToString());
		builder.Current.Add("ground_model", GroundModel.GetLocation().ToString());
		builder.Current.Add("fixed_model", FixedModel.GetLocation().ToString());
		builder.Current.Add("gui_model", GUIModel.GetLocation().ToString());
		return true;
	}

	public override bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		return false;
	}
}
