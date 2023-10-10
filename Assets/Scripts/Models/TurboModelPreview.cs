using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurboModelPreview : MinecraftModelPreview
{
	private TurboRigPreview _Parent = null;
	private TurboRigPreview Parent 
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

	public override MinecraftModel GetModel()
	{
		return Parent?.GetModel();
	}



	public void DeleteChild(int index)
	{
		Section.DeleteChild(index);

		int childCount = transform.childCount;
		Transform existing = transform.Find($"{index}");
		if (existing != null)
		{
			DestroyImmediate(existing.gameObject);
		}
		for(int i = index + 1; i < childCount; i++)
		{
			Transform toMove = transform.Find($"{i}");
			if (toMove != null)
			{
				toMove.name = $"{i - 1}";
				toMove.GetComponent<TurboPiecePreview>().PartIndex--;
			}
		}
	}

	public TurboPiecePreview DuplicateChild(int index)
	{
		Section.DuplicateChild(index);

		int childCount = transform.childCount;
		for (int i = index+1; i < childCount; i++)
		{
			Transform toMove = transform.Find($"{i}");
			if (toMove != null)
			{
				toMove.name = $"{i + 1}";
				toMove.GetComponent<TurboPiecePreview>().PartIndex++;
			}
		}

		Transform existing = transform.Find($"{index}");
		if(existing != null)
		{
			Transform insert = Instantiate(existing);
			insert.name = $"{index + 1}";
			insert.SetParent(transform);
			insert.SetSiblingIndex(index+1);
			return insert.GetComponent<TurboPiecePreview>();
		}

		return null;
	}

	public TurboPiecePreview AddChild()
	{
		Section.AddChild();
		return GetChild(Section.pieces.Length - 1);
	}

	public TurboPiecePreview GetChild(int index)
	{
		Transform existing = transform.Find($"{index}");
		if (existing == null)
		{
			GameObject go = new GameObject($"{index}");
			go.transform.SetParent(transform);
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			existing = go.transform;
		}

		TurboPiecePreview piecePreview = existing.GetComponent<TurboPiecePreview>();
		if (piecePreview == null)
		{
			piecePreview = existing.gameObject.AddComponent<TurboPiecePreview>();
			piecePreview.PartIndex = index;
			piecePreview.SetModel(Model);
		}

		return piecePreview;
	}

	public override void GenerateMesh()
	{
		if (Section == null)
			return;
		for(int i = 0; i < Section.pieces.Length; i++)
		{
			TurboPiecePreview piecePreview = GetChild(i);		
			if (piecePreview == null)
				continue;

			piecePreview.Refresh();
		}
	}
}
