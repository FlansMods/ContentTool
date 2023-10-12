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

	public MinecraftModelPreview Preview = null;

    // Preview models
    public bool IsDirty = false;

	// Currently assuming single texture skinning only
	[Header("Skins")]
    public bool ApplySkin = true;
    public string SelectedSkin = "default";
	public MinecraftModel.NamedTexture GetNamedTexture() 
	{ 
		if(WorkingCopy != null)
			return WorkingCopy.GetNamedTexture(SelectedSkin);
		return null;
	}
	public Texture2D GetTexture2D()
	{
		MinecraftModel.NamedTexture namedTexture = GetNamedTexture();
		if (namedTexture != null)
			return namedTexture.Texture;
		return null;
	}

	[Header("Animations")]
    public bool ApplyAnimation = false;
    public AnimationDefinition SelectedAnimation = null;

	public void SetDirty()
	{
		IsDirty = true;
		//EditorUtility.SetDirty(WorkingCopy);
	}

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
			Preview = CreatePreviewObject(cubeModel, false);
			Preview.Refresh();
		}
        else if (WorkingCopy is TurboRig turboRig)
        {
			Preview = CreatePreviewObject(turboRig, false);
			Preview.Refresh();
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

			MinecraftModel.NamedTexture DefaultTexture = model.GetDefaultTexture();
			if(DefaultTexture != null)
				foreach (TurboPiecePreview piece in GetComponentsInChildren<TurboPiecePreview>())
				{
					piece.CopyExistingTexture(DefaultTexture.Texture);
				}
		}
    }

	private void PreSaveStep()
	{
		if(WorkingCopy != null)
		{
			UVCalculator.AutoUV(WorkingCopy, Preview, SelectedSkin);
		}
	}

    public void SaveAs(string newAssetPath)
    {
		if (WorkingCopy != null)
		{
			PreSaveStep();

			// Serialize-copy our model
			MinecraftModel SaveAsCopy = (MinecraftModel)ScriptableObject.CreateInstance(WorkingCopy.GetType());
			string json = JsonUtility.ToJson(WorkingCopy);
			JsonUtility.FromJsonOverwrite(json, SaveAsCopy);

            // Save to disk
            AssetDatabase.CreateAsset(SaveAsCopy, newAssetPath);
			EditorUtility.SetDirty(SaveAsCopy);

            // Then re-establish the new connection
            ModelOpenedForEdit = AssetDatabase.LoadAssetAtPath<MinecraftModel>(newAssetPath);
            WorkingCopy.name = $"{ModelOpenedForEdit.name}_editing";

			IsDirty = false;
		}
	}

    public void SaveChanges()
    {
        if (WorkingCopy != null && ModelOpenedForEdit != null)
        {
			PreSaveStep();

			string json = JsonUtility.ToJson(WorkingCopy);
            JsonUtility.FromJsonOverwrite(json, ModelOpenedForEdit);
            EditorUtility.SetDirty(ModelOpenedForEdit);
            AssetDatabase.SaveAssets();

			IsDirty = false;
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
		IsDirty = false;

        foreach (MinecraftModelPreview preview in GetComponentsInChildren<MinecraftModelPreview>())
            if (preview != null)
                DestroyImmediate(preview.gameObject);
    }

    #if UNITY_EDITOR
    public void Editor_Toolbox()
    {
		if (GUILayout.Button("Load..."))
			Button_OpenModel();

		EditorGUI.BeginDisabledGroup(WorkingCopy == null);
		{
			if (GUILayout.Button("Save"))
				Button_Save();
			if (GUILayout.Button("Save As..."))
				Button_SaveAs();
			if (GUILayout.Button("Discard"))
				Button_Discard();
		}
		EditorGUI.EndDisabledGroup();
	}

    public void Button_OpenModel()
    {
		bool canLoad = true;
		if (WorkingCopy != null && IsDirty)
		{
			int result = EditorUtility.DisplayDialogComplex(
				"Unsaved changes",
				"Your model has unsaved changes, do you want to save?",
				"Save Changes",
				"Don't Save",
				"Cancel");

			if (result == 0)
				SaveAndCloseModel();
			else if (result == 1)
				DiscardChanges();
			else if (result == 2)
				canLoad = false;
		}

		if (canLoad)
		{
			string loadPath = EditorUtility.OpenFilePanelWithFilters("", "Assets/Content Packs", new string[] { "Imported Model", "asset" });
			if (loadPath != null && loadPath.Length > 0)
			{
				loadPath = loadPath.Substring(loadPath.IndexOf("Assets"));
				MinecraftModel model = AssetDatabase.LoadAssetAtPath<MinecraftModel>(loadPath);
				if (model != null)
					OpenModel(model);
				else
				{
					Debug.LogError($"Could not load model at {loadPath}");
					Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(loadPath);
					for (int i = 0; i < allAssets.Length; i++)
						Debug.LogWarning($"Found asset {allAssets[i]}");
				}
			}
		}
	}

    public void Button_Save()
	{
		if (WorkingCopy != null && IsDirty)
		{
			EditorGUI.BeginDisabledGroup(IsDirty);
			SaveChanges();
			EditorGUI.EndDisabledGroup();
		}
	}

    public void Button_SaveAs()
    {
		if (WorkingCopy != null)
		{
			string savePath = EditorUtility.SaveFilePanelInProject(WorkingCopy.name, "new_model", "asset", "Save Model As...");
			if (savePath != null && savePath.Length > 0)
				SaveAs(savePath);
		}
	}

    public void Button_Discard()
    {
		if (WorkingCopy != null)
		{
			if (!EditorUtility.DisplayDialog("Are you sure?", $"Are you sure you want to discard changes to {WorkingCopy.name}", "Yes", "No"))
				DiscardChanges();
		}
	}
    #endif
}
