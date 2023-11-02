using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TurboRigPreview : MinecraftModelPreview
{
	public TurboRig Rig { get { return Model as TurboRig; } }



	public void DeleteSection(int index)
	{
		DestroyImmediate(GetAndUpdateChild(Rig.Sections[index].PartName).gameObject);
		Rig.DeleteSection(index);
	}
	public void DeleteSection(string partName)
	{
		DestroyImmediate(GetAndUpdateChild(partName).gameObject);
		Rig.DeleteSection(partName);
	}
	public override IEnumerable<MinecraftModelPreview> GetChildren()
	{
		if (Rig != null)
		{
			foreach (TurboModel section in Rig.Sections)
			{
				TurboModelPreview sectionPreview = GetAndUpdateChild(section.PartName);
				if (sectionPreview != null)
					yield return sectionPreview;
			}
		}
	}

	public TurboModelPreview DuplicateSection(int index)
	{
		string newName = $"{Rig.Sections[index].PartName}-";
		Rig.DuplicateSection(index);
		return GetAndUpdateChild(newName);
	}
	public TurboModelPreview DuplicateSection(string partName)
	{
		string newName = $"{partName}-";
		Rig.DuplicateSection(partName);
		return GetAndUpdateChild(newName);
	}

	public TurboModelPreview AddSection()
	{
		return GetAndUpdateChild(Rig.AddSection().PartName);
	}

	public Transform UpdateAPFromHeirarchy(string partName, int depth = 0)
	{
		Transform apTransform = GetOrCreateAPTransform(partName, depth + 1);
		if (apTransform != null && apTransform != transform)
		{
			Vector3 oldOffset = Rig.GetAttachmentOffset(partName);
			if(!Mathf.Approximately((oldOffset - apTransform.localPosition).sqrMagnitude, 0f))
			{
				Vector3 delta = apTransform.localPosition - oldOffset;
				TurboAttachPointPreview apPreview = apTransform.GetComponent<TurboAttachPointPreview>();
				if (apPreview != null)
				{
					if (apPreview.LockPartPositions)
					{
						TurboModel section = Rig.GetSection(partName);
						if (section != null)
						{
							foreach (TurboPiece piece in section.Pieces)
								piece.Pos -= delta;
						}
					}
					if (apPreview.LockAttachPoints)
					{
						foreach (AttachPoint ap in Rig.AttachPoints)
							if (ap.attachedTo == partName)
								ap.position -= delta;
					}
				}
				Rig.SetAttachmentOffset(partName, apTransform.localPosition);
			}
		}
		return apTransform;
	}

	public Transform GetOrCreateAPTransform(string partName, int depth = 0)
	{
		if (partName.Length == 0 || partName == "none")
			return transform;

			string apName = $"AP_{partName}";
		foreach (Transform t in GetComponentsInChildren<Transform>())
			if (t.name == apName)
				return t;

		if (depth >= 50)
		{
			Debug.LogError($"AttachPoint loop in {Rig.name}");
			return transform;
		}

		GameObject go = new GameObject(apName);
		string parentName = Rig.GetAttachedTo(partName);
		Vector3 offset = Rig.GetAttachmentOffset(partName);
		Transform parentTransform = transform;
		parentTransform = UpdateAPFromHeirarchy(parentName, depth + 1);
		go.transform.SetParent(parentTransform);
		go.transform.localPosition = offset;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		go.AddComponent<TurboAttachPointPreview>().PartName = partName;
		return go.transform;
	}

	public TurboModelPreview GetAndUpdateChild(string partName)
	{
		TurboModelPreview child = null;
		foreach(TurboModelPreview model in GetComponentsInChildren<TurboModelPreview>())
		{
			if (model.PartName == partName)
				child = model;
		}
		if(child == null)
		{
			GameObject go = new GameObject(partName);
			child = go.AddComponent<TurboModelPreview>();
		}

		Transform parent = UpdateAPFromHeirarchy(partName);
		child.transform.SetParent(parent);
		child.transform.localPosition = Vector3.zero;
		child.transform.localRotation = Quaternion.identity;
		child.transform.localScale = Vector3.one;
		child.PartName = partName;
		child.SetModel(Model);
		return child;
	}

	public override void GenerateMesh()
	{
		if (Rig == null)
			return;

		foreach (AttachPoint ap in Rig.AttachPoints)
		{
			UpdateAPFromHeirarchy(ap.name);
		}

		foreach (TurboModel section in Rig.Sections)
		{
			TurboModelPreview modelPreview = GetAndUpdateChild(section.PartName);
			if (modelPreview == null)
				continue;

			modelPreview.Refresh();
		}
	}
}
