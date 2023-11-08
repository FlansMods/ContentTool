using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

	public MinecraftModel GetModel(ItemTransformType type)
	{
		 switch(type)
		 {
			case ItemTransformType.HEAD: 
				return HeadModel;
			case ItemTransformType.THIRD_PERSON_LEFT_HAND:
			case ItemTransformType.THIRD_PERSON_RIGHT_HAND:
				return ThirdPersonModel;
			case ItemTransformType.FIRST_PERSON_LEFT_HAND:
			case ItemTransformType.FIRST_PERSON_RIGHT_HAND:
				return FirstPersonModel;
			case ItemTransformType.GUI:
				return GUIModel;
			case ItemTransformType.FIXED:
				return FixedModel;
			case ItemTransformType.GROUND:
			default:
				return GroundModel;
		}
	}
	public void SetModel(ItemTransformType type, MinecraftModel model)
	{
		if (FirstPersonModel == null && ThirdPersonModel == null && GUIModel == null
		&& FixedModel == null && GroundModel == null && HeadModel == null)
		{
			HeadModel = model;
			ThirdPersonModel = model;
			FirstPersonModel = model;
			GUIModel = model;
			FixedModel = model;
			GroundModel = model;
		}

		switch (type)
		{
			case ItemTransformType.HEAD:
				HeadModel = model;
				break;
			case ItemTransformType.THIRD_PERSON_LEFT_HAND:
			case ItemTransformType.THIRD_PERSON_RIGHT_HAND:
				ThirdPersonModel = model;
				break;
			case ItemTransformType.FIRST_PERSON_LEFT_HAND:
			case ItemTransformType.FIRST_PERSON_RIGHT_HAND:
				FirstPersonModel = model;
				break;
			case ItemTransformType.GUI:
				GUIModel = model;
				break;
			case ItemTransformType.FIXED:
				FixedModel = model;
				break;
			case ItemTransformType.GROUND:
				GroundModel = model;
				break;
		}
	}


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
		builder.Current.Add("first_person_model", FirstPersonModel.GetLocation().ExportAsModelPath());
		builder.Current.Add("third_person_model", ThirdPersonModel.GetLocation().ExportAsModelPath());
		builder.Current.Add("head_model", HeadModel.GetLocation().ExportAsModelPath());
		builder.Current.Add("ground_model", GroundModel.GetLocation().ExportAsModelPath());
		builder.Current.Add("fixed_model", FixedModel.GetLocation().ExportAsModelPath());
		builder.Current.Add("gui_model", GUIModel.GetLocation().ExportAsModelPath());
		return base.ExportToJson(builder);
	}

	public override IEnumerable<MinecraftModel> GetChildren() 
	{
		RefreshModelSet();
		foreach (MinecraftModel model in ModelSet)
			yield return model;
	}

	public override void GetVerifications(List<Verification> verifications)
	{
		base.GetVerifications(verifications);

		if (FirstPersonModel == null && ThirdPersonModel == null && GUIModel == null
			&& FixedModel == null && GroundModel == null && HeadModel == null)
		{
			verifications.Add(Verification.Failure("MultiModel has no models at all!"));
		}
		else
		{
			if (FirstPersonModel == null)
				verifications.Add(Verification.Neutral("MultiModel has no First Person Model"));
			if (ThirdPersonModel == null)
				verifications.Add(Verification.Neutral("MultiModel has no Third Person Model"));
			if (GUIModel == null)
				verifications.Add(Verification.Neutral("MultiModel has no GUI Model"));
			if (FixedModel == null)
				verifications.Add(Verification.Neutral("MultiModel has no Fixed Model"));
			if (GroundModel == null)
				verifications.Add(Verification.Neutral("MultiModel has no Ground Model"));
			if (HeadModel == null)
				verifications.Add(Verification.Neutral("MultiModel has no Head Model"));
		}

	}
}
