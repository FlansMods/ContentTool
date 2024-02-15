using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpreadsheetImportExport : MonoBehaviour
{
	public static string CsvExportFolderForPack(string packName) { return $"{ContentManager.ASSET_ROOT}/{packName}/csv/"; }


	public void ImportFromGSheet()
    {

    }

    public void ExportFromGSheet()
    {

    }

    public void ImportFromCSV()
    {

    }

	public static void ExportToCSV(ContentPack pack)
	{
		Dictionary<ENewDefinitionType, Dictionary<string, Dictionary<string, string>>> output = new Dictionary<ENewDefinitionType, Dictionary<string, Dictionary<string, string>>>();
		Dictionary<ENewDefinitionType, List<string>> columnHeaders = new Dictionary<ENewDefinitionType, List<string>>();
		foreach (Definition def in pack.AllContent)
		{
			ENewDefinitionType defType = DefinitionTypes.GetFromObject(def);
			if (!output.ContainsKey(defType))
				output.Add(defType, new Dictionary<string, Dictionary<string, string>>());
			if (!columnHeaders.ContainsKey(defType))
				columnHeaders.Add(defType, new List<string>());
			if (defType == ENewDefinitionType.gun)
			{
				GunDefinition gunDef = def as GunDefinition;
				if (!output[defType].ContainsKey(def.name))
				{
					output[defType].Add(def.name, new Dictionary<string, string>());
					foreach (ActionGroupDefinition actionGroup in gunDef.actionGroups)
					{
						foreach (ActionDefinition action in actionGroup.actions)
						{
							foreach (ModifierDefinition mod in action.modifiers)
							{
								if (mod.setValue.Length > 0)
								{
									string key = $"{mod.stat}_set";
									if (!output[defType][def.name].ContainsKey(key))
										output[defType][def.name].Add(key, mod.setValue);
									if (!columnHeaders[defType].Contains(key))
										columnHeaders[defType].Add(key);
								}
								else
								{
									foreach (StatAccumulatorDefinition accumulator in mod.accumulators)
									{
										string key = $"{mod.stat}_{accumulator.operation.ToString().ToLower()}";
										string valueOutput = $"{accumulator.value}";
										foreach(EAccumulationSource source in accumulator.multiplyPer)
										{
											switch (source)
											{
												case EAccumulationSource.PerLevel: valueOutput += "L"; break;
												case EAccumulationSource.PerStacks: valueOutput += "S"; break;
												case EAccumulationSource.PerAttachment: valueOutput += "A"; break;
												case EAccumulationSource.PerMagFullness: valueOutput += "F"; break;
												case EAccumulationSource.PerMagEmptiness: valueOutput += "E"; break;
											}
										}
										if (output[defType][def.name].ContainsKey(key))
										{
											// If we already have this, add another component
											output[defType][def.name][key] += $"+{valueOutput}";
										}
										else output[defType][def.name].Add(key, $"{valueOutput}");
										if (!columnHeaders[defType].Contains(key))
											columnHeaders[defType].Add(key);
									}
								}
							}
						}
					}
				}
			}
		}

		foreach (var defDictPair in output)
		{
			if (defDictPair.Value.Count > 0)
			{
				string folder = CsvExportFolderForPack(pack.name);
				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);
				string outputPath = $"{folder}/{defDictPair.Key}.csv";
				string csv = "asset_id,";
				foreach (string column in columnHeaders[defDictPair.Key])
				{
					csv += $"{column},";
				}
				csv += "\r\n";

				foreach (var assetIDValuesPair in defDictPair.Value)
				{
					csv += $"{assetIDValuesPair.Key},";
					foreach (string column in columnHeaders[defDictPair.Key])
					{
						if (assetIDValuesPair.Value.ContainsKey(column))
						{
							csv += $"{assetIDValuesPair.Value[column]},";
						}
						else csv += ",";
					}
					csv += "\r\n";
				}

				File.WriteAllText(outputPath, csv);
			}
		}

		//foreach (Definition def in pack.AllContent)
		//{
		//	if (defType == DefinitionTypes.GetFromObject(def))
		//	{
		//		Dictionary<string, string> export = new Dictionary<string, string>();
		//		ExportAsset(def, export);
		//
		//	}
		//}
	}

    public void ExportAsset(Definition def, Dictionary<string, string> dict)
    {
        //if(def is GunDefinition gunDef)
        //{
		//	if (gunDef.TryExportMulParam("primary_fire", EActionType.Shoot, "impact_damage", out float impactDamage))
		//		dict.Add("Hit DMG", $"{impactDamage}");
        //}
    }
}

public static class DefinitionHelpers
{

}
