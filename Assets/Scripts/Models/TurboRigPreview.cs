using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TurboRigPreview : MinecraftModelPreview
{
	private static int IterationCount = 0;
	public TurboRig Rig { get { return Model as TurboRig; } }

	// -------------------------------------------------------------------------------
	// Heirarchy
	public override IEnumerable<MinecraftModelPreview> GetChildren()
	{
		if (Rig != null)
		{
			foreach (TurboModel section in Rig.Sections)
			{
				TurboModelPreview sectionPreview = GetSectionPreview(section.PartName);
				if (sectionPreview != null)
					yield return sectionPreview;
			}
		}
	}
	public override bool CanAdd() { return true; }
	public override ModelEditOperation Add()
	{
		return new TurboAddNewSectionOperation(GetModel());
	}

	// -------------------------------------------------------------------------------
	#region Attach Point Management
	// -------------------------------------------------------------------------------
	private bool TryGetAPPreview(string partName, out TurboAttachPointPreview ap)
	{
		ap = null;
		if (partName.Length == 0 || partName == "none")
			return false;

		Transform apTransform = transform.FindRecursive($"AP_{partName}");
		ap = apTransform?.GetComponent<TurboAttachPointPreview>();
		return ap != null;
	}
	private TurboAttachPointPreview CreateAPPreview(string partName)
	{
		IterationCount++;
		if(IterationCount >= 50)
		{
			Debug.LogError($"AttachPoint loop in {Rig.name}");
			return null;
		}
		GameObject go = new GameObject($"AP_{partName}");
		string parentName = Rig.GetAttachedTo(partName);
		Vector3 offset = Rig.GetAttachmentOffset(partName);
		Transform parentTransform = transform;
		if(parentName != "none")
		{
			parentTransform = GetAPPreview(parentName).transform;
		}
		go.transform.SetParent(parentTransform);
		go.transform.localPosition = offset;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		TurboAttachPointPreview ap = go.AddComponent<TurboAttachPointPreview>();
		ap.PartName = partName;
		IterationCount--;
		return ap;
	}
	public TurboAttachPointPreview GetAPPreview(string partName)
	{
		if (partName == "none" || partName.Length == 0)
			partName = "body";
		if (TryGetAPPreview(partName, out TurboAttachPointPreview ap))
			return ap;
		else // The AP transform does not yet exist, create it
		{
			return CreateAPPreview(partName);
		}
	}
	#endregion
	// -------------------------------------------------------------------------------

	// -------------------------------------------------------------------------------
	#region Section Management
	// -------------------------------------------------------------------------------
	private bool TryGetSectionPreview(string partName, out TurboModelPreview section)
	{
		section = null;
		if (partName.Length == 0 || partName == "none")
			return false;

		Transform apTransform = transform.FindRecursive(partName);
		section = apTransform?.GetComponent<TurboModelPreview>();
		return section != null;
	}
	private TurboModelPreview CreateSectionPreview(string partName)
	{
		TurboAttachPointPreview parentAP = GetAPPreview(partName);
		GameObject go = new GameObject(partName);
		go.transform.SetParent(parentAP.transform);
		go.transform.localPosition = Vector3.one;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		TurboModelPreview section = go.AddComponent<TurboModelPreview>();
		section.PartName = partName;
		section.SetModel(GetModel());
		return section;
	}
	public TurboModelPreview GetSectionPreview(string partName)
	{
		if (TryGetSectionPreview(partName, out TurboModelPreview section))
			return section;
		else // The Section transform does not yet exist, create it
		{
			return CreateSectionPreview(partName);
		}
	}
	public override void InitializePreviews()
	{
		foreach(AttachPoint ap in Rig.AttachPoints)
		{
			TurboAttachPointPreview apPreview = GetAPPreview(ap.name);
		}

		foreach(TurboModel section in Rig.Sections)
		{
			TurboModelPreview sectionPreview = GetSectionPreview(section.PartName);
			if (sectionPreview != null)
				sectionPreview.InitializePreviews();
		}
	}
	#endregion
	// -------------------------------------------------------------------------------

	// -------------------------------------------------------------------------------
	#region Unity Transform
	// -------------------------------------------------------------------------------
	protected override void EditorUpdate()
	{
		base.EditorUpdate();

		if(HasUnityTransformBeenChanged())
		{
			// TODO: Update current pose
		}
	}

	private bool HasUnityTransformBeenChanged()
	{
		return true;
	}
	#endregion
	// -------------------------------------------------------------------------------
}
