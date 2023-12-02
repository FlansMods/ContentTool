using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadsheetImportExport : MonoBehaviour
{
    public void ImportFromGSheet()
    {

    }

    public void ExportFromGSheet()
    {

    }

    public void ImportFromCSV()
    {

    }

	public void ExportToCSV(ContentPack pack, ENewDefinitionType defType)
	{
		foreach (Definition def in pack.AllContent)
		{
			if (defType == DefinitionTypes.GetFromObject(def))
			{
				Dictionary<string, string> export = new Dictionary<string, string>();
				ExportAsset(def, export);

			}
		}
	}

    public void ExportAsset(Definition def, Dictionary<string, string> dict)
    {
        if(def is GunDefinition gunDef)
        {
			if (gunDef.TryExportMulParam("primary_fire", EActionType.Shoot, "impact_damage", out float impactDamage))
				dict.Add("Hit DMG", $"{impactDamage}");
        }
    }
}

public static class DefinitionHelpers
{
    public static bool TryExportAddParam(this GunDefinition gunDef, string actionGroupID, EActionType actionType, string modifierKey, out float result)
    {
        if(gunDef.TryGetModifier(actionGroupID, actionType, modifierKey, out ModifierDefinition modDef))
        {
            result = modDef.Add;
            return true;
        }
        result = 0.0f;
        return false;
    }
	public static bool TryExportMulParam(this GunDefinition gunDef, string actionGroupID, EActionType actionType, string modifierKey, out float result)
	{
		if (gunDef.TryGetModifier(actionGroupID, actionType, modifierKey, out ModifierDefinition modDef))
		{
			result = modDef.Multiply;
			return true;
		}
		result = 1.0f;
		return false;
	}
	public static bool TryExportSetParam(this GunDefinition gunDef, string actionGroupID, EActionType actionType, string modifierKey, out string result)
	{
		if (gunDef.TryGetModifier(actionGroupID, actionType, modifierKey, out ModifierDefinition modDef))
		{
			result = modDef.SetValue;
			return true;
		}
		result = "";
		return false;
	}
	public static bool TryGetModifier(this GunDefinition gunDef, string actionGroupID, EActionType actionType, string modifierKey, out ModifierDefinition result)
	{
		foreach (ActionGroupDefinition actionGroup in gunDef.actionGroups)
			if (actionGroup.key == actionGroupID)
				foreach (ActionDefinition action in actionGroup.actions)
					if (action.actionType == actionType)
						foreach (ModifierDefinition modifier in action.modifiers)
							if (modifier.Stat == modifierKey)
							{
								result = modifier;
								return true;
							}
		result = null;
		return false;
	}
}
