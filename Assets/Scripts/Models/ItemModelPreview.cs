using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemModelPreview : MinecraftModelPreview
{
	public ItemModel Item { get { return Model as ItemModel; } }

	public override void GenerateMesh()
	{
		Texture2D icon = Item.GetIcon();
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();
		List<Vector3> normals = new List<Vector3>();
		for (int i = 0; i < icon.width; i++)
		{
			for (int j = 0; j < icon.height; j++)
			{
				Color pixel = icon.GetPixel(i, j);
				if(pixel.a > 0.0f)
				{
					tris.Add(verts.Count);
					tris.Add(verts.Count + 1);
					tris.Add(verts.Count + 2);
					tris.Add(verts.Count);
					tris.Add(verts.Count + 2);
					tris.Add(verts.Count + 3);

					verts.Add(new Vector3(i, j, 0));
					verts.Add(new Vector3(i + 1, j, 0));
					verts.Add(new Vector3(i + 1, j + 1, 0));
					verts.Add(new Vector3(i, j + 1, 0));
					uvs.Add(new Vector2(i / (float)icon.width, j / (float)icon.height));
					uvs.Add(new Vector2((i+1) / (float)icon.width, j / (float)icon.height));
					uvs.Add(new Vector2((i+1) / (float)icon.width, (j+1) / (float)icon.height));
					uvs.Add(new Vector2(i / (float)icon.width, (j+1) / (float)icon.height));
					for (int n = 0; n < 4; n++)
						normals.Add(Vector3.forward);
				}

			}
		}
		this.Mesh.SetVertices(verts);
		this.Mesh.SetUVs(0, uvs);
		this.Mesh.SetTriangles(tris, 0);
		this.Mesh.SetNormals(normals);
	}
}
