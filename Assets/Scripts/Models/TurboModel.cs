using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Model;
using static System.Collections.Specialized.BitVector32;

[System.Serializable]
public class TurboModel
{
	public string PartName = "";
	public List<TurboPiece> Pieces = new List<TurboPiece>();

	public bool IsUVMapSame(TurboModel other)
	{
		if (other.PartName != PartName)
			return false;
		if (other.Pieces.Count != Pieces.Count)
			return false;
		for(int i = 0; i < Pieces.Count; i++)
		{
			if (!Pieces[i].IsUVMapSame(other.Pieces[i]))
				return false;
		}

		return true;
	}

	// These two functions are guaranteed to construct the shapes at the specified index
	public void SetIndexedTextureUV(int index, int u, int v)
	{
		if (index < 0 || index > 10000)
			return;
		while (Pieces.Count <= index)
			AddChild();
		Pieces[index].textureU = u;
		Pieces[index].textureV = v;
	}
	public TurboPiece GetIndexedPiece(int index)
	{
		if (index < 0 || index > 10000)
			return null;
		while (Pieces.Count <= index)
			AddChild();
		return Pieces[index];
	}
	public Vector2Int GetMaxUV()
	{
		Vector2Int max = Vector2Int.zero;
		foreach(TurboPiece piece in Pieces)
		{
			Vector2Int pieceMax = piece.MaxUV;
			if (pieceMax.x > max.x)
				max.x = pieceMax.x;
			if (pieceMax.y > max.y)
				max.y = pieceMax.y;
		}
		return max;
	}

	public TurboPiece GetPiece(int index)
	{
		if (0 <= index && index < Pieces.Count)
			return Pieces[index];
		return null;
	}
	public bool ExportToJson(QuickJSONBuilder builder)
	{
		return false;
	}
	public bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		return false;
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
	public TurboPiece AddChild()
	{
		TurboPiece newPiece = new TurboPiece();
		Pieces.Add(newPiece);
		return newPiece;
	}
	public TurboPiece DuplicateChild(int index)
	{
		if (0 <= index && index < Pieces.Count)
		{
			TurboPiece dupe = Pieces[index].Copy();
			Pieces.Insert(index, dupe);
			return dupe;
		}
		return null;
	}
	public void DeleteChild(int index)
	{
		if (0 <= index && index < Pieces.Count)
		{
			Pieces.RemoveAt(index);
		}
	}
	public void TranslateAll(float x, float y, float z)
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
	public void DoMirror(bool x, bool y, bool z)
	{
		foreach (TurboPiece piece in Pieces)
			piece.DoMirror(x, y, z);
	}
}
