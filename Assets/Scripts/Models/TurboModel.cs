using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Model;

[System.Serializable]
public class TurboModel
{
	public string partName = "";
	public TurboPiece[] pieces = new TurboPiece[0];

	public TurboPiece GetPiece(int index)
	{
		if (0 <= index && index < pieces.Length)
			return pieces[index];
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
			partName = partName,
			pieces = new TurboPiece[pieces.Length],
		};
		for (int i = 0; i < pieces.Length; i++)
			copy.pieces[i] = pieces[i].Copy();
		return copy;
	}

	public void AddChild()
	{
		TurboPiece[] newArray = new TurboPiece[pieces.Length + 1];
		for (int i = 0; i < pieces.Length; i++)
			newArray[i] = pieces[i];
		newArray[pieces.Length] = new TurboPiece();
		pieces = newArray;
	}

	public void DuplicateChild(int index)
	{
		if (0 <= index && index < pieces.Length)
		{
			TurboPiece[] newArray = new TurboPiece[pieces.Length + 1];
			for (int i = 0; i <= index; i++)
				newArray[i] = pieces[i];
			for (int i = index + 2; i < pieces.Length + 1; i++)
				newArray[i] = pieces[i-1];

			newArray[index + 1] = pieces[index].Copy();
			pieces = newArray;
		}
	}

	public void DeleteChild(int index)
	{
		if (0 <= index && index < pieces.Length)
		{
			TurboPiece[] newArray = new TurboPiece[pieces.Length - 1];
			for (int i = 0; i < index; i++)
				newArray[i] = pieces[i];
			for (int i = index + 1; i < pieces.Length; i++)
				newArray[i - 1] = pieces[i];
			pieces = newArray;
		}
	}
}
