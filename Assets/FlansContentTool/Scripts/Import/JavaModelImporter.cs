using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class JavaModelImporter
{
	private static readonly string VariableNameCapture = "([A-Za-z][A-Za-z0-9_]*)";
	private static readonly string BoolCapture = "([Tt]rue|[Ff]alse)";
	private static readonly string IntCapture = "(-?[0-9]*)";
	private static readonly string AdvancedFloatCapture = "(?:\\/\\*[0-9 ]*\\*\\/)?\\s*([\\-0-9.\\/\\*Ff]*)";
	private static readonly string WhitespaceCommaCapture = "\\s*,\\s*";

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

	public static VanillaCubeRootNode ImportBlock(string modName, BoxType box)
	{
		GameObject go = new GameObject(Minecraft.SanitiseID(box.shortName));
		VanillaCubeRootNode cubeNode = go.AddComponent<VanillaCubeRootNode>();

		cubeNode.AddDefaultTransforms();

		ResourceLocation side = new ResourceLocation(modName, Utils.ToLowerWithUnderscores(box.sideTexturePath));
		ResourceLocation top = new ResourceLocation(modName, Utils.ToLowerWithUnderscores(box.topTexturePath));
		ResourceLocation bottom = new ResourceLocation(modName, Utils.ToLowerWithUnderscores(box.bottomTexturePath));

		cubeNode.Textures.Add(new NamedTexture("up", top));
		cubeNode.Textures.Add(new NamedTexture("down", bottom));
		cubeNode.Textures.Add(new NamedTexture("north", side));
		cubeNode.Textures.Add(new NamedTexture("east", side));
		cubeNode.Textures.Add(new NamedTexture("west", side));
		cubeNode.Textures.Add(new NamedTexture("south", side));
		cubeNode.Textures.Add(new NamedTexture("particle", side));

		return cubeNode;
	}

	private static UVMap TempUVStorage = null;
	public static TurboRootNode ImportJavaModel(string javaPath, IVerificationLogger logger = null)
	{
		GameObject go = new GameObject($"Temp: Importing {javaPath}");
		TurboRootNode rootNode = go.AddComponent<TurboRootNode>();
		// TODO: rootNode.AddDefaultTransforms();
		TempUVStorage = new UVMap(); 

		try
		{
			using (StreamReader reader = new StreamReader(javaPath, Encoding.Default))
			{
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					// Strip initial and trailing whitespace
					line = line.Trim(' ', '\t');
					// Then ignore comment lines and stuff we don't need
					if (line.Length == 0
					|| line.StartsWith("//")
					|| line.StartsWith("import")
					|| line.StartsWith("public")
					|| line.StartsWith("{")
					|| line.StartsWith("}")
					|| line.StartsWith("package"))
						continue;

					// After this point, every line should have a ; in it so add further lines until we get it
					while (!line.Contains(";"))
					{
						if(reader.EndOfStream)
						{
							logger?.Failure($"Found a multi-line without a semicolon '{line}' in {javaPath}");
							break;
						}
						// Get a new line, strip it, skip comments
						string nextLine = reader.ReadLine();
						nextLine = nextLine.Trim(' ', '\t');
						if (nextLine.StartsWith("//")) 
							continue;

						// Let's go with just a space. Seems safest for our parsers as written
						line = $"{line} {nextLine}";
					}

					// Now process our line
					bool matched = MatchPieceOperation(rootNode, line)
								|| MatchTextureSize(rootNode, line)
								|| MatchPieceSetValue(rootNode, line)
								|| MatchSetParameter(rootNode, line)
								|| MatchSetVec3Parameter(rootNode, line)
								|| MatchNewMRTurbo(rootNode, line)
								|| MatchPiece2DOperation(rootNode, line)
								|| MatchNewMR2DTurbo(line)
								|| MatchGlobalFunc(rootNode, line)
								|| MatchArrayInitializerTurbo(line);
					if(!matched)
					{
						logger?.Neutral($"Could not match line '{line}' when importing '{javaPath}'");
					}
				}
			}
		}
		catch (Exception e)
		{
			logger?.Failure($"Failed to find Java model {javaPath} because of {e.Message}");
            UnityEngine.Object.DestroyImmediate(go);
			return null;
		}

		// ----------------------------------------------------------------------------
		// Post-read functions
		// ----------------------------------------------------------------------------
		List<GeometryNode> nodesWithTempParents = new List<GeometryNode>();
		// #1 Move temporary UV storage into GeometryNodes
		foreach (SectionNode sectionNode in rootNode.GetAllDescendantNodes<SectionNode>())
			foreach (GeometryNode geomNode in sectionNode.GetAllDescendantNodes<GeometryNode>())
			{
				BoxUVPlacement placement = TempUVStorage.GetPlacedPatch($"{sectionNode.PartName}/{geomNode.name.Substring("part_".Length)}");
				if (placement != null)
					geomNode.BakedUV = new RectInt(placement.Origin, geomNode.BoxUVBounds);
				else
					logger?.Failure($"Failed to match a UV patch for {geomNode.name}");

				if (geomNode.transform.parent != null && geomNode.ParentNode is EmptyNode emptyNode)
					nodesWithTempParents.Add(geomNode);
			}
		TempUVStorage.Clear();
		
		// #2 Bake in the (origin > rotate > pos) = (pos, rot)
		foreach(GeometryNode geomNode in nodesWithTempParents)
		{
			// Set parent to our grandparent, while keeping world position
			// In theory, this is equivalent to removing the TEMP object from in-between
			Transform tempParent = geomNode.transform.parent;
			geomNode.transform.SetParent(geomNode.transform.parent.parent, true);

			// On the off chance we somehow get other stuff attached here, warn?
			if(tempParent.childCount == 0)
                UnityEngine.Object.DestroyImmediate(tempParent.gameObject);
			else
				logger?.Failure($"{tempParent} temporary object has multiple children?");
		}

		// #3 Attach our Sections to our AttachPoints
		// Also, section nodes should be at 0,0,0 local space. Bake this in
		// List must be cached so we can start modifying the heirarchy
		List<SectionNode> sections = new List<SectionNode>(rootNode.GetAllDescendantNodes<SectionNode>());
		foreach(SectionNode sectionNode in sections)
		{
			if (sectionNode.name != "body")
			{
				// Here, we keep world pos, which is kinda how the old APs used to work
				// They would un-translate, rotate, then translate
				AttachPointNode apNode = ConvertToNodes.GetOrCreateAttachPointNode(rootNode, sectionNode.name);
				sectionNode.transform.SetParent(apNode.transform, true);
			}

			if (!sectionNode.LocalOrigin.Approximately(Vector3.zero))
				sectionNode.transform.TranslateButNotChildren(-sectionNode.LocalOrigin);

			sectionNode.Rotate(new Vector3(0f, 90f, 0f));
		}

		rootNode.BakeOutSectionTransforms();


		// ----------------------------------------------------------------------------

		return rootNode;
	}

	// int textureX = 45;
	private static readonly Regex TextureSizeRegex = new Regex($"int\\s*texture(X|Y)\\s*=\\s*{IntCapture};");
	public static bool MatchTextureSize(TurboRootNode rootNode, string line)
	{
		Match match = TextureSizeRegex.Match(line);
		if (match.Success)
		{
			if (match.Groups[1].Value == "X")
				rootNode.UVMapSize.x = int.Parse(match.Groups[2].Value);
			else if (match.Groups[1].Value == "Y")
				rootNode.UVMapSize.y = int.Parse(match.Groups[2].Value);
			return true;
		}
		return false;
	}

	// defaultStockModel = new ModelRendererTurbo[3];
	private static readonly Regex ArrayInitializerRegex = new Regex($"{VariableNameCapture}\\s*=\\s*new\\s*ModelRenderer(?:Turbo)?\\[{IntCapture}\\];");
	public static bool MatchArrayInitializerTurbo(string line)
	{
		return ArrayInitializerRegex.Match(line).Success;
	}
	// defaultStockModel[0][1] = new ModelRendererTurbo(this, 27, 10, textureX, textureY);
	private static readonly Regex NewMR2DRegex = new Regex("([a-zA-Z0-9_]*)Model(?:s)?\\[([0-9]*)\\]\\[([0-9]*)\\]\\s*=\\s*new ModelRenderer(?:Turbo)?\\s*\\(this,\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchNewMR2DTurbo(string line)
	{
		Match match = NewMR2DRegex.Match(line);
		if (match.Success)
		{
			// Get the right section
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index_0 = int.Parse(match.Groups[2].Value);
			int index_1 = int.Parse(match.Groups[3].Value);

			// Set the right piece UVs
			List<float> floats = ResolveParameters(match.Groups[4].Value, 2);
			if (floats.Count == 2)
			{
				int u = Mathf.FloorToInt(floats[0]);
				int v = Mathf.FloorToInt(floats[1]);
				TempUVStorage.SetUVPlacement($"{partName}_{index_0}/{index_1}", new Vector2Int(u, v));
				return true;
			}
		}
		return false;
	}
	// defaultStockModel[0] = new ModelRendererTurbo(this, 27, 10, textureX, textureY);
	private static readonly Regex NewMRRegex = new Regex("([a-zA-Z0-9_]*)Model(?:s)?\\[([0-9]*)\\]\\s*=\\s*new ModelRenderer(?:Turbo)?\\s*\\(this,\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchNewMRTurbo(TurboRootNode rootNode, string line)
	{
		Match match = NewMRRegex.Match(line);
		if (match.Success)
		{
			// Get the right section
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			// Get the right piece
			int index = int.Parse(match.Groups[2].Value);
			List<float> floats = ResolveParameters(match.Groups[3].Value, 2);
			if (floats.Count == 2)
			{
				int u = Mathf.FloorToInt(floats[0]);
				int v = Mathf.FloorToInt(floats[1]);
				TempUVStorage.SetUVPlacement($"{partName}/{index}", new Vector2Int(u, v));
				return true;
			}
		}
		return false;
	}
	// gunModels[0][1].addBox(... / addTrapezoid(... / add...
	private static readonly Regex PieceOperation2DRegex = new Regex("([A-Za-z]*)Model(?:s)?\\[([0-9]*)\\]\\[([0-9]*)\\].([a-zA-Z]*)\\(([-0-9A-Za-z\\/\\*\\,\\.\\s_]*)\\);");
	public static bool MatchPiece2DOperation(TurboRootNode rootNode, string line)
	{
		Match match = PieceOperation2DRegex.Match(line);
		if (match.Success)
		{
			// Get or Create the right section
			string partName = Minecraft.ConvertPartName(match.Groups[1].Value);
			int index_0 = int.Parse(match.Groups[2].Value);
			int index_1 = int.Parse(match.Groups[3].Value);
			string sectionName = $"{partName}_{index_0}";
			string pieceName = $"part_{index_1}";
			SectionNode sectionNode = ConvertToNodes.GetOrCreateSectionNode(rootNode, sectionName);

			// Then run the "addX" function
			string function = match.Groups[4].Value;
			string parameters = match.Groups[5].Value;
			switch (function)
			{
				case "addBox":				return AddBox(sectionNode, pieceName, parameters);
				case "addTrapezoid":		return AddTrapezoid(sectionNode, pieceName, parameters);
				case "addShapeBox":			return AddShapebox(sectionNode, pieceName, parameters);
				case "addShape3D":			return AddShape3D(sectionNode, pieceName, parameters);
				case "setRotationPoint":	return SetRotationPoint(sectionNode, pieceName, parameters);
				case "setPosition":			return SetPosition(sectionNode, pieceName, parameters);
				case "doMirror":			return DoMirror(sectionNode, pieceName, parameters);
				// This is fine to skip
				case "render": return true;
				default:
				{
					Debug.LogWarning($"Unknown piece operation '{function}' in line '{line}'");
					return false;
				}
			}

		}
		return false;
	}

	private static readonly Regex PieceOperationRegex = new Regex("([A-Za-z]*)Model(?:s)?\\[([0-9]*)\\].([a-zA-Z]*)\\(([-0-9A-Za-z\\/\\*\\,\\.\\s_]*)\\);");
	public static bool MatchPieceOperation(TurboRootNode rootNode, string line)
	{
		Match match = PieceOperationRegex.Match(line);
		if (match.Success)
		{
			// Get or Create the right section
			string sectionName = Minecraft.ConvertPartName(match.Groups[1].Value);
			SectionNode sectionNode = ConvertToNodes.GetOrCreateSectionNode(rootNode, sectionName);
			string pieceName = $"part_{int.Parse(match.Groups[2].Value)}";

			// Then run the function
			string function = match.Groups[3].Value;
			string parameters = match.Groups[4].Value;
			switch(function)
			{
				case "addBox":				return AddBox(sectionNode, pieceName, parameters);
				case "addTrapezoid":		return AddTrapezoid(sectionNode, pieceName, parameters);
				case "addShapeBox":			return AddShapebox(sectionNode, pieceName, parameters);
				case "addShape3D":			return AddShape3D(sectionNode, pieceName, parameters);
				case "setRotationPoint":	return SetRotationPoint(sectionNode, pieceName, parameters);
				case "setPosition":			return SetPosition(sectionNode, pieceName, parameters);
				case "doMirror":			return DoMirror(sectionNode, pieceName, parameters);
				// This is fine to skip
				case "render": return true;
				default:
				{ 
					Debug.LogWarning($"Unknown piece operation '{function}' in line '{line}'");
					return false;
				}
			}
		}
		return false;
	}

	public static bool AddBox(SectionNode sectionNode, string pieceName, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count >= 6)
		{
			float ex = floats.Count >= 7 ? floats[6] : 0.0f;
			Vector3 pos = new Vector3(floats[0] - ex, floats[1] - ex, floats[2] - ex);
			Vector3 dim = new Vector3(floats[3] + 2 * ex, floats[4] + 2 * ex, floats[5] + 2 * ex);

			BoxGeometryNode boxNode = ConvertToNodes.GetOrCreateGeometryNode<BoxGeometryNode>(sectionNode, pieceName);
			boxNode.transform.localPosition = pos;
			boxNode.Dim = dim;
			return true;
		}
		return false;
	}

	public static bool AddTrapezoid(SectionNode sectionNode, string pieceName, string parameters)
	{
		List<float> floats = ResolveParameters(parameters, 8);
		if (floats.Count >= 8)
		{
			float ex = floats[6];
			float taper = floats[7];
			ShapeboxGeometryNode shapeboxNode = ConvertToNodes.GetOrCreateGeometryNode<ShapeboxGeometryNode>(sectionNode, pieceName);
			shapeboxNode.LocalOrigin = new Vector3(floats[0] - ex, floats[1] - ex, floats[2] - ex);
			shapeboxNode.Dim = new Vector3(floats[3] + 2 * ex, floats[4] + 2 * ex, floats[5] + 2 * ex);
			
			if (parameters.Contains("MR_LEFT"))
			{
				// expand +x face
				shapeboxNode.Offsets[1] = new Vector3(0f, -taper, -taper);
				shapeboxNode.Offsets[3] = new Vector3(0f, taper, -taper);
				shapeboxNode.Offsets[7] = new Vector3(0f, taper, taper);
				shapeboxNode.Offsets[5] = new Vector3(0f, -taper, taper);
			}
			else if (parameters.Contains("MR_RIGHT"))
			{
				// expand -x face
				shapeboxNode.Offsets[4] = new Vector3(0f, -taper, taper);
				shapeboxNode.Offsets[6] = new Vector3(0f, taper, taper);
				shapeboxNode.Offsets[2] = new Vector3(0f, taper, -taper);
				shapeboxNode.Offsets[0] = new Vector3(0f, -taper, -taper);
			}
			else if (parameters.Contains("MR_BACK"))
			{
				// expand +z face
				shapeboxNode.Offsets[5] = new Vector3(taper, -taper, 0f);
				shapeboxNode.Offsets[7] = new Vector3(taper, taper, 0f);
				shapeboxNode.Offsets[6] = new Vector3(-taper, taper, 0f);
				shapeboxNode.Offsets[4] = new Vector3(-taper, -taper, 0f);
			}
			else if (parameters.Contains("MR_FRONT"))
			{
				// expand - z face
				shapeboxNode.Offsets[0] = new Vector3(-taper, -taper, 0f);
				shapeboxNode.Offsets[2] = new Vector3(-taper, taper, 0f);
				shapeboxNode.Offsets[3] = new Vector3(taper, taper, 0f);
				shapeboxNode.Offsets[1] = new Vector3(taper, -taper, 0f);
			}
			else if (parameters.Contains("MR_BOTTOM"))
			{
				// expand +y face
				shapeboxNode.Offsets[7] = new Vector3(taper, 0f, taper);
				shapeboxNode.Offsets[3] = new Vector3(taper, 0f, -taper);
				shapeboxNode.Offsets[2] = new Vector3(-taper, 0f, -taper);
				shapeboxNode.Offsets[6] = new Vector3(-taper, 0f, taper);
			}
			else if (parameters.Contains("MR_TOP"))
			{
				// expand -y face
				shapeboxNode.Offsets[5] = new Vector3(taper, 0f, taper);
				shapeboxNode.Offsets[4] = new Vector3(-taper, 0f, taper);
				shapeboxNode.Offsets[0] = new Vector3(-taper, 0f, -taper);
				shapeboxNode.Offsets[1] = new Vector3(taper, 0f, -taper);
			}
			return true;
		}
		return false;
	}
	
	public static bool AddShape3D(SectionNode sectionNode, string pieceName, string parameters)
	{
		// TODO: Shape3D
		return true;
	}

	public static bool AddShapebox(SectionNode sectionNode, string pieceName, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count >= 31)
		{
			ShapeboxGeometryNode shapeboxNode = ConvertToNodes.GetOrCreateGeometryNode<ShapeboxGeometryNode>(sectionNode, pieceName);

			shapeboxNode.LocalOrigin = new Vector3(floats[0], floats[1], floats[2]);
			shapeboxNode.Dim = new Vector3(floats[3], floats[4], floats[5]);
			// ignore expand = floats[6]
			for (int i = 0; i < 8; i++)
			{
				int iPermuted = aiPermutation[i];
				shapeboxNode.Offsets[i] = new Vector3(
					floats[7 + iPermuted * 3 + 0],
					floats[7 + iPermuted * 3 + 1],
					floats[7 + iPermuted * 3 + 2]);
				if ((i & 0x1) == 0) shapeboxNode.Offsets[i].x *= -1;
				if ((i & 0x2) == 0) shapeboxNode.Offsets[i].y *= -1;
				if ((i & 0x4) == 0) shapeboxNode.Offsets[i].z *= -1;
			}
			return true;
		}
		return false;
	}

	public static bool SetPosition(SectionNode sectionNode, string pieceName, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count == 3)
		{
			if (sectionNode.TryFindChild(pieceName, out GeometryNode geomNode))
			{
				geomNode.LocalOrigin = new Vector3(floats[0], floats[1], floats[2]);
				return true;
			}
		}
		return false;
	}
	public static bool SetRotationPoint(SectionNode sectionNode, string pieceName, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count >= 3)
		{
			if (sectionNode.TryFindChild(pieceName, out GeometryNode geomNode))
			{
				ConvertToNodes.ApplyTemporaryRotationOrigin(geomNode,
					new Vector3(floats[0], floats[1], floats[2]));
				return true;
			}
		}
		return false;
	}
	public static bool DoMirror(SectionNode sectionNode, string pieceName, string parameters)
	{
		string[] boolParams = parameters.Split(',');
		if(boolParams.Length == 3)
		{
			if (sectionNode.TryFindChild(pieceName, out GeometryNode geomNode))
			{
				if (geomNode.SupportsMirror())
				{
					geomNode.Mirror(bool.Parse(boolParams[0]), bool.Parse(boolParams[1]), bool.Parse(boolParams[2]));
					return true;
				}
			}
		}
		return false;
	}
	
	private static readonly Regex PieceSetValueRegex = new Regex("([A-Za-z]*)Model(?:s)?\\[([0-9]*)].([a-zA-Z]*)\\s*=\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s]*);");
	public static bool MatchPieceSetValue(TurboRootNode rootNode, string line)
	{
		Match match = PieceSetValueRegex.Match(line);
		if (match.Success)
		{
			// Get or Create the right section
			string sectionName = Minecraft.ConvertPartName(match.Groups[1].Value);
			SectionNode sectionNode = ConvertToNodes.GetOrCreateSectionNode(rootNode, sectionName);
			string pieceName = $"part_{int.Parse(match.Groups[2].Value)}";
			if (sectionNode.TryFindChild(pieceName, out GeometryNode geomNode))
			{
				// Then run the function
				string field = match.Groups[3].Value;
				string setValue = match.Groups[4].Value;
				switch (field)
				{
					case "rotateAngleX":	return SetRotateAngleX(geomNode, setValue);
					case "rotateAngleY":	return SetRotateAngleY(geomNode, setValue);
					case "rotateAngleZ":	return SetRotateAngleZ(geomNode, setValue);
					case "flip":			return SetFlip(geomNode, setValue);
					default:
					{
						Debug.LogWarning($"Unknown piece set value '{field}' in line '{line}'");
						return false;
					}
				}
			}
			else Debug.LogWarning($"Encountered line '{line}' before piece '{sectionName}/{pieceName}' was created");
		}
		return false;
	}
	public static bool SetRotateAngleX(GeometryNode piece, string setValue)
	{
		List<float> floats = ResolveParameters(setValue, 1);
		if (floats.Count == 1)
		{
			ConvertToNodes.ApplyTemporaryRotationAngleX(piece, floats[0] * Mathf.Rad2Deg);
			return true;
		}
		return false;
	}
	public static bool SetRotateAngleY(GeometryNode piece, string setValue)
	{
		List<float> floats = ResolveParameters(setValue, 1);
		if (floats.Count == 1)
		{
			ConvertToNodes.ApplyTemporaryRotationAngleY(piece, floats[0] * Mathf.Rad2Deg);
			return true;
		}
		return false;
	}
	public static bool SetRotateAngleZ(GeometryNode piece, string setValue)
	{
		List<float> floats = ResolveParameters(setValue, 1);
		if (floats.Count == 1)
		{
			ConvertToNodes.ApplyTemporaryRotationAngleZ(piece, floats[0] * Mathf.Rad2Deg);
			return true;
		}
		return false;
	}
	public static bool SetFlip(GeometryNode piece, string setValue)
	{
		bool flip = bool.Parse(setValue);
		// TODO: Hmm, what should this do?		
		return true;
	}
	private static readonly Regex GlobalFuncRegex = new Regex("([A-Za-z]*)\\s*\\(([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchGlobalFunc(TurboRootNode rootNode, string line)
	{
		Match match = GlobalFuncRegex.Match(line);
		if (match.Success)
		{
			// Run the function
			string function = match.Groups[1].Value;
			string parameters = match.Groups[2].Value;

			switch (function)
			{
				case "translateAll": return TranslateAll(rootNode, parameters);
				case "flipAll": return FlipAll(rootNode);
				// This is fine to skip
				case "render": return true;
				case "scale": return true;
				// GL functions also fine to skip
				case "pushMatrix": return true;
				case "popMatrix": return true;
				case "translate": return true;
				case "rotate": return true;
				default:
				{
					Debug.LogWarning($"Unknown global function '{function}' in line '{line}'");
					return false;
				}
			}
		}
		return false;
	}
	public static bool TranslateAll(TurboRootNode rootNode, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count == 3)
		{
			if (rootNode.SupportsTranslate())
			{
				rootNode.Translate(new Vector3(floats[0], floats[1], floats[2]));
				return true;
			}
		}
		return false;
	}
	public static bool FlipAll(TurboRootNode rootNode)
	{
		if (rootNode.SupportsMirror())
		{
			//GameObject.Instantiate(rootNode).name = "Pre-FlipAll";
			rootNode.Mirror(false, true, true);
			//GameObject.Instantiate(rootNode).name = "Post-FlipAll";
			return true;
		}
		return false;
	}

	private static readonly Regex SetParameterRegex = new Regex("([A-Za-z]*)\\s*=\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s_]*);");
	public static bool MatchSetParameter(string line, out string parameter, out string value)
	{
		Match match = SetParameterRegex.Match(line);
		if (match.Success)
		{
			parameter = match.Groups[1].Value;
			value = match.Groups[2].Value;
			return true;
		}
		parameter = "";
		value = "";
		return false;
	}
	public static bool MatchSetFloatParameter(string line, out string parameter, out float value)
	{
		if(MatchSetParameter(line, out parameter, out string strValue))
		{
			return float.TryParse(strValue, out value);
		}
		value = 0.0f;
		return false;
	}
	public static bool MatchSetParameter(TurboRootNode rootNode, string line)
	{
		Match match = SetParameterRegex.Match(line);
		if (match.Success)
		{
			// Find the parameter and set it
			string parameter = match.Groups[1].Value;
			string value = match.Groups[2].Value;

			if (parameter == "animationType")
			{ 
				return true; // Consumed in ContentManager:ImportType_Internal
			}

			List<float> floats = ResolveParameters(value, 1);
			switch (parameter)
			{
				case "untiltGunTime":
				case "tiltGunTime":
				case "unloadClipTime":
				case "loadClipTime":
					return true; // Consumed in ContentManager:ImportType_Internal
				case "gripIsOnPump":
					AttachPointNode gripAP = ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "grip");
					AttachPointNode pumpAP = ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "pump");
					gripAP.transform.SetParent(pumpAP.transform);
					return true;
				case "scopeIsOnSlide":
					{
						AttachPointNode sightsAP = ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "sights");
						AttachPointNode slideAP = ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "slide");
						sightsAP.transform.SetParent(slideAP.transform);
						return true;
					}
				case "scopeIsOnBreakAction":
					{ 
						AttachPointNode sightsAP = ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "sights");
						AttachPointNode breakAP = ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "break_action");
						sightsAP.transform.SetParent(breakAP.transform);
						return true;
					}
				default:
					if (floats.Count == 1)
					{
						rootNode.AnimationParameters.Add(new AnimationParameter()
						{
							key = Utils.ToLowerWithUnderscores(parameter),
							isVec3 = false,
							floatValue = floats[0]
						});
						return true;
					}
					return false;
			}
		}
		return false;
	}

	private static readonly Regex SetVec3ParameterRegex = new Regex("([A-Za-z]*)\\s*=\\s*new\\s*Vector3f\\s*\\(([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchSetVec3Parameter(string line, out string parameterName, out Vector3 vec)
	{
		Match match = SetVec3ParameterRegex.Match(line);
		if (match.Success)
		{
			// Find the parameter and set it
			parameterName = match.Groups[1].Value;
			string value = match.Groups[2].Value;
			List<float> floats = ResolveParameters(value, 3);
			if (floats.Count == 3)
			{
				vec = new Vector3(floats[0] * 16f, floats[1] * 16f, floats[2] * 16f);
				return true;
			}
		}
		parameterName = "";
		vec = Vector3.zero;
		return false;
	}
	public static bool MatchSetVec3Parameter(TurboRootNode rootNode, string line)
	{
		Match match = SetVec3ParameterRegex.Match(line);
		if (MatchSetVec3Parameter(line, out string parameter, out Vector3 pos))
		{
			switch(parameter)
			{
				case "itemFrameOffset":
					ConvertToNodes.GetOrCreateItemPoseNode(rootNode, ItemDisplayContext.FIXED).LocalOrigin = pos;
					break;
				case "thirdPersonOffset":
					ConvertToNodes.GetOrCreateItemPoseNode(rootNode, ItemDisplayContext.THIRD_PERSON_RIGHT_HAND).LocalOrigin = pos;
					ConvertToNodes.GetOrCreateItemPoseNode(rootNode, ItemDisplayContext.THIRD_PERSON_LEFT_HAND).LocalOrigin = pos;
					break;
				case "barrelBreakPoint":
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "break_action").LocalOrigin = pos;
					break;
				case "revolverFlipPoint":
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "revolver_barrel").LocalOrigin = pos;
					break;
				case "minigunBarrelOrigin":
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "minigun_rotator").LocalOrigin = pos;
					break;
				case "stockAttachPoint":
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "stock").LocalOrigin = pos;
					break;
				case "barrelAttachPoint":
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "barrel").LocalOrigin = pos;
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "shoot_origin").LocalOrigin = pos;
					break;
				case "scopeAttachPoint":
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "sights").LocalOrigin = pos;
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "eye_line").LocalOrigin = pos + new Vector3(0, 2, 0);
					break;
				case "gripAttachPoint":
					ConvertToNodes.GetOrCreateAttachPointNode(rootNode, "grip").LocalOrigin = pos;
					break;
				default:
					rootNode.AnimationParameters.Add(new AnimationParameter()
					{
						key = Utils.ToLowerWithUnderscores(parameter),
						isVec3 = true,
						vec3Value = pos,
					});
					break;
			}
			return true;
		}
		return false;
	}

	


	// Accepts standard 0f, 0, 3.f etc, OR "16F / 16F" style
	// // TODO: OR "30f * 180f / 3.14159f"
	private static readonly Regex FloatDivRegex = new Regex("([-0-9.Ff]*)\\s*\\/\\s*([-0-9.Ff]*)");
	public static float ParseFloat(string value)
	{
		Match match = FloatDivRegex.Match(value);
		if (match.Success)
		{
		 /// TODO:!
			return float.Parse(match.Groups[1].Value) / float.Parse(match.Groups[2].Value);
		}
		return float.Parse(value.Replace("f", "").Replace("F", ""));
	}

	public static List<float> ResolveParameters(string parameters, int max = 100)
	{
		parameters = parameters.Replace("(float)Math.PI", "3.14159265");
		List<float> splits = new List<float>();
		float accumulated = 0.0f;
		bool divideMode = false;
		bool multiplyMode = false;
		int currentFloatStart = 0;
		for (int ch = 0; ch < parameters.Length + 1; ch++)
		{
			char nextChar = ch == parameters.Length ? ',' : parameters[ch];
			if (('0' <= nextChar && nextChar <= '9') || nextChar == '.')
				continue;
			if (nextChar == '-')
			{
				if (currentFloatStart != ch)
					Debug.LogWarning($"Found - char mid-float in {parameters} at index:{ch}");
				continue;
			}

			// At this point, we should finish the float we are on
			if (ch > currentFloatStart)
			{
				float component = 0.0f;
				string floatToParse = parameters.Substring(currentFloatStart, ch - currentFloatStart);
				if (!float.TryParse(floatToParse, out component))
				{
					Debug.LogError($"Failed to parse '{floatToParse}' as a float from parameters '{parameters}'");
				}
				if (multiplyMode)
					accumulated *= component;
				else if (divideMode)
					accumulated /= component;
				else
					accumulated = component;
				multiplyMode = false;
				divideMode = false;
			}
			currentFloatStart = ch + 1;

			if (nextChar == '*')
				multiplyMode = true;
			if (nextChar == '/')
				divideMode = true;
			if (nextChar == ',')
			{
				splits.Add(accumulated);
				accumulated = 0.0f;
			}
			if (nextChar == ')')
				break;
			if (splits.Count >= max)
				break;
		}

		return splits;
	}
}
