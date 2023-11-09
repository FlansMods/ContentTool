using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkinSwitcherModel))]
public class SkinSwitcherEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Vanilla Item Icon Editor"); }

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		FlanStyles.HorizontalLine();
		GUILayout.Label("Skin Switcher Settings", FlanStyles.BoldLabel);

		if (target is SkinSwitcherModel skinSwitcher)
		{
			
		}
	}
}
