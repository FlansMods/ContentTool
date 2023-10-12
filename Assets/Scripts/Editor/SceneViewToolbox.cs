using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "FM Tools", true)]
public class SceneViewToolbox : Overlay
{
	public override VisualElement CreatePanelContent()
	{
		var root = new VisualElement() { name = "FM Tools" };
		root.Add(new Label() { text = $"{CurrentRigName()}" });
		root.Add(new Button(LoadModel) { text = "Load" });
		root.Add(new Button(SaveChanges) { text = "Save" });
		root.Add(new Button(SaveChangesAs) { text = "Save As..." });
		root.Add(new Button(DiscardChanges) { text = "Discard" });

		return root;

	}

	public string CurrentRigName()
	{
		ModelEditingRig currentRig = GetCurrentRig();
		if (currentRig == null || currentRig.WorkingCopy == null)
			return "N/A";
		return currentRig.WorkingCopy.name;
	}
	public void LoadModel()
	{
		GetCurrentRig()?.Button_OpenModel();
	}
	public void SaveChanges()
	{
		GetCurrentRig()?.Button_Save();
	}
	public void SaveChangesAs()
	{
		GetCurrentRig()?.Button_SaveAs();
	}
	public void DiscardChanges()
	{
		GetCurrentRig()?.Button_Discard();
	}

	private ModelEditingRig GetCurrentRig()
	{
		ModelEditingRig[] rigs = Object.FindObjectsOfType<ModelEditingRig>();
		if (rigs.Length == 1)
			return rigs[0];

		foreach(Object obj in Selection.objects)
		{
			ModelEditingRig rigInParent = obj.GetComponentInParent<ModelEditingRig>();
			if (rigInParent != null)
				return rigInParent;
		}

		return rigs.Length > 0 ? rigs[0] : null;
	}
}
