using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/MultiModel")]
public class MultiModel : MinecraftModel
{
	public override bool IsUVMapSame(MinecraftModel other)
	{
		if(other is MultiModel multi)
		{
			return multi.FirstPersonModel.IsUVMapSame(FirstPersonModel)
			&& multi.ThirdPersonModel.IsUVMapSame(ThirdPersonModel)
			&& multi.HeadModel.IsUVMapSame(HeadModel)
			&& multi.GroundModel.IsUVMapSame(GroundModel)
			&& multi.FixedModel.IsUVMapSame(FixedModel)
			&& multi.GUIModel.IsUVMapSame(GUIModel);
		}
		return false;
	}
	public override void GenerateUVPatches(Dictionary<string, UVPatch> patches)
	{
		FirstPersonModel?.GenerateUVPatches(patches);
		ThirdPersonModel?.GenerateUVPatches(patches);
		HeadModel?.GenerateUVPatches(patches);
		GroundModel?.GenerateUVPatches(patches);
		FixedModel?.GenerateUVPatches(patches);
		GUIModel?.GenerateUVPatches(patches);
	}
	public override void ExportUVMap(Dictionary<string, UVMap.UVPlacement> placements)
	{
		FirstPersonModel?.ExportUVMap(placements);
		ThirdPersonModel?.ExportUVMap(placements);
		HeadModel?.ExportUVMap(placements);
		GroundModel?.ExportUVMap(placements);
		FixedModel?.ExportUVMap(placements);
		GUIModel?.ExportUVMap(placements);
	}

	public MinecraftModel FirstPersonModel;
	public MinecraftModel ThirdPersonModel;
	public MinecraftModel HeadModel;
	public MinecraftModel GroundModel;
	public MinecraftModel FixedModel;
	public MinecraftModel GUIModel;

	private List<MinecraftModel> ModelSet = new List<MinecraftModel>();
	private void RefreshModelSet()
	{
		ModelSet.Clear();
		if (FirstPersonModel != null && !ModelSet.Contains(FirstPersonModel))
			ModelSet.Add(FirstPersonModel);
		if (ThirdPersonModel != null && !ModelSet.Contains(ThirdPersonModel))
			ModelSet.Add(ThirdPersonModel);
		if (HeadModel != null && !ModelSet.Contains(HeadModel))
			ModelSet.Add(HeadModel);
		if (GroundModel != null && !ModelSet.Contains(GroundModel))
			ModelSet.Add(GroundModel);
		if (FixedModel != null && !ModelSet.Contains(FixedModel))
			ModelSet.Add(FixedModel);
		if (GUIModel != null && !ModelSet.Contains(GUIModel))
			ModelSet.Add(GUIModel);
	}

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("loader", "flansmod:multimodel");
		builder.Current.Add("first_person_model", FirstPersonModel.GetLocation().ToString());
		builder.Current.Add("third_person_model", ThirdPersonModel.GetLocation().ToString());
		builder.Current.Add("head_model", HeadModel.GetLocation().ToString());
		builder.Current.Add("ground_model", GroundModel.GetLocation().ToString());
		builder.Current.Add("fixed_model", FixedModel.GetLocation().ToString());
		builder.Current.Add("gui_model", GUIModel.GetLocation().ToString());
		return base.ExportToJson(builder);
	}

	public override IEnumerable<MinecraftModel> GetChildren() 
	{
		RefreshModelSet();
		foreach (MinecraftModel model in ModelSet)
			yield return model;
	}
}
