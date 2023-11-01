using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using UnityEditor.Build;
using Unity.VisualScripting;

public class FlansModToolbox : EditorWindow
{
    [MenuItem ("Flan's Mod/Toolbox")]
    public static void ShowWindow () 
	{
        GetWindow(typeof(FlansModToolbox));
    }

	public void OnEnable()
	{
		EditorApplication.update += Repaint;
	}

	public void OnDisable()
	{
		EditorApplication.update -= Repaint;
	}

	private DefinitionImporter DefinitionImporter = null;
	private List<ContentPack> Packs 
	{
		get	
		{
			if (DefinitionImporter == null)
				DefinitionImporter = FindObjectOfType<DefinitionImporter>();
			return DefinitionImporter.Packs;
		}
	}
	private enum Tab
	{
		Import,
		ContentPacks,
		Rigs,
	}
	private static readonly string[] TabNames = new string[]
	{
		"Import",
		"Content Packs",
		"Rig Editor",
	};

	private Tab SelectedTab = Tab.ContentPacks;
	

	private string recipeFolder = "";
	private string copyFromMat = "iron";
	private string copyToMat = "aluminium";

	private Vector2 scroller = Vector2.zero;
	void OnGUI()
	{
		scroller = GUILayout.BeginScrollView(scroller);
		GUILayout.BeginVertical();
		SelectedTab = (Tab)GUILayout.Toolbar((int)SelectedTab, TabNames, GUILayout.MaxWidth(Screen.width));
		switch (SelectedTab)
		{
			case Tab.Import:
				ImportTab();
				break;
			case Tab.ContentPacks:
				ContentPacksTab();
				break;
			case Tab.Rigs:
				RigsTab();
				break;
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}

	// -------------------------------------------------------------------------------------------------------
	#region Import Tab
	// -------------------------------------------------------------------------------------------------------
	private Editor DefinitionImporterEditor = null;
	private void ImportTab()
	{
		DefinitionImporter inst = DefinitionImporter.inst;
		if(DefinitionImporterEditor == null || DefinitionImporterEditor.target != inst)
		{
			DefinitionImporterEditor = Editor.CreateEditor(inst);
		}
		if(DefinitionImporterEditor != null)
		{
			DefinitionImporterEditor.OnInspectorGUI();
		}
	}



	/*
	private void p()
	{


		// Back to front? Should be imports

		foreach (ContentPack pack in inst.Packs)
		{
			string packPath = $"{pack.name}";

			List<string> originalAssetPaths = new List<string>();
			bool hasOriginalImportFolder = false;
			if (inst.UnimportedPacks.Contains(pack.name))
			{
				hasOriginalImportFolder = true;
			}

			if (NestedFoldout(packPath, pack.name))
			{
				EditorGUI.indentLevel++;

				GUILayout.Label($"Pack: {pack.name}", FlanStyles.BoldLabel);
				GUILayout.Label(hasOriginalImportFolder ? $"Matching source assets at Import/Content Packs/{pack.name}" : "No matching assets in Import/Content Packs", FlanStyles.BoldLabel);

				string defsPath = $"{packPath}/Definitions";
				if(NestedFoldout(defsPath, "Definitions"))
				{
					EditorGUI.indentLevel++;
					foreach (var kvp in pack.GetSortedContent())
					{
						string typePath = $"{defsPath}/{kvp.Key}";
						if (NestedFoldout(typePath, kvp.Key.ToString()))
						{
							EditorGUI.indentLevel++;
							foreach (Definition def in kvp.Value)
							{
								ContentNode(def, pack.name);
							}
							EditorGUI.indentLevel--;
						}
					}
					
					EditorGUI.indentLevel--;
				}
				string modelsPath = $"{packPath}/Models";
				if (NestedFoldout(modelsPath, "Models"))
				{

				}
				EditorGUI.indentLevel--;
			}
			
		}


		// Import location
		FolderSelector("Export Location", inst.ExportRoot, "Assets/Export");
	}
	*/
	
	#endregion
	// -------------------------------------------------------------------------------------------------------


	// -------------------------------------------------------------------------------------------------------
	#region Content Packs Tab
	// -------------------------------------------------------------------------------------------------------
	private int SelectedContentPackIndex = -1;
	private string SelectedContentPackName { get { return SelectedContentPackIndex >= 0 ? Packs[SelectedContentPackIndex].ModName : "None"; } }
	private ContentPack SelectedContentPack { get { return SelectedContentPackIndex >= 0 ? Packs[SelectedContentPackIndex] : null; } }
	private Editor ContentPackEditor = null;
	private void ContentPacksTab()
	{
		FlanStyles.BigHeader("Content Packs");
		List<string> packNames = new List<string>();
		packNames.Add("None");
		for (int i = 0; i < Packs.Count; i++)
		{
			ContentPack pack = Packs[i];
			packNames.Add(pack.ModName);
		}
		SelectedContentPackIndex = EditorGUILayout.Popup(SelectedContentPackIndex+1, packNames.ToArray()) - 1;
		EditorGUILayout.ObjectField(SelectedContentPack, typeof(ContentPack), false);

		if(ContentPackEditor == null || ContentPackEditor.target != SelectedContentPack)
		{
			ContentPackEditor = Editor.CreateEditor(SelectedContentPack);
		}
		if(ContentPackEditor != null)
		{
			ContentPackEditor.OnInspectorGUI();
		}
	}
	#endregion
	// -------------------------------------------------------------------------------------------------------


	// -------------------------------------------------------------------------------------------------------
	#region Rigs Tab
	// -------------------------------------------------------------------------------------------------------
	private List<ModelEditingRig> ActiveRigs = new List<ModelEditingRig>();
	private ModelEditingRig SelectedRig { get { return 0 <= SelectedRigIndex && SelectedRigIndex < ActiveRigs.Count ? ActiveRigs[SelectedRigIndex] : null; } }
	private int SelectedRigIndex = 0;
	private Editor SelectedRigEditor = null;
	private void RigsTab()
	{
		List<string> modelNames = new List<string>();
		ActiveRigs.Clear();
		foreach (ModelEditingRig rig in FindObjectsOfType<ModelEditingRig>())
		{
			ActiveRigs.Add(rig);
			modelNames.Add(rig.ModelOpenedForEdit != null ? rig.ModelOpenedForEdit.name : "No model opened");
		}

		FlanStyles.BigHeader("Rig Editor");

		if (GUILayout.Button("Create New Rig"))
		{
			GameObject newGO = new GameObject("ModelRig");
			newGO.AddComponent<ModelEditingRig>();
		}

		var RIG_COL_X = GUILayout.Width(64);
		var MODEL_COL_X = GUILayout.Width(128);
		var ATTACH_COL_X = GUILayout.Width(128);
		var AP_COL_X = GUILayout.Width(64);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Rig", RIG_COL_X);
		GUILayout.Label("Model", MODEL_COL_X);
		GUILayout.Label("Attachment", ATTACH_COL_X);
		GUILayout.Label("AP", AP_COL_X);
		GUILayout.Label("Select");
		GUILayout.EndHorizontal();

		for (int i = 0; i < ActiveRigs.Count; i++)
		{
			ModelEditingRig rig = ActiveRigs[i];
			GUILayout.BeginHorizontal();
			//if(GUILayout.Button("Inspect", GUILayout.Width(32)))
			//{
			//	SelectedRigIndex = i;
			//	Selection.SetActiveObjectWithContext(rig, this);
			//}
			EditorGUILayout.ObjectField(rig, typeof(ModelEditingRig), false, RIG_COL_X);
			ModelButton(rig, MODEL_COL_X);

			AttachPoseDropdown(rig, ATTACH_COL_X);
			AttachPointDropdown(rig, AP_COL_X);

			EditorGUI.BeginDisabledGroup(SelectedRigIndex == i);
			if (GUILayout.Button("Select"))
			{
				SelectedRigIndex = i;
				Selection.SetActiveObjectWithContext(SelectedRig, this);
			}
			EditorGUI.EndDisabledGroup();

			GUILayout.EndHorizontal();
		}

		
		EditorGUI.BeginDisabledGroup(SelectedRig == null);
		if(SelectedRig != null)
			FlanStyles.BigHeader($"{SelectedRig.name} [{SelectedRig.ModelName}]");
		if(SelectedRigEditor == null || SelectedRigEditor.target != SelectedRig)
		{
			SelectedRigEditor = Editor.CreateEditor(SelectedRig);
		}
		if(SelectedRigEditor != null)
		{
			SelectedRigEditor.OnInspectorGUI();
		}		
		EditorGUI.EndDisabledGroup();
	}

	private void ModelButton(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Object changedModel = EditorGUILayout.ObjectField(rig.ModelOpenedForEdit, typeof(MinecraftModel), false, options);
		if (changedModel != rig.ModelOpenedForEdit)
			rig.OpenModel(changedModel as MinecraftModel);
	}

	private static readonly string[] APDefaults = new string[] {
		"NotAttached",
		"DefaultPose",
		"Alex_RightHandPose",
		"Alex_LeftHandPose",
		"Steve_RightHandPose",
		"Steve_LeftHandPose",
		"GUIPose",
	};

	private void AttachPoseDropdown(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		List<string> APs = new List<string>(APDefaults);

		// First, check if this is attached to one of our known parents
		int selectedIndex = 0;
		if (rig.transform.parent != null)
		{
			selectedIndex = APs.IndexOf(rig.transform.parent.name);
			if (selectedIndex == -1)
				selectedIndex = 0;
		}

		// Then check if this is attached to another rig
		int myRigIndex = 0;
		for (int index = 0; index < ActiveRigs.Count; index++)
		{
			ModelEditingRig attachToRig = ActiveRigs[index];
			if (attachToRig != rig)
			{
				APs.Add($"{attachToRig.name}_{index}");
				if (rig.transform.parent != null && rig.transform.parent.GetComponentInParent<ModelEditingRig>() == attachToRig)
					selectedIndex = APs.Count - 1;
			}
			else
				myRigIndex = index;
		}

		int changedIndex = EditorGUILayout.Popup(selectedIndex, APs.ToArray(), options);
		if (changedIndex != selectedIndex)
		{
			Transform attachTo = null;
			if (changedIndex >= APDefaults.Length)
			{
				int relativeIndex = changedIndex - APDefaults.Length;
				if (relativeIndex >= myRigIndex)
					relativeIndex++;
				attachTo = ActiveRigs[relativeIndex].transform;
			}
			else
				attachTo = GameObject.Find(APs[changedIndex])?.transform;
			rig.transform.SetParent(attachTo);
			rig.transform.localPosition = Vector3.zero;
			rig.transform.localRotation = Quaternion.identity;
			rig.transform.localScale = Vector3.one;
		}
	}

	private void AttachPointDropdown(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Transform parent = rig.transform.parent;
		ModelEditingRig parentRig = parent?.GetComponentInParent<ModelEditingRig>();
		if (parentRig != null && parentRig.ModelOpenedForEdit is TurboRig turbo)
		{
			List<string> apNames = new List<string>();
			int attachedTo = 0;
			apNames.Add("none");
			for (int i = 0; i < turbo.AttachPoints.Count; i++)
			{
				AttachPoint ap = turbo.AttachPoints[i];
				apNames.Add(ap.name);
				if (ap.name == parent.name)
					attachedTo = i + 1;
			}
			int changedAttachedTo = EditorGUILayout.Popup(attachedTo, apNames.ToArray(), options);
			if (changedAttachedTo != attachedTo)
			{
				Transform newParent = parentRig.transform;
				if (changedAttachedTo != 0)
				{
					newParent = parentRig.transform.FindRecursive(turbo.AttachPoints[changedAttachedTo - 1].name);
				}
				rig.transform.SetParent(newParent);
				rig.transform.localPosition = Vector3.zero;
				rig.transform.localRotation = Quaternion.identity;
				rig.transform.localScale = Vector3.one;
			}
		}
		else
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Popup(0, new string[] { "N/A" }, options);
			EditorGUI.EndDisabledGroup();
		}
	}

