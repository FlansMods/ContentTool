using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

		if(MatchNewMRTurbo(rig, line))
		{ }
		else if (MatchAddBox(rig, line))
		{ }
		else if (MatchAddShapeBox(rig, line))
		{ }
		else if(MatchAddTrapezoid(rig, line))
		{ }
		else if (MatchDoMirror(rig, line))
		{ }
		else if (MatchSetRotateAngle(rig, line))
		{ }
		else if (MatchSetRotationPoint(rig, line))
		{ }
		else if (MatchTranslateAll(rig, line))
		{ }
		else if (MatchFlipAll(rig, line))
		{ }
		else if (MatchFloatParam(rig, line))
		{ }
		else if(MatchVec3Param(rig, line))
		{ }


		if(optionalType != null)
		{
			// Auto-import anim parameters
			// TODO: Check we covered this case
			TxtImport.ImportFromModel(line, DefinitionTypes.GetFromObject(optionalType), optionalType);
			return;
		}

		if(line.Contains("for") || line.Contains("GlStateManager"))
			return;

		Debug.Assert(false, "Hit bad line in model (" + file.name + "): " + line);
	}

	// defaultStockModel[0] = new ModelRendererTurbo(this, 27, 10, textureX, textureY);
	//private static readonly Regex NewMRTurboRegex = new Regex("([a-zA-Z0-9_]*)\\[([0-9]*)\\]\\s*=\\s*new ModelRendererTurbo\\(\\s*this\\s*,\\s*([0-9]*)\\s*,\\s*([0-9]*)\\s*,\\s*[a-zA-Z,0-9\\s]*\\);");
	private static readonly Regex NewMRTurboRegex = CreateRegex("%var%\\[%index%\\] = new ModelRendererTurbo(this, %2xFloat%, textureX, textureY);");
	public static bool MatchNewMRTurbo(TurboRig rig, string line)
	{
		Match match = NewMRTurboRegex.Match(line);
		if (match.Success)
		{
			string partName = Utils.ConvertPartName(match.Groups[0].Value);
			int index = int.Parse(match.Groups[1].Value);
			int u = int.Parse(match.Groups[2].Value);
			int v = int.Parse(match.Groups[3].Value);

			TurboModel section = rig.GetOrCreateSection(partName);
			section.SetIndexedTextureUV(index, u, v);
			return true;
		}
		return false;
	}

	// defaultScopeModel[0].addBox(-2F, 4.5F, -0.5F, 6, 1, 1);
	//private static readonly Regex AddBoxRegex = new Regex("([a-zA-Z0-9_]*)\\[([0-9]*)\\]\\s*\\.addBox\\s*\\(\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*\\);");
	private static readonly Regex AddBoxRegex = CreateRegex("%var%\\[%index%\\].addBox\\(%6xFloat%\\);"); 
	public static bool MatchAddBox(TurboRig rig, string line)
	{
		Match match = AddBoxRegex.Match(line);
		if (match.Success)
		{
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index = int.Parse(match.Groups[2].Value);
			float x = ParseFloat(match.Groups[3].Value);
			float y = ParseFloat(match.Groups[4].Value);
			float z = ParseFloat(match.Groups[5].Value);
			float w = ParseFloat(match.Groups[6].Value);
			float h = ParseFloat(match.Groups[7].Value);
			float d = ParseFloat(match.Groups[8].Value);

			TurboModel section = rig.GetOrCreateSection(partName);
			TurboPiece piece = section.GetIndexedPiece(index);
			piece.Pos = new Vector3(x, y, z);
			piece.Dim = new Vector3(w, h, d);
			return true;
		}
		return false;
	}

	// defaultScopeModel[0].addBox(-2F, 4.5F, -0.5F, 6, 1, 1);
	//private static readonly Regex AddBoxRegex = new Regex("([a-zA-Z0-9_]*)\\[([0-9]*)\\]\\s*\\.addBox\\s*\\(\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*\\);");
	private static readonly Regex AddTrapezoidRegex = CreateRegex("%var%\\[%index%\\].addTrapezoid\\(%8xFloat%,\\s*(MR_[A-Z]*)\\);");
	public static bool MatchAddTrapezoid(TurboRig rig, string line)
	{
		Match match = AddTrapezoidRegex.Match(line);
		if (match.Success)
		{
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index = int.Parse(match.Groups[2].Value);
			float x = ParseFloat(match.Groups[3].Value);
			float y = ParseFloat(match.Groups[4].Value);
			float z = ParseFloat(match.Groups[5].Value);
			float w = ParseFloat(match.Groups[6].Value);
			float h = ParseFloat(match.Groups[7].Value);
			float d = ParseFloat(match.Groups[8].Value);
			float expand = ParseFloat(match.Groups[9].Value);
			float taper = ParseFloat(match.Groups[10].Value);
			string eDirection = match.Groups[11].Value;

			TurboModel section = rig.GetOrCreateSection(partName);
			TurboPiece piece = section.GetIndexedPiece(index);
			piece.Pos = new Vector3(x - expand, y - expand, z - expand);
			piece.Dim = new Vector3(w + 2 * expand, h + 2 * expand, d + 2 * expand);

			switch(eDirection)
			{
				case "MR_LEFT":
				{
					// expand +x face
					piece.Offsets[1] = new Vector3(0f, -taper, -taper);
					piece.Offsets[3] = new Vector3(0f, taper, -taper);
					piece.Offsets[7] = new Vector3(0f, taper, taper);
					piece.Offsets[5] = new Vector3(0f, -taper, taper);
					break;
				}
				case "MR_RIGHT": // expand -x face
				{
					piece.Offsets[4] = new Vector3(0f, -taper, taper);
					piece.Offsets[6] = new Vector3(0f, taper, taper);
					piece.Offsets[2] = new Vector3(0f, taper, -taper);
					piece.Offsets[0] = new Vector3(0f, -taper, -taper);
					break;
				}
				case "MR_BACK": // expand +z face
				{
					piece.Offsets[5] = new Vector3(taper, -taper, 0f);
					piece.Offsets[7] = new Vector3(taper, taper, 0f);
					piece.Offsets[6] = new Vector3(-taper, taper, 0f);
					piece.Offsets[4] = new Vector3(-taper, -taper, 0f);
					break;
				}
				case "MR_FRONT": // expand -z face
				{
					piece.Offsets[0] = new Vector3(-taper, -taper, 0f);
					piece.Offsets[2] = new Vector3(-taper, taper, 0f);
					piece.Offsets[3] = new Vector3(taper, taper, 0f);
					piece.Offsets[1] = new Vector3(taper, -taper, 0f);
					break;
				}
				case "MR_BOTTOM": // expand +y face
				{
					piece.Offsets[7] = new Vector3(taper, 0f, taper);
					piece.Offsets[3] = new Vector3(taper, 0f, -taper);
					piece.Offsets[2] = new Vector3(-taper, 0f, -taper);
					piece.Offsets[6] = new Vector3(-taper, 0f, taper);
					break;
				}
				case "MR_TOP": // expand -y face
				{
					piece.Offsets[5] = new Vector3(taper, 0f, taper);
					piece.Offsets[4] = new Vector3(-taper, 0f, taper);
					piece.Offsets[0] = new Vector3(-taper, 0f, -taper);
					piece.Offsets[1] = new Vector3(taper, 0f, -taper);
					break;
				}
			}

			return true;
		}
		return false;
	}


	// defaultScopeModel[0].addShapeBox(0F, 5F, -0.5F, 1, 2, 1, 0F, 0F, 0F, -0.375F, 0F, 0F, -0.375F, 0F, 0F, -0.375F, 0F, 0F, -0.375F, -0.5F, -0.5F, -0.375F, 0F, -0.5F, -0.375F, 0F, -0.5F, -0.375F, -0.5F, -0.5F, -0.375F); // Box 12
	//private static readonly Regex AddShapeBoxRegex = new Regex("([a-zA-Z0-9_]*)\\[([0-9]*)\\]\\s*\\.addBox\\s*\\(\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*,\\s*([\\-0-9.\\/Ff]*)\\s*\\);");
	private static readonly Regex AddShapeBoxRegex = CreateRegex("%var%\\[%index%\\].addShapeBox\\(%31xFloat%\\);");
	public static bool MatchAddShapeBox(TurboRig rig, string line)
	{
		Match match = AddShapeBoxRegex.Match(line);
		if (match.Success)
		{
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index = int.Parse(match.Groups[2].Value);
			TurboModel section = rig.GetOrCreateSection(partName);
			TurboPiece piece = section.GetIndexedPiece(index);

			float x = ParseFloat(match.Groups[3].Value);
			float y = ParseFloat(match.Groups[4].Value);
			float z = ParseFloat(match.Groups[5].Value);
			float w = ParseFloat(match.Groups[6].Value);
			float h = ParseFloat(match.Groups[7].Value);
			float d = ParseFloat(match.Groups[8].Value);
			piece.Pos = new Vector3(x, y, z);
			piece.Dim = new Vector3(w, h, d);

			// floats[9] What is this? Scale?

			for (int i = 0; i < 8; i++)
			{
				int iPermuted = aiPermutation[i];
				piece.Offsets[i] = new Vector3(
					ParseFloat(match.Groups[10 + iPermuted * 3 + 0].Value),
					ParseFloat(match.Groups[10 + iPermuted * 3 + 1].Value),
					ParseFloat(match.Groups[10 + iPermuted * 3 + 2].Value));
				if ((i & 0x1) == 0) piece.Offsets[i].x *= -1;
				if ((i & 0x2) == 0) piece.Offsets[i].y *= -1;
				if ((i & 0x4) == 0) piece.Offsets[i].z *= -1;
			}
			return true;
		}
		return false;
	}

	// gunModel[4].setRotationPoint(0F, 1F, 0F);
	private static readonly Regex SetRotationPointRegex = CreateRegex("%var%\\[%index%\\].setRotationPoint(%3xFloat%);");
	public static bool MatchSetRotationPoint(TurboRig rig, string line)
	{
		Match match = SetRotationPointRegex.Match(line);
		if (match.Success)
		{
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index = int.Parse(match.Groups[2].Value);
			TurboModel section = rig.GetOrCreateSection(partName);
			TurboPiece piece = section.GetIndexedPiece(index);
			float x = ParseFloat(match.Groups[3].Value);
			float y = ParseFloat(match.Groups[4].Value);
			float z = ParseFloat(match.Groups[5].Value);
			piece.Origin = new Vector3(x, y, z);

			return true;
		}
		return false;
	}

	// gunModel[4].rotateAngleZ = -0.5F;
	private static readonly Regex SetRotateAngleRegex = CreateRegex("%var%\\[%index%\\].rotateAngle(X|Y|Z)\\s*=\\s*%1xFloat%;");
	public static bool MatchSetRotateAngle(TurboRig rig, string line)
	{
		Match match = SetRotateAngleRegex.Match(line);
		if (match.Success)
		{
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index = int.Parse(match.Groups[2].Value);
			TurboModel section = rig.GetOrCreateSection(partName);
			TurboPiece piece = section.GetIndexedPiece(index);

			string axis = match.Groups[3].Value;
			float angle = ParseFloat(match.Groups[4].Value);
			switch(axis)
			{
				case "X": piece.Euler.x = angle * Mathf.Rad2Deg; break;
				case "Y": piece.Euler.y = angle * Mathf.Rad2Deg; break;
				case "Z": piece.Euler.z = angle * Mathf.Rad2Deg; break;
			}

			return true;
		}
		return false;
	}

