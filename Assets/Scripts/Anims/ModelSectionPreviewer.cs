using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ModelSectionPreviewer : MonoBehaviour
{
	public ModelPiecePreviewer PiecePreviewPrefab;
	public string PartName = "";
	public List<ModelPiecePreviewer> piecePreviews = new List<ModelPiecePreviewer>();

    public void SetSection(Model.Section section, Model model)
	{
		PartName = Utils.ConvertPartName(section.partName);
		name = $"{PartName}";
		foreach(ModelPiecePreviewer piecePreviewer in piecePreviews)
			DestroyImmediate(piecePreviewer.gameObject);
		piecePreviews.Clear();

		foreach(Model.Piece piece in section.pieces)
		{
			ModelPiecePreviewer preview = Instantiate(PiecePreviewPrefab);
			preview.transform.SetParent(transform);
			preview.transform.localPosition = Vector3.zero;
			preview.transform.localRotation = Quaternion.identity;
			preview.transform.localScale = Vector3.one;
			preview.SetPiece(piece, model);
			piecePreviews.Add(preview);
		}
	}
}
