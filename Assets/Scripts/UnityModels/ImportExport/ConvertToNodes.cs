using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static MinecraftModel;

public static class ConvertToNodes
{
	public static RootNode CreateEmpty(string name)
	{
		GameObject go = new GameObject(name);
		RootNode rootNode = go.AddComponent<RootNode>();
		return rootNode;
	}

	public static RootNode FromTurboRig(TurboRig root)
	{
		GameObject go = new GameObject(root.name);
		RootNode rootNode = go.AddComponent<RootNode>();

		foreach (MinecraftModel.NamedTexture iconTex in root.Icons)
			rootNode.Icons.Add(new NamedTexture()
			{
				Key = iconTex.Key,
				Texture = iconTex.Texture,
				Location = iconTex.Location,
			});
		foreach (MinecraftModel.NamedTexture skinTex in root.Textures)
			rootNode.Textures.Add(new NamedTexture()
			{
				Key = skinTex.Key,
				Texture = skinTex.Texture,
				Location = skinTex.Location,
			});
		rootNode.AnimationParameters.AddRange(root.AnimationParameters);
		rootNode.UVMapSize = root.BakedUVMap.MaxSize;
		foreach(ItemTransform pose in root.Transforms)
		{
			GameObject poseGO = new GameObject($"pose_{pose.Type}");
			ItemPoseNode poseNode = poseGO.AddComponent<ItemPoseNode>();
			poseNode.TransformType = (ItemDisplayContext)(int)pose.Type;
			poseNode.transform.parent = rootNode.transform;
			poseNode.transform.localPosition = pose.Position;
			poseNode.transform.localRotation = pose.Rotation;
			poseNode.transform.localScale = pose.Scale;
		}

		// Create AttachPointNodes 
		Dictionary<string, AttachPointNode> apNodes = new Dictionary<string, AttachPointNode>();
		foreach(AttachPoint ap in root.AttachPoints)
			if(ap.name != "body")
				apNodes[ap.name] = CreateAPNode(ap.name);
		foreach (TurboModel section in root.Sections)
			if (!apNodes.ContainsKey(section.PartName) && section.PartName != "body")
				apNodes.Add(section.PartName, CreateAPNode(section.PartName));

		// And create the Unity heirarchy for them
		foreach(AttachPointNode node in apNodes.Values)
		{
			string attachedTo = root.GetAttachedTo(node.APName);
			if(apNodes.TryGetValue(attachedTo, out AttachPointNode parentNode))
				node.transform.SetParent(parentNode.transform);
			else
				node.transform.SetParent(rootNode.transform);
			node.transform.localPosition = root.GetAttachmentOffset(node.APName);
			node.transform.localEulerAngles = root.GetAttachmentEuler(node.APName);
			node.transform.localScale = Vector3.one;
		}


		// Create SectionNodes
		Dictionary<string, SectionNode> sectionNodes = new Dictionary<string, SectionNode>();
		foreach(TurboModel section in root.Sections)
		{
			SectionNode node = CreateSectionNode(section.PartName);
			node.Material = section.Material;
			sectionNodes.Add(section.PartName, node);
			if (apNodes.TryGetValue(section.PartName, out AttachPointNode parentNode))
				node.transform.SetParent(parentNode.transform);
			else
				node.transform.SetParent(rootNode.transform);
			node.transform.localPosition = Vector3.zero;
			node.transform.localEulerAngles = Vector3.zero;
			node.transform.localScale = Vector3.one;

			// And add pieces to them
			for(int i = 0; i < section.Pieces.Count; i++)
			{
				TurboPiece piece = section.Pieces[i];
				GeometryNode geomNode = CreateBoxNode(piece, $"part_{i}");
				geomNode.transform.SetParent(node.transform);
				geomNode.transform.localPosition = Quaternion.Euler(piece.Euler) * piece.Pos + piece.Origin;
				geomNode.transform.localEulerAngles = piece.Euler;
				geomNode.BakedUV = root.BakedUVMap.GetPlacedPatch($"{section.PartName}/{i}").Bounds;
			}
		}

		return rootNode;
	}

