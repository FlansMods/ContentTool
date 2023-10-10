using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[ExecuteInEditMode]
public class ModelEditingRig : MonoBehaviour
{
    // Selection
    //[HideInInspector]
    public MinecraftModel ModelOpenedForEdit = null;
    //[HideInInspector]
	public MinecraftModel WorkingCopy = null;

    // Preview models
    public List<ModelSectionPreviewer> Sections = new List<ModelSectionPreviewer>();

    [Header("Skins")]
    public bool ApplySkin = true;
    public Texture2D SelectedSkin = null;

    [Header("Animations")]
    public bool ApplyAnimation = false;
    public AnimationDefinition SelectedAnimation = null;

	public void Update()
	{
        RefreshMeshes();
	}

	public void RefreshMeshes()
    {
        if (WorkingCopy == null)
            return;

        if (WorkingCopy is CubeModel cubeModel)
        {
			MinecraftModelPreview preview = CreatePreviewObject(cubeModel, false);
			preview.Refresh();
		}
        else if (WorkingCopy is TurboRig turboRig)
        {
            MinecraftModelPreview preview = CreatePreviewObject(turboRig, false);
            preview.Refresh();
        }

	}

	public MinecraftModelPreview CreatePreviewObject(TurboRig model, bool forceUpdate)
	{
        Transform existing = transform.Find("turbo");
        if (existing == null || forceUpdate)
        {
            if (existing != null)
                DestroyImmediate(existing.gameObject);
            GameObject go = new GameObject("turbo");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            TurboRigPreview preview = go.AddComponent<TurboRigPreview>();
			preview.SetModel(model);
			return preview;
        }
		existing.GetComponent<TurboRigPreview>().SetModel(model);
		return existing.GetComponent<TurboRigPreview>();
	}

	public MinecraftModelPreview CreatePreviewObject(CubeModel model, bool forceUpdate)
    {
		Transform existing = transform.Find("cube");
		if (existing == null || forceUpdate)
		{
			if (existing != null)
				DestroyImmediate(existing.gameObject);
			GameObject go = new GameObject("cube");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            CubeModelPreview preview = go.AddComponent<CubeModelPreview>();
            preview.SetModel(model);
            return preview;
		}
		return existing.GetComponent<CubeModelPreview>();
	}

    public void OpenModel(MinecraftModel model)
    {
		if (WorkingCopy != null)
		{
			DiscardChanges();
		}

		ModelOpenedForEdit = model;
        
        if(WorkingCopy == null)
        {
            WorkingCopy = (MinecraftModel)ScriptableObject.CreateInstance(model.GetType());
            
            string json = JsonUtility.ToJson(model);
            JsonUtility.FromJsonOverwrite(json, WorkingCopy);
            WorkingCopy.name = $"{ModelOpenedForEdit.name}_editing";

            RefreshMeshes();
		}
    }

    public void SaveChanges()
    {
        if (WorkingCopy != null && ModelOpenedForEdit != null)
        {
            string json = JsonUtility.ToJson(WorkingCopy);
            JsonUtility.FromJsonOverwrite(json, ModelOpenedForEdit);
            EditorUtility.SetDirty(ModelOpenedForEdit);
            AssetDatabase.SaveAssets();
        }
        else Debug.LogError("Can't save changes without an open model");
	}

    public void SaveAndCloseModel()
    {
        SaveChanges();
        ModelOpenedForEdit = null;
        WorkingCopy = null;

		foreach (MinecraftModelPreview preview in GetComponentsInChildren<MinecraftModelPreview>())
			if (preview != null)
				DestroyImmediate(preview.gameObject);
	}

    public void DiscardChanges()
    {
        ModelOpenedForEdit = null;
        WorkingCopy = null;

        foreach (MinecraftModelPreview preview in GetComponentsInChildren<MinecraftModelPreview>())
            if(preview != null)
                DestroyImmediate(preview.gameObject);
    }
}
