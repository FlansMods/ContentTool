using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class SpreadsheetImportExport
{
	public static string CsvExportFolderForPack(string packName) { return $"{ContentManager.ASSET_ROOT}/{packName}/csv/"; }

	public static string ToShortString(this EAccumulationOperation op)
	{
		switch(op)
		{
			case EAccumulationOperation.BaseAdd: return "Base+";
			case EAccumulationOperation.StackablePercentage: return "Stack%";
			case EAccumulationOperation.IndependentPercentage: return "Indep%";
			case EAccumulationOperation.FinalAdd: return "Final+";
			default: return "Set";
		}
	}
	public static EAccumulationOperation FromShortString(string shortString)
	{
		switch (shortString)
		{
			case "Base+": return EAccumulationOperation.BaseAdd;
			case "Stack%": return EAccumulationOperation.StackablePercentage;
			case "Indep%": return EAccumulationOperation.IndependentPercentage;
			case "Final+": return EAccumulationOperation.FinalAdd;
			default: return EAccumulationOperation.BaseAdd;
		}
	}

	private class RowData
	{
		public string id = "";
		public Dictionary<string, RowData> subRows = new Dictionary<string, RowData>();
		public Dictionary<KeyValuePair<string, EAccumulationOperation>, string> values = new Dictionary<KeyValuePair<string, EAccumulationOperation>, string>();

		public RowData GetOrCreate(string subID)
		{
			if (subRows.TryGetValue(subID, out RowData child))
				return child;

			subRows.Add(subID, new RowData() { id = subID });
			return subRows[subID];
		}
		public bool TryGetRow(string subID, out RowData row)
		{
			return subRows.TryGetValue(subID, out row);
		}
		public void AddValue(string stat, EAccumulationOperation op, string value)
		{
			values[new KeyValuePair<string, EAccumulationOperation>(stat, op)] = value;
		}
		public bool TryGetString(string stat, out string result)
		{
			return values.TryGetValue(new KeyValuePair<string, EAccumulationOperation>(stat, EAccumulationOperation.BaseAdd), out result);
		}
		public bool TryGetFloat(string stat, EAccumulationOperation op, out float result)
		{
			if(values.TryGetValue(new KeyValuePair<string, EAccumulationOperation>(stat, op), out string value))
			{
				return float.TryParse(value, out result);
			}
			result = 0.0f;
			return false;
		}
		public bool TryGetInt(string stat, EAccumulationOperation op, out int result)
		{
			if (values.TryGetValue(new KeyValuePair<string, EAccumulationOperation>(stat, op), out string value))
			{
				return int.TryParse(value, out result);
			}
			result = 0;
			return false;
		}
	}

	private class Sheet
	{
		private int MaxChildDepth;
		public Sheet(int maxChildDepth)
		{
			MaxChildDepth = maxChildDepth;
		}

		private List<KeyValuePair<string, EAccumulationOperation>> Columns = new List<KeyValuePair<string, EAccumulationOperation>>();
		private List<string> IDs = new List<string>();
		private RowData Root = new RowData();
		public void PushID(string id)
		{
			IDs.Add(id);
		}
		public void PopID()
		{
			IDs.RemoveAt(IDs.Count - 1);
		}

		public void AddValue(string stat, string value)
		{
			AddValue(stat, EAccumulationOperation.BaseAdd, value);
		}
		public void AddValue(string stat, EAccumulationOperation op, float value)
		{
			AddValue(stat, op, $"{value}");
		}
		public void AddValue(string stat, EAccumulationOperation op, string value)
		{
			RowData rowData = Root;
			foreach (string id in IDs)
				rowData = rowData.GetOrCreate(id);

			rowData.AddValue(stat, op, value);
			var kvp = new KeyValuePair<string, EAccumulationOperation>(stat, op);
			if (!Columns.Contains(kvp))
				Columns.Add(kvp);
		}

		public string ToCSV(string[] headers)
		{
			Columns.Sort((a, b) => {
				bool aUS = a.Key.Contains('_');
				bool bUS = b.Key.Contains('_');
				if (aUS && !bUS)
					return 1;
				if (!aUS && bUS)
					return -1;

				int compareName = a.Key.CompareTo(b.Key);
				if (compareName == 0)
					return a.Value.CompareTo(b.Value);
				return compareName;
			});


			StringBuilder csv = new StringBuilder();
			for (int i = 0; i < MaxChildDepth; i++)
				csv.Append(headers[i]).Append(',');
			for (int i = 0; i < Columns.Count; i++)
				csv.Append(Columns[i].Key).Append(',');
			csv.Append("\r\n");

			for (int i = 0; i < MaxChildDepth; i++)
				csv.Append("").Append(',');
			for (int i = 0; i < Columns.Count; i++)
				csv.Append(Columns[i].Value.ToShortString()).Append(',');
			csv.Append("\r\n");

			IterativeExport(csv, Root); 

			return csv.ToString();
		}

		public void IterativeExport(StringBuilder csv, RowData rowData)
		{
			if (rowData.values.Count > 0)
			{
				for (int i = 0; i < MaxChildDepth; i++)
				{
					if (i < IDs.Count)
						csv.Append(IDs[i]).Append(',');
					else
						csv.Append(',');
				}
				for (int i = 0; i < Columns.Count; i++)
				{
					if (rowData.values.TryGetValue(Columns[i], out string value))
					{
						csv.Append(value).Append(',');
					}
					else
					{
						csv.Append(',');
					}
				}
				csv.Append("\r\n");
			}

			foreach (var kvp in rowData.subRows)
			{
				PushID(kvp.Key);
				IterativeExport(csv, kvp.Value);
				PopID();
			}
		}

		public void FromCSV(string csv)
		{
			CSVReader reader = new CSVReader(csv, ',', '\n');

			// Step 1 - Parse the headers
			if (reader.ReadLine(out string[] headers) && reader.ReadLine(out string[] subheaders))
			{
				for(int i = MaxChildDepth; i < headers.Length; i++)
				{
					Columns.Add(new KeyValuePair<string, EAccumulationOperation>(headers[i], FromShortString(subheaders[i])));
				}
			}
			
			while(reader.ReadLine(out string[] values))
			{
				int idDepth = 0;
				for(int i = 0; i < MaxChildDepth; i++)
				{
					string id = values[i];
					if (id.Length > 0)
					{
						PushID(id);
						idDepth++;
					}
				}

				for(int i = MaxChildDepth; i < values.Length; i++)
				{
					if(values[i].Length != 0)
						AddValue(Columns[i - MaxChildDepth].Key, Columns[i - MaxChildDepth].Value, values[i]);
				}

				for (int i = 0; i < idDepth; i++)
					PopID();
			}
		}

		private bool TryGetRowData(string[] path, out RowData rowData)
		{
			rowData = Root;
			for (int i = 0; i < path.Length; i++)
			{
				if (rowData.TryGetRow(path[i], out RowData nextRow))
					rowData = nextRow;
				else
				{
					rowData = null;
					return false;
				}
			}
			return true;
		}
		public int GetIntOrDefault(string[] path, string stat, EAccumulationOperation op, int defaultValue = 0)
		{
			if (TryGetInt(path, stat, op, out int result))
				return result;
			return defaultValue;
		}
		public bool TryGetInt(string[] path, string stat, EAccumulationOperation op, out int result)
		{
			if (TryGetRowData(path, out RowData row))
				return row.TryGetInt(stat, op, out result);

			result = 0;
			return false;
		}
		public float GetFloatOrDefault(string[] path, string stat, EAccumulationOperation op, float defaultValue = 0)
		{
			if (TryGetFloat(path, stat, op, out float result))
				return result;
			return defaultValue;
		}
		public bool TryGetFloat(string[] path, string stat, EAccumulationOperation op, out float result)
		{
			if (TryGetRowData(path, out RowData row))
				return row.TryGetFloat(stat, op, out result);

			result = 0f;
			return false;
		}
		public bool GetBoolOrDefault(string[] path, string stat, bool defaultValue = false)
		{
			return TryGetRowData(path, out RowData row) && row.TryGetString(stat, out string value) && bool.TryParse(value, out defaultValue);
		}
		public bool TryGetString(string[] path, string stat, out string result)
		{
			if (TryGetRowData(path, out RowData row))
				return row.TryGetString(stat, out result);

			result = "";
			return false;
		}
		public Dictionary<string, ModifierDefinition> GetModifierArrayFor(string[] path)
		{
			Dictionary<string, ModifierDefinition> map = new Dictionary<string, ModifierDefinition>();
			if (TryGetRowData(path, out RowData row))
			{
				foreach(var kvp in row.values)
				{
					string stat = kvp.Key.Key;

					// Don't import specifics of certain actions
					if (stat.Contains(":"))
						continue;

					// Don't import default names as modifiers (things like "mode", "2h" etc.)
					if (NonModifierNames.Contains(stat))
						continue;

					EAccumulationOperation op = kvp.Key.Value;
					bool isFloat = TryParseValue(kvp.Value, out float value, out EAccumulationSource[] sources);

					if (isFloat)
					{
						if (!map.ContainsKey(stat))
						{
							map[stat] = new ModifierDefinition()
							{
								stat = stat,
							};
						}

						List<StatAccumulatorDefinition> accumulators = new List<StatAccumulatorDefinition>(map[stat].accumulators);
						bool matched = false;
						foreach (StatAccumulatorDefinition accumulator in accumulators)
						{
							bool sameSources = Compare(accumulator.multiplyPer, sources);
							bool sameOp = accumulator.operation == op;
							if (sameSources && sameOp)
							{
								Debug.LogWarning($"Same accumulator appeared twice in csv {path}/{stat}/{kvp.Value}");
								accumulator.value += value;
								matched = true;
								break;
							}
						}
						if (!matched)
						{
							accumulators.Add(new StatAccumulatorDefinition()
							{
								operation = op,
								multiplyPer = sources,
								value = value,
							});
						}
						map[stat].accumulators = accumulators.ToArray();
					}
					else
					{
						// We could not parse the floats, so it must be a string
						map[stat] = new ModifierDefinition()
						{
							stat = stat,
							setValue = kvp.Value,
						};
					}
				}
			}
			return map;
		}
		public bool Compare(EAccumulationSource[] a, EAccumulationSource[] b)
		{
			for(int i = 0; i < AccumulationSources.NUM_SOURCES; i++)
			{
				if (a.Contains((EAccumulationSource)i) != b.Contains((EAccumulationSource)i))
					return false;
			}
			return true;
		}
		public static readonly Regex FloatAndMulRegex = new Regex("(\\-?[0-9]+\\.?[0-9]*)([LSAFE]*)");
		public bool TryParseValue(string value, out float result, out EAccumulationSource[] sources)
		{
			try
			{
				Match match = FloatAndMulRegex.Match(value);
				if(match.Success)
				{
					result = float.Parse(match.Groups[1].Value);
					string sourcesString = match.Groups[2].Value;
					List<EAccumulationSource> srcs = new List<EAccumulationSource>();
					if (sourcesString.Contains("L"))
						srcs.Add(EAccumulationSource.PerLevel);
					if (sourcesString.Contains("S"))
						srcs.Add(EAccumulationSource.PerStacks);
					if (sourcesString.Contains("A"))
						srcs.Add(EAccumulationSource.PerAttachment);
					if (sourcesString.Contains("F"))
						srcs.Add(EAccumulationSource.PerMagFullness);
					if (sourcesString.Contains("E"))
						srcs.Add(EAccumulationSource.PerMagEmptiness);
					sources = srcs.ToArray();
					return true;
				}
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}

			result = 0.0f;
			sources = new EAccumulationSource[0];
			return false;
		}
	}

	public static void ImportFromGSheet()
    {

    }

    public static void ExportFromGSheet()
    {

    }

    public static void ImportFromCSV()
    {

    }


	private static void ImportSheet(Sheet sheet, string path)
	{
		if(File.Exists(path))
		{
			sheet.FromCSV(File.ReadAllText(path));
		}
	}
	private static void ExportSheet(Sheet sheet, string[] headers, string path)
	{
		string output = sheet.ToCSV(headers);
		File.WriteAllText(path, output);
		Debug.Log($"Exported balance CSV at {path}");
	}

	public static void ImportFromCSV(ContentPack pack)
	{
		Sheet gunsSheet = new Sheet(2); // ChildDepth 2 = "gun_asset_name"/"action_group_path"
		string csvFolder = CsvExportFolderForPack(pack.name);
		if (Directory.Exists(csvFolder))
		{
			ImportSheet(gunsSheet, $"{csvFolder}/guns.csv");
		}

		foreach (Definition def in pack.AllContent)
		{
			if (def is GunDefinition gunDef)
			{
				string[] gunPath = new string[] { def.name };
				gunDef.barrelAttachments.numAttachmentSlots = gunsSheet.GetIntOrDefault(gunPath, "#barrel", EAccumulationOperation.BaseAdd, 0);
				gunDef.scopeAttachments.numAttachmentSlots = gunsSheet.GetIntOrDefault(gunPath, "#sights", EAccumulationOperation.BaseAdd, 0);
				gunDef.gripAttachments.numAttachmentSlots = gunsSheet.GetIntOrDefault(gunPath, "#grips", EAccumulationOperation.BaseAdd, 0);
				gunDef.stockAttachments.numAttachmentSlots = gunsSheet.GetIntOrDefault(gunPath, "#stocks", EAccumulationOperation.BaseAdd, 0);
				gunDef.genericAttachments.numAttachmentSlots = gunsSheet.GetIntOrDefault(gunPath, "#generics", EAccumulationOperation.BaseAdd, 0);

				foreach (ActionGroupDefinition actionGroup in gunDef.actionGroups)
				{
					string[] actionGroupPath = new string[] { def.name, actionGroup.key };
	

					if (gunsSheet.TryGetString(actionGroupPath, "mode", out string value) && Enum.TryParse(value, out ERepeatMode mode))
						actionGroup.repeatMode = mode;
					actionGroup.repeatDelay = gunsSheet.GetFloatOrDefault(actionGroupPath, "repeatDelay", EAccumulationOperation.BaseAdd, 0.05f);
					actionGroup.repeatCount = gunsSheet.GetIntOrDefault(actionGroupPath, "repeatCount", EAccumulationOperation.BaseAdd, 0);
					actionGroup.loudness = gunsSheet.GetFloatOrDefault(actionGroupPath, "loudness", EAccumulationOperation.BaseAdd, 100f);
					actionGroup.loudness = gunsSheet.GetFloatOrDefault(actionGroupPath, "loudness", EAccumulationOperation.BaseAdd, 100f);
					actionGroup.twoHanded = gunsSheet.GetBoolOrDefault(actionGroupPath, "2h", false);
					actionGroup.canBeOverriden = gunsSheet.GetBoolOrDefault(actionGroupPath, "canOverride", false);
					actionGroup.canActUnderwater = gunsSheet.GetBoolOrDefault(actionGroupPath, "underwater", false);
					actionGroup.canActUnderOtherLiquid = gunsSheet.GetBoolOrDefault(actionGroupPath, "underliquid", false);


					foreach (ActionDefinition action in actionGroup.actions)
					{
						if (gunsSheet.TryGetFloat(actionGroupPath, $"{action.actionType}:duration", EAccumulationOperation.BaseAdd, out float duration))
							action.duration = duration;
						if (gunsSheet.TryGetString(actionGroupPath, $"{action.actionType}:anim", out string anim))
							action.anim = anim;
						if (gunsSheet.TryGetString(actionGroupPath, $"{action.actionType}:scope", out string scope))
							action.scopeOverlay = scope;
						if (gunsSheet.TryGetString(actionGroupPath, $"{action.actionType}:itemStack", out string itemStack))
							action.itemStack = itemStack;

						foreach (SoundDefinition sound in action.sounds)
						{
							if (gunsSheet.TryGetString(actionGroupPath, $"{action.actionType}:sfxID", out string sfxID))
								sound.sound = new ResourceLocation(sfxID);
							if (gunsSheet.TryGetFloat(actionGroupPath, $"{action.actionType}:sfxLength", EAccumulationOperation.BaseAdd, out float length))
								sound.length = length;
							if (gunsSheet.TryGetFloat(actionGroupPath, $"{action.actionType}:sfxMinPitch", EAccumulationOperation.BaseAdd, out float minPitchMultiplier))
								sound.minPitchMultiplier = minPitchMultiplier;
							if (gunsSheet.TryGetFloat(actionGroupPath, $"{action.actionType}:sfxMaxPitch", EAccumulationOperation.BaseAdd, out float maxPitchMultiplier))
								sound.maxPitchMultiplier = maxPitchMultiplier;
							if (gunsSheet.TryGetFloat(actionGroupPath, $"{action.actionType}:sfxMinVolume", EAccumulationOperation.BaseAdd, out float minVolume))
								sound.minVolume = minVolume;
							if (gunsSheet.TryGetFloat(actionGroupPath, $"{action.actionType}:sfxMaxVolume", EAccumulationOperation.BaseAdd, out float maxVolume))
								sound.maxVolume = maxVolume;
							if (gunsSheet.TryGetFloat(actionGroupPath, $"{action.actionType}:sfxRange", EAccumulationOperation.BaseAdd, out float maxRange))
								sound.maxRange = maxRange;
						}
					}

					var modifiers = gunsSheet.GetModifierArrayFor(actionGroupPath);
					actionGroup.modifiers = modifiers.Values.ToArray();
				}
				EditorUtility.SetDirty(def);
			}
		}
	}

	public static readonly List<string> NonModifierNames = new List<string>(
		new string[] { 
			"mode",
			"repeatDelay",
			"repeatCount",
			"loudness",
			"2h",
			"canOverride",
			"underwater",
			"underliquid",
			
			
			//"duration",
			//"anim",
			//"scope",
			//"itemStack",

	});
	public static void ExportToCSV(ContentPack pack)
	{
		Sheet gunsSheet = new Sheet(2); // ChildDepth 2 = "gun_asset_name"/"action_group_path"
		string csvFolder = CsvExportFolderForPack(pack.name);
		if (!Directory.Exists(csvFolder))
			Directory.CreateDirectory(csvFolder);
		foreach (Definition def in pack.AllContent)
		{
			if (def is GunDefinition gunDef)
			{
				gunsSheet.PushID(gunDef.name);

				gunsSheet.AddValue("#barrel", EAccumulationOperation.BaseAdd, gunDef.barrelAttachments.numAttachmentSlots);
				gunsSheet.AddValue("#sights", EAccumulationOperation.BaseAdd, gunDef.scopeAttachments.numAttachmentSlots);
				gunsSheet.AddValue("#grips", EAccumulationOperation.BaseAdd, gunDef.gripAttachments.numAttachmentSlots);
				gunsSheet.AddValue("#stocks", EAccumulationOperation.BaseAdd, gunDef.stockAttachments.numAttachmentSlots);
				gunsSheet.AddValue("#generics", EAccumulationOperation.BaseAdd, gunDef.genericAttachments.numAttachmentSlots);

				foreach (ActionGroupDefinition actionGroup in gunDef.actionGroups)
				{
					gunsSheet.PushID(actionGroup.key);

					gunsSheet.AddValue("mode", actionGroup.repeatMode.ToString());
					gunsSheet.AddValue("repeatDelay", EAccumulationOperation.BaseAdd, actionGroup.repeatDelay);
					if(actionGroup.repeatCount != 0)
						gunsSheet.AddValue("repeatCount", EAccumulationOperation.BaseAdd, actionGroup.repeatCount);
					gunsSheet.AddValue("loudness", EAccumulationOperation.BaseAdd, actionGroup.loudness);
					gunsSheet.AddValue("2h", actionGroup.twoHanded.ToString());
					gunsSheet.AddValue("canOverride", actionGroup.canBeOverriden.ToString());
					gunsSheet.AddValue("underwater", actionGroup.canActUnderwater.ToString());
					gunsSheet.AddValue("underliquid", actionGroup.canActUnderOtherLiquid.ToString());

					foreach (ActionDefinition action in actionGroup.actions)
					{
						if(action.duration > 0.0f)
							gunsSheet.AddValue($"{action.actionType}:duration", EAccumulationOperation.BaseAdd, action.duration);
						if(action.anim.Length > 0)
							gunsSheet.AddValue($"{action.actionType}:anim", action.anim);
						if (action.scopeOverlay.Length > 0)
							gunsSheet.AddValue($"{action.actionType}:scope", action.scopeOverlay);
						if (action.itemStack.Length > 0)
							gunsSheet.AddValue($"{action.actionType}:itemStack", action.itemStack);

						foreach(SoundDefinition sound in action.sounds)
						{
							gunsSheet.AddValue($"{action.actionType}:sfxID", sound.sound.ToString());
							if (sound.length != 0f)
								gunsSheet.AddValue($"{action.actionType}:sfxLength", EAccumulationOperation.BaseAdd, sound.length);
							if (sound.minPitchMultiplier != 1.0f)
								gunsSheet.AddValue($"{action.actionType}:sfxMinPitch", EAccumulationOperation.BaseAdd, sound.minPitchMultiplier);
							if(sound.maxPitchMultiplier != 1.0f)
								gunsSheet.AddValue($"{action.actionType}:sfxMaxPitch", EAccumulationOperation.BaseAdd, sound.maxPitchMultiplier);
							if(sound.minVolume != 1.0f)
								gunsSheet.AddValue($"{action.actionType}:sfxMinVolume", EAccumulationOperation.BaseAdd, sound.minVolume);
							if(sound.maxVolume != 1.0f)
								gunsSheet.AddValue($"{action.actionType}:sfxMaxVolume", EAccumulationOperation.BaseAdd, sound.maxVolume);
							if (sound.maxRange != 100f)
								gunsSheet.AddValue($"{action.actionType}:sfxRange", EAccumulationOperation.BaseAdd, sound.maxRange);
						}
					}

					foreach (ModifierDefinition mod in actionGroup.modifiers)
					{
						if (mod.setValue.Length > 0)
						{
							gunsSheet.AddValue(mod.stat, EAccumulationOperation.BaseAdd, mod.setValue);
						}
						else
						{
							foreach (StatAccumulatorDefinition accumulator in mod.accumulators)
							{
								string valueOutput = $"{accumulator.value}";
								foreach (EAccumulationSource source in accumulator.multiplyPer)
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
								gunsSheet.AddValue(mod.stat, accumulator.operation, valueOutput);
							}
						}
					}
					gunsSheet.PopID();
				}

				gunsSheet.PopID();
			}
		}

		ExportSheet(gunsSheet, new string[] { "gun_name", "action_group", "type" }, $"{csvFolder}/guns.csv");



			/*
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

			*/


		//foreach (var defDictPair in output)
		//{
		//	if (defDictPair.Value.Count > 0)
		//	{
		//		
		//		string csv = "asset_id,";
		//		foreach (string column in columnHeaders[defDictPair.Key])
		//		{
		//			csv += $"{column},";
		//		}
		//		csv += "\r\n";
		//
		//		foreach (var assetIDValuesPair in defDictPair.Value)
		//		{
		//			csv += $"{assetIDValuesPair.Key},";
		//			foreach (string column in columnHeaders[defDictPair.Key])
		//			{
		//				if (assetIDValuesPair.Value.ContainsKey(column))
		//				{
		//					csv += $"{assetIDValuesPair.Value[column]},";
		//				}
		//				else csv += ",";
		//			}
		//			csv += "\r\n";
		//		}
		//
		//		File.WriteAllText(outputPath, csv);
		//	}
		//}

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

    public static void ExportAsset(Definition def, Dictionary<string, string> dict)
    {
        //if(def is GunDefinition gunDef)
        //{
		//	if (gunDef.TryExportMulParam("primary_fire", EActionType.Shoot, "impact_damage", out float impactDamage))
		//		dict.Add("Hit DMG", $"{impactDamage}");
        //}
    }

	public static readonly string[,] GunHeaders = new string[,] {
		{ "primary_fire/impact_damage", "Impact DMG" },
		{ "primary_fire/vertical_recoil", "V. Recoil" },
		{ "primary_fire/mode", "Fire Mode" },
		{ "primary_fire/repeat_delay", "Frequency" },
	};
}

public static class DefinitionHelpers
{

}
