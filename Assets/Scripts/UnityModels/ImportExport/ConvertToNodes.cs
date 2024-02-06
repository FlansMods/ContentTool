using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class ConvertToNodes
{
	public static TurboRootNode CreateEmpty(string name)
	{
		GameObject go = new GameObject(name);
		TurboRootNode rootNode = go.AddComponent<TurboRootNode>();
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
	public static SectionNode GetOrCreateSectionNode(TurboRootNode rootNode, string sectionName)
	{
		SectionNode sectionNode = rootNode.FindDescendant<SectionNode>(sectionName);
		if(sectionNode == null)
		{
			sectionNode = CreateSectionNode(sectionName);
			sectionNode.transform.SetParentZero(rootNode.transform);
		}
		return sectionNode;
	}
	public static ItemPoseNode GetOrCreateItemPoseNode(TurboRootNode rootNode, ItemDisplayContext transformType)
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
