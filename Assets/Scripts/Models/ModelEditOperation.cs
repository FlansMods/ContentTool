using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ModelEditOperation
{
	public abstract MinecraftModel Model { get; }
	public abstract string ID { get; }
	public abstract string UndoMessage { get; }
	public abstract void ApplyToModel();
	public abstract void ApplyToPreview(MinecraftModelPreview previewer);
	public virtual bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public ModelEditOperation(MinecraftModel model) { }
}

public abstract class ModelEditOperation<TModelType> : ModelEditOperation where TModelType : MinecraftModel
{
	public override MinecraftModel Model { get { return RootModel; } }
	public TModelType RootModel { get; private set; } = null;
	public ModelEditOperation(MinecraftModel model) : base(model)
	{
		if (model is TModelType rootModel)
			RootModel = rootModel;
	}
}

// -------------------------------------------------------------------------------
#region TurboRig operations
// -------------------------------------------------------------------------------
public abstract class TurboRigEditOperation : ModelEditOperation<TurboRig>
{
	public TurboRigEditOperation(MinecraftModel model) : base(model)
	{
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		if (previewer is TurboRigPreview rigPreview)
		{
			rigPreview.RefreshGeometry();
		}
	}
}
public class TurboAddNewSectionOperation : TurboRigEditOperation
{
	public TurboAddNewSectionOperation(MinecraftModel model)
		: base(model) { }
	public override string ID { get { return "TURBO_SECTION_ADD"; } }
	public override string UndoMessage { get { return "Add TurboSection"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_AddSection();
	}
}
public class TurboAttachPointAddOperation : TurboRigEditOperation
{
	public TurboAttachPointAddOperation(MinecraftModel model)
		: base(model) { }
	public override string ID { get { return "TURBO_AP_ADD"; } }
	public override string UndoMessage { get { return "Add TurboAttachPoint"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_SetAttachment("new_ap", "body");
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview("new_ap");
			if (apPreview != null)
				apPreview.UpdatePreviewFromModel();
		}
	}
}

#endregion
// -------------------------------------------------------------------------------


// -------------------------------------------------------------------------------
#region TurboAttachPoint operations
// -------------------------------------------------------------------------------
public abstract class TurboAttachPointEditOperation : TurboRigEditOperation
{
	public string PartName { get; private set; } = "";
	public string APPreviewName { get { return $"AP_{PartName}"; } }
	public TurboAttachPointEditOperation(MinecraftModel model, string partName)
		: base(model)
	{
		PartName = partName.StartsWith("AP_") ? partName.Substring(3) : partName;
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview(PartName);
			if(apPreview != null)
				apPreview.UpdatePreviewFromModel();	
		}
	}
}
public class TurboAttachPointMoveOperation : TurboAttachPointEditOperation
{
	public Vector3 LocalPos { get; private set; }
	public bool LockPartPositions { get; private set; }
	public bool LockAttachPoints { get; private set; }
	public TurboAttachPointMoveOperation(MinecraftModel model, string partName, Vector3 localPos, bool lockParts, bool lockAPs)
		: base(model, partName) 
	{
		LocalPos = localPos;
		LockPartPositions = lockParts;
		LockAttachPoints = lockAPs;
	}
	public override string ID { get { return "TURBO_AP_MOVE"; } }
	public override string UndoMessage { get { return "Move TurboAttachPoint"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		Vector3 existingPos = RootModel.GetAttachmentOffset(PartName);
		Vector3 delta = LocalPos - existingPos;
		if (LockPartPositions)
		{
			TurboModel section = RootModel.GetSection(PartName);
			if (section != null)
			{
				foreach (TurboPiece piece in section.Pieces)
					piece.Pos -= delta;
			}
		}
		if (LockAttachPoints)
		{
			foreach (AttachPoint ap in RootModel.AttachPoints)
				if (ap.attachedTo == PartName)
					ap.position -= delta;
		}
		RootModel.Operation_SetAttachmentOffset(PartName, LocalPos);
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview(PartName);
			if (apPreview != null)
				apPreview.UpdatePreviewFromModel();
			if (LockPartPositions)
			{
				TurboModelPreview sectionPreview = rigPreview.GetSectionPreview(PartName);
				foreach(TurboPiecePreview piecePreview in sectionPreview.GetChildren())
				{
					piecePreview.RefreshGeometry();
				}
			}
		}
		
	}
}
public class TurboAttachPointRotateOperation : TurboAttachPointEditOperation
{
	public Vector3 LocalEuler { get; private set; }
	public bool LockPartPositions { get; private set; }
	public bool LockAttachPoints { get; private set; }
	public TurboAttachPointRotateOperation(MinecraftModel model, string partName, Vector3 localEuler, bool lockParts, bool lockAPs)
		: base(model, partName)
	{
		LocalEuler = localEuler;
		LockPartPositions = lockParts;
		LockAttachPoints = lockAPs;
	}
	public override string ID { get { return "TURBO_AP_ROTATE"; } }
	public override string UndoMessage { get { return "Rotate TurboAttachPoint"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		Vector3 existingEuler = RootModel.GetAttachmentOffset(PartName);
		Vector3 delta = LocalEuler - existingEuler;
		//if (LockPartPositions)
		//{
		//	TurboModel section = RootModel.GetSection(PartName);
		//	if (section != null)
		//	{
		//		foreach (TurboPiece piece in section.Pieces)
		//			piece.Pos -= delta;
		//	}
		//}
		//if (LockAttachPoints)
		//{
		//	foreach (AttachPoint ap in RootModel.AttachPoints)
		//		if (ap.attachedTo == PartName)
		//			ap.position -= delta;
		//}
		RootModel.Operation_SetAttachmentEuler(PartName, LocalEuler);
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview(PartName);
			if (apPreview != null)
				apPreview.UpdatePreviewFromModel();
			if (LockPartPositions)
			{
				TurboModelPreview sectionPreview = rigPreview.GetSectionPreview(PartName);
				foreach (TurboPiecePreview piecePreview in sectionPreview.GetChildren())
				{
					piecePreview.RefreshGeometry();
				}
			}
		}

	}
}
public class TurboAttachPointReparentOperation : TurboAttachPointEditOperation
{
	public string AttachedTo { get; private set; }
	public TurboAttachPointReparentOperation(MinecraftModel model, string partName, string attachedTo)
		: base(model, partName) 
	{
		AttachedTo = attachedTo;
	}
	public override string ID { get { return "TURBO_AP_REPARENT"; } }
	public override string UndoMessage { get { return "Re-Parent TurboAttachPoint"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_SetAttachment(PartName, AttachedTo);
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview($"{PartName}");
			TurboAttachPointPreview parentAPPreview = rigPreview.GetAPPreview($"{AttachedTo}");
			if (apPreview != null && parentAPPreview != null)
			{
				apPreview.transform.SetParent(parentAPPreview.transform);
				apPreview.transform.localPosition = rigPreview.Rig.GetAttachmentOffset(PartName);
			}
		}
	}
}
public class TurboAttachPointRenameOperation : TurboAttachPointEditOperation
{
	public string NewName { get; private set; }
	public TurboAttachPointRenameOperation(MinecraftModel model, string partName, string newName)
		: base(model, partName)
	{
		NewName = newName;
	}
	public override string ID { get { return "TURBO_AP_REPARENT"; } }
	public override string UndoMessage { get { return "Re-Parent TurboAttachPoint"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_RenameAttachment(PartName, NewName);
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview($"{PartName}");
			if (apPreview != null)
			{
				apPreview.PartName = NewName;
				apPreview.name = $"AP_{NewName}";
			}
		}
	}
}
public class TurboAttachPointDeleteOperation : TurboAttachPointEditOperation
{
	public TurboAttachPointDeleteOperation(MinecraftModel model, string partName)
		: base(model, partName) { }
	public override string ID { get { return "TURBO_AP_DELETE"; } }
	public override string UndoMessage { get { return "Delete TurboAttachPoint"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_RemoveAttachment(PartName);
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview($"{PartName}");
			if (apPreview != null)
				UnityEngine.Object.DestroyImmediate(apPreview.gameObject);
		}
	}
}
public class TurboAttachPointDuplicateOperation : TurboAttachPointEditOperation
{
	public TurboAttachPointDuplicateOperation(MinecraftModel model, string partName)
		: base(model, partName) { }
	public override string ID { get { return "TURBO_AP_DUPLICATE"; } }
	public override string UndoMessage { get { return "Duplicate TurboAttachPoint"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_DuplicateAttachment(PartName);
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboAttachPointPreview apPreview = rigPreview.GetAPPreview($"{PartName}-");
			if(apPreview != null)
				apPreview.UpdatePreviewFromModel();
		}
	}
}
#endregion
// -------------------------------------------------------------------------------


