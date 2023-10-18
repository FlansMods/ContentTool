using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Model;

[CreateAssetMenu(menuName = "Minecraft Models/TurboRig")]
public class TurboRig : MinecraftModel
{


	// --------------------------------------------------------------------------
	#region Model definition and helpers
	// --------------------------------------------------------------------------
	public ResourceLocation Icon;
	public int TextureX = 16;
	public int TextureY = 16;
	public List<TurboModel> Sections = new List<TurboModel>();
	public List<AnimationParameter> AnimationParameters = new List<AnimationParameter>();
	public List<AttachPoint> AttachPoints = new List<AttachPoint>();	

	public TurboModel GetSection(string key)
	{
		foreach (TurboModel section in Sections)
			if (section.PartName == key)
				return section;
		return null;
	}
	public TurboModel GetOrCreateSection(string key)
	{
		foreach (TurboModel section in Sections)
			if (section.PartName == key)
				return section;
		TurboModel newSection = new TurboModel()
		{
			PartName = key
		};
		Sections.Add(newSection);
		return newSection;
	}
	public TurboModel AddSection()
	{
		Sections.Add(new TurboModel() {
			PartName = $"new_{Sections.Count}"
		});
		return Sections[Sections.Count - 1];
	}

	public void DuplicateSection(int index)
	{
		if (0 <= index && index < Sections.Count)
		{
			TurboModel copy = Sections[index].Copy();
			copy.PartName = $"{copy.PartName}-";
			Sections.Insert(index + 1, copy);
		}
	}

	public void DeleteSection(int index)
	{
		Sections.RemoveAt(index);
	}

	public float GetFloatParamOrDefault(string key, float defaultValue)
	{
		TryGetFloatParam(key, out float f, defaultValue);
		return f;
	}

	public bool TryGetFloatParam(string key, out float value, float defaultValue = 0.0f)
	{
		foreach (AnimationParameter parameter in AnimationParameters)
			if (parameter.key == key)
			{
				value = parameter.floatValue;
				return true;
			}
		value = defaultValue;
		return false;
	}

	public Vector3 GetVec3ParamOrDefault(string key, Vector3 defaultValue)
	{
		TryGetVec3Param(key, out Vector3 v, defaultValue);
		return v;
	}

	public bool TryGetVec3Param(string key, out Vector3 value)
	{
		return TryGetVec3Param(key, out value, Vector3.zero);
	}

	public bool TryGetVec3Param(string key, out Vector3 value, Vector3 defaultValue)
	{
		foreach (AnimationParameter parameter in AnimationParameters)
			if (parameter.key == key)
			{
				value = parameter.vec3Value;
				return true;
			}
		value = defaultValue;
		return false;
	}

	public bool IsAttached(string parent, string child, bool defaultValue = false)
	{
		foreach (AttachPoint ap in AttachPoints)
		{
			if (ap.attachedTo == parent && ap.name == child)
				return true;
		}
		return defaultValue;
	}

	public void TranslateAll(float x, float y, float z)
	{
		foreach (TurboModel section in Sections)
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

		foreach (AttachPoint ap in AttachPoints)
		{
			ap.position -= new Vector3(x, y, z);
		}
	}

	public void FlipAll()
	{
		foreach (TurboModel section in Sections)
		{
			foreach (TurboPiece piece in section.Pieces)
			{
				piece.DoMirror(false, true, true);
			}
		}
	}

	public AttachPoint GetOrCreate(string name)
	{
		foreach (AttachPoint point in AttachPoints)
			if (point.name == name)
				return point;
		AttachPoint point1 = new AttachPoint()
		{
			name = name,
			attachedTo = "body",
			position = Vector3.zero,
		};
		AttachPoints.Add(point1);
		return point1;
	}
	public AttachPoint GetAttachPoint(string key)
	{
		foreach (AttachPoint point in AttachPoints)
			if (point.name == key)
				return point;
		return null;
	}
	public void SetAttachment(string name, string attachedTo)
	{
		AttachPoint ap = GetOrCreate(name);
		ap.attachedTo = attachedTo;
	}
	public void SetAttachmentOffset(string name, Vector3 offset)
	{
		AttachPoint ap = GetOrCreate(name);
		ap.position = offset * 16f;
	}

	#endregion
	// --------------------------------------------------------------------------


