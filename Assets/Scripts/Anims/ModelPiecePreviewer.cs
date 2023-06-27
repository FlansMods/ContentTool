using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ModelPiecePreviewer : MonoBehaviour
{
	private MeshRenderer mr;
	private MeshFilter mf;
	private Mesh mesh;

	public Model.Piece Piece;

	private void CheckInit()
	{
		if(mesh == null)
			mesh = new Mesh();
		if(mf == null)
			mf = gameObject.AddComponent<MeshFilter>();
		mr = GetComponent<MeshRenderer>();
		if(mr == null)
			mr = gameObject.AddComponent<MeshRenderer>();

	}

    public void SetPiece(Model.Piece piece, Model model)
	{
		Piece = piece;
		CheckInit();
		piece.ExportToMesh(mesh, model.textureX, model.textureY);
		mesh.name = "PieceMesh";
		transform.localEulerAngles = piece.Euler;
		transform.localPosition = piece.Origin;
		//mesh.SetVertices(piece.GetVerts());
		//mesh.SetTriangles(piece.GetTris(), 0);
		mf.mesh = mesh;
	}
}
