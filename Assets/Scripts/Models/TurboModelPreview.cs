using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TurboModelPreview : MinecraftModelPreview
{
	private TurboRigPreview _Parent = null;
	public TurboRigPreview Parent 
	{
		get 
		{
			if (_Parent == null)
				_Parent = GetComponentInParent<TurboRigPreview>();
			return _Parent;
		}
	}
	public TurboModel Section
	{
		get
		{
			if (Parent != null && Parent.Rig != null)
				return Parent.Rig.GetSection(PartName);
			return null;
		}
	}
	public string PartName = "";

	public override MinecraftModel GetModel() { return Parent?.GetModel(); }
	public override MinecraftModelPreview GetParent() { return Parent; }
	public override IEnumerable<MinecraftModelPreview> GetChildren()
	{
		foreach(Transform t in transform)
		{
			TurboPiecePreview piecePreview = t.GetComponent<TurboPiecePreview>();
			if (piecePreview != null)
				yield return piecePreview;
		}
	}
	public override bool CanAdd() { return true; }
	public override bool CanDelete() { return true; }
	public override bool CanDuplicate() { return true; }
	public override ModelEditOperation Add()
	{
		return new TurboAddNewPieceOperation(GetModel(), PartName);
	}
	public override ModelEditOperation Delete()
	{
		return new TurboDeleteSectionOperation(GetModel(), PartName);
	}
	public override ModelEditOperation Duplicate()
	{
		return new TurboDuplicateSectionOperation(GetModel(), PartName);
	}
	public override void Compact_Editor_GUI()
	{
		string changedName = EditorGUILayout.DelayedTextField(PartName);
		if(changedName != PartName)
		{
			ModelEditingSystem.ApplyOperation(
				new TurboRenameSectionOperation(
					GetModel(),
					PartName,
					changedName));
		}
		ETurboRenderMaterial changedMaterial = (ETurboRenderMaterial)EditorGUILayout.EnumPopup("Material", Section.Material);
		if(changedMaterial != Section.Material)
		{
			ModelEditingSystem.ApplyOperation(
				new TurboChangeMaterialOperation(
				GetModel(),
				PartName,
				changedMaterial));
		}

		// Also embed the AP settings in this GUI
		TurboAttachPointPreview apPreview = Parent.GetAPPreview(PartName);
		if(apPreview != null)
		{
			apPreview.Compact_Editor_GUI();
		}
	}

	protected override void EditorUpdate()
	{
		base.EditorUpdate();

		if (HasUnityTransformBeenChanged())
		{
			// TODO: Update current pose
		}
	}

	private bool HasUnityTransformBeenChanged()
	{
		return true;
	}

	// -------------------------------------------------------------------------------
	#region Piece Management
	// -------------------------------------------------------------------------------
	private bool TryGetPiecePreview(int pieceIndex, out TurboPiecePreview piece)
	{
		piece = null;
		if (Section == null || pieceIndex < 0 || pieceIndex >= Section.Pieces.Count)
			return false;

		Transform pieceTransform = transform.FindRecursive($"{pieceIndex}");
		piece = pieceTransform?.GetComponent<TurboPiecePreview>();
		return piece != null;
	}
	private TurboPiecePreview CreatePiecePreview(int partIndex)
	{
		GameObject go = new GameObject($"{partIndex}");
		go.transform.SetParent(transform);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		TurboPiecePreview piece = go.AddComponent<TurboPiecePreview>();
		piece.PartIndex = partIndex;
		piece.SetModel(GetModel());
		return piece;
	}
	public TurboPiecePreview GetPiecePreview(int partIndex)
	{
		if (TryGetPiecePreview(partIndex, out TurboPiecePreview piece))
			return piece;
		else // The piece transform does not yet exist, create it
		{
			return CreatePiecePreview(partIndex);
		}
	}
	public override void InitializePreviews()
	{
		for(int i = 0; i < Section.Pieces.Count; i++)
		{
			TurboPiecePreview piecePreview = GetPiecePreview(i);
			if (piecePreview != null)
			{
				piecePreview.CopyToUnityTransform();
				piecePreview.RefreshGeometry();
			}
		}
	}
	#endregion
	// -------------------------------------------------------------------------------
}
