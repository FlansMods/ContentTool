using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class JavaModelImporter
{
    public static readonly int[] aiPermutation = { 0, 1, 4, 5, 3, 2, 7, 6 };

	public static Vector3[] MirrorOffsets(Vector3[] offsets, bool bX, bool bY, bool bZ)
	{
		Vector3 v0 = offsets[(bX ? 1 : 0) + (bY ? 2 : 0) + (bZ ? 4 : 0)];
		Vector3 v1 = offsets[(bX ? 0 : 1) + (bY ? 2 : 0) + (bZ ? 4 : 0)];
		Vector3 v2 = offsets[(bX ? 1 : 0) + (bY ? 0 : 2) + (bZ ? 4 : 0)];
		Vector3 v3 = offsets[(bX ? 0 : 1) + (bY ? 0 : 2) + (bZ ? 4 : 0)];
		Vector3 v4 = offsets[(bX ? 1 : 0) + (bY ? 2 : 0) + (bZ ? 0 : 4)];
		Vector3 v5 = offsets[(bX ? 0 : 1) + (bY ? 2 : 0) + (bZ ? 0 : 4)];
		Vector3 v6 = offsets[(bX ? 1 : 0) + (bY ? 0 : 2) + (bZ ? 0 : 4)];
		Vector3 v7 = offsets[(bX ? 0 : 1) + (bY ? 0 : 2) + (bZ ? 0 : 4)];
		//return offsets;
		return new Vector3[]
		{
			new Vector3(bX ? -v0.x : v0.x, bY ? -v0.y : v0.y, bZ ? -v0.z : v0.z),
			new Vector3(bX ? -v1.x : v1.x, bY ? -v1.y : v1.y, bZ ? -v1.z : v1.z),
			new Vector3(bX ? -v2.x : v2.x, bY ? -v2.y : v2.y, bZ ? -v2.z : v2.z),
			new Vector3(bX ? -v3.x : v3.x, bY ? -v3.y : v3.y, bZ ? -v3.z : v3.z),
			new Vector3(bX ? -v4.x : v4.x, bY ? -v4.y : v4.y, bZ ? -v4.z : v4.z),
			new Vector3(bX ? -v5.x : v5.x, bY ? -v5.y : v5.y, bZ ? -v5.z : v5.z),
			new Vector3(bX ? -v6.x : v6.x, bY ? -v6.y : v6.y, bZ ? -v6.z : v6.z),
			new Vector3(bX ? -v7.x : v7.x, bY ? -v7.y : v7.y, bZ ? -v7.z : v7.z),
		};
	}

	public static Model CreateBoxModel(string modName, BoxType box)
	{
		Model model = new Model();
		model.Type = Model.ModelType.Block;
		model.top = Utils.ToLowerWithUnderscores(box.topTexturePath);
		model.bottom = Utils.ToLowerWithUnderscores(box.bottomTexturePath);
		model.north = Utils.ToLowerWithUnderscores(box.sideTexturePath);
		model.east = Utils.ToLowerWithUnderscores(box.sideTexturePath);
		model.south = Utils.ToLowerWithUnderscores(box.sideTexturePath);
		model.west = Utils.ToLowerWithUnderscores(box.sideTexturePath);
		return model;
	}

   	public static Model ImportJava(string path, InfoType optionalType)
	{
		Model model = new Model();
		model.Type = Model.ModelType.TurboRig;

		TypeFile file = new TypeFile(path);
		using(StreamReader reader = new StreamReader(path, Encoding.Default))
		{
			string line = null;
			do
            {
                line = reader.ReadLine();
                if (line != null)
                {
                    file.addLine(line);
                }
            }
            while (line != null);
		}

		for (;;)
		{
			string line = file.readLine();
			if (line == null)
				break;
			// Strip whitespace
			line = line.Trim(' ', '\t');
			// Then ignore comment lines
			if (line.StartsWith("//"))
				continue;

			ReadLine(model, file, line, optionalType);
		}

		if(optionalType != null)
			model.name = Utils.ToLowerWithUnderscores(optionalType.shortName);

		Debug.Log($"Imported model {model.name} with {model.sections.Count} parts from {path} with {file.lines.Count} lines");

		return model;
	}

	private static void ReadLine(Model model, TypeFile file, string line, InfoType optionalType)
	{
		// Skip simple / unimportant lines
		if (line == null || line.Length == 0) return;
		if (line.StartsWith("import")) return;
		if (line.StartsWith("public")) return;
		if (line.StartsWith("{") || line.StartsWith("}")) return;
		if (line.StartsWith("package")) return;

		 // After this point, every line should have a ; in it so add further lines until we get it
		while (!line.Contains(";"))
		{
			// Get a new line, strip it, skip comments
			string newLine = file.readLine();
			if (newLine == null)
			{
				Debug.Assert(false, "BAD .ADD! Hit end of file!");
				return;
			}
			newLine = newLine.Trim(' ', '\t');
			if (newLine.StartsWith("//")) continue;

			line = line + newLine;
		}

		// int textureX = 45;
		if (line.StartsWith("int"))
		{
			string[] split = line.Split(' ');
			if (split.Length == 4)
			{
				if (split[1].Equals("textureX")) model.textureX = int.Parse(split[3].Trim(';'));
				if (split[1].Equals("textureY")) model.textureY = int.Parse(split[3].Trim(';'));
			}
			else Debug.Assert(false, $"Invalid length textureX/Y string '{line}' in {file.name}");
			return;
		}

		// gunModel = new ModelRendererTurbo[11];
		int iIndex = line.IndexOf("Model = new Model");
		if(iIndex == -1)
		{
			iIndex = line.IndexOf("Models = new Model");
		}
		if (iIndex != -1)
		{
			string pieceName = line.Substring(0, iIndex);
			int iIndexOfOpenBrace = line.IndexOf('[');
			int iIndexOfClosedBrace = line.IndexOf(']');
			int iNumPieces = int.Parse(line.Substring(iIndexOfOpenBrace + 1, iIndexOfClosedBrace - iIndexOfOpenBrace - 1));
			model.sections.Add(new Model.Section()
			{
				partName = pieceName, 
				pieces = new Model.Piece[iNumPieces]
			});
			return;
		}

		// Model setup lines
		if (line.Contains("Model["))
		{
			int iIndexOfOpenBrace = line.IndexOf('[');
			int iIndexOfClosedBrace = line.IndexOf(']');
			int iPieceNum = 0;
			try
			{
				iPieceNum = int.Parse(line.Substring(iIndexOfOpenBrace + 1, iIndexOfClosedBrace - iIndexOfOpenBrace - 1));
			}
			catch(Exception e)
			{
				Debug.Log("Failed to parse model piece number :" + line.Substring(iIndexOfOpenBrace + 1, iIndexOfClosedBrace - iIndexOfOpenBrace - 1));
				Debug.Log(e.StackTrace);
				return;
			}
			string pieceName = line.Substring(0, iIndexOfOpenBrace - 5); // 5 = Length("Model")
			Model.Section section = model.GetSection(pieceName);
			if(section == null)
			{
				Debug.Assert(false, "Failed to find key " + pieceName + " in model " + file.name);
				return;
			}

			// defaultStockModel[0] = new ModelRendererTurbo(this, 27, 10, textureX, textureY);
			iIndex = line.IndexOf("] = new Model");
			if (iIndex != -1)
			{
				int iThis = line.IndexOf("(this,");
				string coordString = line.Substring(iThis + "(this,".Length);
				int[] coords = Utils.ParseInts(2, coordString);
				section.pieces[iPieceNum] = new Model.Piece()
				{
					textureU = coords[0],
					textureV = coords[1],
				};
				return;
			}

			Model.Piece wrapper = section.pieces[iPieceNum];
			Debug.Assert(wrapper != null, $"Could not find model piece {iPieceNum} in section {section.partName} of {file.name}");
			if(wrapper == null)
			{
				return;
			}

			// defaultScopeModel[0].addBox(-2F, 4.5F, -0.5F, 6, 1, 1);
			// defaultScopeModel[1].addTrapezoid(4F, 4F, -1F, 4, 2, 2, 0F, -0.5F, ModelRendererTurbo.MR_RIGHT);
			// ammoModel[0].addShapeBox(3F, -2F, -1F, 3, 2, 2, 0F, /* 0 */ 0F, 1F, 0F, /* 1 */ 0F, 0F, 0F, /* 2 */ 0F, 0F, 0F, /* 3 */ 0F, 1F, 0F, /* 4 */ 0F, 0F, 0F, /* 5 */ 0F, 0F, 0F, /* 6 */ 0F, 0F, 0F, /* 7 */ 0F, 0F, 0F);
			iIndex = line.IndexOf("].add");
			if (iIndex != -1)
			{
				if (line.Contains("ShapeBox"))
				{
					float[] floats = Utils.ParseFloats(31, line.Substring(iIndex + "].addShapeBox(".Length));
					wrapper.Pos = new Vector3(floats[0], floats[1], floats[2]);
					wrapper.Dim = new Vector3(floats[3], floats[4], floats[5]);
					// floats[6] What is this? Scale?
					for (int i = 0; i < 8; i++)
					{
						int iPermuted = aiPermutation[i];
						wrapper.Offsets[i] = new Vector3(floats[7 + iPermuted * 3 + 0], floats[7 + iPermuted * 3 + 1], floats[7 + iPermuted * 3 + 2]);
						if ((i & 0x1) == 0) wrapper.Offsets[i].x *= -1;
						if ((i & 0x2) == 0) wrapper.Offsets[i].y *= -1;
						if ((i & 0x4) == 0) wrapper.Offsets[i].z *= -1;
					}
					wrapper.Shape = Model.EShape.ShapeBox;
				}
				else if (line.Contains("Box"))
				{
					float[] floats = Utils.ParseFloats(6, line.Substring(iIndex + "].addBox(".Length));
					wrapper.Pos = new Vector3(floats[0], floats[1], floats[2]);
					wrapper.Dim = new Vector3(floats[3], floats[4], floats[5]);
					wrapper.Shape = Model.EShape.Box;
				}
				else if (line.Contains("Trapezoid"))
				{
					float[] floats = Utils.ParseFloats(8, line.Substring(iIndex + "].addTrapezoid(".Length));
					float expand = floats[6];
					wrapper.Pos = new Vector3(floats[0] - expand, floats[1] - expand, floats[2] - expand);
					wrapper.Dim = new Vector3(floats[3] + 2*expand, floats[4] + 2*expand, floats[5] + 2*expand);
					
					float taper = floats[7];
					if(line.Contains("MR_RIGHT"))
					{
						// expand +x face
						wrapper.Offsets[1] = new Vector3(0f, -taper, -taper);
						wrapper.Offsets[3] = new Vector3(0f, taper, -taper);
						wrapper.Offsets[7] = new Vector3(0f, -taper, taper);
						wrapper.Offsets[5] = new Vector3(0f, taper, taper);
					}
					else if(line.Contains("MR_LEFT"))
					{
						// expand -x face
						wrapper.Offsets[4] = new Vector3(0f, -taper, -taper);
						wrapper.Offsets[6] = new Vector3(0f, taper, -taper);
						wrapper.Offsets[2] = new Vector3(0f, -taper, taper);
						wrapper.Offsets[0] = new Vector3(0f, taper, taper);
					}
					else if(line.Contains("MR_FRONT"))
					{
						// expand +z face
						wrapper.Offsets[5] = new Vector3(-taper, -taper, 0f);
						wrapper.Offsets[7] = new Vector3(taper, -taper, 0f);
						wrapper.Offsets[6] = new Vector3(-taper, taper, 0f);
						wrapper.Offsets[4] = new Vector3(taper, taper, 0f);
					}
					else if(line.Contains("MR_BACK"))
					{
						// expand -z face
						wrapper.Offsets[0] = new Vector3(-taper, -taper, 0f);
						wrapper.Offsets[2] = new Vector3(taper, -taper, 0f);
						wrapper.Offsets[3] = new Vector3(-taper, taper, 0f);
						wrapper.Offsets[1] = new Vector3(taper, taper, 0f);
					}
					else if(line.Contains("MR_TOP"))
					{
						// expand +y face
						wrapper.Offsets[7] = new Vector3(-taper, 0f, -taper);
						wrapper.Offsets[3] = new Vector3(taper, 0f, -taper);
						wrapper.Offsets[2] = new Vector3(-taper, 0f, taper);
						wrapper.Offsets[6] = new Vector3(taper, 0f, taper);
					}
					else if(line.Contains("MR_BOTTOM"))
					{
						// expand -y face
						wrapper.Offsets[5] = new Vector3(-taper, 0f, -taper);
						wrapper.Offsets[4] = new Vector3(taper, 0f, -taper);
						wrapper.Offsets[0] = new Vector3(-taper, 0f, taper);
						wrapper.Offsets[1] = new Vector3(taper, 0f, taper);
					}
					wrapper.Shape = Model.EShape.ShapeBox;
				}

				return;
			}

			// gunModel[4].setRotationPoint(0F, 1F, 0F);
			iIndex = line.IndexOf("setRotationPoint");
			if (iIndex != -1)
			{
				float[] floats = Utils.ParseFloats(3, line.Substring(iIndex));
				wrapper.Origin = new Vector3(floats[0], floats[1], floats[2]);
				return;
			}

			// gunModel[4].rotateAngleZ = -0.5F;
			iIndex = line.IndexOf("rotateAngle");
			if (iIndex != -1)
			{
				char cAxis = line[iIndex + "rotateAngle".Length];

				float[] floats = Utils.ParseFloats(1, line.Substring(iIndex + "rotateAngle".Length));
				switch(cAxis)
				{
					case 'X': wrapper.Euler.x = floats[0];   break;
					case 'Y': wrapper.Euler.y = floats[0];   break;
					case 'Z': wrapper.Euler.z = floats[0];   break;
				}
				return;
			}

			// gunModel[4].doMirror(false, true, true);
			iIndex = line.IndexOf("doMirror");
			if (iIndex != -1)
			{
				line = line.Substring(iIndex);
				int iFirstComma = line.IndexOf(',');
				int iSecondComma = line.IndexOf(',');

				bool bMirrorX = line.Substring(0, iFirstComma).Contains("true");
				bool bMirrorY = line.Substring(iFirstComma, iSecondComma).Contains("true");
				bool bMirrorZ = line.Substring(iSecondComma).Contains("true");

				wrapper.DoMirror(bMirrorX, bMirrorY, bMirrorZ);
				return;
			}

		}

		// translateAll
		iIndex = line.IndexOf("translateAll");
		if(iIndex != -1)
		{							
			float[] floats = Utils.ParseFloats(3, line.Substring(iIndex));

			foreach(Model.Section section in model.sections)
			{
				foreach(Model.Piece wrapper in section.pieces)
				{
					if (wrapper != null)
					{
						wrapper.Origin.x += floats[0];
						wrapper.Origin.y += floats[1];
						wrapper.Origin.z += floats[2];
					}
				}
			}
			return;
		}

		// flipAll
		iIndex = line.IndexOf("flipAll");
		if (iIndex != -1)
		{						
			foreach (Model.Section section in model.sections)
			{
				foreach(Model.Piece piece in section.pieces)
				{
					piece.DoMirror(false, true, true);
				}
				/*
				Model.Piece[] copies = new Model.Piece[section.pieces.Length * 2];
				for(int i = 0; i < section.pieces.Length; i++)
				{
					if (section.pieces[i] != null)
					{
						copies[i*2] = section.pieces[i];
						copies[i*2+1] = section.pieces[i].Copy();
						copies[i*2+1].DoMirror(false, true, true);
					}
				}

				section.pieces = copies;
				*/
			}
			return;
		}

		if(optionalType != null)
		{
			TxtImport.ImportFromModel(line, DefinitionTypes.GetFromObject(optionalType), model, optionalType);
			return;
		}

		if(line.Contains("for") || line.Contains("GlStateManager"))
			return;

		Debug.Assert(false, "Hit bad line in model (" + file.name + "): " + line);
	}
}
