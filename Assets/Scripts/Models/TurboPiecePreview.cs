using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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


		if (MR.sharedMaterial == null)
		{
			MR.sharedMaterial = new Material(Shader.Find("Standard"));
			MR.sharedMaterial.name = "TemporaryTexture";
			MR.sharedMaterial.SetTexture("_MainTex", PieceTexture);
		}
		if (PieceTexture == null
			|| PieceTexture.width != textureSize.x
			|| PieceTexture.height != textureSize.y)
		{
			PieceTexture = new Texture2D(textureSize.x, textureSize.y);
			PieceTexture.filterMode = FilterMode.Point;
			MR.sharedMaterial.SetTexture("_MainTex", PieceTexture);
			MR.sharedMaterial.EnableKeyword("_NORMALMAP");
			MR.sharedMaterial.EnableKeyword("_DETAIL_MULX2");
		}
		CreateFreshUVMap(PieceTexture);

		//Vector3[] corners = GenerateCubeCorners(Piece.Origin, Piece.Dim);
		//BakeCubeToMesh(corners, Vector2Int.zero, Piece.Dim);
	}

	private static readonly Color[] Faces = new Color[]
	{
		new Color(0.25f, 0.5f, 1.0f),
		new Color(0.25f, 0.5f, 1.0f),
		new Color(0.25f, 0.5f, 1.0f),
		new Color(0.25f, 0.5f, 1.0f),
		new Color(0.25f, 0.5f, 1.0f),
		new Color(0.25f, 0.5f, 1.0f),
	};
	private static readonly Color[] Outlines = new Color[]
	{
		new Color(0.75f, 0.5f, 1.0f),
		new Color(0.75f, 0.5f, 1.0f),
		new Color(0.75f, 0.5f, 1.0f),
		new Color(0.75f, 0.5f, 1.0f),
		new Color(0.75f, 0.5f, 1.0f),
		new Color(0.75f, 0.5f, 1.0f),
	};

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
	private void ResizeUV(Texture2D newTexture)
	{

	}
}