	// --------------------------------------------------------------------------
	#region JSON Export
	// --------------------------------------------------------------------------
	private void CalculateGUIPose(out Vector3 scale, out Vector3 offset, out Vector3 euler)
	{
		Vector3 min = Vector3.one * 1000f;
		Vector3 max = Vector3.one * -1000f;
		foreach (TurboModel section in Sections)
		{
			foreach (TurboPiece piece in section.Pieces)
			{
				piece.GetBounds(out Vector3 pieceMin, out Vector3 pieceMax);
				min = Vector3.Min(min, pieceMin);
				max = Vector3.Max(max, pieceMax);
			}
		}

		Vector3 center = (min + max) / 2f;
		Vector3 size = (max - min) / 2f;

		offset = -center / 16f;
		float maxDim = Mathf.Max(size.x, size.y, size.z) * 2f;
		scale = Vector3.one / maxDim;
		euler = new Vector3(-30f, 160f, 45f);
		offset = Quaternion.Euler(euler) * offset;
		offset /= maxDim;
	}

	public bool ExportSectionToJson(QuickJSONBuilder builder, string part)
	{
		TurboModel section = GetSection(part);
		if (section == null)
			return false;

		Vector3 origin = Vector3.zero;
		switch (part)
		{
			case "defaultBarrel": origin = GetVec3ParamOrDefault("barrelBreakOrigin", origin); break;
			case "revolverBarrel": origin = GetVec3ParamOrDefault("revolverFlipPoint", origin); break;
			case "minigunBarrel": origin = GetVec3ParamOrDefault("minigunBarrelOrigin", origin); break;
		}

		origin *= 16f;
		builder.Current.Add("origin", JSONHelpers.ToJSON(origin));

		using (builder.Tabulation("turboelements"))
		{
			foreach (TurboPiece wrapper in section.Pieces)
			{
				using (builder.TableEntry())
				{
					using (builder.Tabulation("verts"))
					{
						Vector3[] verts = wrapper.GetVerts();
						for (int i = 0; i < 8; i++)
						{
							builder.CurrentTable.Add(JSONHelpers.ToJSON(verts[i]));
						}
					}
					builder.Current.Add("eulerRotations", JSONHelpers.ToJSON(wrapper.Euler));
					builder.Current.Add("rotationOrigin", JSONHelpers.ToJSON(wrapper.Origin - origin));
					using (builder.Indentation("faces"))
					{
						Action<TurboPiece, String, EFace, int, int> buildUVs = (wrapper, name, face, texX, texY) =>
						{
							using (builder.Indentation(name))
							{
								using (builder.Tabulation("uv"))
								{
									int[] uvs = wrapper.GetIntUV(wrapper.textureU, wrapper.textureV, face);
									builder.CurrentTable.Add((float)uvs[0] / texX);
									builder.CurrentTable.Add((float)uvs[1] / texY);
									builder.CurrentTable.Add((float)uvs[2] / texX);
									builder.CurrentTable.Add((float)uvs[3] / texY);
								}
								if (face == EFace.up)
									builder.Current.Add("rotation", 180);
								if (face == EFace.down)
									builder.Current.Add("rotation", 90);
								builder.Current.Add("texture", "#default");
							}
						};

						buildUVs(wrapper, "north", EFace.north, TextureX, TextureY);
						buildUVs(wrapper, "east", EFace.east, TextureX, TextureY);
						buildUVs(wrapper, "south", EFace.south, TextureX, TextureY);
						buildUVs(wrapper, "west", EFace.west, TextureX, TextureY);
						buildUVs(wrapper, "up", EFace.up, TextureX, TextureY);
						buildUVs(wrapper, "down", EFace.down, TextureX, TextureY);
					}
				}
			}
		}
		return true;
	}


	private void WriteItemTransforms(QuickJSONBuilder builder, string key, Vector3 pos, Vector3 euler, Vector3 scale)
	{
		using (builder.Indentation(key))
		{
			builder.Current.Add("rotation", JSONHelpers.ToJSON(pos));
			builder.Current.Add("translation", JSONHelpers.ToJSON(euler));
			builder.Current.Add("scale", JSONHelpers.ToJSON(scale));
		}
	}

