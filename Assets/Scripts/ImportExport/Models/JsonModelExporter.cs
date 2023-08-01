using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class JsonModelExporter
{	
	public static bool ExportInventoryVariantModel(Model model, string modName, string modelName, QuickJSONBuilder builder)
	{
		switch(model.Type)
		{
			case Model.ModelType.TurboRig:
			{
				ExportVanillaItem(model, modName, modelName, builder);
				return true;
			}
		}	
		return false;
	}


	public static bool ExportItemModel(Model model, Dictionary<string, string> textures, string modName, string modelName, QuickJSONBuilder builder)
	{
		switch(model.Type)
		{
			case Model.ModelType.Block:
			{
				ExportVanillaBlockItem(model, modName, modelName, builder);
				return true;
			}
			case Model.ModelType.Item:
			{
				ExportVanillaItem(model, modName, modelName, builder);
				return true;
			}
			case Model.ModelType.TurboRig:
			{
				ExportTurboRig(model, textures, modName, modelName, builder);
				return true;
			}
		}	
		return false;
	}

    public static bool ExportBlockModel(Model model, string modName, string modelName, QuickJSONBuilder builder)
	{
		switch(model.Type)
		{
			case Model.ModelType.Block:
			{
				ExportVanillaBlock(model, modName, modelName, builder);
				return true;
			}
		}	
		return false;
	}

	private static void ExportVanillaBlock(Model model, string modName, string modelName, QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", "block/cube");
		using(builder.Indentation("textures"))
		{
			builder.Current.Add("particle", $"{modName}:block/{model.north}");
			builder.Current.Add("up", $"{modName}:block/{model.top}");
			builder.Current.Add("down", $"{modName}:block/{model.bottom}");
			builder.Current.Add("north", $"{modName}:block/{model.north}");
			builder.Current.Add("east", $"{modName}:block/{model.east}");
			builder.Current.Add("south", $"{modName}:block/{model.south}");
			builder.Current.Add("west", $"{modName}:block/{model.west}");
		}
	}

	private static void ExportVanillaBlockItem(Model model, string modName, string modelName, QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", $"{modName}:block/{modelName}");
		using(builder.Indentation("display"))
		{
			using(builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(10f, -45f, 170f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 1.5f, -2.75f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(0.375f, 0.375f, 0.375f)));
			}
		}
	}

	private static void ExportVanillaItem(Model model, string modName, string modelName, QuickJSONBuilder builder)
	{
		builder.Current.Add("parent", $"item/generated");
		using(builder.Indentation("textures"))
		{
			builder.Current.Add("layer0", $"{modName}:item/{model.icon}");
		}
		using(builder.Indentation("display"))
		{
			using(builder.Indentation("thirdperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(-90f, 0f, 0f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 1f, -3f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(0.55f, 0.55f, 0.55f)));
			}
			using(builder.Indentation("firstperson"))
			{
				builder.Current.Add("rotation", JSONHelpers.ToJSON(new Vector3(0f, -135f, 25f)));
				builder.Current.Add("translation", JSONHelpers.ToJSON(new Vector3(0f, 4f, 2f)));
				builder.Current.Add("scale", JSONHelpers.ToJSON(new Vector3(1.7f, 1.7f, 1.7f)));
			}
		}
	}

	private static void ExportVanillaItemSkinSwitcher(Model model, string modName, string modelName, QuickJSONBuilder builder)
	{
		using(builder.Tabulation("overrides"))
		{
			// tODO
			using(builder.TableEntry())
			{
				using(builder.Indentation("predicate"))
				{
					//builder.Current.Add("custom_model_data", i);
				}
				builder.Current.Add("model");
			}
		}
	}

	private static void CalculateGUIPose(Model model, out Vector3 scale, out Vector3 offset, out Vector3 euler)
	{
		Vector3 min = Vector3.one * 1000f;
		Vector3 max = Vector3.one * -1000f;
		foreach(Model.Section section in model.sections)
		{
			foreach(Model.Piece piece in section.pieces)
			{
				piece.GetBounds(out Vector3 pieceMin, out Vector3 pieceMax);
				min = Vector3.Min(min, pieceMin);
				max = Vector3.Max(max, pieceMax);
			}
		}

		Vector3 center = (min + max) / 2f;
		Vector3 size = (max - min) / 2f;

		offset = -center / 16f;
		float maxDim = Mathf.Max(size.x, size.y, size.z)* 2f;
		scale = Vector3.one / maxDim;
		euler = new Vector3(-30f, 160f, 45f);
		offset = Quaternion.Euler(euler) * offset;
		offset /= maxDim;
	}

	private static void ExportTurboRig(Model model, Dictionary<string, string> textures, string modName, string modelName, QuickJSONBuilder builder)
	{
		if(model.textureX == 0)
			model.textureX = 16;
		if(model.textureY == 0)
			model.textureY = 16;

		builder.Current.Add("loader", "flansmod:turborig");
		using (builder.Indentation("textures"))
		{
			foreach(var kvp in textures)
			{
				builder.Current.Add(kvp.Key, kvp.Value);
			}
			builder.Current.Add("default", $"{modName}:skins/{modelName}");
			builder.Current.Add("particle", $"minecraft:block/iron_block");
		}	
		using(builder.Indentation("animations"))
		{
			foreach(Model.AnimationParameter animParam in model.animations)
			{
				if(animParam.key != null && animParam.key.Length > 0 && !builder.Current.ContainsKey(animParam.key))
				{
					if(animParam.isVec3)
						builder.Current.Add(animParam.key, JSONHelpers.ToJSON(animParam.vec3Value));
					else
						builder.Current.Add(animParam.key, animParam.floatValue);
				}
			}
		}
		using(builder.Indentation("display"))
		{
			
			WriteItemTransforms(builder, "firstperson_lefthand", 	new Vector3(0f, 90f, 0f), 	new Vector3(-8f, -7f, -13f), 	Vector3.one);
			WriteItemTransforms(builder, "firstperson_righthand", 	new Vector3(0f, 90f, 0f), 	new Vector3(8f, -7f, -13f), 	Vector3.one);
			WriteItemTransforms(builder, "thirdperson_lefthand", 	new Vector3(0f, -90f, 0f), 	new Vector3(0f, 3.75f, 0f), 	Vector3.one);
			WriteItemTransforms(builder, "thirdperson_righthand",	new Vector3(0f, 90f, 0f), 	new Vector3(0f, 3.25f, 0f), 	Vector3.one);
			
			CalculateGUIPose(model, out Vector3 scale, out Vector3 offset, out Vector3 euler);
			WriteItemTransforms(builder, "gui", 					euler, 	offset, 	scale);
			WriteItemTransforms(builder, "head", 					new Vector3(-90f, 0f, 0f), 	new Vector3(), 					Vector3.one);
			WriteItemTransforms(builder, "ground", 					new Vector3(0f, 0f, 0f), 	new Vector3(0f, 0.15f, 0f), 	Vector3.one / 16f);
			WriteItemTransforms(builder, "fixed", 					new Vector3(0f, 160f, 0f), 	new Vector3(0.5f, 0.5f, 0f), 	Vector3.one);
		} 
		using(builder.Indentation("parts"))
		{
			foreach(Model.Section section in model.sections)
			{
				string partName = Utils.ConvertPartName(section.partName);
				using(builder.Indentation(partName))
				{
					ToJSON(model, builder, section.partName, modelName);
				}
			}
		}
		using(builder.Tabulation("attachPoints"))
		{
			foreach(Model.AttachPoint attachPoint in model.attachPoints)
			{
				using(builder.TableEntry())
				{
					if(attachPoint.name == "scope")
						builder.Current.Add("name", "sights");
					else
						builder.Current.Add("name", attachPoint.name);
					builder.Current.Add("attachTo", attachPoint.attachedTo);
					builder.Current.Add("offset", JSONHelpers.ToJSON(attachPoint.position));
				}
			}
		}
	}

	public static bool ToJSON(Model model, QuickJSONBuilder builder, string part, string modelName)
	{
		Model.Section section = model.GetSection(part);
		if(section == null)
			return false;

		Vector3 origin = Vector3.zero;
		switch(part)
		{
			case "defaultBarrel": origin = model.GetVec3ParamOrDefault("barrelBreakOrigin", origin); break;
			case "revolverBarrel": origin = model.GetVec3ParamOrDefault("revolverFlipPoint", origin); break;
			case "minigunBarrel": origin = model.GetVec3ParamOrDefault("minigunBarrelOrigin", origin); break;
		}

		origin *= 16f;
		builder.Current.Add("origin", JSONHelpers.ToJSON(origin));
		
		using(builder.Tabulation("turboelements"))
		{
			foreach(Model.Piece wrapper in section.pieces)
			{
				using(builder.TableEntry())
				{
					using(builder.Tabulation("verts"))
					{
						Vector3[] verts = wrapper.GetVerts();
						for(int i = 0; i < 8; i++)
						{
							builder.CurrentTable.Add(JSONHelpers.ToJSON(verts[i]));
						}
					}
					builder.Current.Add("eulerRotations", JSONHelpers.ToJSON(wrapper.Euler));
					builder.Current.Add("rotationOrigin", JSONHelpers.ToJSON(wrapper.Origin - origin));
					using(builder.Indentation("faces"))
					{
						Action<Model.Piece, String, EFace, int, int> buildUVs = (wrapper, name, face, texX, texY) =>
						{
							using(builder.Indentation(name))
							{
								using(builder.Tabulation("uv"))
								{
									int[] uvs = wrapper.GetIntUV(wrapper.textureU, wrapper.textureV, face);
									builder.CurrentTable.Add((float)uvs[0] / texX);
									builder.CurrentTable.Add((float)uvs[1] / texY);
									builder.CurrentTable.Add((float)uvs[2] / texX);
									builder.CurrentTable.Add((float)uvs[3] / texY);
								}
								if(face == EFace.up)
									builder.Current.Add("rotation", 180);
								if(face == EFace.down)
									builder.Current.Add("rotation", 90);
								builder.Current.Add("texture", "#default");
							}
						};

						buildUVs(wrapper, "north", EFace.north, model.textureX, model.textureY);
						buildUVs(wrapper, "east", EFace.east, model.textureX, model.textureY);
						buildUVs(wrapper, "south", EFace.south, model.textureX, model.textureY);
						buildUVs(wrapper, "west", EFace.west, model.textureX, model.textureY);
						buildUVs(wrapper, "up", EFace.up, model.textureX, model.textureY);
						buildUVs(wrapper, "down", EFace.down, model.textureX, model.textureY);
					}
				}
			}
		}
		return true;
	}


	private static void WriteItemTransforms(QuickJSONBuilder builder, string key, Vector3 pos, Vector3 euler, Vector3 scale)
	{
		using(builder.Indentation(key))
		{
			builder.Current.Add("rotation", JSONHelpers.ToJSON(pos));
			builder.Current.Add("translation", JSONHelpers.ToJSON(euler));
			builder.Current.Add("scale", JSONHelpers.ToJSON(scale));
		}
	}
}
