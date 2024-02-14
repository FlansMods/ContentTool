using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CraftingTraitDefinition))]
public class CraftingTraitDefinitionEditor : Editor
{
    private string GetConnective(EAccumulationOperation opType)
    {
		switch(opType)
        {
            case EAccumulationOperation.Constant: return "";
            case EAccumulationOperation.Add: return "+";
            case EAccumulationOperation.Multiply: return "*";
			case EAccumulationOperation.Exponent: return "^";
        }
        return "";
	}

	private string FormatValue(float value, EAccumulationOperation opType)
	{
		switch (opType)
		{
			case EAccumulationOperation.Constant: return $"{value}";
			case EAccumulationOperation.Add: return $"{value}";
			case EAccumulationOperation.Multiply:
				float percent = 100.0f * (value - 1.0f);
				return percent > 0.0f ? $"+{percent}%" : $"{percent}%";
			case EAccumulationOperation.Exponent: return $"{value}";
		}
		return "";
	}

    private int testStackCount = 1;
    private int testLevel = 1;
    private int testAttachCount = 0;
	private int testMagSize = 1;
	private int testBulletCount = 1;

	public override void OnInspectorGUI()
    {
        if(target is CraftingTraitDefinition def)
        {
			List<EAccumulationSource> stackingTypes = new List<EAccumulationSource>();
			int highestStack = 0;
			foreach (AbilityDefinition abilityDef in def.abilities)
			{
				foreach (StatAccumulatorDefinition stackSource in abilityDef.stacking.intensity.additional)
				{
					EAccumulationSource typeToTrack = stackSource.multiplyPer;
					if (typeToTrack == EAccumulationSource.One)
						continue;
					if (typeToTrack == EAccumulationSource.PerMagEmptiness)
						typeToTrack = EAccumulationSource.PerMagFullness;

					if (!stackingTypes.Contains(stackSource.multiplyPer))
						stackingTypes.Add(stackSource.multiplyPer);
				}
				if (abilityDef.stacking.maxStacks > highestStack)
					highestStack = abilityDef.stacking.maxStacks;
			}

			if (stackingTypes.Count > 0)
			{
				FlanStyles.BigHeader("Ability Scaling Tester");
				GUILayout.BeginHorizontal();
				foreach (EAccumulationSource stackingType in stackingTypes)
				{
					switch (stackingType)
					{
						case EAccumulationSource.PerStacks:
							testStackCount = Mathf.Clamp(EditorGUILayout.IntField("Stack Count = ", testStackCount), 0, highestStack);
							break;
						case EAccumulationSource.PerLevel:
							testLevel = Mathf.Clamp(EditorGUILayout.IntField("Ability Level = ", testLevel), 1, def.maxLevel);
							break;
						case EAccumulationSource.PerAttachment:
							testAttachCount = Mathf.Clamp(EditorGUILayout.IntField("# Attachments on Gun = ", testAttachCount), 0, 20);
							break;
						case EAccumulationSource.PerMagFullness:
							testBulletCount = Mathf.Clamp(EditorGUILayout.IntField("Magazine = ", testBulletCount), 0, testMagSize);
							testMagSize = Mathf.Clamp(EditorGUILayout.IntField("/", testMagSize), 1, 1000);
							GUILayout.Label($" ({testBulletCount * 100f / testMagSize}% Full)");
							break;
					}
				}
				GUILayout.EndHorizontal();

				
				foreach (AbilityDefinition abilityDef in def.abilities)
				{
					float decayTime = FormulaGUI("DecayTime (seconds) = ", abilityDef.stacking.decayTime);
					float intensity = FormulaGUI("Intensity = ", abilityDef.stacking.intensity);

					// TODO
					//foreach (AbilityEffectDefinition effectDef in abilityDef.effects)
					//{
					//	foreach (ModifierDefinition modifierDef in effectDef.modifiers)
					//	{
					//		GUILayout.BeginHorizontal();
					//		GUILayout.FlexibleSpace();
					//		
					//		if (modifierDef.setValue.Length > 0)
					//		{
					//			GUILayout.Label($"Set '{modifierDef.stat}' = '{modifierDef.setValue}'");
					//		}
					//		else
					//		{
					//			GUILayout.Label($"Modify '{modifierDef.stat}': ");
					//			if (modifierDef.Add * intensity != 0.0f)
					//				GUILayout.Label($"+ {modifierDef.Add * intensity}");
					//			float mulAmount = (modifierDef.Multiply - 1.0f) * intensity + 1.0f;
					//			if (mulAmount != 1.0f)
					//				GUILayout.Label($"* {mulAmount}");
					//		}
					//		GUILayout.EndHorizontal();
					//	}
					//}
				}
				FlanStyles.BigSpacer();
			}
        }

		base.OnInspectorGUI();
	}

