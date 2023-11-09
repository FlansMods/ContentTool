using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class JsonModelExporter
{	
	public static ExportTree ExportToModelJsonFiles(this MinecraftModel model, ExportDirectory assetsDir, List<string> outputFileList = null)
	{
		ExportTree tree = new ExportTree();
		model.BuildExportTree(tree);
		tree.ExportToModelJsonFiles(assetsDir);
		return tree;
	}

	public static void ExportToModelJsonFiles(this ExportTree tree, ExportDirectory assetsDir)
	{
		if(tree.Asset is MinecraftModel model)
		{
			using (var exportDir = assetsDir.Subdir(tree.AssetRelativeExportPath))
			{
				QuickJSONBuilder builder = new QuickJSONBuilder();
				if (model.ExportToJson(builder))
				{
					builder.Root.ExportToFile(exportDir.File($"{model.name}.json"));
				}
				else
				{
					string raw = model.WriteRawJson();
					if (raw.Length > 0)
						File.WriteAllText(exportDir.File($"{model.name}.json"), raw);
				}
			}
		}

		foreach (ExportTree branch in tree.Children)
			branch.ExportToModelJsonFiles(assetsDir);
	}
}
