using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FlansModProjectView : EditorWindow
{
	[MenuItem("Flan's Mod/Project View")]
	public static void ShowWindow()
	{
		GetWindow(typeof(FlansModProjectView), false, "Project (Flan's Mod)");
	}

	

	public void CreateGUI()
	{
		// First step, content pack selector
	
		//rootVisualElement.Add(new PopupField<>()
	}



}
