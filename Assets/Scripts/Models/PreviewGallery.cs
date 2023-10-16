using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewGallery : MonoBehaviour
{
	public enum EPreviewContext
	{
		Default,
		First,
		Third_Steve,
		Third_Alex,
		GUI,

	}

	public EPreviewContext Context = EPreviewContext.Default;
	public ModelEditingRig ModelRig = null;

	public Transform DefaultPreview = null;
	public Transform FirstPersonPreview = null;
	public Transform SteveHandPreview = null;
	public Transform AlexHandPreview = null;
	public Transform GUIPreview = null;

	public Transform GetTransform(EPreviewContext context)
	{
		switch(context)
		{
			case EPreviewContext.Default: return DefaultPreview;
			case EPreviewContext.First: return FirstPersonPreview;
			case EPreviewContext.Third_Steve: return SteveHandPreview;
			case EPreviewContext.Third_Alex: return AlexHandPreview;
			case EPreviewContext.GUI: return GUIPreview;
			default: return DefaultPreview;
		}
	}

	public void SelectContext(EPreviewContext context)
	{
		Context = context;
		Transform root = GetTransform(context);
		if(root != null && ModelRig != null)
		{
			ModelRig.transform.SetParent(root);
			ModelRig.transform.localPosition = Vector3.zero;
			ModelRig.transform.localRotation = Quaternion.identity;
			ModelRig.transform.localScale = Vector3.one;
		}
	}

}
