using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[ExecuteInEditMode]
public class TurboPiecePreview : MinecraftModelPreview
{
	private TurboModelPreview _Parent = null;
	private TurboModelPreview Parent
	{
		get
		{
			if (_Parent == null)
				_Parent = GetComponentInParent<TurboModelPreview>();
			return _Parent;
		}
	}
	public int PartIndex = 0;

	private Texture2D PieceTexture = null;
	private Vector3Int lastDims = new Vector3Int();

	public override MinecraftModel GetModel()
	{
		return Parent?.GetModel();
	}

	public void Duplicate()
	{
		Parent?.DuplicateChild(PartIndex);
	}

	protected override void Update()
	{
		base.Update();
	}

	public override Vector3 SetPos(Vector3 localPos)
	{
		Vector3 resultPos = base.SetPos(localPos);
		if (Piece != null)
			Piece.Origin = resultPos;
		return resultPos;
	}

	public override Vector3 SetEuler(Vector3 localEuler)
	{
		Vector3 resultEuler = base.SetEuler(localEuler);
		if (Piece != null)
			Piece.Euler = resultEuler;
		return resultEuler;
	}

	public TurboPiece Piece 
	{ 
		get 
		{
			if (Parent != null && Parent.Section != null)
				return Parent.Section.GetPiece(PartIndex);
			return null;
		} 
	}

	public Texture2D GetTemporaryTexture()
	{
		return PieceTexture;
	}

	private Vector2Int CalculateTemporaryTextureSize()
	{
		int maxU = Mathf.FloorToInt(Piece.Dim.z + Piece.Dim.x) * 2;
		int maxV = Mathf.FloorToInt(Piece.Dim.z + Piece.Dim.y);
		return new Vector2Int(
			Mathf.Max(Mathf.NextPowerOfTwo(maxU), 4), 
			Mathf.Max(Mathf.NextPowerOfTwo(maxV), 4));
	}

	public override void GenerateMesh()
	{
		if (Piece == null)
			return;

		Vector2Int textureSize = CalculateTemporaryTextureSize();
		Piece.ExportToMesh(Mesh, textureSize.x, textureSize.y);
		Vector3Int uvDims = new Vector3Int(Mathf.FloorToInt(Piece.Dim.x), Mathf.FloorToInt(Piece.Dim.y), Mathf.FloorToInt(Piece.Dim.z));

		if (MR.sharedMaterial == null)
		{
			MR.sharedMaterial = new Material(Shader.Find("Standard"));
			MR.sharedMaterial.name = "TemporaryTexture";
			MR.sharedMaterial.SetTexture("_MainTex", PieceTexture);
		}
		if (PieceTexture == null)
		{
			PieceTexture = new Texture2D(textureSize.x, textureSize.y);
			PieceTexture.filterMode = FilterMode.Point;
			MR.sharedMaterial.SetTexture("_MainTex", PieceTexture);
			MR.sharedMaterial.EnableKeyword("_NORMALMAP");
			MR.sharedMaterial.EnableKeyword("_DETAIL_MULX2");
			CreateFreshUVMap(PieceTexture);
		}
		else if (uvDims != lastDims)
		{
			ResizeUV(lastDims, uvDims);
			lastDims = uvDims;
		}
	}

	public void CopyExistingTexture(Texture2D full)
	{
		if (full != null && PieceTexture != null)
		{
			Vector2Int textureSize = CalculateTemporaryTextureSize();
			Vector2Int uvMapSize = Piece.GetBoxUVSize();
			for (int i = 0; i < uvMapSize.x; i++)
				for(int j = 0; j < uvMapSize.y; j++)
				{
					PieceTexture.SetPixel(
					i,
					textureSize.y - (j+1),
					full.GetPixel(Piece.textureU + i, full.height - (Piece.textureV + (j+1))));
				}
			PieceTexture.Apply();
			lastDims = new Vector3Int(Mathf.FloorToInt(Piece.Dim.x), Mathf.FloorToInt(Piece.Dim.y), Mathf.FloorToInt(Piece.Dim.z));
		}
	}

