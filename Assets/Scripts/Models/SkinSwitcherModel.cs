using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/Skin Switcher")]
public class SkinSwitcherModel : MinecraftModel
{
	public List<NamedTexture> Icons = new List<NamedTexture>();
	public NamedTexture Default { 
		get
		{
			foreach (NamedTexture texture in Icons)
				if (texture.Key == "default")
					return texture;
			if (Icons.Count > 0)
				return Icons[0];
			return null;
		}
	}

	public override void GenerateUVPatches(Dictionary<string, UVPatch> patches)
	{
		
	}
	public override void ExportUVMap(Dictionary<string, UVMap.UVPlacement> placements)
	{
		
	}

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", Default.Location.ToString());
		using (builder.Tabulation("overrides"))
		{
			for (int i = 0; i < Icons.Count; i++)
				using (builder.TableEntry())
				{
					using (builder.Indentation("predicate"))
					{
						builder.Current.Add("custom_model_data", i + 1);
					}
					builder.Current.Add("model", Icons[i].Location.ToString());
				}
		}
		return true;
	}

	public override bool IsUVMapSame(MinecraftModel other)
	{
		return true;
	}
}
