using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurboModel
{
	public string PartName = "";
	public List<TurboPiece> Pieces = new List<TurboPiece>();
	public TurboPiece GetPiece(int index)
	{
		if (0 <= index && index < Pieces.Count)
			return Pieces[index];
		return null;
	}
	public TurboModel Copy()
	{
		TurboModel copy = new TurboModel()
		{
			PartName = PartName,
		};
		for (int i = 0; i < Pieces.Count; i++)
			copy.Pieces.Add(Pieces[i].Copy());
		return copy;
	}
	public bool ExportToJson(QuickJSONBuilder builder)
	{
		return false;
	}
	public bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		return false;
	}

	// ----------------------------------------------------------------
	#region Import-Only functions
	// ----------------------------------------------------------------
	public TurboPiece Import_GetIndexedPiece(int index)
	{
		if (index < 0 || index > 10000)
			return null;
		while (Pieces.Count <= index)
			Pieces.Add(new TurboPiece());
		return Pieces[index];
	}
	#endregion
	// ----------------------------------------------------------------

	// ----------------------------------------------------------------
	#region Operations - Use ModelEditingSystem to access these
	// ----------------------------------------------------------------
	public TurboPiece Operation_AddChild()
	{
		TurboPiece newPiece = new TurboPiece();
		Pieces.Add(newPiece);
		return newPiece;
	}
	public TurboPiece Operation_DuplicateChild(int index)
	{
		if (0 <= index && index < Pieces.Count)
		{
			TurboPiece dupe = Pieces[index].Copy();
			Pieces.Insert(index, dupe);
			return dupe;
		}
		return null;
	}
	public void Operation_DeleteChild(int index)
	{
		if (0 <= index && index < Pieces.Count)
		{
			Pieces.RemoveAt(index);
		}
	}
	public void Operation_TranslateAll(float x, float y, float z)
	{
		foreach (TurboPiece piece in Pieces)
		{
			if (piece != null)
			{
				piece.Origin.x += x;
				piece.Origin.y += y;
				piece.Origin.z += z;
			}
		}
	}
	public void Operation_DoMirror(bool x, bool y, bool z)
	{
		foreach (TurboPiece piece in Pieces)
			piece.Operation_DoMirror(x, y, z);
	}
	#endregion
	// ----------------------------------------------------------------
}
