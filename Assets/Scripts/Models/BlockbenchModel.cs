using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft Models/From Blockbench")]
public class BlockbenchModel : MinecraftModel
{
	public string Json;

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		return base.ExportToJson(builder);
	}

	public override bool IsUVMapSame(MinecraftModel other)
	{
		return other is BlockbenchModel;
	}
	public override void GenerateUVPatches(Dictionary<string, UVPatch> patches)
	{

	}
	public override void ExportUVMap(Dictionary<string, UVMap.UVPlacement> placements)
	{

	}
	public override void ApplyUVMap(UVMap map)
	{
	}
}
