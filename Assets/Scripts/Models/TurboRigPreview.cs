using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TurboRigPreview : MinecraftModelPreview
{
	public TurboRig Rig { get { return Model as TurboRig; } }



	public void DeleteSection(int index)
	{
		DestroyImmediate(GetChild(Rig.Sections[index].PartName).gameObject);
		Rig.DeleteSection(index);
	}
	public void DeleteSection(string partName)
	{
		DestroyImmediate(GetChild(partName).gameObject);
		Rig.DeleteSection(partName);
	}
	public override IEnumerable<MinecraftModelPreview> GetChildren()
	{
		foreach(TurboModel section in Rig.Sections)
		{
			TurboModelPreview sectionPreview = GetChild(section.PartName);
			if (sectionPreview != null)
				yield return sectionPreview;
		}
	}

	public TurboModelPreview DuplicateSection(int index)
	{
		string newName = $"{Rig.Sections[index].PartName}-";
		Rig.DuplicateSection(index);
		return GetChild(newName);
	}
	public TurboModelPreview DuplicateSection(string partName)
	{
		string newName = $"{partName}-";
		Rig.DuplicateSection(partName);
		return GetChild(newName);
	}

	public TurboModelPreview AddSection()
	{
		return GetChild(Rig.AddSection().PartName);
	}

	public Transform GetEmptyChild(string apName)
	{
		foreach (Transform t in GetComponentsInChildren<Transform>())
			if (t.name == apName)
				return t;

		Transform parent = transform;
		Vector3 offset = Vector3.zero;
		AttachPoint ap = Rig.GetAttachPoint(apName);
		if (ap != null)
		{
			TurboModelPreview parentPreview = GetChild(ap.attachedTo);
			parent = parentPreview.transform;
			offset = ap.position;
		}

		GameObject go = new GameObject(apName);
		go.transform.SetParent(parent);
		go.transform.localPosition = offset;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		return go.transform;
	}

	public TurboModelPreview GetChild(string partName)
	{
		foreach(TurboModelPreview model in GetComponentsInChildren<TurboModelPreview>())
		{
			if (model.PartName == partName)
				return model;
		}

		Transform parent = transform;
		AttachPoint ap = Rig.GetAttachPoint(partName);
		Vector3 offset = Vector3.zero;
		if (ap != null)
		{
			TurboModelPreview parentPreview = GetChild(ap.attachedTo);
			parent = parentPreview.transform;
			offset = ap.position;
		}
	
		GameObject go = new GameObject(partName);
		go.transform.SetParent(parent);
		go.transform.localPosition = offset;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		TurboModelPreview modelPreview = go.AddComponent<TurboModelPreview>();
		modelPreview.PartName = partName;
		modelPreview.SetModel(Model);
		return modelPreview;
	}

	public void UpdateAttachPointPositions()
	{
		foreach(AttachPoint ap in Rig.AttachPoints)
		{
			Transform child = GetEmptyChild(ap.name);
			child.localPosition = ap.position;
		}
	}

	public override void GenerateMesh()
	{
		if (Rig == null)
			return;

		foreach(TurboModel section in Rig.Sections)
		{
			TurboModelPreview modelPreview = GetChild(section.PartName);
			if (modelPreview == null)
				continue;

			modelPreview.Refresh();
		}

		// Load all non-defaulted APs as empty transforms
		foreach(AttachPoint ap in Rig.AttachPoints)
		{
			if(ap.position.sqrMagnitude > 0.0f)
				GetEmptyChild(ap.name);
		}
	}
}
