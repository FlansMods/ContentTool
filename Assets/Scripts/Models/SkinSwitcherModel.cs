using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/Skin Switcher")]
public class SkinSwitcherModel : MinecraftModel
{
	public MinecraftModel DefaultModel = null;
	public List<MinecraftModel> Models = new List<MinecraftModel>();

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		if(DefaultModel != null)
			builder.Current.Add("parent", DefaultModel.GetLocation().ExportAsModelPath());
		using (builder.Tabulation("overrides"))
		{
			for (int i = 0; i < Models.Count; i++)
				using (builder.TableEntry())
				{
					using (builder.Indentation("predicate"))
					{
						builder.Current.Add("custom_model_data", i + 1);
					}
					builder.Current.Add("model", Models[i].GetLocation().ExportAsModelPath());
				}
		}
		return true;
	}
}