// gunModel[4].doMirror(false, true, true);
private static readonly Regex DoMirrorRegex = CreateRegex("%var%\\[%index%\\].doMirror\\s*\\(%3xBool%\\);");
	public static bool MatchDoMirror(TurboRig rig, string line)
	{
		Match match = DoMirrorRegex.Match(line);
		if (match.Success)
		{
			string partName = Utils.ConvertPartName(match.Groups[1].Value);
			int index = int.Parse(match.Groups[2].Value);
			TurboModel section = rig.GetOrCreateSection(partName);
			TurboPiece piece = section.GetIndexedPiece(index);

			piece.DoMirror(bool.Parse(match.Groups[3].Value),
							bool.Parse(match.Groups[4].Value),
							bool.Parse(match.Groups[5].Value));

			return true;
		}
		return false;
	}

	// translateAll();
	//private static readonly Regex TranslateAllRegex = new Regex("translateAll\\s*\\(([-0-9.\\/Ff]*)\\s*,\\s*([-0-9.\\/Ff]*)\\s*,\\s*([-0-9.\\/Ff]*)\\s*\\)\\s*;");
	private static readonly Regex TranslateAllRegex = CreateRegex("translateAll(\\s*%3xFloat%);");
	public static bool MatchTranslateAll(TurboRig rig, string line)
	{
		Match match = TranslateAllRegex.Match(line);
		if (match.Success)
		{
			float x = ParseFloat(match.Groups[1].Value);
			float y = ParseFloat(match.Groups[2].Value); 
			float z = ParseFloat(match.Groups[3].Value);

			foreach (TurboModel section in rig.Sections)
			{
				foreach (TurboPiece piece in section.Pieces)
				{
					if (piece != null)
					{
						piece.Origin.x += x;
						piece.Origin.y += y;
						piece.Origin.z += z;
					}
				}
			}

			foreach (AttachPoint ap in rig.AttachPoints)
			{
				ap.position -= new Vector3(x, y, z);
			}
			return true;
		}
		return false;
	}

	// flipAll();
	private static readonly Regex FlipAllRegex = new Regex("flipAll");
	public static bool MatchFlipAll(TurboRig rig, string line)
	{
		Match match = FlipAllRegex.Match(line);
		if (match.Success)
		{
			foreach (TurboModel section in rig.Sections)
			{
				foreach (TurboPiece piece in section.Pieces)
				{
					piece.DoMirror(false, true, true);
				}
			}
			return true;
		}
		return false;
	}

	// gunSlideDistance = 0.0f;
	//private static readonly Regex FloatParamRegex = new Regex("\\s*([a-zA-Z0-9_]*)\\s*=\\s*([-0-9.]*)[fF]?;");
	private static readonly Regex FloatParamRegex = CreateRegex("%var%\\s*=\\s*%1xFloat%;");
	public static bool MatchFloatParam(TurboRig rig, string line)
	{
		Match match = FloatParamRegex.Match(line);
		if (match.Success)
		{
			string parameterName = match.Groups[1].Value;
			string value = match.Groups[2].Value;
			rig.AnimationParameters.Add(new AnimationParameter()
			{
				key = parameterName,
				isVec3 = false,
				floatValue = ParseFloat(value)
			});
			return true;
		}
		return false;
	}

	// scopeAttachPoint = new Vector3f(3.5F / 16F, 5F / 16F, 0F);
	//private static readonly Regex Vec3ParamRegex = new Regex("\\s*([a-zA-Z0-9_]*)\\s*=\\s*new Vector3f\\s*\\(([-0-9.\\/Ff]*)\\s*,\\s*([-0-9.\\/Ff]*)\\s*,\\s*([-0-9.\\/Ff]*)\\s*\\);");
	private static readonly Regex Vec3ParamRegex = CreateRegex("%var%\\s*=\\s*new Vector3f(%3xFloat%);");
	public static bool MatchVec3Param(TurboRig rig, string line)
	{
		Match match = Vec3ParamRegex.Match(line);
		if (match.Success)
		{
			string parameterName = match.Groups[1].Value;
			string xValue = match.Groups[2].Value;
			string yValue = match.Groups[3].Value;
			string zValue = match.Groups[4].Value;
			rig.AnimationParameters.Add(new AnimationParameter()
			{
				key = parameterName,
				isVec3 = true,
				vec3Value = new Vector3(
					ParseFloat(xValue),
					ParseFloat(yValue),
					ParseFloat(zValue))
			});
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
}