	public override bool IsUVMapSame(MinecraftModel other)
	{
		if (other is TurboRig otherRig)
		{
			if (Sections.Count != otherRig.Sections.Count)
				return false;
			for(int i = 0; i < Sections.Count; i++)
			{
				if (!Sections[i].IsUVMapSame(otherRig.Sections[i]))
					return false;
			}
		}
		
		return true;
	}
	public Vector2Int GetMaxUV()
	{
		Vector2Int max = Vector2Int.zero;
		foreach (TurboModel section in Sections)
		{
			Vector2Int sectionMax = section.GetMaxUV();
			if (sectionMax.x > max.x)
				max.x = sectionMax.x;
			if (sectionMax.y > max.y)
				max.y = sectionMax.y;
		}
		return max;
	}

	public override bool ExportToJson(QuickJSONBuilder builder)
	{
		if (TextureX == 0)
			TextureX = 16;
		if (TextureY == 0)
			TextureY = 16;

		builder.Current.Add("loader", "flansmod:turborig");
		using (builder.Indentation("textures"))
		{
			foreach (var kvp in Textures)
			{
				builder.Current.Add(kvp.Key, kvp.Location.ResolveWithSubdir("skins"));
			}
			builder.Current.Add("default", this.GetLocation().ResolveWithSubdir("skins"));
			builder.Current.Add("particle", $"minecraft:block/iron_block");
		}
		using (builder.Indentation("animations"))
		{
			foreach (AnimationParameter animParam in AnimationParameters)
			{
				if (animParam.key != null && animParam.key.Length > 0 && !builder.Current.ContainsKey(animParam.key))
				{
					if (animParam.isVec3)
						builder.Current.Add(animParam.key, JSONHelpers.ToJSON(animParam.vec3Value));
					else
						builder.Current.Add(animParam.key, animParam.floatValue);
				}
			}
		}
		using (builder.Indentation("display"))
		{

			WriteItemTransforms(builder, "firstperson_lefthand", new Vector3(0f, 90f, 0f), new Vector3(-8f, -7f, -13f), Vector3.one);
			WriteItemTransforms(builder, "firstperson_righthand", new Vector3(0f, 90f, 0f), new Vector3(8f, -7f, -13f), Vector3.one);
			WriteItemTransforms(builder, "thirdperson_lefthand", new Vector3(0f, -90f, 0f), new Vector3(0f, 3.75f, 0f), Vector3.one);
			WriteItemTransforms(builder, "thirdperson_righthand", new Vector3(0f, 90f, 0f), new Vector3(0f, 3.25f, 0f), Vector3.one);

			CalculateGUIPose(out Vector3 scale, out Vector3 offset, out Vector3 euler);
			WriteItemTransforms(builder, "gui", euler, offset, scale);
			WriteItemTransforms(builder, "head", new Vector3(-90f, 0f, 0f), new Vector3(), Vector3.one);
			WriteItemTransforms(builder, "ground", new Vector3(0f, 0f, 0f), new Vector3(0f, 0.15f, 0f), Vector3.one / 16f);
			WriteItemTransforms(builder, "fixed", new Vector3(0f, 160f, 0f), new Vector3(0.5f, 0.5f, 0f), Vector3.one);
		}
		using (builder.Indentation("parts"))
		{
			foreach (TurboModel section in Sections)
			{
				string partName = Utils.ConvertPartName(section.PartName);
				using (builder.Indentation(partName))
				{
					ExportSectionToJson(builder, section.PartName);
				}
			}
		}
		using (builder.Tabulation("attachPoints"))
		{
			foreach (AttachPoint attachPoint in AttachPoints)
			{
				using (builder.TableEntry())
				{
					if (attachPoint.name == "scope")
						builder.Current.Add("name", "sights");
					else
						builder.Current.Add("name", attachPoint.name);
					builder.Current.Add("attachTo", attachPoint.attachedTo);
					builder.Current.Add("offset", JSONHelpers.ToJSON(attachPoint.position));
				}
			}
		}

		return true;
	}

	public override bool ExportInventoryVariantToJson(QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", $"item/generated");
		using (builder.Indentation("textures"))
		{
			builder.Current.Add("layer0", Icon.ResolveWithSubdir("item"));
		}
		using (builder.Indentation("display"))
		{
			using (builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(-90f, 0f, 0f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 1f, -3f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(0.55f, 0.55f, 0.55f)));
			}
			using (builder.Indentation("firstperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(0f, -135f, 25f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 4f, 2f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(1.7f, 1.7f, 1.7f)));
			}
		}
		return true;
	}

	#endregion
	// --------------------------------------------------------------------------
}