	#endregion
	// -------------------------------------------------------------------------------------------------------

	private MinecraftModel UpdateModel(Model model, ContentPack pack, Definition def)
	{
		switch (model.Type)
		{
			case Model.ModelType.TurboRig:
			{
				TurboRig rig = CreateInstance<TurboRig>();
				foreach(Model.Section modelSection in model.sections)
				{
					TurboModel section = new TurboModel();
					section.PartName = Utils.ConvertPartName(modelSection.partName);
					section.Pieces = new List<TurboPiece>();
					for (int i = 0; i < modelSection.pieces.Length; i++)
						section.Pieces.Add( modelSection.pieces[i].CopyAsTurbo());
					rig.Sections.Add(section);
				}
				foreach (Model.AnimationParameter animParam in model.animations)
					rig.AnimationParameters.Add(new AnimationParameter(animParam.key, animParam.isVec3, animParam.floatValue, animParam.vec3Value));
				foreach (Model.AttachPoint attachPoint in model.attachPoints)
					rig.AttachPoints.Add(new AttachPoint(
						Utils.ConvertPartName(attachPoint.name), 
						Utils.ConvertPartName(attachPoint.attachedTo), 
						attachPoint.position));

				rig.GetOrCreate("barrel");
				rig.GetOrCreate("slide");
				rig.GetOrCreate("ammo_0");
				rig.GetOrCreate("stock");
				rig.GetOrCreate("sights");
				rig.GetOrCreate("grip");


				rig.TextureX = model.textureX;
				rig.TextureY = model.textureY;

				if(def.Skin != null)
					rig.AddTexture("default", pack.ModName, def.Skin);
				if(def is GunDefinition gun)
				{
					foreach(PaintjobDefinition paint in gun.paints.paintjobs)
					{
						rig.AddTexture(paint.textureName, pack.ModName, def.GetSkin(paint.textureName));
					}
				}
				return rig;
			}
			case Model.ModelType.Block:
			{
				CubeModel cube = CreateInstance<CubeModel>();
				cube.north = new ResourceLocation(model.north);
				cube.east = new ResourceLocation(model.east);
				cube.south = new ResourceLocation(model.south);
				cube.west = new ResourceLocation(model.west);
				cube.top = new ResourceLocation(model.top);
				cube.bottom = new ResourceLocation(model.bottom);
				cube.particle = new ResourceLocation(model.north);
				return cube;
			}
			case Model.ModelType.Custom:
			{
				BlockbenchModel bbModel = CreateInstance<BlockbenchModel>();
				return bbModel;
			}
			case Model.ModelType.Item:
			{
				ItemModel itemModel = CreateInstance<ItemModel>();
				return itemModel;
			}
		}
		return null;
	}
}
