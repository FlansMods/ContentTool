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
		return false;
	}

	public override bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		return false;
	}
}
