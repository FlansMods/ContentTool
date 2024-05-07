using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static Spreadsheet;

public class Spreadsheet
{
	public enum CSVSubHeading
	{
		Unmodded,
		Set,
		Base,
		Stack,
		Indep,
		Final,
	}

	public static CSVSubHeading Heading(EAccumulationOperation op)
	{
		switch (op)
		{
			case EAccumulationOperation.BaseAdd:				return CSVSubHeading.Base;
			case EAccumulationOperation.StackablePercentage:	return CSVSubHeading.Stack;
			case EAccumulationOperation.IndependentPercentage:	return CSVSubHeading.Indep;
			case EAccumulationOperation.FinalAdd:				return CSVSubHeading.Final;
			default: return CSVSubHeading.Set;
		}
	}
	public static EAccumulationOperation Operation(CSVSubHeading heading)
	{
		switch (heading)
		{
			case CSVSubHeading.Base:	return EAccumulationOperation.BaseAdd;
			case CSVSubHeading.Stack:	return EAccumulationOperation.StackablePercentage;
			case CSVSubHeading.Indep:	return EAccumulationOperation.IndependentPercentage;
			case CSVSubHeading.Final:	return EAccumulationOperation.FinalAdd;
			default:					return EAccumulationOperation.BaseAdd;
		}
	}

	private class RowData
	{
		public string id = "";
		public Dictionary<string, RowData> subRows = new Dictionary<string, RowData>();
		public Dictionary<KeyValuePair<string, CSVSubHeading>, string> values = new Dictionary<KeyValuePair<string, CSVSubHeading>, string>();

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
		public void AddValue(string stat, string value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			values[new KeyValuePair<string, CSVSubHeading>(stat, heading)] = value;
		}
		public bool TryGetString(string stat, out string result, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return values.TryGetValue(new KeyValuePair<string, CSVSubHeading>(stat, heading), out result);
		}
		public bool TryGetFloat(string stat, out float result, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (values.TryGetValue(new KeyValuePair<string, CSVSubHeading>(stat, heading), out string value))
			{
				return float.TryParse(value, out result);
			}
			result = 0.0f;
			return false;
		}
		public bool TryGetInt(string stat, out int result, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (values.TryGetValue(new KeyValuePair<string, CSVSubHeading>(stat, heading), out string value))
			{
				return int.TryParse(value, out result);
			}
			result = 0;
			return false;
		}
		public Dictionary<CSVSubHeading, string> ReadAllAccumulators(string stat)
		{
			Dictionary<CSVSubHeading, string> dict = new Dictionary<CSVSubHeading, string>();
			foreach(var kvp in values)
			{
				if(kvp.Key.Key == stat)
				{
					dict.Add(kvp.Key.Value, kvp.Value);
				}
			}
			return dict;
		}
	}

	// ----------------------------------------------------------------------------------------
	#region Writer
	// ----------------------------------------------------------------------------------------
	public class ValueWriter
	{
		private Spreadsheet Sheet;
		private List<string> IDStack = new List<string>();
		public ValueWriter(Spreadsheet sheet)
		{
			Sheet = sheet;
		}
		public void PushID(string id) { IDStack.Add(id); }
		public void PopID() { IDStack.RemoveAt(IDStack.Count - 1); }

		public void Write(string stat, string value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			RowData rowData = Sheet.Root;
			foreach (string id in IDStack)
				rowData = rowData.GetOrCreate(id);

			rowData.AddValue(stat, value, heading);
			var kvp = new KeyValuePair<string, CSVSubHeading>(stat, heading);
			if (!Sheet.Columns.Contains(kvp))
				Sheet.Columns.Add(kvp);
		}
		public void Write(string stat, bool value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			Write(stat, value.ToString(), heading);
		}
		public void Write<TEnum>(string stat, TEnum value, CSVSubHeading heading = CSVSubHeading.Unmodded) where TEnum : struct
		{
			Write(stat, value.ToString(), heading);
		}
		public void Write(string stat, float value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			Write(stat, $"{value}", heading);
		}
	}
	#endregion
	// ----------------------------------------------------------------------------------------