// -------------------------------------------------------------------------------
#region TurboSection operations
// -------------------------------------------------------------------------------
public abstract class TurboSectionEditOperation : TurboRigEditOperation
{
	public string SectionName { get; private set; } = "";
	public TurboModel Section { get { return RootModel.GetSection(SectionName); } }
	public TurboSectionEditOperation(MinecraftModel model, string partName)
		: base(model)
	{
		SectionName = partName;
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboModelPreview sectionPreview = rigPreview.GetSectionPreview(SectionName);
			if (sectionPreview != null)
				ApplyToSectionPreview(sectionPreview);
		}
	}
	protected abstract void ApplyToSectionPreview(TurboModelPreview sectionPreview);
}
public class TurboRenameSectionOperation : TurboSectionEditOperation
{
	public string NewName { get; private set; }
	public TurboRenameSectionOperation(MinecraftModel model, string partName, string newName)
		: base(model, partName) 
	{
		NewName = newName;
	}
	public override string ID { get { return "TURBO_SECTION_RENAME"; } }
	public override string UndoMessage { get { return "Rename TurboSection"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_RenameSection(SectionName, NewName);
	}
	public override void ApplyToPreview(MinecraftModelPreview previewer)
	{
		base.ApplyToPreview(previewer);
		if (previewer is TurboRigPreview rigPreview)
		{
			TurboModelPreview modelPreview = rigPreview.GetSectionPreview(SectionName);
			if (modelPreview != null)
				modelPreview.name = NewName;
		}
	}
	protected override void ApplyToSectionPreview(TurboModelPreview sectionPreview)
	{
		sectionPreview.name = NewName;
		sectionPreview.PartName = NewName;
	}
}
public class TurboDuplicateSectionOperation : TurboSectionEditOperation
{
	public TurboDuplicateSectionOperation(MinecraftModel model, string partName)
		: base(model, partName) { }
	public override string ID { get { return "TURBO_SECTION_DUPLICATE"; } }
	public override string UndoMessage { get { return "Duplicate TurboSection"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return true; }
	public override void ApplyToModel()
	{
		RootModel.Operation_DuplicateSection(SectionName);
	}
	protected override void ApplyToSectionPreview(TurboModelPreview sectionPreview)
	{
		// Nothing to do to the source section I guess
	}
}
public class TurboDeleteSectionOperation : TurboSectionEditOperation
{
	public TurboDeleteSectionOperation(MinecraftModel model, string partName)
		: base(model, partName) { }
	public override string ID { get { return "TURBO_SECTION_DELETE"; } }
	public override string UndoMessage { get { return "Delete TurboSection"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		RootModel.Operation_DeleteSection(SectionName);
	}
	protected override void ApplyToSectionPreview(TurboModelPreview sectionPreview)
	{
		UnityEngine.Object.DestroyImmediate(sectionPreview.gameObject);
	}
}
public class TurboAddNewPieceOperation : TurboSectionEditOperation
{
	public TurboAddNewPieceOperation(MinecraftModel model, string partName)
		: base(model, partName) { }
	public override string ID { get { return "TURBO_ADD_PIECE"; } }
	public override string UndoMessage { get { return "Add new TurboPiece"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return true; }
	public override void ApplyToModel()
	{
		Section.Operation_AddChild();
	}
	protected override void ApplyToSectionPreview(TurboModelPreview sectionPreview)
	{
		TurboPiecePreview piecePreview = sectionPreview.GetPiecePreview(Section.Pieces.Count - 1);
		piecePreview.RefreshGeometry();
	}
}
#endregion
// -------------------------------------------------------------------------------

// -------------------------------------------------------------------------------
#region TurboPiece operations
// -------------------------------------------------------------------------------
public abstract class TurboPieceEditOperation : TurboSectionEditOperation
{
	public string PartName { get; private set; } = "";
	public int PieceIndex { get { return int.Parse(PartName); } }
	public TurboPiece Piece { get { return Section.GetPiece(PieceIndex); } }
	public TurboPieceEditOperation(MinecraftModel model, string partName, int pieceIndex)
		: base(model, partName) 
	{
		PartName = $"{pieceIndex}";
	}
	protected override void ApplyToSectionPreview(TurboModelPreview sectionPreview)
	{
		TurboPiecePreview piecePreview = sectionPreview.GetPiecePreview(PieceIndex);
		if (piecePreview != null)
			ApplyToPiecePreview(piecePreview);
	}
	protected abstract void ApplyToPiecePreview(TurboPiecePreview piecePreview);
}
public class TurboUnityTransformOperation : TurboPieceEditOperation
{
	public Vector3 NewLocalPos { get; private set; }
	public Quaternion NewLocalRot { get; private set; }
	public TurboUnityTransformOperation(MinecraftModel model, string partName, int pieceIndex, Vector3 localPos, Quaternion localRot)
		: base(model, partName, pieceIndex)
	{
		NewLocalPos = localPos;
		NewLocalRot = localRot;
	}
	public override string ID { get { return "TURBO_MOVE_IN_EDITOR"; } }
	public override string UndoMessage { get { return "TurboPiece transform"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		Piece.Origin = NewLocalPos;
		Piece.Euler = NewLocalRot.eulerAngles;
	}
	protected override void ApplyToPiecePreview(TurboPiecePreview piecePreview)
	{
		piecePreview.CopyToUnityTransform();
	}
}
public class TurboResizeBoxOperation : TurboPieceEditOperation
{
	public Bounds NewBounds { get; private set; }
	public TurboResizeBoxOperation(MinecraftModel model, string partName, int pieceIndex, Bounds bounds)
		: base(model, partName, pieceIndex) 
	{
		NewBounds = bounds;
	}
	public override string ID { get { return "TURBO_RESIZE"; } }
	public override string UndoMessage { get { return "TurboPiece resize"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return true; }
	public override void ApplyToModel()
	{
		Piece.Pos = NewBounds.min;
		Piece.Dim = NewBounds.size;
	}
	protected override void ApplyToPiecePreview(TurboPiecePreview piecePreview)
	{
		piecePreview.RefreshGeometry();
	}
}
public class TurboEditOffsetsOperation : TurboPieceEditOperation
{
	public List<int> OffsetIndices { get; private set; }
	public List<Vector3> NewOffsetPositions { get; private set; }
	public TurboEditOffsetsOperation(MinecraftModel model, string partName, int pieceIndex, List<int> indices, List<Vector3> offsets)
		: base(model, partName, pieceIndex) 
	{
		OffsetIndices = indices;
		NewOffsetPositions = offsets;
	}
	public override string ID { get { return "TURBO_OFFSET"; } }
	public override string UndoMessage { get { return "TurboPiece edit offsets"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		for(int i = 0; i < OffsetIndices.Count; i++)
		{
			Piece.Offsets[OffsetIndices[i]] = NewOffsetPositions[i];
		}
	}
	protected override void ApplyToPiecePreview(TurboPiecePreview piecePreview)
	{
		piecePreview.RefreshGeometry();
	}
}
public class TurboDuplicatePieceOperation : TurboPieceEditOperation
{
	public TurboDuplicatePieceOperation(MinecraftModel model, string partName, int pieceIndex)
		: base(model, partName, pieceIndex) { }
	public override string ID { get { return "TURBO_DUPLICATE"; } }
	public override string UndoMessage { get { return "Duplicate TurboPiece"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return true; }
	public override void ApplyToModel()
	{
		Section.Operation_DuplicateChild(PieceIndex);
	}
	protected override void ApplyToSectionPreview(TurboModelPreview modelPreview)
	{
		int childCount = modelPreview.transform.childCount;
		for (int i = PieceIndex + 1; i < childCount; i++)
		{
			Transform toMove = modelPreview.transform.Find($"{i}");
			if (toMove != null)
			{
				toMove.name = $"{i + 1}";
				toMove.GetComponent<TurboPiecePreview>().PartIndex = i + 1;
			}
		}
		Transform existing = modelPreview.transform.Find($"{PieceIndex}");
		if (existing != null)
		{
			TurboPiecePreview insert = (TurboPiecePreview)existing.GetComponent<TurboPiecePreview>().DuplicatePreviewObject();
			insert.GetComponent<TurboPiecePreview>().PartIndex = PieceIndex + 1;
			insert.name = $"{PieceIndex + 1}";
			insert.transform.SetParent(modelPreview.transform);
			insert.CopyToUnityTransform();
			insert.transform.SetSiblingIndex(PieceIndex + 1);
		}
	}
	protected override void ApplyToPiecePreview(TurboPiecePreview piecePreview)
	{
		// Nothing to do to the original piece
	}
}
public class TurboDeletePieceOperation : TurboPieceEditOperation
{
	public TurboDeletePieceOperation(MinecraftModel model, string partName, int pieceIndex)
		: base(model, partName, pieceIndex) { }
	public override string ID { get { return "TURBO_DELETE"; } }
	public override string UndoMessage { get { return "Delete TurboPiece"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		Section.Operation_DeleteChild(PieceIndex);
	}
	protected override void ApplyToSectionPreview(TurboModelPreview modelPreview)
	{
		int childCount = modelPreview.transform.childCount;
		Transform existing = modelPreview.transform.Find($"{PieceIndex}");
		if (existing != null)
		{
			UnityEngine.Object.DestroyImmediate(existing.gameObject);
		}
		for (int i = PieceIndex + 1; i < childCount; i++)
		{
			Transform toMove = modelPreview.transform.Find($"{i}");
			if (toMove != null)
			{
				toMove.name = $"{i - 1}";
				toMove.GetComponent<TurboPiecePreview>().PartIndex--;
			}
		}
	}
	protected override void ApplyToPiecePreview(TurboPiecePreview piecePreview)
	{
		// Nothing to do to the original piece (it is gone)
	}
}
public class TurboMirrorPieceOperation : TurboPieceEditOperation
{
	public bool MirrorX { get; private set; }
	public bool MirrorY { get; private set; }
	public bool MirrorZ { get; private set; }
	public TurboMirrorPieceOperation(MinecraftModel model, string partName, int pieceIndex, bool x, bool y, bool z)
		: base(model, partName, pieceIndex) 
	{
		MirrorX = x;
		MirrorY = y;
		MirrorZ = z;
	}
	public override string ID { get { return "TURBO_DELETE"; } }
	public override string UndoMessage { get { return "Delete TurboPiece"; } }
	public override bool WillInvalidateUVMap(UVMap originalMap) { return false; }
	public override void ApplyToModel()
	{
		Piece.Operation_DoMirror(MirrorX, MirrorY, MirrorZ);
	}
	protected override void ApplyToPiecePreview(TurboPiecePreview piecePreview)
	{
		piecePreview.CopyToUnityTransform();
		piecePreview.RefreshGeometry();
	}
}
#endregion
// -------------------------------------------------------------------------------