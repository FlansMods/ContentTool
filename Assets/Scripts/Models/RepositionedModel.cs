using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/Repositioning")]
public class RepositionedModel : MinecraftModel
{
	public MinecraftModel Parent = null;
	public Vector3 Translation = new Vector3(0f, 1.5f, -2.75f);
	public Quaternion Rotation = Quaternion.Euler(10f, -45f, 170f);
	public Vector3 Scale = new Vector3(0.375f, 0.375f, 0.375f);
	public override void GenerateUVPatches(Dictionary<string, UVPatch> patches)
	{
		Parent.GenerateUVPatches(patches);
	}
	public override void ExportUVMap(Dictionary<string, UVMap.UVPlacement> placements)
	{
		Parent.ExportUVMap(placements);
	}
	public override void ApplyUVMap(UVMap map)
	{
		Parent.ApplyUVMap(map);
	}
	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", Parent.GetLocation().ToString());
		using (builder.Indentation("display"))
		{
			using (builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(Rotation.eulerAngles));
				builder.Current.Add("translation", JSONHelpers.ToJSON(Translation));
				builder.Current.Add("scale", JSONHelpers.ToJSON(Scale));
			}
		}
		return true;
	}

	public override bool IsUVMapSame(MinecraftModel other)
	{
		return true;
	}
}