	// ----------------------------------------------------------------------------------------
	#region Reader
	// ----------------------------------------------------------------------------------------
	public class ValueReader
	{
		private Spreadsheet Sheet;
		private Stack<RowData> CurrentRow = new Stack<RowData>();
		public ValueReader(Spreadsheet sheet)
		{
			Sheet = sheet;
			CurrentRow.Push(Sheet.Root);
		}
		public void PushID(string id)
		{
			if (CurrentRow.Peek().TryGetRow(id, out RowData childRow))
				CurrentRow.Push(childRow);
			else
				Debug.LogError($"Could not get row with ID '{id}' on top of stack: {CurrentRow}");
		}
		public void PopID()
		{
			CurrentRow.Pop();
			if (CurrentRow.Count == 0)
			{
				Debug.LogError($"Popped entire row stack!");
				CurrentRow.Push(Sheet.Root);
			}
		}
		public int ReadInt(string stat, int defaultValue = 0, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return TryReadInt(stat, out int result, heading) ? result : defaultValue;
		}
		public bool TryReadInt(string stat, out int value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return CurrentRow.Peek().TryGetInt(stat, out value, heading);
		}
		public float ReadFloat(string stat, float defaultValue = 0, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return TryReadFloat(stat, out float result, heading) ? result : defaultValue;
		}
		public bool TryReadFloat(string stat, out float value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return CurrentRow.Peek().TryGetFloat(stat, out value, heading);
		}
		//public Dictionary<EAccumulationOperation, float> ReadAllFloats(string stat)
		//{
		//	Dictionary<EAccumulationOperation, float> dict = new Dictionary<EAccumulationOperation, float>();
		//	if (TryReadFloat(stat, EAccumulationOperation.BaseAdd, out float baseAdd))
		//		dict.Add(EAccumulationOperation.BaseAdd, baseAdd);
		//	return dict;
		//}
		public Dictionary<CSVSubHeading, string> ReadAllAccumulators(string stat)
		{
			return CurrentRow.Peek().ReadAllAccumulators(stat);
		}
		public string ReadString(string stat, string defaultValue = "", CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return TryReadString(stat, out string result, heading) ? result : defaultValue;
		}
		public bool TryReadString(string stat, out string value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return CurrentRow.Peek().TryGetString(stat, out value, heading);
		}
		public bool ReadBool(string stat, bool defaultValue = false, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			return TryReadString(stat, out string stringValue, heading) && bool.TryParse(stringValue, out bool boolValue) ? boolValue : defaultValue;
		}
		public bool TryReadBool(string stat, out bool value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (TryReadString(stat, out string stringValue, heading) && bool.TryParse(stringValue, out value))
				return true;
			value = false;
			return false;
		}
		public void Read(string stat, ref float value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (TryReadFloat(stat, out float result, heading))
				value = result;
		}
		public void Read(string stat, ref int value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (TryReadInt(stat, out int result, heading))
				value = result;
		}
		public void Read(string stat, ref bool value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (TryReadBool(stat, out bool result, heading))
				value = result;
		}
		public void Read(string stat, ref string value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (TryReadString(stat, out string result, heading))
				value = result;
		}
		public void Read(string stat, ref ResourceLocation value, CSVSubHeading heading = CSVSubHeading.Unmodded)
		{
			if (TryReadString(stat, out string result, heading))
			{
				value = new ResourceLocation(result);
			}
		}
		public void Read<TEnum>(string stat, ref TEnum value, CSVSubHeading heading = CSVSubHeading.Unmodded) where TEnum : struct
		{
			if (TryReadString(stat, out string result, heading))
			{
				value = Enum.Parse<TEnum>(result);
			}
		}
	}
	#endregion
	// ----------------------------------------------------------------------------------------


	private int MaxChildDepth;
	public Spreadsheet(int maxChildDepth)
	{
		MaxChildDepth = maxChildDepth;
	}

	private List<KeyValuePair<string, CSVSubHeading>> Columns = new List<KeyValuePair<string, CSVSubHeading>>();