	private float FormulaGUI(string label, FloatStatDefinition sources)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, FlanStyles.BoldLabel);
		GUILayout.FlexibleSpace();
		float value = 0.0f;

		foreach (StatAccumulatorDefinition stackSource in sources.additional)
		{
			float multiplier = 0.0f;
			string valueFormatted = FormatValue(stackSource.value, stackSource.operation);
			switch (stackSource.multiplyPer)
			{
				case EAccumulationSource.One:
					GUILayout.Label($"{GetConnective(stackSource.operation)} {valueFormatted}");
					multiplier = 1.0f;
					break;
				case EAccumulationSource.PerStacks:
					GUILayout.Label($"{GetConnective(stackSource.operation)} ({valueFormatted} * #Stack)");
					multiplier = testStackCount;
					break;
				case EAccumulationSource.PerLevel:
					GUILayout.Label($"{GetConnective(stackSource.operation)} ({valueFormatted} * #Level)");
					multiplier = testLevel;
					break;
				case EAccumulationSource.PerAttachment:
					GUILayout.Label($"{GetConnective(stackSource.operation)} ({valueFormatted} * #Attach)");
					multiplier = testAttachCount;
					break;
				case EAccumulationSource.PerMagFullness:
					GUILayout.Label($"{GetConnective(stackSource.operation)} ({valueFormatted} * MagFullness%)");
					multiplier = (testBulletCount / testMagSize);
					break;
				case EAccumulationSource.PerMagEmptiness:
					GUILayout.Label($"{GetConnective(stackSource.operation)} ({valueFormatted} * MagEmptiness%)");
					multiplier = (1.0f - (testBulletCount / testMagSize));
					break;
			}

			switch (stackSource.operation)
			{
				case EAccumulationOperation.Constant:
					value = stackSource.value * multiplier;
					break;
				case EAccumulationOperation.Add:
					value += stackSource.value * multiplier;
					break;
				case EAccumulationOperation.Multiply:
					value *= (stackSource.value - 1.0f) * multiplier + 1.0f;
					break;
			}
		}

		GUILayout.Label(" = ");

		// Second pass, summarise them
		foreach (StatAccumulatorDefinition stackSource in sources.additional)
		{
			float multiplier = 0.0f;
			float component = 0.0f;
			switch (stackSource.multiplyPer)
			{
				case EAccumulationSource.One:
					multiplier = 1.0f;
					break;
				case EAccumulationSource.PerStacks:
					multiplier = testStackCount;
					break;
				case EAccumulationSource.PerLevel:
					multiplier = testLevel;
					break;
				case EAccumulationSource.PerAttachment:
					multiplier = testAttachCount;
					break;
				case EAccumulationSource.PerMagFullness:
					multiplier = (testBulletCount / testMagSize);
					break;
				case EAccumulationSource.PerMagEmptiness:
					multiplier = (1.0f - (testBulletCount / testMagSize));
					break;
			}

			switch (stackSource.operation)
			{
				case EAccumulationOperation.Constant:
					component = stackSource.value * multiplier;
					break;
				case EAccumulationOperation.Add:
					component = stackSource.value * multiplier;
					break;
				case EAccumulationOperation.Multiply:
					component = (stackSource.value - 1.0f) * multiplier + 1.0f;
					break;
			}

			string componentFormatted = FormatValue(component, stackSource.operation);
			GUILayout.Label($"{GetConnective(stackSource.operation)} {componentFormatted}");
		}

		GUILayout.Label($" = {value}", FlanStyles.BoldLabel);
		GUILayout.EndHorizontal();
		return value;
	}
}