	private static readonly Color[] Faces = new Color[]
	{
		new Color(0.25f, 0.5f, 1.0f),
		new Color(0.5f, 0.25f, 1.0f),
		new Color(1f, 0.5f, 0.25f),
		new Color(1f, 0.25f, 0.5f),
		new Color(0.25f, 1f, 0.5f),
		new Color(0.5f, 1f, 0.25f),
	};
	private static readonly Color[] Outlines = new Color[]
	{
		new Color(0.5f, 0.75f, 1.0f),
		new Color(0.75f, 0.5f, 1.0f),
		new Color(1f, 0.75f, 0.5f),
		new Color(1f, 0.5f, 0.75f),
		new Color(0.5f, 1f, 0.75f),
		new Color(0.75f, 1f, 0.5f),
	};
	public void ResetTexture()
	{
		DestroyImmediate(PieceTexture);
		PieceTexture = null;
		GenerateMesh();
	}
	private void CreateFreshUVMap(Texture2D texture)
	{
		int x = Mathf.FloorToInt(Piece.Dim.x);
		int y = Mathf.FloorToInt(Piece.Dim.y);
		int z = Mathf.FloorToInt(Piece.Dim.z);

		FillTexture(texture, 0, 0, texture.width, texture.height, Color.clear, Color.clear);

		FillTexture(texture, z, x, 0, z, Faces[0], Outlines[0]);
		FillTexture(texture, z+x, x, 0, z, Faces[1], Outlines[1]);

		FillTexture(texture, 0, z, z, y, Faces[2], Outlines[2]);
		FillTexture(texture, z, x, z, y, Faces[3], Outlines[3]);
		FillTexture(texture, z+x, z, z, y, Faces[4], Outlines[4]);
		FillTexture(texture, z+x+z, x, z, y, Faces[5], Outlines[5]);
		lastDims = new Vector3Int(x, y, z);
		texture.Apply();
	}
	private void FillTexture(Texture2D texture, int x, int w, int y, int h, Color color, Color outline)
	{
		for (int i = x; i < x + w; i++)
			for (int j = y; j < y + h; j++)
			{
				if(i == x || i == x + w - 1 || j == y || j == y + h - 1)
					texture.SetPixel(i, texture.height - 1 - j, outline);
				else
					texture.SetPixel(i, texture.height - 1 - j, color);
			}
	}
	private Color[] GetPx(Texture2D tex, int x, int y, int w, int h)
	{
		return tex.GetPixels(x, tex.height - y - h, w, h);
	}
	private void SetPx(Texture2D tex, int x, int y, int w, int h, Color[] px)
	{
		tex.SetPixels(x, tex.height - y - h, w, h, px);
	}

