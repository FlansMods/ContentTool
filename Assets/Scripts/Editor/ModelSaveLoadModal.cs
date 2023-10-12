using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModelSaveLoadModal : EditorWindow
{
    private enum EType
	{
		SaveAs,

	}
	private EType Type;
	private MinecraftModel Model;

    public static void ConfirmSaveAs(MinecraftModel model)
    {
        ModelSaveLoadModal popup = CreateInstance<ModelSaveLoadModal>();
		popup.Type = EType.SaveAs;
		popup.Model = model;
		popup.ShowModalUtility();
    }

	public void OnGUI()
	{
		switch(Type)
		{
			case EType.SaveAs:
			{
				string path = EditorUtility.SaveFilePanelInProject(Model.name, "new_model", ".asset", "Save Model As...");

				break;
			}
		}
	}

	public void OnInspectorUpdate()
	{
		Repaint();
	}
}
