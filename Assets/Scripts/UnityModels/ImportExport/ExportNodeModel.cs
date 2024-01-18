using Newtonsoft.Json.Linq;
using UnityEngine;

public static class ExportNodeModel
{
    public static JObject ExportRoot(TurboRootNode root)
    {
        return new JObject
        {
            ["loader"] = "flansmod:turborig",
            ["display"] = ExportTransforms(root),
            ["textures"] = ExportTextures(root),
            ["icons"] = ExportIcons(root),
            ["animations"] = ExportAnimations(root),
            ["parts"] = ExportSections(root),
            ["attachPoints"] = ExportAttachPoints(root),
		};
	}

	public static JObject ExportSection(SectionNode section, Vector2Int textureSize)
	{
		return new JObject()
		{
			["origin"] = ToJson(section.LocalOrigin),
			["material"] = section.Material.ToString(),
			["turboelements"] = ExportParts(section, textureSize),
		};
	}

	public static JObject ExportGeometry(GeometryNode geometryNode, Vector2Int textureSize)
	{
        return geometryNode.ExportGeometryNode(textureSize, geometryNode.BakedUV.min);
	}

    public static JObject ExportAttachPoint(AttachPointNode apNode)
    {
        AttachPointNode parent = apNode.ParentNode?.GetParentOfType<AttachPointNode>();

        return new JObject()
        {
            ["name"] = apNode.APName,
            ["attachTo"] = parent != null ? parent.APName : "body",
            ["offset"] = ToJson(apNode.ExportOrigin),
            ["euler"] = ToJson(apNode.ExportEuler),
		};
    }

    // -----
    // Internals
    private static JArray ExportAttachPoints(TurboRootNode root)
    {
        JArray jAttachPoints = new JArray();
        foreach(AttachPointNode apNode in root.GetAllDescendantNodes<AttachPointNode>())
        {
            jAttachPoints.Add(ExportAttachPoint(apNode));
        }
        return jAttachPoints;
    }
	private static JObject ExportSections(TurboRootNode root)
    {
		JObject jSections = new JObject();
        foreach (SectionNode section in root.GetAllDescendantNodes<SectionNode>())
            jSections[section.PartName] = ExportSection(section, root.UVMapSize);
        return jSections;
	}
    private static JArray ExportParts(SectionNode section, Vector2Int textureSize)
    {
		JArray jParts = new JArray();
        foreach(GeometryNode geometryNode in section.GetChildNodes<GeometryNode>())
        {
            jParts.Add(ExportGeometry(geometryNode, textureSize));
        }
        return jParts;
    }
	private static JObject ExportAnimations(TurboRootNode root)
	{
		JObject jAnimations = new JObject();
		foreach (AnimationParameter animParam in root.AnimationParameters)
			if (animParam.key != null && animParam.key.Length > 0)
			    jAnimations[animParam.key] = animParam.isVec3 ? ToJson(animParam.vec3Value) : animParam.floatValue;
		return jAnimations;
	}

	private static JObject ExportTextures(TurboRootNode root)
    {
		JObject jTextures = new JObject();
        foreach (NamedTexture texture in root.Textures)
            jTextures[texture.Key] = texture.Location.ExportAsTexturePath();
        return jTextures;
	}

	private static JObject ExportIcons(TurboRootNode root)
	{
		JObject jTextures = new JObject();
		foreach (NamedTexture icon in root.Icons)
			jTextures[icon.Key] = icon.Location.ExportAsTexturePath();
		return jTextures;
	}

	private static JObject ExportTransforms(TurboRootNode root)
    {
        JObject jDisplay = new JObject();
        foreach (ItemPoseNode itemPoseNode in root.GetChildNodes<ItemPoseNode>())
            jDisplay[itemPoseNode.TransformType.GetOutputKey()] = new JObject()
            {
                ["translation"] = ToJson(itemPoseNode.LocalOrigin),
                ["rotation"] = ToJson(itemPoseNode.LocalEuler),
                ["scale"] = ToJson(itemPoseNode.LocalScale)
            };
        return jDisplay;
    }

    public static JArray ToJson(this Vector3 v)
    {
        return new JArray() { v.x, v.y, v.z };
    }
}
