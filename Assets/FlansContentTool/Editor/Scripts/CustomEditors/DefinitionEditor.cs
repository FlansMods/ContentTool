using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MagazineDefinition))]
public class MagazineDefinitionEditor : DefinitionEditor
{
}
[CustomEditor(typeof(VehicleDefinition))]
public class VehicleDefinitionEditor : DefinitionEditor
{
}
[CustomEditor(typeof(AttachmentDefinition))]
public class AttachmentDefinitionEditor : DefinitionEditor
{
}


[CustomEditor(typeof(Definition))]
public class DefinitionEditor : Editor
{
	private Texture2D _PreviewTexture = null;
	public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
	{
		//if (_PreviewTexture == null)
		{
			Definition def = ((Definition)serializedObject.targetObject);
			_PreviewTexture = def.RenderStaticPreview();
		}
		return _PreviewTexture;
	}
}
