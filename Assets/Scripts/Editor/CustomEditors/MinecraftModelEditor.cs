using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static MinecraftModel;

public abstract class MinecraftModelEditor : Editor
{
	public static readonly char[] SLASHES = new char[] { '/', '\\' };

	private static ContentManager DefImporter = null;
	protected static ContentManager DefinitionImporter
	{ get 
		{
			if (DefImporter == null)
				DefImporter = FindObjectOfType<ContentManager>();
			return DefImporter;
		}
	}

	private List<Definition> RelatedDefinitions = null;
	private Dictionary<Texture2D, string> LinkedTextures = null;
	private Dictionary<Texture2D, string> RelatedTextures = null;

	private void RefreshMatches(ContentPack pack, ResourceLocation resLoc, MinecraftModel mcModel)
	{
		RelatedDefinitions = new List<Definition>();
		foreach (Definition def in pack.AllContent)
		{
			if (ResourceLocation.IsSameObjectGroup(def.GetLocation(), resLoc))
				RelatedDefinitions.Add(def);
		}
		LinkedTextures = new Dictionary<Texture2D, string>();
		foreach (NamedTexture namedTexture in mcModel.Textures)
		{
			string path = AssetDatabase.GetAssetPath(namedTexture.Texture);
			int lastSlash = path.LastIndexOf('/');
			if (lastSlash != -1)
			{
				path = path.Substring(0, lastSlash);
				lastSlash = path.LastIndexOf('/');
				if (lastSlash != -1)
					path = path.Substring(lastSlash + 1);
			}

			if(!LinkedTextures.ContainsKey(namedTexture.Texture))
				LinkedTextures.Add(namedTexture.Texture, path);
		}
		RelatedTextures = new Dictionary<Texture2D, string>();
		foreach(string textureGUID in AssetDatabase.FindAssets("t:Texture"))
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(textureGUID);
			string convertedPath = assetPath;
			string subfolder = "";
			int lastSlash = convertedPath.LastIndexOf('/');
			if (lastSlash != -1)
			{
				subfolder = convertedPath.Substring(0, lastSlash);
				convertedPath = convertedPath.Substring(lastSlash + 1);
				lastSlash = subfolder.LastIndexOf('/');
				if (lastSlash != -1)
					subfolder = subfolder.Substring(lastSlash + 1);
			}
			if (subfolder.Length == 0 || subfolder == "skins")
			{
				int dot = convertedPath.IndexOf('.');
				if (dot != -1)
					convertedPath = convertedPath.Substring(0, dot);
				if (convertedPath.Contains(resLoc.IDWithoutPrefixes()))
				{
					Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (!LinkedTextures.ContainsKey(tex))
						RelatedTextures.Add(tex, subfolder);
				}
			}
		}
	}

	private enum Tab
	{
		Modelling,
		Texturing,
		Posing,
		Verification,
		Debug
	}
	private static readonly string[] TabNames = new string[] {
		"Modelling",
		"Texturing",
		"Posing",
		"Verification",
		"Debug"
	};
	private Tab CurrentTab = Tab.Modelling;


	public override void OnInspectorGUI()
	{
		if (target is MinecraftModel mcModel)
		{
			Header();
			CurrentTab = (Tab)GUILayout.Toolbar((int)CurrentTab, TabNames);
			switch (CurrentTab)
			{
				case Tab.Modelling:
					ModellingTab(mcModel);
					break;
				case Tab.Texturing:
					TexturingTab(mcModel);
					break;
				case Tab.Posing:
					PosingTab(mcModel);
					break;
				case Tab.Verification:
					VerificationsTab(mcModel);
					break;
				case Tab.Debug:
					DebugTab(mcModel);
					break;
			}
		}
		else GUILayout.Label($"Invalid target {target} for this editor");
	}

	protected virtual void Header()
	{
		FlanStyles.BigHeader("Model Editor");
	}

	private ModelEditingRig RigSelector(MinecraftModel mcModel)
	{
		ModelEditingRig[] rigs = FindObjectsOfType<ModelEditingRig>();
		ModelEditingRig matchingRig = null;
		for (int i = 0; i < rigs.Length; i++)
		{
			if (rigs[i].ModelOpenedForEdit == mcModel)
				matchingRig = rigs[i];
		}

		if(matchingRig != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Model is open in:");
			EditorGUILayout.ObjectField(matchingRig, typeof(ModelEditingRig), true);
			GUILayout.EndHorizontal();
		}
		else if(rigs.Length > 0)
		{
			GUILayout.Label("Model is not open. Select a rig in which to edit it.");
			for (int i = 0; i < rigs.Length; i++)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.ObjectField(rigs[i], typeof(ModelEditingRig), true);
				if(GUILayout.Button("Select"))
				{
					matchingRig = rigs[i];
					matchingRig.OpenModel(mcModel);
				}
				GUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Create"))
			{
				GameObject newRigGO = new GameObject("New Rig");
				matchingRig = newRigGO.AddComponent<ModelEditingRig>();
				matchingRig.OpenModel(mcModel);
			}
		}
		else
		{
			GUILayout.Label("Model is not open, and there are no rigs. Do you want to create one?");
			if (GUILayout.Button("Create"))
			{
				GameObject newRigGO = new GameObject("New Rig");
				matchingRig = newRigGO.AddComponent<ModelEditingRig>();
				matchingRig.OpenModel(mcModel);
			}
		}

		return matchingRig;
	}

	// ------------------------------------------------------------------------------------
	#region Modelling Tab
	// ------------------------------------------------------------------------------------
	private Editor ModelSubEditor = null;
	private List<string> SubPieceFoldouts = new List<string>();
	private void ModellingTab(MinecraftModel mcModel)
	{
		ModelEditingRig rig = RigSelector(mcModel);
		if (rig != null)
		{
			//if (GUILayout.Button("Go to Root"))
			//	Selection.SetActiveObjectWithContext(rig.Preview, this);

			InitialModellingNode(rig.Preview);

			Object objectToInspect = null;
			if (Selection.activeGameObject != null)
			{
				MinecraftModelPreview modelPreview = Selection.activeGameObject.GetComponent<MinecraftModelPreview>();
				if(modelPreview != null && modelPreview.GetModel() == mcModel)
				{
					objectToInspect = modelPreview;
				}
			}
			if(ModelSubEditor == null || objectToInspect != ModelSubEditor.target)
			{
				ModelSubEditor = objectToInspect == null ? null : CreateEditor(objectToInspect);
			}
			if(ModelSubEditor != null)
			{
				//ModelSubEditor.OnInspectorGUI();
			}
		}
	}
	public void InitialModellingNode(MinecraftModelPreview root)
	{
		MinecraftModelPreview parent = root.GetParent();
		if(parent != null)
		{
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(FlanStyles.NavigateBack, GUILayout.Width(MODELLING_BUTTON_X)))
				Selection.activeObject = parent;
			GUILayout.Label($"Parent: {parent}");
			GUILayout.EndHorizontal();
		}

		ModellingNode(root, "", true);
	}
	public void AttachPointNode(TurboAttachPointPreview ap)
	{
		MinecraftModelPreview model = ap.Parent;
		TurboAttachPointPreview apParent = ap.transform.parent.GetComponent<TurboAttachPointPreview>();
		if(apParent != null)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(FlanStyles.NavigateBack, GUILayout.Width(MODELLING_BUTTON_X)))
				Selection.activeObject = apParent;
			GUILayout.Label($"Attached to AP: {apParent}");
			GUILayout.EndHorizontal();
		}
		else if (model != null)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(FlanStyles.NavigateBack, GUILayout.Width(MODELLING_BUTTON_X)))
				Selection.activeObject = model;
			GUILayout.Label($"Parent: {model}");
			GUILayout.EndHorizontal();
		}

		foreach(Transform child in ap.transform)
		{
			TurboAttachPointPreview apChild = child.GetComponent<TurboAttachPointPreview>();
			if(apChild != null)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Child AP: {apChild.name}");
				if (GUILayout.Button(FlanStyles.GoToEntry, GUILayout.Width(MODELLING_BUTTON_X)))
					Selection.activeObject = apChild;
				GUILayout.EndHorizontal();
			}
		}
	}
	private const float MODELLING_BUTTON_X = 32f;
	private void ModellingNode(MinecraftModelPreview node, string parentPath, bool alwaysExpanded = false)
	{
		string path = $"{parentPath}/{node.name}";

		// ------------------------------------------------------------------------------------
		// Header with foldout and some quick access buttons
		GUILayout.BeginHorizontal();
		bool foldout = alwaysExpanded || SubPieceFoldouts.Contains(path);
		if (alwaysExpanded)
		{
			GUILayout.Label($"{node.name} (Root Object)");
		}
		else
		{ 
			bool updatedFolout = EditorGUILayout.Foldout(foldout, node.Compact_Editor_Header());
			if (updatedFolout && !foldout)
				SubPieceFoldouts.Add(path);
			else if (!updatedFolout && foldout)
				SubPieceFoldouts.Remove(path);
			foldout = updatedFolout;
		}

		
		if (GUILayout.Button(FlanStyles.GoToEntry, GUILayout.Width(MODELLING_BUTTON_X)))
			Selection.activeObject = node;

		if(node.CanDuplicate() && parentPath.Length > 0)
			if(GUILayout.Button(FlanStyles.DuplicateEntry, GUILayout.Width(MODELLING_BUTTON_X)))
				node.Duplicate();
		if (node.CanDelete() && parentPath.Length > 0)
			if (GUILayout.Button(FlanStyles.DeleteEntry, GUILayout.Width(MODELLING_BUTTON_X)))
				node.Delete();

		GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 16));
		GUILayout.EndHorizontal();
		// ------------------------------------------------------------------------------------

		if (foldout)
		{
			//if (!ModelSubEditors.ContainsKey(path))
			//	ModelSubEditors.Add(path, CreateEditor(node));
			//ModelSubEditors[path].OnInspectorGUI();
			string oldName = node.name;
			EditorGUI.indentLevel++;
			node.Compact_Editor_GUI();

			foreach (MinecraftModelPreview child in node.GetChildren())
			{
				ModellingNode(child, path);
			}

			

			EditorGUI.indentLevel--;
			if (node.name != oldName)
			{
				SubPieceFoldouts.Add($"{parentPath}/{node.name}");
			}
		}
		else
		{
			//if (ModelSubEditors.ContainsKey(path))
			//	ModelSubEditors.Remove(path);
		}
	}
	#endregion
	// ------------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------------
	#region Texturing Tab
	// ------------------------------------------------------------------------------------
	private void TexturingTab(MinecraftModel mcModel)
	{
		TexturingTabImpl(mcModel);
	}

	protected virtual void TexturingTabImpl(MinecraftModel mcModel)
	{

	}
	#endregion
	// ------------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------------
	#region Verification Tab
	// ------------------------------------------------------------------------------------
	private void VerificationsTab(MinecraftModel mcModel)
	{
		List<Verification> verifications = new List<Verification>();
		ResourceLocation resLoc = mcModel.GetLocation();
		GUILayout.Label(resLoc.ToString());
		// Potential definition matches
		if (DefinitionImporter != null)
		{
			ContentPack pack = DefinitionImporter.FindContentPack(resLoc.Namespace);
			if (pack != null)
			{
				if (GUILayout.Button($"Go to content pack {resLoc.Namespace}"))
				{
					Selection.SetActiveObjectWithContext(pack, target);
				}

				if (RelatedDefinitions == null)
				{
					RefreshMatches(pack, resLoc, mcModel);
				}

				GUILayout.BeginHorizontal();
				GUILayout.Label("Linked Assets");
				if (GUILayout.Button("Refresh"))
				{
					RefreshMatches(pack, resLoc, mcModel);
				}
				GUILayout.EndHorizontal();

				for (int i = 0; i < RelatedDefinitions.Count; i++)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.ObjectField(RelatedDefinitions[i], typeof(Definition), false);
					GUILayout.EndHorizontal();
				}

				foreach (var kvp in LinkedTextures)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(kvp.Value, GUILayout.Width(48));
					EditorGUILayout.ObjectField(kvp.Key, typeof(Texture2D), false);
					GUILayout.EndHorizontal();
				}

				GUILayout.Label("Unlinked textures with possible match");
				foreach (var kvp in RelatedTextures)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(kvp.Value, GUILayout.Width(48));
					EditorGUILayout.ObjectField(kvp.Key, typeof(Texture2D), false);
					GUILayout.EndHorizontal();
					verifications.Add(
						Verification.Neutral(
							$"{kvp.Key.name} could be added as a skin",
							() =>
							{
								mcModel.Textures.Add(new MinecraftModel.NamedTexture()
								{
									Key = kvp.Key.name,
									Location = kvp.Key.GetLocation(),
									Texture = kvp.Key
								});
							}));
				}

				if (GUIVerify.VerificationsBox(mcModel, verifications))
					RefreshMatches(pack, resLoc, mcModel);
			}
			else GUILayout.Label($"Could not locate {resLoc.Namespace} content pack.");
		}
		else GUILayout.Label("Could not locate DefinitionImporter");
	}
	#endregion
	// ------------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------------
	#region Posing Tab
	// ------------------------------------------------------------------------------------
	private List<int> PoseFoldouts = new List<int>();
	private void PosingTab(MinecraftModel model)
	{
		ModelEditingRig rig = RigSelector(model);

		int indexToDelete = -1, indexToDuplicate = -1;
		for(int i = 0; i < model.Transforms.Count; i++)
		{
			MinecraftModel.ItemTransform itemTransform = model.Transforms[i];
			bool foldout = PoseFoldouts.Contains(i);
			GUILayout.BeginHorizontal();
			bool isAppliedToRig = rig != null && IsCurrentlyApplied(rig, itemTransform.Type);
			bool newFoldout = EditorGUILayout.Foldout(foldout, itemTransform.Type.ToNiceString());
			GUILayout.Label(isAppliedToRig ? $"Previewed on Rig {rig.name}" : "Not Previewed");
			if (newFoldout && !foldout)
				PoseFoldouts.Add(i);
			else if (!newFoldout && foldout)
				PoseFoldouts.Remove(i);

			int numPossiblePoses = GetNumPoses(itemTransform.Type);
			EditorGUI.BeginDisabledGroup(isAppliedToRig);
			if (GUILayout.Button(FlanStyles.ApplyPose, GUILayout.Width(MODELLING_BUTTON_X)))
				ApplyPose(rig, itemTransform, 0);
			if(numPossiblePoses > 1)
				if (GUILayout.Button(FlanStyles.ApplyPose, GUILayout.Width(MODELLING_BUTTON_X)))
					ApplyPose(rig, itemTransform, 1);
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!isAppliedToRig);
			if (GUILayout.Button(FlanStyles.ViewPose, GUILayout.Width(MODELLING_BUTTON_X)))
				MoveSceneCamera(rig, itemTransform);
			EditorGUI.EndDisabledGroup();


			if (GUILayout.Button(FlanStyles.DuplicateEntry, GUILayout.Width(MODELLING_BUTTON_X)))
				indexToDuplicate = i;
			if (GUILayout.Button(FlanStyles.DeleteEntry, GUILayout.Width(MODELLING_BUTTON_X)))
				indexToDelete = i;

			GUILayout.EndHorizontal();
			
			if(newFoldout)
			{
				EditorGUI.indentLevel++;
				itemTransform.Type = (ItemTransformType)EditorGUILayout.EnumPopup("Type", itemTransform.Type);
				
				Vector3 newPos = EditorGUILayout.Vector3Field("Position", itemTransform.Position);
				Vector3 newEuler = EditorGUILayout.Vector3Field("Rotation", itemTransform.Rotation.eulerAngles);
				Vector3 newScale = EditorGUILayout.Vector3Field("Scale", itemTransform.Scale);
				if(!newPos.Approximately(itemTransform.Position)
				|| !newEuler.Approximately(itemTransform.Rotation.eulerAngles)
				|| !newScale.Approximately(itemTransform.Scale))
				{
					Undo.RegisterCompleteObjectUndo(model, $"Adjust {itemTransform.Type.ToNiceString()} pose");
					itemTransform.Position = newPos;
					itemTransform.Rotation = Quaternion.Euler(newEuler);
					itemTransform.Scale = newScale;
					EditorUtility.SetDirty(model);
					UpdatePose(rig, itemTransform); 
				}
				EditorGUI.indentLevel--;
			}
		}
		if(indexToDuplicate != -1)
		{
			Undo.RegisterCompleteObjectUndo(model, $"Duplicated {model.Transforms[indexToDuplicate].Type.ToNiceString()} pose");
			ItemTransform clone = new ItemTransform()
			{
				Type = model.Transforms[indexToDuplicate].Type,
				Position = model.Transforms[indexToDuplicate].Position,
				Rotation = model.Transforms[indexToDuplicate].Rotation,
				Scale = model.Transforms[indexToDuplicate].Scale,
			};
			model.Transforms.Insert(indexToDuplicate + 1, clone);
			EditorUtility.SetDirty(model);
		}
		if (indexToDelete != -1)
		{
			Undo.RegisterCompleteObjectUndo(model, $"Deleted {model.Transforms[indexToDelete].Type.ToNiceString()} pose");
			model.Transforms.RemoveAt(indexToDelete);
			EditorUtility.SetDirty(model);
		}
		if(model.Transforms.Count == 0)
		{
			if (GUILayout.Button("Auto-Add Default Poses"))
			{
				Undo.RegisterCompleteObjectUndo(model, $"Added default poses to model");
				model.AddDefaultTransforms();
				EditorUtility.SetDirty(model);
			}
		}
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("+", GUILayout.Width(32)))
		{
			Undo.RegisterCompleteObjectUndo(model, $"Added new pose to model");
			model.Transforms.Add(new ItemTransform());
			EditorUtility.SetDirty(model);
		}
		GUILayout.EndHorizontal();
	}
	private static string[] GetAttachTransformNames(ItemTransformType transformType)
	{
		switch(transformType)
		{
			case ItemTransformType.THIRD_PERSON_LEFT_HAND: return new string[] { "Steve_LeftHandPose" };
			case ItemTransformType.THIRD_PERSON_RIGHT_HAND: return new string[] { "Steve_RightHandPose" };
			case ItemTransformType.FIRST_PERSON_LEFT_HAND: return new string[] { "FirstPerson_LeftHandPose" };
			case ItemTransformType.FIRST_PERSON_RIGHT_HAND: return new string[] { "FirstPerson_RightHandPose" };
			case ItemTransformType.GUI: return new string[] { "GUIPose" };
			case ItemTransformType.GROUND: return new string[] { "GroundItemPose" };
			case ItemTransformType.NONE:
			case ItemTransformType.FIXED:
			default: 
				return new string[] { "DefaultPose" };
		}
	}
	private int GetNumPoses(ItemTransformType transformType)
	{
		return GetAttachTransformNames(transformType).Length;
	}
	private bool IsCurrentlyApplied(ModelEditingRig rig, ItemTransformType type)
	{
		if (rig.transform.parent == null)
			return false;

		// We only need to adjust the Unity transform if this is the transform we are currently previewing
		string[] transformNames = GetAttachTransformNames(type);
		for (int i = 0; i < transformNames.Length; i++)
			if (transformNames[i] == rig.transform.parent.name)
				return true;

		return false;
	}
	private void UpdatePose(ModelEditingRig rig, ItemTransform itemTransform)
	{
		if(rig != null)
		{
			if (!IsCurrentlyApplied(rig, itemTransform.Type))
				return;

			// So we are attached to this pose, update it
			rig.transform.localPosition = itemTransform.Position;
			rig.transform.localRotation = itemTransform.Rotation;
			rig.transform.localScale = itemTransform.Scale;
		}
	}
	private void ApplyPose(ModelEditingRig rig, ItemTransform itemTransform, int index)
	{
		if(rig != null)
		{
			string[] transformNames = GetAttachTransformNames(itemTransform.Type);
			string targetName = transformNames[index];

			Transform attachTo = GameObject.Find(targetName)?.transform;
			if (attachTo != null)
			{
				rig.transform.SetParent(attachTo);
				rig.transform.localPosition = itemTransform.Position;
				rig.transform.localRotation = itemTransform.Rotation;
				rig.transform.localScale = itemTransform.Scale;
			}
			else Debug.LogError($"Could not attach rig to {targetName} as it could not be found");
		}
	}
	private void MoveSceneCamera(ModelEditingRig rig, ItemTransform itemTransform)
	{
		switch(itemTransform.Type)
		{
			case ItemTransformType.FIRST_PERSON_LEFT_HAND:
			case ItemTransformType.FIRST_PERSON_RIGHT_HAND:
			{
				GameObject firstPersonOutput = GameObject.Find("FirstPersonOutput");
				if (firstPersonOutput != null)
				{
					SceneView.lastActiveSceneView.LookAt(
						firstPersonOutput.transform.position,
						Quaternion.LookRotation(-firstPersonOutput.transform.up, -firstPersonOutput.transform.forward),
						100f);
					return;
				}
				break;
			}
			case ItemTransformType.GUI:
			{
					GameObject guiOutput = GameObject.Find("GUIOutput");
					if (guiOutput != null)
					{
						SceneView.lastActiveSceneView.LookAt(
							guiOutput.transform.position,
							Quaternion.LookRotation(-guiOutput.transform.up, -guiOutput.transform.forward),
							32f);
						return;
					}
					break;
			}
		}

		SceneView.lastActiveSceneView.LookAt(rig.transform.position);
	}
	#endregion
	// ------------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------------
	#region Debug Tab
	// ------------------------------------------------------------------------------------
	private void DebugTab(MinecraftModel mcModel)
	{
		ResourceLocation resLoc = mcModel.GetLocation();
		GUILayout.Label(resLoc.ToString());
		// Potential definition matches
		if (DefinitionImporter != null)
		{
			ContentPack pack = DefinitionImporter.FindContentPack(resLoc.Namespace);
			if (pack != null)
			{
				EditorGUI.BeginChangeCheck();
				FlanStyles.HorizontalLine();
				GUILayout.Label("Default Inspector", FlanStyles.BoldLabel);
				base.OnInspectorGUI();
				if (EditorGUI.EndChangeCheck())
					RefreshMatches(pack, resLoc, mcModel);
			}
		}
	}
	#endregion
	// ------------------------------------------------------------------------------------

	protected static ModelEditingRig GetCurrentRig()
	{
		ModelEditingRig[] rigs = Object.FindObjectsOfType<ModelEditingRig>();
		return rigs.Length > 0 ? rigs[0] : null;
	}
}







