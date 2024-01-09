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
	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", Parent.GetLocation().ToString());
		using (builder.Indentation("display"))
		{
			using (builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", Rotation.eulerAngles.ToJson());
				builder.Current.Add("translation", Translation.ToJson());
				builder.Current.Add("scale", Scale.ToJson());
			}
		}
		return true;
	}
}
