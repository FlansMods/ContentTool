using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class JavaModelImporter
{
	private static readonly string VariableNameCapture = "([A-Za-z][A-Za-z0-9_]*)";
	private static readonly string BoolCapture = "([Tt]rue|[Ff]alse)";
	private static readonly string IntCapture = "(-?[0-9]*)";
	private static readonly string AdvancedFloatCapture = "(?:\\/\\*[0-9 ]*\\*\\/)?\\s*([\\-0-9.\\/\\*Ff]*)";
	private static readonly string WhitespaceCommaCapture = "\\s*,\\s*";
	private static readonly Regex RegexExpanderRegex = new Regex("(.*)%(var)%(.*)%(index)%(.*)%([0-9]*(?:xFloat|xBool))%(.*)|(.*)%(var)%(.*)%([0-9]*(?:xFloat|xBool))%(.*)|(.*)%([0-9]*(?:xFloat|xBool))%(.*)");
	private static Regex CreateRegex(string src)
	{
		// "%var%[%index%].addShapeBox(%32xFloat%);"
		Match match = RegexExpanderRegex.Match(src);
		if (match.Success)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 1; i < match.Groups.Count; i++)
			{
				string value = match.Groups[i].Value;
				if (value == "var")
					builder.Append(VariableNameCapture);
				else if (value == "index")
					builder.Append(IntCapture);
				else if (value.Contains("xFloat"))
				{ 
					int numFloats = int.Parse(value.Substring(0, value.IndexOf("xFloat")));
					for (int j = 0; j < numFloats; j++)
					{
						builder.Append(AdvancedFloatCapture);
						if (j < numFloats - 1) 
							builder.Append(WhitespaceCommaCapture);
					}
				}
				else if (value.Contains("xBool"))
				{
					int numBools = int.Parse(value.Substring(0, value.IndexOf("xBool")));
					for (int j = 0; j < numBools; j++)
					{
						builder.Append(BoolCapture);
						if (j < numBools - 1)
							builder.Append(WhitespaceCommaCapture);
					}
				}
				else
					builder.Append(value);
			}
			return new Regex(builder.ToString());
		}
		Debug.LogError($"Failed to parse {src} string into regex");
		return new Regex(src);

	}

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

	public static CubeModel ImportBlock(string modName, BoxType box)
	{
		CubeModel cube = ScriptableObject.CreateInstance<CubeModel>();

		ResourceLocation side = new ResourceLocation(modName, Utils.ToLowerWithUnderscores(box.sideTexturePath));
		ResourceLocation top = new ResourceLocation(modName, Utils.ToLowerWithUnderscores(box.topTexturePath));
		ResourceLocation bottom = new ResourceLocation(modName, Utils.ToLowerWithUnderscores(box.bottomTexturePath));

		cube.top = top;
		cube.north = cube.east = cube.south = cube.west = side;
		cube.bottom = bottom;
		cube.particle = side;
		return cube;
	}

	public static TurboRig ImportTurboModel(string modName, string javaPath, InfoType optionalType)
	{
		TypeFile file = new TypeFile(javaPath);
		try
		{
			using (StreamReader reader = new StreamReader(javaPath, Encoding.Default))
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
		}
		catch(Exception e)
		{
			Debug.LogError($"Failed to find Java model {javaPath} because of {e.Message}");
			return null;
		}

		TurboRig rig = ScriptableObject.CreateInstance<TurboRig>();
		for (; ; )
		{
			string line = file.readLine();
			if (line == null)
				break;
			// Strip whitespace
			line = line.Trim(' ', '\t');
			// Then ignore comment lines
			if (line.StartsWith("//"))
				continue;

			ReadLine(rig, file, line, optionalType);
		}
		if (optionalType != null)
			rig.name = Utils.ToLowerWithUnderscores(optionalType.shortName);
		return rig;
	}

	private static void ReadLine(TurboRig rig, TypeFile file, string line, InfoType optionalType)
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
				if (split[1].Equals("textureX")) rig.TextureX = int.Parse(split[3].Trim(';'));
				if (split[1].Equals("textureY")) rig.TextureY = int.Parse(split[3].Trim(';'));
			}
			else Debug.Assert(false, $"Invalid length textureX/Y string '{line}' in {file.name}");
			return;
		}

		if (MatchPieceOperation(rig, line))
		{ }
		else if(MatchPieceSetValue(rig, line))
		{ }
		else if(MatchSetParameter(rig, line))
		{ }
		else if(MatchSetVec3Parameter(rig, line))
		{ }
		else if (MatchNewMRTurbo(rig, line))
		{ }
		else if(MatchPiece2DOperation(rig, line))
		{ }
		else if(MatchNewMR2DTurbo(rig, line))
		{ }
		else if (MatchGlobalFunc(rig, line))
		{ }

		// Throwaway safe-ignore lines
		else if (MatchArrayInitializerTurbo(rig, line) || line.Contains("for") || line.Contains("GlStateManager"))
		{  }
		else
		{
			Debug.LogError($"Could not parse '{line}' in {file.name}");
		}


		if (optionalType != null)
		{
			// Auto-import anim parameters
			// TODO: Check we covered this case
			TxtImport.ImportFromModel(line, DefinitionTypes.GetFromObject(optionalType), optionalType);
			return;
		}
	}
	// defaultStockModel[0] = new ModelRendererTurbo(this, 27, 10, textureX, textureY);
	//private static readonly Regex NewMRTurboRegex = new Regex("([a-zA-Z0-9_]*)\\[([0-9]*)\\]\\s*=\\s*new ModelRendererTurbo\\(\\s*this\\s*,\\s*([0-9]*)\\s*,\\s*([0-9]*)\\s*,\\s*[a-zA-Z,0-9\\s]*\\);");
	private static readonly Regex ArrayInitializerRegex = new Regex($"{VariableNameCapture}\\s*=\\s*new\\s*ModelRenderer(?:Turbo)?\\[{IntCapture}\\];");
	public static bool MatchArrayInitializerTurbo(TurboRig rig, string line)
	{ 
		return ArrayInitializerRegex.Match(line).Success;
	}

	private static readonly Regex NewMR2DRegex = new Regex("([a-zA-Z0-9_]*)Model(?:s)?\\[([0-9]*)\\]\\[([0-9]*)\\]\\s*=\\s*new ModelRenderer(?:Turbo)?\\s*\\(this,\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchNewMR2DTurbo(TurboRig rig, string line)
	{
		Match match = NewMR2DRegex.Match(line);
		if (match.Success)
		{
			// Get the right section
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index_0 = int.Parse(match.Groups[2].Value);
			int index_1 = int.Parse(match.Groups[3].Value);
			TurboModel section = rig.GetOrCreateSection($"{partName}_{index_0}");

			// Set the right piece UVs
			List<float> floats = ResolveParameters(match.Groups[4].Value, 2);
			if (floats.Count == 2)
			{
				int u = Mathf.FloorToInt(floats[0]);
				int v = Mathf.FloorToInt(floats[1]);
				section.SetIndexedTextureUV(index_1, u, v);
				return true;
			}
		}
		return false;
	}

	private static readonly Regex NewMRRegex = new Regex("([a-zA-Z0-9_]*)Model(?:s)?\\[([0-9]*)\\]\\s*=\\s*new ModelRenderer(?:Turbo)?\\s*\\(this,\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchNewMRTurbo(TurboRig rig, string line)
	{
		Match match = NewMRRegex.Match(line);
		if (match.Success)
		{
			// Get the right section
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			TurboModel section = rig.GetOrCreateSection(partName);
			// Get the right piece
			int index = int.Parse(match.Groups[2].Value);
			List<float> floats = ResolveParameters(match.Groups[3].Value, 2);
			if (floats.Count == 2)
			{
				int u = Mathf.FloorToInt(floats[0]);
				int v = Mathf.FloorToInt(floats[1]);
				section.SetIndexedTextureUV(index, u, v);
				return true;
			}
		}
		return false;
	}

	private static readonly Regex PieceOperation2DRegex = new Regex("([A-Za-z]*)Model(?:s)?\\[([0-9]*)\\]\\[([0-9]*)\\].([a-zA-Z]*)\\(([-0-9A-Za-z\\/\\*\\,\\.\\s_]*)\\);");
	public static bool MatchPiece2DOperation(TurboRig rig, string line)
	{
		Match match = PieceOperation2DRegex.Match(line);
		if (match.Success)
		{
			// Get the right section
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index_0 = int.Parse(match.Groups[2].Value);
			int index_1 = int.Parse(match.Groups[3].Value);
			TurboModel section = rig.GetOrCreateSection($"{partName}_{index_0}");

			// Get the right piece
			TurboPiece piece = section.GetIndexedPiece(index_1);

			// Then run the function
			string function = match.Groups[4].Value;
			string parameters = match.Groups[5].Value;
			switch (function)
			{
				case "addBox": return AddBox(piece, parameters);
				case "addTrapezoid": return AddTrapezoid(piece, parameters);
				case "addShapeBox": return AddShapebox(piece, parameters);
				case "addShape3D": return AddShape3D(piece, parameters);
				case "setRotationPoint": return SetRotationPoint(piece, parameters);
				case "setPosition": return SetPosition(piece, parameters);
				case "doMirror": return DoMirror(piece, parameters);
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
	public static bool MatchPieceOperation(TurboRig rig, string line)
	{
		Match match = PieceOperationRegex.Match(line);
		if (match.Success)
		{
			// Get the right section
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			TurboModel section = rig.GetOrCreateSection(partName);

			// Get the right piece
			int index = int.Parse(match.Groups[2].Value);
			TurboPiece piece = section.GetIndexedPiece(index);

			// Then run the function
			string function = match.Groups[3].Value;
			string parameters = match.Groups[4].Value;

			switch(function)
			{
				case "addBox": return AddBox(piece, parameters);
				case "addTrapezoid": return AddTrapezoid(piece, parameters);
				case "addShapeBox": return AddShapebox(piece, parameters);
				case "addShape3D": return AddShape3D(piece, parameters);
				case "setRotationPoint": return SetRotationPoint(piece, parameters);
				case "setPosition": return SetPosition(piece, parameters);
				case "doMirror": return DoMirror(piece, parameters);
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

	public static bool AddBox(TurboPiece piece, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if(floats.Count == 6)
		{
			piece.Pos = new Vector3(floats[0], floats[1], floats[2]);
			piece.Dim = new Vector3(floats[3], floats[4], floats[5]);
			return true;
		}
		else if(floats.Count == 7)
		{
			float ex = floats[6];
			piece.Pos = new Vector3(floats[0] - ex, floats[1] - ex, floats[2] - ex);
			piece.Dim = new Vector3(floats[3]+2*ex, floats[4]+2*ex, floats[5]+2*ex);
			return true;
		}
		return false;
	}

	public static bool AddTrapezoid(TurboPiece piece, string parameters)
	{
		List<float> floats = ResolveParameters(parameters, 8);
		if (floats.Count == 8)
		{
			float ex = floats[6];
			float taper = floats[7];
			piece.Pos = new Vector3(floats[0] - ex, floats[1] - ex, floats[2] - ex);
			piece.Dim = new Vector3(floats[3] + 2 * ex, floats[4] + 2 * ex, floats[5] + 2 * ex);

			if (parameters.Contains("MR_LEFT"))
			{
				// expand +x face
				piece.Offsets[1] = new Vector3(0f, -taper, -taper);
				piece.Offsets[3] = new Vector3(0f, taper, -taper);
				piece.Offsets[7] = new Vector3(0f, taper, taper);
				piece.Offsets[5] = new Vector3(0f, -taper, taper);
			}
			else if (parameters.Contains("MR_RIGHT"))
			{
				// expand -x face
				piece.Offsets[4] = new Vector3(0f, -taper, taper);
				piece.Offsets[6] = new Vector3(0f, taper, taper);
				piece.Offsets[2] = new Vector3(0f, taper, -taper);
				piece.Offsets[0] = new Vector3(0f, -taper, -taper);
			}
			else if (parameters.Contains("MR_BACK"))
			{
				// expand +z face
				piece.Offsets[5] = new Vector3(taper, -taper, 0f);
				piece.Offsets[7] = new Vector3(taper, taper, 0f);
				piece.Offsets[6] = new Vector3(-taper, taper, 0f);
				piece.Offsets[4] = new Vector3(-taper, -taper, 0f);
			}
			else if (parameters.Contains("MR_FRONT"))
			{
				// expand - z face
				piece.Offsets[0] = new Vector3(-taper, -taper, 0f);
				piece.Offsets[2] = new Vector3(-taper, taper, 0f);
				piece.Offsets[3] = new Vector3(taper, taper, 0f);
				piece.Offsets[1] = new Vector3(taper, -taper, 0f);
			}
			else if (parameters.Contains("MR_BOTTOM"))
			{
				// expand +y face
				piece.Offsets[7] = new Vector3(taper, 0f, taper);
				piece.Offsets[3] = new Vector3(taper, 0f, -taper);
				piece.Offsets[2] = new Vector3(-taper, 0f, -taper);
				piece.Offsets[6] = new Vector3(-taper, 0f, taper);
			}
			else if (parameters.Contains("MR_TOP"))
			{
				// expand -y face
				piece.Offsets[5] = new Vector3(taper, 0f, taper);
				piece.Offsets[4] = new Vector3(-taper, 0f, taper);
				piece.Offsets[0] = new Vector3(-taper, 0f, -taper);
				piece.Offsets[1] = new Vector3(taper, 0f, -taper);
			}
			return true;
		}
		return false;
	}
	
	public static bool AddShape3D(TurboPiece piece, string parameters)
	{
		// TODO: Shape3D
		return true;
	}

	public static bool AddShapebox(TurboPiece piece, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count == 31)
		{
			piece.Pos = new Vector3(floats[0], floats[1], floats[2]);
			piece.Dim = new Vector3(floats[3], floats[4], floats[5]);
			// ignore expand = floats[6]
			for (int i = 0; i < 8; i++)
			{
				int iPermuted = aiPermutation[i];
				piece.Offsets[i] = new Vector3(
					floats[7 + iPermuted * 3 + 0],
					floats[7 + iPermuted * 3 + 1],
					floats[7 + iPermuted * 3 + 2]);
				if ((i & 0x1) == 0) piece.Offsets[i].x *= -1;
				if ((i & 0x2) == 0) piece.Offsets[i].y *= -1;
				if ((i & 0x4) == 0) piece.Offsets[i].z *= -1;
			}
			return true;
		}
		return false;
	}

	public static bool SetPosition(TurboPiece piece, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count == 3)
		{
			piece.Pos = new Vector3(floats[0], floats[1], floats[2]);
			return true;
		}
		return false;
	}
	public static bool SetRotationPoint(TurboPiece piece, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count == 3)
		{
			piece.Origin = new Vector3(floats[0], floats[1], floats[2]);
			return true;
		}
		return false;
	}
	public static bool DoMirror(TurboPiece piece, string parameters)
	{
		string[] boolParams = parameters.Split(',');
		if(boolParams.Length == 3)
		{
			piece.DoMirror(bool.Parse(boolParams[0]), bool.Parse(boolParams[1]), bool.Parse(boolParams[2]));
			return true;
		}
		return false;
	}
	


	private static readonly Regex PieceSetValueRegex = new Regex("([A-Za-z]*)Model(?:s)?\\[([0-9]*)].([a-zA-Z]*)\\s*=\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s]*);");
	public static bool MatchPieceSetValue(TurboRig rig, string line)
	{
		Match match = PieceSetValueRegex.Match(line);
		if (match.Success)
		{
			// Get the right section
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			TurboModel section = rig.GetOrCreateSection(partName);

			// Get the right piece
			int index = int.Parse(match.Groups[2].Value);
			TurboPiece piece = section.GetIndexedPiece(index);

			// Then run the function
			string field = match.Groups[3].Value;
			string setValue = match.Groups[4].Value;

			switch (field)
			{
				case "rotateAngleX": return SetRotateAngleX(piece, setValue);
				case "rotateAngleY": return SetRotateAngleY(piece, setValue);
				case "rotateAngleZ": return SetRotateAngleZ(piece, setValue);
				case "flip": return SetFlip(piece, setValue);
				default:
				{
					Debug.LogWarning($"Unknown piece set value '{field}' in line '{line}'");
					return false;
				}
			}
		}
		return false;
	}
	public static bool SetRotateAngleX(TurboPiece piece, string setValue)
	{
		List<float> floats = ResolveParameters(setValue, 1);
		if (floats.Count == 1)
		{
			piece.Euler.x = floats[0] * Mathf.Rad2Deg;
			return true;
		}
		return false;
	}
	public static bool SetRotateAngleY(TurboPiece piece, string setValue)
	{
		List<float> floats = ResolveParameters(setValue, 1);
		if (floats.Count == 1)
		{
			piece.Euler.y = floats[0] * Mathf.Rad2Deg;
			return true;
		}
		return false;
	}
	public static bool SetRotateAngleZ(TurboPiece piece, string setValue)
	{
		List<float> floats = ResolveParameters(setValue, 1);
		if (floats.Count == 1)
		{
			piece.Euler.z = floats[0] * Mathf.Rad2Deg;
			return true;
		}
		return false;
	}
	public static bool SetFlip(TurboPiece piece, string setValue)
	{
		bool flip = bool.Parse(setValue);
		// TODO: Hmm, what should this do?		
		return true;
	}
	private static readonly Regex GlobalFuncRegex = new Regex("([A-Za-z]*)\\s*\\(([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchGlobalFunc(TurboRig rig, string line)
	{
		Match match = GlobalFuncRegex.Match(line);
		if (match.Success)
		{
			// Run the function
			string function = Utils.ConvertPartName(match.Groups[1].Value);
			string parameters = match.Groups[2].Value;

			switch (function)
			{
				case "translateAll": return TranslateAll(rig, parameters);
				case "flipAll": return FlipAll(rig);
				// This is fine to skip
				case "render": return true;
				case "scale": return true;
				default:
				{
					Debug.LogWarning($"Unknown global function '{function}' in line '{line}'");
					return false;
				}
			}
		}
		return false;
	}
	public static bool TranslateAll(TurboRig rig, string parameters)
	{
		List<float> floats = ResolveParameters(parameters);
		if (floats.Count == 3)
		{
			rig.TranslateAll(floats[0], floats[1], floats[2]);
			return true;
		}
		return false;
	}
	public static bool FlipAll(TurboRig rig)
	{
		rig.FlipAll();
		return true;
	}

	private static readonly Regex SetParameterRegex = new Regex("([A-Za-z]*)\\s*=\\s*([-0-9A-Za-z\\/\\*\\,\\.\\s_]*);");
	public static bool MatchSetParameter(TurboRig rig, string line)
	{
		Match match = SetParameterRegex.Match(line);
		if (match.Success)
		{
			// Find the parameter and set it
			string parameter = match.Groups[1].Value;
			string value = match.Groups[2].Value;

			if (parameter == "animationType")
			{
				// Hmm? Send to type?
				EAnimationType animationType = (EAnimationType)Enum.Parse(typeof(EAnimationType), value.Split('.')[1]);
				Debug.Log($"Detected animation type {animationType} in line '{line}'");
				return true;
			}
			else
			{
				List<float> floats = ResolveParameters(value, 1);
				if (floats.Count == 1)
				{
					rig.AnimationParameters.Add(new AnimationParameter()
					{
						key = Utils.ToLowerWithUnderscores(parameter),
						isVec3 = false,
						floatValue = floats[0]
					});
					return true;
				}
			}
		}
		return false;
	}

	private static readonly Regex SetVec3ParameterRegex = new Regex("([A-Za-z]*)\\s*=\\s*new\\s*Vector3f\\s*\\(([-0-9A-Za-z\\/\\*\\,\\.\\s]*)\\);");
	public static bool MatchSetVec3Parameter(TurboRig rig, string line)
	{
		Match match = SetVec3ParameterRegex.Match(line);
		if (match.Success)
		{
			// Find the parameter and set it
			string parameter = match.Groups[1].Value;
			string value = match.Groups[2].Value;
			List<float> floats = ResolveParameters(value, 3);
			if (floats.Count == 3)
			{
				rig.AnimationParameters.Add(new AnimationParameter()
				{
					key = Utils.ToLowerWithUnderscores(parameter),
					isVec3 = true,
					vec3Value = new Vector3(floats[0] * 16f, floats[1] * 16f, floats[2] * 16f),
				});
				return true;
			}
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

	private static List<float> ResolveParameters(string parameters, int max = 100)
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
