using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CraftingTraitDefinition))]
public class CraftingTraitDefinitionEditor : Editor
{
	public override void OnInspectorGUI()
    {
        if(target is CraftingTraitDefinition def)
        {
            GUILayout.BeginHorizontal();
            foreach(AbilityDefinition abilityDef in def.abilities)
            {
                //string startLabel = "";
                //foreach(AbilityTriggerDefinition triggerDef in abilityDef.startTriggers)
                //    startLabel += 
            }
            GUILayout.EndHorizontal();
        }

		base.OnInspectorGUI();
	}
}