	public static EmptyNode GetOrCreateEmptyParent(Node node)
	{
		if (!(node.ParentNode is EmptyNode emptyNode))
		{
			// Insert "TEMP_NODE" in the heirarchy to hold this rotation
			GameObject emptyGO = new GameObject("temp");
			emptyGO.transform.SetParentZero(node.transform.parent);
			node.transform.SetParentZero(emptyGO.transform);
			emptyNode = emptyGO.AddComponent<EmptyNode>();
		}
		return emptyNode;
	}
	public static void ApplyTemporaryRotationOrigin(Node node, Vector3 rotOrigin)
	{
		GetOrCreateEmptyParent(node).LocalOrigin = rotOrigin;
	}
	public static void ApplyTemporaryRotationAngleX(Node node, float angleX)
	{
		Vector3 eulers = GetOrCreateEmptyParent(node).LocalEuler;
		eulers.x = angleX;
		GetOrCreateEmptyParent(node).LocalEuler = eulers;
	}
	public static void ApplyTemporaryRotationAngleY(Node node, float angleY)
	{
		Vector3 eulers = GetOrCreateEmptyParent(node).LocalEuler;
		eulers.y = angleY;
		GetOrCreateEmptyParent(node).LocalEuler = eulers;
	}
	public static void ApplyTemporaryRotationAngleZ(Node node, float angleZ)
	{
		Vector3 eulers = GetOrCreateEmptyParent(node).LocalEuler;
		eulers.z = angleZ;
		GetOrCreateEmptyParent(node).LocalEuler = eulers;
	}

	public static AttachPointNode GetOrCreateAttachPointNode(Node parent, string apName)
	{
		AttachPointNode apNode = parent.FindDescendant<AttachPointNode>($"ap_{apName}");
		if (apNode == null)
		{
			apNode = CreateAPNode(apName);
			apNode.transform.SetParent(parent.transform);
			apNode.transform.localPosition = Vector3.zero;
			apNode.transform.localRotation = Quaternion.identity;
			apNode.transform.localScale = Vector3.one;
		}
		return apNode;
	}

	public static TGeometryNodeType GetOrCreateGeometryNode<TGeometryNodeType>(Node parent, string pieceName) where TGeometryNodeType : GeometryNode
	{
		TGeometryNodeType geomNode = parent.FindDescendant<TGeometryNodeType>(pieceName);
		if (geomNode == null)
		{
			GameObject go = new GameObject(pieceName);
			geomNode = go.AddComponent<TGeometryNodeType>();
			geomNode.transform.SetParentZero(parent.transform);
		}
		return geomNode;
	}

	// When in this "build" mode, don't set up the AP/section heirarchy yet
	public static SectionNode GetOrCreateSectionNode(RootNode rootNode, string sectionName)
	{
		SectionNode sectionNode = rootNode.FindDescendant<SectionNode>(sectionName);
		if(sectionNode == null)
		{
			sectionNode = CreateSectionNode(sectionName);
			sectionNode.transform.SetParentZero(rootNode.transform);
		}
		return sectionNode;
	}
	public static ItemPoseNode GetOrCreateItemPoseNode(RootNode rootNode, ItemDisplayContext transformType)
	{
		string key = $"pose_{transformType}";
		ItemPoseNode poseNode = rootNode.FindChild<ItemPoseNode>(key);
		if(poseNode == null)
		{
			GameObject poseGO = new GameObject();
			poseNode = poseGO.AddComponent<ItemPoseNode>();
			poseNode.transform.SetParentZero(rootNode.transform);
		}
		return poseNode;
	}

	public static AttachPointNode CreateAPNode(string name)
	{
		GameObject go = new GameObject($"ap_{name}");
		AttachPointNode apNode = go.AddComponent<AttachPointNode>();
		return apNode;
	}

	public static SectionNode CreateSectionNode(string name)
	{
		GameObject go = new GameObject(name);
		SectionNode sectionNode = go.AddComponent<SectionNode>();
		return sectionNode;
	}

	public static GeometryNode CreateBoxNode(TurboPiece piece, string name)
	{
		GameObject go = new GameObject(name);
		if (piece.IsBox())
		{
			BoxGeometryNode boxNode = go.AddComponent<BoxGeometryNode>();
			boxNode.Dim = piece.Dim;
			return boxNode;
		}
		else
		{
			ShapeboxGeometryNode shapeboxNode = go.AddComponent<ShapeboxGeometryNode>();
			shapeboxNode.Dim = piece.Dim;
			shapeboxNode.Offsets = piece.Offsets;
			return shapeboxNode;
		}
	}

	public static BoxGeometryNode CreateBoxNode(string name)
	{
		GameObject go = new GameObject(name);
		BoxGeometryNode boxNode = go.AddComponent<BoxGeometryNode>();
		return boxNode;
	}

	public static ShapeboxGeometryNode CreateShapeboxNode(string name)
	{
		GameObject go = new GameObject(name);
		ShapeboxGeometryNode boxNode = go.AddComponent<ShapeboxGeometryNode>();
		return boxNode;
	}
}
