using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;

public enum EModelExportType
{
	ITEM_MODEL,
	BLOCK_MODEL,
	BLOCKSTATE,
}

public abstract class NodeModelExporter : AssetToJsonExporter<EModelExportType>
{
	public static NodeModelExporter TURBO_MODELS = new TurboModelExporter();
	public static NodeModelExporter VANILLA_ICONS = new VanillaIconExporter();
	public static NodeModelExporter VANILLA_BLOCKS = new VanillaCubeExporter();
	public static NodeModelExporter VANILLA_JSON_MODELS = new VanillaJsonExporter();

	private System.Type TYPE_TO_MATCH;
	public override bool MatchesAssetType(System.Type type) { return TYPE_TO_MATCH.IsAssignableFrom(type); }

	public override string GetOutputFolder() { return "blocks"; }

	public NodeModelExporter(System.Type type)
	{
		TYPE_TO_MATCH = type;
	}

	protected JObject ExportGeometry(GeometryNode geometryNode, Vector2Int textureSize)
	{
		return geometryNode.ExportGeometryNode(textureSize, geometryNode.BakedUV.min);
	}

	protected override bool CanExport(EModelExportType exportType, Object asset, out string path)
	{
		if (asset.TryGetLocation(out ResourceLocation loc))
		{
			switch (exportType)
			{
				case EModelExportType.ITEM_MODEL:
					path = $"{FlansModExport.EXPORT_ROOT}/assets/{loc.Namespace}/models/item/{loc.IDWithoutPrefixes()}.{GetOutputExtension()}";
					return true;
				case EModelExportType.BLOCK_MODEL:
					if (IsBlockModel(loc))
					{
						path = $"{FlansModExport.EXPORT_ROOT}/assets/{loc.Namespace}/models/block/{loc.IDWithoutPrefixes()}.{GetOutputExtension()}";
						return true;
					}
					break;
				case EModelExportType.BLOCKSTATE:
					if (IsBlockModel(loc))
					{
						path = $"{FlansModExport.EXPORT_ROOT}/assets/{loc.Namespace}/blockstates/{loc.IDWithoutPrefixes()}.{GetOutputExtension()}";
						return true;
					}
					break;
			}
		}

		path = "";
		return false;
	}

	protected bool IsBlockModel(Object asset)
	{
		return asset.TryGetLocation(out ResourceLocation loc) && loc.GetPrefixes().Contains("block");
	}
	protected bool IsBlockModel(ResourceLocation loc)
	{
		return loc.GetPrefixes().Contains("block");
	}

	protected override JObject ToJson(EModelExportType exportType, Object asset, IVerificationLogger verifications = null)
	{
		if (asset.TryGetLocation(out ResourceLocation loc))
		{
			switch (exportType)
			{
				// Two setups effectively
				//  - An item just exports to models/item
				//  - A block exports itself to models/block AND creates models/item and blockstates .jsons
				case EModelExportType.ITEM_MODEL:
					return IsBlockModel(loc) ? ExportBlockItemJson(loc) : ExportMainModel(asset, verifications);
				case EModelExportType.BLOCKSTATE:
					return IsBlockModel(loc) ? ExportBlockstateJson(loc) : new JObject();
				case EModelExportType.BLOCK_MODEL:
					return IsBlockModel(loc) ? ExportMainModel(asset, verifications) : new JObject();
			}
		}
		verifications?.Failure("Could not export VanillaJsonRootNode");
		return new JObject();
	}

	protected abstract JObject ExportMainModel(Object asset, IVerificationLogger verifications = null);

	protected JObject ExportBlockstateJson(ResourceLocation loc)
	{
		string blockName = $"{loc.Namespace}:block/{loc.IDWithoutPrefixes()}";

		return new JObject
		{
			["variants"] = new JObject
			{
				["facing=north"] = new JObject
				{
					["model"] = blockName,
					["y"] = 90,
				},
				["facing=east"] = new JObject
				{
					["model"] = blockName,
					["y"] = 180,
				},
				["facing=south"] = new JObject
				{
					["model"] = blockName,
					["y"] = 270,
				},
				["facing=west"] = new JObject
				{
					["model"] = blockName,
					["y"] = 0,
				},
			}
		};
	}

	protected JObject ExportBlockItemJson(ResourceLocation loc)
	{
		string blockName = $"{loc.Namespace}:block/{loc.IDWithoutPrefixes()}";
		return new JObject
		{
			["parent"] = blockName,
			["display"] = new JObject 
			{
				["thirdperson"] = new JObject
				{
					["rotation"] = new JArray(10f, -45f, 170f),
					["translation"] = new JArray(0f, 1.5f, -2.75f),
					["scale"] = new JArray(0.375f, 0.375f, 0.375f),
				}
			}
		};
	}
}

public class VanillaJsonExporter : NodeModelExporter
{
	public VanillaJsonExporter()
		: base(typeof(VanillaJsonRootNode))
	{
	}

	protected override JObject ExportMainModel(Object asset, IVerificationLogger verifications = null)
	{
		if(asset is VanillaJsonRootNode jsonModel)
			if (JToken.ReadFrom(new JsonTextReader(new StringReader(jsonModel.Json))) is JObject jResult)
				return jResult;
		return new JObject();
	}
}

public class VanillaCubeExporter : NodeModelExporter
{
	public VanillaCubeExporter() 
		: base(typeof(VanillaCubeRootNode))
	{
	}