	private void ResizeUV(Vector3Int oldDims, Vector3Int newDims)
	{
		if (PieceTexture == null)
			return;

		Vector2Int textureSize = CalculateTemporaryTextureSize();
		// If different power of 2 size, we need to copy to a new texture
		if (textureSize.x > PieceTexture.width || textureSize.y > PieceTexture.height)
		{
			Texture2D dst = new Texture2D(textureSize.x, textureSize.y);
			dst.filterMode = FilterMode.Point;
			MR.sharedMaterial.SetTexture("_MainTex", dst);
			MR.sharedMaterial.EnableKeyword("_NORMALMAP");
			MR.sharedMaterial.EnableKeyword("_DETAIL_MULX2");
			int minX = Mathf.Min(PieceTexture.width, textureSize.x);
			int minY = Mathf.Min(PieceTexture.height, textureSize.y);

			dst.SetPixels(0, textureSize.y - minY, minX, minY, PieceTexture.GetPixels(0, PieceTexture.height - minY, minX, minY));
			PieceTexture = dst;
		}

		Vector3Int d = oldDims;
		// Now do resize operations one at a time
		if (newDims.y > d.y)
		{
			// Expand Y, add more px below using the last row
			Color[] bottomRow = PieceTexture.GetPixels(0, d.z + d.y, textureSize.x, 1);
			for (int i = 0; i < newDims.y - d.y; i++)
				PieceTexture.SetPixels(0, d.z + d.y + i, textureSize.x, 1, bottomRow);
		}
		else if (newDims.y < d.y)
		{
			// Actually, do we care? The px just get left behind
		}
		d.y = newDims.y;

		if (newDims.x > d.x)
		{
			Color[] lastColOfTop = GetPx(PieceTexture, d.z + d.x - 1, 0, 1, d.z);
			Color[] lastColOfBottom = GetPx(PieceTexture, d.z + d.x + d.x - 1, 0, 1, d.z);
			Color[] lastColOfFront = GetPx(PieceTexture, d.z + d.x - 1, d.z, 1, d.y);
			Color[] lastColOfBack = GetPx(PieceTexture, d.z + d.x + d.z + d.x - 1, d.z, 1, d.y);

			Color[] leftAndBackFaces = GetPx(PieceTexture, d.z + d.x, d.z, d.z + d.x, d.y);
			SetPx(PieceTexture, d.z + newDims.x, d.z, d.z + d.x, d.y, leftAndBackFaces);
			Color[] bottomFace = GetPx(PieceTexture, d.z + d.x, 0, d.x, d.z);
			SetPx(PieceTexture, d.z + newDims.x, 0, d.x, d.z, bottomFace);

			for(int i = 0; i < newDims.x - d.x; i++)
			{
				SetPx(PieceTexture, d.z + d.x + i, 0, 1, d.z, lastColOfTop);
				SetPx(PieceTexture, d.z + newDims.x + d.x + i, 0, 1, d.z, lastColOfBottom);
				SetPx(PieceTexture, d.z + d.x + i, d.z, 1, d.y, lastColOfFront);
				SetPx(PieceTexture, d.z + newDims.x + d.z + d.x + i, d.z, 1, d.y, lastColOfBack);
			}
		}
		else if(newDims.x < d.x)
		{
			Color[] croppedBottom = GetPx(PieceTexture, d.z + d.x, 0, newDims.x, d.z);
			SetPx(PieceTexture, d.z + newDims.x, 0, newDims.x, d.z, croppedBottom);
			Color[] croppedLeftAndBack = GetPx(PieceTexture, d.z + d.x, d.z, d.z + newDims.x, d.y);
			SetPx(PieceTexture, d.z + newDims.x, d.z, d.z + newDims.x, d.y, croppedLeftAndBack);
		}
		d.x = newDims.x;

		if(newDims.z > d.z)
		{
			Color[] lastRowOfTopBottom = GetPx(PieceTexture, d.z, d.z - 1, d.x + d.x, 1);
			Color[] lastColOfLeft = GetPx(PieceTexture, d.z - 1, d.z, 1, d.y);
			Color[] lastColOfRight = GetPx(PieceTexture, d.z + d.x + d.z - 1, d.z, 1, d.y);

			// Move top and bottom
			Color[] topAndBottom = GetPx(PieceTexture, d.z, 0, d.x + d.x, d.z);
			SetPx(PieceTexture, newDims.z, 0, d.x + d.x, d.z, topAndBottom);
			// Move left, front, right, back
			Color[] back = GetPx(PieceTexture, d.z + d.x + d.z, d.z, d.x, d.y);
			SetPx(PieceTexture, newDims.z + d.x + newDims.z, newDims.z, d.x, d.y, back);
			Color[] frontAndRight = GetPx(PieceTexture, d.z, d.z, d.x + d.z, d.y);
			SetPx(PieceTexture, newDims.z, newDims.z, d.x + d.z, d.y, frontAndRight);
			Color[] left = GetPx(PieceTexture, 0, d.z, d.z, d.y);
			SetPx(PieceTexture, 0, newDims.z, d.z, d.y, left);

			// And fill in gaps
			for (int i = 0; i < newDims.z - d.z; i++)
			{
				SetPx(PieceTexture, newDims.z, d.z + i, d.x + d.x, 1, lastRowOfTopBottom);
				SetPx(PieceTexture, d.z + i, newDims.z, 1, d.y, lastColOfLeft);
				SetPx(PieceTexture, newDims.z + d.x + d.z + i, newDims.z, 1, d.y, lastColOfRight);
			}
		}
		else if(newDims.z < d.z)
		{
			// Move pieces
			Color[] croppedTopAndBottom = GetPx(PieceTexture, d.z, 0, d.x + d.x, newDims.z);
			SetPx(PieceTexture, newDims.z, 0, d.x + d.x, newDims.z, croppedTopAndBottom);
			Color[] croppedLeft = GetPx(PieceTexture, 0, d.z, newDims.z, d.y);
			SetPx(PieceTexture, 0, newDims.z, newDims.z, d.y, croppedLeft);
			Color[] croppedFrontAndRight = GetPx(PieceTexture, d.z, d.z, d.x + newDims.z, d.y);
			SetPx(PieceTexture, newDims.z, newDims.z, d.x + newDims.z, d.y, croppedFrontAndRight);
			Color[] back = GetPx(PieceTexture, d.z + d.x + d.z, d.z, d.x, d.y);
			SetPx(PieceTexture, newDims.z + d.x + newDims.z, newDims.z, d.x, d.y, back);
		}
		d.z = newDims.z;

		PieceTexture.Apply();

		// If different power of 2 size, we need to copy to a new texture
		if (textureSize.x < PieceTexture.width || textureSize.y < PieceTexture.height)
		{
			Texture2D dst = new Texture2D(textureSize.x, textureSize.y);
			dst.filterMode = FilterMode.Point;
			MR.sharedMaterial.SetTexture("_MainTex", dst);
			MR.sharedMaterial.EnableKeyword("_NORMALMAP");
			MR.sharedMaterial.EnableKeyword("_DETAIL_MULX2");
			int minX = Mathf.Min(PieceTexture.width, textureSize.x);
			int minY = Mathf.Min(PieceTexture.height, textureSize.y);

			dst.SetPixels(0, textureSize.y - minY, minX, minY, PieceTexture.GetPixels(0, PieceTexture.height - minY, minX, minY));
			PieceTexture = dst;
			PieceTexture.Apply();
		}
	}
}