	// "Rows" are nested by each of our key columns
	// i.e. Root[key1][key2] = RowData{ a, b, c, d } 
	private RowData Root = new RowData();

	// ----------------------------------------------------------------------------------------
	#region Export to CSV
	// ----------------------------------------------------------------------------------------
	public string ExportToCSV(string[] headers)
	{
		Columns.Sort((a, b) => {
			bool aUS = a.Key.Contains(':');
			bool bUS = b.Key.Contains(':');
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
			csv.Append(Columns[i].Value.ToString()).Append(',');
		csv.Append("\r\n");

		IterativeExport(csv, new List<string>(), Root);

		return csv.ToString();
	}
	private void IterativeExport(StringBuilder csv, List<string> rowPath, RowData rowData)
	{
		if (rowData.values.Count > 0)
		{
			for (int i = 0; i < MaxChildDepth; i++)
			{
				if (i < rowPath.Count)
					csv.Append(rowPath[i]).Append(',');
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
			rowPath.Add(kvp.Key);
			IterativeExport(csv, rowPath, kvp.Value);
			rowPath.RemoveAt(rowPath.Count - 1);
		}
	}
	#endregion
	// ----------------------------------------------------------------------------------------


	// ----------------------------------------------------------------------------------------
	#region Import from CSV
	// ----------------------------------------------------------------------------------------
	public void ReadFromCSV(string csv)
	{
		CSVReader reader = new CSVReader(csv, ',', '\n');

		// Step 1 - Parse the headers
		if (reader.ReadLine(out string[] headers) && reader.ReadLine(out string[] subheaders))
		{
			for (int i = MaxChildDepth; i < headers.Length; i++)
			{
				Columns.Add(new KeyValuePair<string, CSVSubHeading>(headers[i], Enum.Parse<CSVSubHeading>(subheaders[i])));
			}
		}

		while (reader.ReadLine(out string[] values))
		{
			RowData intoRow = Root;
			for (int i = 0; i < MaxChildDepth; i++)
			{
				string id = values[i];
				if (id.Length > 0)
				{
					intoRow = intoRow.GetOrCreate(id);
				}
			}

			for (int i = MaxChildDepth; i < values.Length; i++)
			{
				if (values[i].Length != 0)
					intoRow.AddValue(Columns[i - MaxChildDepth].Key, values[i], Columns[i - MaxChildDepth].Value);
			}
		}
	}
	#endregion
	// ----------------------------------------------------------------------------------------
}


public static class SpreadsheetExtensions
{
	// ------------------------------------------------------------------------------------------
	// BulletDefinition
	public static void Read(this Spreadsheet.ValueReader reader, BulletDefinition bullet)
	{
		reader.PushID(bullet.name);
		foreach (AbilityDefinition abilityDef in bullet.triggers)
		{
			reader.Read(abilityDef);
		}
		foreach (ActionGroupDefinition actionGroup in bullet.actionGroups)
		{
			reader.Read(actionGroup);
		}
		reader.PopID();
	}
	public static void Write(this Spreadsheet.ValueWriter writer, BulletDefinition bullet)
	{
		writer.PushID(bullet.name);
		foreach (AbilityDefinition abilityDef in bullet.triggers)
		{
			writer.Write(abilityDef);
		}
		foreach (ActionGroupDefinition actionGroup in bullet.actionGroups)
		{
			writer.Write(actionGroup);
		}
		writer.PopID();
	}

	// ------------------------------------------------------------------------------------------
	// AttachmentDefinition
	public static void Read(this Spreadsheet.ValueReader reader, AttachmentDefinition attachment)
	{
		reader.PushID(attachment.name);
		foreach (ModifierDefinition mod in attachment.modifiers)
		{
			reader.Read(mod);
		}
		foreach (ActionGroupDefinition actionGroup in attachment.actionOverrides)
		{
			reader.Read(actionGroup);
		}
		reader.PopID();
	}
	public static void Write(this Spreadsheet.ValueWriter writer, AttachmentDefinition attachment)
	{
		writer.PushID(attachment.name);
		foreach (ModifierDefinition mod in attachment.modifiers)
		{
			writer.Write(mod);
		}
		foreach (ActionGroupDefinition actionGroup in attachment.actionOverrides)
		{
			writer.Write(actionGroup);
		}
		writer.PopID();
	}

	// ------------------------------------------------------------------------------------------
	// GunDefinition
	public static void Read(this ValueReader reader, GunDefinition gun)
	{
		reader.PushID(gun.name);
		reader.Read("#barrel", ref gun.barrelAttachments.numAttachmentSlots);
		reader.Read("#sights", ref gun.scopeAttachments.numAttachmentSlots);
		reader.Read("#grips", ref gun.gripAttachments.numAttachmentSlots);
		reader.Read("#stocks", ref gun.stockAttachments.numAttachmentSlots);
		reader.Read("#generics", ref gun.genericAttachments.numAttachmentSlots);

		foreach (ActionGroupDefinition actionGroup in gun.actionGroups)
		{
			reader.PushID(actionGroup.key);
			reader.Read("mode", ref actionGroup.repeatMode);
			reader.Read("repeatDelay", ref actionGroup.repeatDelay);
			reader.Read("repeatCount", ref actionGroup.repeatCount);
			reader.Read("loudness", ref actionGroup.loudness);
			reader.Read("2h", ref actionGroup.twoHanded);
			reader.Read("canOverride", ref actionGroup.canBeOverriden);
			reader.Read("underwater", ref actionGroup.canActUnderwater);
			reader.Read("underliquid", ref actionGroup.canActUnderOtherLiquid);

			Dictionary<EActionType, int> actionCount = new Dictionary<EActionType, int>();
			foreach (ActionDefinition action in actionGroup.actions)
				actionCount[action.actionType] = actionCount.GetValueOrDefault(action.actionType, 0) + 1;

			foreach (ActionDefinition action in actionGroup.actions)
			{
				string importActionKey = $"{action.actionType}";
				if (actionCount.GetValueOrDefault(action.actionType, 1) > 1)
				{
					importActionKey += $"_{actionCount[action.actionType]}";
					actionCount[action.actionType]--;
				}

				reader.Read($"{action.actionType}:duration", ref action.duration);
				reader.Read($"{action.actionType}:anim", ref action.anim);
				reader.Read($"{action.actionType}:scope", ref action.scopeOverlay);
				reader.Read($"{action.actionType}:itemStack", ref action.itemStack);

				foreach (SoundDefinition sound in action.sounds)
				{
					reader.Read($"{action.actionType}:sfxID", ref sound.sound);
					reader.Read($"{action.actionType}:sfxLength", ref sound.length);
					reader.Read($"{action.actionType}:sfxMinPitch", ref sound.minPitchMultiplier);
					reader.Read($"{action.actionType}:sfxMaxPitch", ref sound.maxPitchMultiplier);
					reader.Read($"{action.actionType}:sfxMinVolume", ref sound.minVolume);
					reader.Read($"{action.actionType}:sfxMaxVolume", ref sound.maxVolume);
					reader.Read($"{action.actionType}:sfxRange", ref sound.maxRange);
				}
			}

			foreach (ModifierDefinition mod in actionGroup.modifiers)
			{
				reader.Read(mod);
			}

			reader.PopID();
		}

		reader.PopID();
	}
	public static void Write(this ValueWriter writer, GunDefinition gun)
	{
		writer.PushID(gun.name); 
		writer.Write("#barrel", gun.barrelAttachments.numAttachmentSlots);
		writer.Write("#sights", gun.scopeAttachments.numAttachmentSlots);
		writer.Write("#grips", gun.gripAttachments.numAttachmentSlots);
		writer.Write("#stocks", gun.stockAttachments.numAttachmentSlots);
		writer.Write("#generics", gun.genericAttachments.numAttachmentSlots);

		foreach (ActionGroupDefinition actionGroup in gun.actionGroups)
		{
			writer.PushID(actionGroup.key);
			writer.Write("mode", actionGroup.repeatMode);
			writer.Write("repeatDelay", actionGroup.repeatDelay);
			writer.Write("repeatCount", actionGroup.repeatCount);
			writer.Write("loudness", actionGroup.loudness);
			writer.Write("2h", actionGroup.twoHanded);
			writer.Write("canOverride", actionGroup.canBeOverriden);
			writer.Write("underwater", actionGroup.canActUnderwater);
			writer.Write("underliquid", actionGroup.canActUnderOtherLiquid);

			Dictionary<EActionType, int> actionCount = new Dictionary<EActionType, int>();
			foreach (ActionDefinition action in actionGroup.actions)
				actionCount[action.actionType] = actionCount.GetValueOrDefault(action.actionType, 0) + 1;

			foreach (ActionDefinition action in actionGroup.actions)
			{
				int indexOfActionByType = actionCount.GetValueOrDefault(action.actionType, 1);
				if (indexOfActionByType > 1)
				{
					writer.PushID($"{indexOfActionByType}");
					actionCount[action.actionType]--;
				}

				if (action.duration > 0.0f)
					writer.Write($"{action.actionType}:duration", action.duration);
				if (action.anim.Length > 0)
					writer.Write($"{action.actionType}:anim", action.anim);
				if (action.scopeOverlay.Length > 0)
					writer.Write($"{action.actionType}:scope", action.scopeOverlay);
				if (action.itemStack.Length > 0)
					writer.Write($"{action.actionType}:itemStack", action.itemStack);

				foreach (SoundDefinition sound in action.sounds)
				{
					writer.Write($"{action.actionType}:sfxID", sound.sound);
					writer.Write($"{action.actionType}:sfxLength", sound.length);
					writer.Write($"{action.actionType}:sfxMinPitch", sound.minPitchMultiplier);
					writer.Write($"{action.actionType}:sfxMaxPitch", sound.maxPitchMultiplier);
					writer.Write($"{action.actionType}:sfxMinVolume", sound.minVolume);
					writer.Write($"{action.actionType}:sfxMaxVolume", sound.maxVolume);
					writer.Write($"{action.actionType}:sfxRange", sound.maxRange);
				}

				if (indexOfActionByType > 1)
				{
					writer.PopID();
				}
			}

			foreach(ModifierDefinition mod in actionGroup.modifiers)
			{
				writer.Write(mod);
			}

			writer.PopID();
		}
		
		writer.PopID();
	}


	// ------------------------------------------------------------------------------------------
	// AbilityDefinition
	public static void Read(this ValueReader reader, AbilityDefinition ability)
	{
		reader.PushID(ability.stacking.stackingKey);
		reader.Read("maxStacks", ref ability.stacking.maxStacks);
		reader.Read("decayAllAtOnce", ref ability.stacking.decayAllAtOnce);
		reader.Read("decayTime", ref ability.stacking.decayTime);
		foreach (AbilityEffectDefinition effect in ability.effects)
		{
			foreach (ModifierDefinition mod in effect.modifiers)
			{
				reader.Read(mod);
			}
		}
		reader.PopID();
	}

	public static void Write(this ValueWriter writer, AbilityDefinition ability)
	{
		writer.PushID(ability.stacking.stackingKey);
		writer.Write("maxStacks", ability.stacking.maxStacks);
		if (ability.stacking.decayAllAtOnce)
			writer.Write("decayAllAtOnce", ability.stacking.decayAllAtOnce);
		if (ability.stacking.decayTime > 0.0f)
			writer.Write("decayTime", ability.stacking.decayTime);

		foreach (AbilityEffectDefinition effect in ability.effects)
		{
			foreach (ModifierDefinition mod in effect.modifiers)
			{
				writer.Write(mod);
			}
		}

		writer.PopID();
	}

	// ------------------------------------------------------------------------------------------
	// ImpactDefinition
	public static void Read(this ValueReader reader, ImpactDefinition impact)
	{
		//reader.Read("damageToTarget", ref impact.damageToTarget);
		//reader.Read("multiplierVsPlayers", ref impact.multiplierVsPlayers);
		//reader.Read("multiplierVsVehicles", ref impact.multiplierVsVehicles);
		//reader.Read("knockback", ref impact.knockback);
		//reader.Read("potionEffectOnTarget", ref impact.potionEffectOnTarget);
		//reader.Read("splashDamage", ref impact.splashDamage);
		//reader.Read("splashDamageRadius", ref impact.splashDamageRadius);
		//reader.Read("splashDamageFalloff", ref impact.splashDamageFalloff);
		//reader.Read("potionEffectOnSplash", ref impact.potionEffectOnSplash);
		//reader.Read("setFireToTarget", ref impact.setFireToTarget);
		//reader.Read("fireSpreadRadius", ref impact.fireSpreadRadius);
		//reader.Read("fireSpreadAmount", ref impact.fireSpreadAmount);
		//reader.Read("explosionRadius", ref impact.explosionRadius);
	}

	public static void Write(this Spreadsheet.ValueWriter writer, ImpactDefinition impact)
	{
		//writer.Write("damageToTarget", impact.damageToTarget);
		//if (impact.multiplierVsPlayers != 1.0f)
		//	writer.Write("multiplierVsPlayers", impact.multiplierVsPlayers);
		//if (impact.multiplierVsVehicles != 1.0f)
		//	writer.Write("multiplierVsVehicles", impact.multiplierVsVehicles);
		//if (impact.knockback != 0.0f)
		//	writer.Write("knockback", impact.knockback);
		//if (impact.potionEffectOnTarget.Length > 0)
		//	writer.Write("potionEffectOnTarget", impact.potionEffectOnTarget);
		//
		//if (impact.splashDamage != 0.0f)
		//	writer.Write("splashDamage", impact.splashDamage);
		//if (impact.splashDamageRadius != 0.0f)
		//	writer.Write("splashDamageRadius", impact.splashDamageRadius);
		//if (impact.splashDamageFalloff != 0.0f)
		//	writer.Write("splashDamageFalloff", impact.splashDamageFalloff);
		//if (impact.potionEffectOnSplash.Length > 0)
		//	writer.Write("potionEffectOnSplash", impact.potionEffectOnSplash);
		//
		//if (impact.setFireToTarget != 0.0f)
		//	writer.Write("setFireToTarget", impact.setFireToTarget);
		//if (impact.fireSpreadRadius != 0.0f)
		//	writer.Write("fireSpreadRadius", impact.fireSpreadRadius);
		//if (impact.fireSpreadAmount != 0.0f)
		//	writer.Write("fireSpreadAmount", impact.fireSpreadAmount);
		//
		//if (impact.explosionRadius != 0.0f)
		//	writer.Write("explosionRadius", impact.explosionRadius);
	}

	// ------------------------------------------------------------------------------------------
	// SoundDefinition
	public static void Read(this Spreadsheet.ValueReader reader, SoundDefinition sound, string prefix = "")
	{
		reader.Read($"{prefix}sfxID", ref sound.sound);
		reader.Read($"{prefix}sfxLength", ref sound.length);
		reader.Read($"{prefix}sfxMinPitch", ref sound.minPitchMultiplier);
		reader.Read($"{prefix}sfxMaxPitch", ref sound.maxPitchMultiplier);
		reader.Read($"{prefix}sfxMinVolume", ref sound.minVolume);
		reader.Read($"{prefix}sfxMaxVolume", ref sound.maxVolume);
		reader.Read($"{prefix}sfxRange", ref sound.maxRange);
	}
	public static void Write(this Spreadsheet.ValueWriter writer, SoundDefinition sound, string prefix = "")
	{
		writer.Write($"{prefix}sfxID", sound.sound.ToString());
		if (sound.length != 0f)
			writer.Write($"{prefix}sfxLength", sound.length);
		if (sound.minPitchMultiplier != 1.0f)
			writer.Write($"{prefix}sfxMinPitch", sound.minPitchMultiplier);
		if (sound.maxPitchMultiplier != 1.0f)
			writer.Write($"{prefix}sfxMaxPitch", sound.maxPitchMultiplier);
		if (sound.minVolume != 1.0f)
			writer.Write($"{prefix}sfxMinVolume", sound.minVolume);
		if (sound.maxVolume != 1.0f)
			writer.Write($"{prefix}sfxMaxVolume", sound.maxVolume);
		if (sound.maxRange != 100f)
			writer.Write($"{prefix}sfxRange", sound.maxRange);
	}

	// ------------------------------------------------------------------------------------------
	// ShotDefinition
	public static void Read(this Spreadsheet.ValueReader reader, ShotDefinition shot)
	{
		//reader.Read("hitscan", ref shot.hitscan);
		//reader.Read("shotSpeed", ref shot.speed);
		//reader.Read("#shots", ref shot.bulletCount);
		//reader.Read("penetrationPower", ref shot.penetrationPower);
		//reader.Read(shot.impact);
		//
		//if (!shot.hitscan)
		//{
		//	reader.Read("fuseTime", ref shot.fuseTime);
		//	reader.Read("gravityFactor", ref shot.gravityFactor);
		//	reader.Read("sticky", ref shot.sticky);
		//	reader.Read("turnRate", ref shot.turnRate);
		//	reader.Read("dragInAir", ref shot.dragInAir);
		//	reader.Read("secondsBetweenTrailParticles", ref shot.secondsBetweenTrailParticles);
		//	reader.Read("dragInWater", ref shot.dragInWater);
		//}
	}
	public static void Write(this Spreadsheet.ValueWriter writer, ShotDefinition shot)
	{
		//writer.Write("hitscan", shot.hitscan.ToString());
		//writer.Write("shotSpeed", shot.speed);
		//writer.Write("#shots", shot.bulletCount);
		//writer.Write("penetrationPower", shot.penetrationPower);
		//writer.Write(shot.impact);
		//
		//if (!shot.hitscan)
		//{
		//	writer.Write("fuseTime", shot.fuseTime);
		//	writer.Write("gravityFactor", shot.gravityFactor);
		//	writer.Write("sticky", shot.sticky.ToString());
		//	writer.Write("turnRate", shot.turnRate);
		//	writer.Write("dragInAir", shot.dragInAir);
		//	if (shot.trailParticles.Length > 0 || shot.waterParticles.Length > 0)
		//		writer.Write("secondsBetweenTrailParticles", shot.secondsBetweenTrailParticles);
		//	writer.Write("dragInWater", shot.dragInWater);
		//}
	}

	// ------------------------------------------------------------------------------------------
	// ActionGroupDefinition
	public static void Read(this Spreadsheet.ValueReader reader, ActionGroupDefinition actionGroup)
	{
		reader.PushID(actionGroup.key);

		reader.Read("mode", ref actionGroup.repeatMode);
		reader.Read("repeatDelay", ref actionGroup.repeatDelay);
		reader.Read("repeatCount", ref actionGroup.repeatCount);
		reader.Read("loudness", ref actionGroup.loudness);
		reader.Read("2h", ref actionGroup.twoHanded);
		reader.Read("canOverride", ref actionGroup.canBeOverriden);
		reader.Read("underwater", ref actionGroup.canActUnderwater);
		reader.Read("underliquid", ref actionGroup.canActUnderOtherLiquid);

		foreach (ActionDefinition action in actionGroup.actions)
		{
			reader.Read($"{action.actionType}:duration", ref action.duration);
			reader.Read($"{action.actionType}:anim", ref action.anim);
			reader.Read($"{action.actionType}:scope", ref action.scopeOverlay);
			reader.Read($"{action.actionType}:itemStack", ref action.itemStack);

			foreach (SoundDefinition sound in action.sounds)
			{
				reader.Read(sound, $"{action.actionType}:");
			}
		}

		foreach (ModifierDefinition mod in actionGroup.modifiers)
		{
			reader.Read(mod);
		}
		reader.PopID();
	}
	public static void Write(this Spreadsheet.ValueWriter writer, ActionGroupDefinition actionGroup)
	{
		writer.PushID(actionGroup.key);

		writer.Write("mode", actionGroup.repeatMode);
		writer.Write("repeatDelay", actionGroup.repeatDelay);
		if (actionGroup.repeatCount != 0)
			writer.Write("repeatCount", actionGroup.repeatCount);
		writer.Write("loudness", actionGroup.loudness);
		writer.Write("2h", actionGroup.twoHanded.ToString());
		writer.Write("canOverride", actionGroup.canBeOverriden);
		writer.Write("underwater", actionGroup.canActUnderwater);
		writer.Write("underliquid", actionGroup.canActUnderOtherLiquid);

		foreach (ActionDefinition action in actionGroup.actions)
		{
			if (action.duration > 0.0f)
				writer.Write($"{action.actionType}:duration", action.duration);
			if (action.anim.Length > 0)
				writer.Write($"{action.actionType}:anim", action.anim);
			if (action.scopeOverlay.Length > 0)
				writer.Write($"{action.actionType}:scope", action.scopeOverlay);
			if (action.itemStack.Length > 0)
				writer.Write($"{action.actionType}:itemStack", action.itemStack);

			foreach (SoundDefinition sound in action.sounds)
			{
				writer.Write(sound, $"{action.actionType}:");
			}
		}

		foreach (ModifierDefinition mod in actionGroup.modifiers)
		{
			writer.Write(mod);
		}
		writer.PopID();
	}

	// ------------------------------------------------------------------------------------------
	// ModifierDefinition
	public static readonly Regex FloatAndMulRegex = new Regex("(\\-?[0-9]+\\.?[0-9]*)([LSAFE]*)");
	public static bool TryParseFormulaString(string value, EAccumulationOperation op, out StatAccumulatorDefinition result)
	{
		try
		{
			Match match = FloatAndMulRegex.Match(value);
			if (match.Success)
			{
				float coeff = float.Parse(match.Groups[1].Value);
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
				result = new StatAccumulatorDefinition()
				{
					value = coeff,
					multiplyPer = srcs.ToArray(),
					operation = op,
				};
				return true;
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}

		result = new StatAccumulatorDefinition();
		return false;
	}
	public static bool TryParseMultiFormulaString(string value, EAccumulationOperation op, out StatAccumulatorDefinition[] results)
	{
		string[] components = value.Split('+');
		List<StatAccumulatorDefinition> accumulators = new List<StatAccumulatorDefinition>();
		foreach (string component in components)
		{
			if (TryParseFormulaString(component, op, out StatAccumulatorDefinition accum))
				accumulators.Add(accum);
			else
			{
				results = new StatAccumulatorDefinition[0];
				return false;
			}
		}
		results = accumulators.ToArray();
		return true;
	}
	public static void Read(this ValueReader reader, ModifierDefinition mod)
	{
		var accumulatorDict = reader.ReadAllAccumulators(mod.stat);

		List<StatAccumulatorDefinition> accumulators = new List<StatAccumulatorDefinition>();
		foreach(var kvp in accumulatorDict)
		{
			switch (kvp.Key)
			{
				case CSVSubHeading.Set:
					mod.setValue = kvp.Value;
					break;
				case CSVSubHeading.Base:
				case CSVSubHeading.Stack:
				case CSVSubHeading.Indep:
				case CSVSubHeading.Final:
					if (TryParseMultiFormulaString(kvp.Value, Operation(kvp.Key), out StatAccumulatorDefinition[] results))
					{
						accumulators.AddRange(results);
					}
					else
					{
						Debug.LogError($"Could not parse {kvp.Value} for mod {mod.stat}");
					}
					break;
				default: // Skip unmodded settings
					break;
			}
			
		}
	}
	public static void Write(this ValueWriter writer, ModifierDefinition mod)
	{
		if (mod.setValue.Length > 0)
		{
			writer.Write(mod.stat, mod.setValue, CSVSubHeading.Set);
		}
		else
		{
			Dictionary<EAccumulationOperation, string> output = new Dictionary<EAccumulationOperation, string>();
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
				if (output.ContainsKey(accumulator.operation))
				{
					output[accumulator.operation] += $"+{valueOutput}";
				}
				else output.Add(accumulator.operation, valueOutput);
			}

			foreach(var kvp in output)
			{
				writer.Write(mod.stat, kvp.Value, Heading(kvp.Key));
			}
		}
	}
}