	protected override JObject ExportMainModel(Object asset, IVerificationLogger verifications = null)
	{
		if (asset is VanillaCubeRootNode cube)
		{
			// TODO:
		}
		verifications?.Failure("Could not export VanillaCubeRootNode");
		return new JObject();
	}
}

public class VanillaIconExporter : NodeModelExporter
{
	public VanillaIconExporter()
		: base(typeof(VanillaIconRootNode))
	{
	}

	protected override JObject ExportMainModel(Object asset, IVerificationLogger verifications = null)
	{
		if(asset is VanillaIconRootNode icon)
		{
			return new JObject
			{
				["parent"] = "item/generated",
				["textures"] = new JObject
				{
					["layer0"] = icon.GetLocation().ExportAsModelPath(),
				},
				//["display"] = new JObject
				//{
				//	["thirdperson_righthand"] = new JObject
				//	{
				//		["rotation"] = new JArray(270f, 0f, 0f),
				//		["translation"] = new JArray(0f, 1f, -3f),
				//		["scale"] = new JArray(0.55f, 0.55f, 0.55f),
				//	},
				//	["firstperson_righthand"] = new JObject
				//	{
				//		["rotation"] = new JArray(0f, 225f, 25f),
				//		["translation"] = new JArray(0f, 4f, 2f),
				//		["scale"] = new JArray(1.7f, 1.7f, 1.7f),
				//	}
				//}
			};
		}
		verifications?.Failure("Could not export VanillaIconRootNode");
		return new JObject();
	}
}

public class TurboModelExporter : NodeModelExporter
{
	public TurboModelExporter()
		: base(typeof(TurboRootNode)) 
	{ 
	}

	protected override JObject ExportMainModel(Object asset, IVerificationLogger verifications = null)
	{
		if(asset is TurboRootNode root)
		{
			Vector2Int texSize = root.Textures.Count > 0
				? new Vector2Int(root.Textures[0].Texture.width, root.Textures[0].Texture.height)
				: new Vector2Int(16, 16);

			return new JObject
			{
				["loader"] = "flansmod:turborig",
				["display"] = ExportTransforms(root),
				["textures"] = ExportTextures(root),
				["icons"] = ExportIcons(root),
				["animations"] = ExportAnimations(root),
				["parts"] = ExportSections(root),
				["attachPoints"] = ExportAttachPoints(root),
				["textureSize"] = texSize.ToJson(),
            };
		}

		verifications?.Failure("Could not export definition");
		return new JObject();
	}

	public JObject ExportSection(SectionNode section, Vector2Int textureSize)
	{
		return new JObject()
		{
			["origin"] = section.LocalOrigin.ToJson(),
			["material"] = section.Material.ToString(),
			["turboelements"] = ExportParts(section, textureSize),
		};
	}
	public JObject ExportAttachPoint(AttachPointNode apNode)
	{
		AttachPointNode parent = apNode.ParentNode?.GetParentOfType<AttachPointNode>();

		return new JObject()
		{
			["name"] = apNode.APName,
			["attachTo"] = parent != null ? parent.APName : "body",
			["offset"] = apNode.ExportOrigin.ToJson(),
			["euler"] = apNode.ExportEuler.ToJson(),
		};
	}

	// -----
	// Internals
	private JArray ExportAttachPoints(TurboRootNode root)
	{
		JArray jAttachPoints = new JArray();
		foreach (AttachPointNode apNode in root.GetAllDescendantNodes<AttachPointNode>())
		{
			jAttachPoints.Add(ExportAttachPoint(apNode));
		}
		return jAttachPoints;
	}
	private JObject ExportSections(TurboRootNode root)
	{
		JObject jSections = new JObject();
		foreach (SectionNode section in root.GetAllDescendantNodes<SectionNode>())
			jSections[section.PartName] = ExportSection(section, root.UVMapSize);
		return jSections;
	}
	private JArray ExportParts(SectionNode section, Vector2Int textureSize)
	{
		JArray jParts = new JArray();
		foreach (GeometryNode geometryNode in section.GetChildNodes<GeometryNode>())
		{
			jParts.Add(ExportGeometry(geometryNode, textureSize));
		}
		return jParts;
	}
	private JObject ExportAnimations(TurboRootNode root)
	{
		JObject jAnimations = new JObject();
		foreach (AnimationParameter animParam in root.AnimationParameters)
			if (animParam.key != null && animParam.key.Length > 0)
				jAnimations[animParam.key] = animParam.isVec3 ? animParam.vec3Value.ToJson() : animParam.floatValue;
		return jAnimations;
	}

	private JObject ExportTextures(TurboRootNode root)
	{
		JObject jTextures = new JObject();
		foreach (NamedTexture texture in root.Textures)
			jTextures[texture.Key] = texture.Location.ExportAsTexturePath();
		return jTextures;
	}

	private JObject ExportIcons(TurboRootNode root)
	{
		JObject jTextures = new JObject();
		foreach (NamedTexture icon in root.Icons)
			jTextures[icon.Key] = icon.Location.ExportAsTexturePath();
		return jTextures;
	}

	private JObject ExportTransforms(TurboRootNode root)
	{
		JObject jDisplay = new JObject();
		foreach (ItemPoseNode itemPoseNode in root.GetChildNodes<ItemPoseNode>())
			jDisplay[itemPoseNode.TransformType.GetOutputKey()] = new JObject()
			{
				["translation"] = itemPoseNode.LocalOrigin.ToJson(),
				["rotation"] = itemPoseNode.LocalEuler.ToJson(),
				["scale"] = itemPoseNode.LocalScale.ToJson()
			};
		return jDisplay;
	}
}
