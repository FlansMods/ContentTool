using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using static Codice.Client.Common.Servers.RecentlyUsedServers;

[CustomEditor(typeof(CraftingTraitDefinition))]
public class CraftingTraitDefinitionEditor : Editor
{
 private string AsPercent(float value)
	{
		float percent = 100.0f * (value - 1.0f);
		return percent > 0.0f ? $"+{percent}%" : $"{percent}%";
	}

	private List<EAccumulationSource> TrackingSources = new List<EAccumulationSource>();
	private void Track(EAccumulationSource typeToTrack)
	{
		if (typeToTrack == EAccumulationSource.PerMagEmptiness)
			typeToTrack = EAccumulationSource.PerMagFullness;
		if (!TrackingSources.Contains(typeToTrack))
			TrackingSources.Add(typeToTrack);
	}

    private int testStackCount = 1;
    private int testLevel = 1;
    private int testAttachCount = 0;
	private int testMagSize = 1;
	private int testBulletCount = 1;
	private float inputTest = 1;

	public override void OnInspectorGUI()
    {
        if(target is CraftingTraitDefinition def)
        {
			TrackingSources.Clear();
			int highestStack = 0;
			foreach (AbilityDefinition abilityDef in def.abilities)
			{
				foreach(AbilityEffectDefinition effect in abilityDef.effects)
					foreach(ModifierDefinition mod in effect.modifiers)
						foreach(StatAccumulatorDefinition accumulator in mod.accumulators)
							foreach(EAccumulationSource source in accumulator.multiplyPer)
								Track(source);

				//foreach (StatAccumulatorDefinition stackSource in abilityDef.stacking.decayTime.additional)
				//	foreach(EAccumulationSource source in stackSource.multiplyPer)
				//		Track(source);

				if (abilityDef.stacking.maxStacks > highestStack)
					highestStack = abilityDef.stacking.maxStacks;
			}

			FlanStyles.BigHeader("Ability Scaling Tester");

			foreach (EAccumulationSource stackingType in TrackingSources)
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
						GUILayout.BeginHorizontal();
						testBulletCount = Mathf.Clamp(EditorGUILayout.IntField("Magazine = ", testBulletCount), 0, testMagSize);
						GUILayout.Label("/");
						testMagSize = Mathf.Clamp(EditorGUILayout.IntField("", testMagSize, GUILayout.ExpandWidth(false)), 1, 1000);
						GUILayout.Label($" ({testBulletCount * 100f / testMagSize}% Full)");
						GUILayout.EndHorizontal();
						break;
				}
			}
			inputTest = EditorGUILayout.FloatField("[Input]=", inputTest);


			foreach (AbilityDefinition abilityDef in def.abilities)
			{
				foreach(AbilityEffectDefinition effect in abilityDef.effects)
				{
					foreach(ModifierDefinition mod in effect.modifiers)
					{
						string statName = mod.stat;
						if (mod.matchGroupPaths.Length > 0)
						{
							statName += "['";
							for (int i = 0; i < mod.matchGroupPaths.Length; i++)
							{
								statName += mod.matchGroupPaths[i];
								if (i != mod.matchGroupPaths.Length - 1)
									statName += "'|'";
							}
							statName += "']";
						}
						if (mod.setValue.Length > 0)
							StringGUI(statName, mod.setValue);
						else 
							FormulaGUI($"{statName} = ", inputTest, mod.accumulators);
					}
				}

				//float decayTime = FormulaGUI("DecayTime (seconds) = ", abilityDef.stacking.decayTime.baseValue, abilityDef.stacking.decayTime.additional);
				//float intensity = FormulaGUI("Intensity = ", abilityDef.stacking.intensity.additional);

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

		base.OnInspectorGUI();
	}

	private void StringGUI(string label, string value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, FlanStyles.BoldLabel);
		GUILayout.FlexibleSpace();
		GUILayout.Label(value, FlanStyles.BoldLabel);
		GUILayout.EndHorizontal();
	}

	private float FormulaGUI(string label, float baseStat, IEnumerable<StatAccumulatorDefinition> sources)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, FlanStyles.BoldLabel);
		GUILayout.FlexibleSpace();

		float baseAdd = 0.0f;
		float stackableMul = 0.0f;
		float independentMul = 1.0f;
		float finalAdd = 0.0f;

		foreach (StatAccumulatorDefinition stackSource in sources)
		{
			float multiplier = 1.0f;
			string valueFormatted = $"{stackSource.value}";
			switch(stackSource.operation)
			{
				case EAccumulationOperation.IndependentPercentage:
				case EAccumulationOperation.StackablePercentage:
					valueFormatted = $"{(stackSource.value > 0.0f ? "+" : "")}{stackSource.value}%";
					break;
			}
			foreach(EAccumulationSource source in stackSource.multiplyPer)
			{
				switch(source)
				{
					case EAccumulationSource.PerStacks:			
						valueFormatted += " * #Stack";
						multiplier *= testStackCount;
						break;
					case EAccumulationSource.PerLevel:			
						valueFormatted += " * #Level";
						multiplier *= testLevel;
						break;
					case EAccumulationSource.PerAttachment:		
						valueFormatted += " * #Attachment";
						multiplier *= testAttachCount;
						break;
					case EAccumulationSource.PerMagFullness:	
						valueFormatted += " * #MagFullness%";
						multiplier *= testBulletCount / testMagSize;
						break;
					case EAccumulationSource.PerMagEmptiness:	
						valueFormatted += " * #MagEmptiness%";
						multiplier *= (1.0f - testBulletCount / testMagSize);
						break;
				}
			}
			switch (stackSource.operation)
			{
				case EAccumulationOperation.BaseAdd:
					GUILayout.Label($"Base({valueFormatted})");
					break;
				case EAccumulationOperation.StackablePercentage:
					GUILayout.Label($"Base({valueFormatted})");
					break;
				case EAccumulationOperation.IndependentPercentage:
					GUILayout.Label($"Final({valueFormatted})");
					break;
				case EAccumulationOperation.FinalAdd:
					GUILayout.Label($"Final({valueFormatted})");
					break;
			}
			

			switch (stackSource.operation)
			{
				case EAccumulationOperation.BaseAdd:
					baseAdd += stackSource.value * multiplier;
					break;
				case EAccumulationOperation.StackablePercentage:
					stackableMul += (stackSource.value / 100f) * multiplier;
					break;
				case EAccumulationOperation.IndependentPercentage:
					independentMul *= (1.0f + (stackSource.value / 100f) * multiplier);
					break;
				case EAccumulationOperation.FinalAdd:
					finalAdd += stackSource.value * multiplier;
					break;
			}
		}

		// Component-wise formula
		GUILayout.Label($" = ([Input] + {baseAdd}) * {AsPercent(1.0f + stackableMul)} * {AsPercent(independentMul)} + {finalAdd}", FlanStyles.BoldLabel);

		float result = (baseStat + baseAdd) * (1.0f + stackableMul) * independentMul + finalAdd;

		GUILayout.Label($" = {result}", FlanStyles.BoldLabel);
		GUILayout.EndHorizontal();
		return result;
	}
}
