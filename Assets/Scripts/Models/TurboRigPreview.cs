using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TurboRigPreview : MinecraftModelPreview
{
	public TurboRig Rig { get { return Model as TurboRig; } }

	public void DeleteSection(int index)
	{
		DestroyImmediate(GetChild(Rig.Sections[index].partName).gameObject);
		Rig.DeleteSection(index);
	}

	public TurboModelPreview DuplicateSection(int index)
	{
		string newName = $"{Rig.Sections[index].partName}-";
		Rig.DuplicateSection(index);
		return GetChild(newName);
	}

	public TurboModelPreview AddSection()
	{
		return GetChild(Rig.AddSection().partName);
	}

	public TurboModelPreview GetChild(string partName)
	{
		Transform existing = transform.Find(partName);
		if (existing == null)
		{
			GameObject go = new GameObject(partName);
			go.transform.SetParent(transform);
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			existing = go.transform;
		}

		TurboModelPreview modelPreview = existing.GetComponent<TurboModelPreview>();
		if(modelPreview == null)
		{
			modelPreview = existing.gameObject.AddComponent<TurboModelPreview>();
			modelPreview.PartName = partName;
			modelPreview.SetModel(Model);
		}

		return modelPreview;
	}

	public override void GenerateMesh()
	{
		if (Rig == null)
			return;

		foreach(TurboModel section in Rig.Sections)
		{
			TurboModelPreview modelPreview = GetChild(section.partName);
			if (modelPreview == null)
				continue;

			modelPreview.Refresh();
		}
	}
}
