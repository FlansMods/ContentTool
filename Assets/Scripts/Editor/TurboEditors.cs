using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static MinecraftModel;
using static PlasticGui.LaunchDiffParameters;

public abstract class MinecraftModelEditor : Editor
{
	private static DefinitionImporter DefImporter = null;
	protected static DefinitionImporter DefinitionImporter
	{ get 
		{
			if (DefImporter == null)
				DefImporter = FindObjectOfType<DefinitionImporter>();
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
		foreach (MinecraftModel.NamedTexture namedTexture in mcModel.Textures)
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
		ModelEditingRig[] rigs = Object.FindObjectsOfType<ModelEditingRig>();
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
			if(GUILayout.Button(EditorGUIUtility.IconContent("back"), GUILayout.Width(MODELLING_BUTTON_X)))
				Selection.activeObject = parent;
			GUILayout.Label($"Parent: {parent}");
			GUILayout.EndHorizontal();
		}

		ModellingNode(root, "", true);
	}
	public void AttachPointNode(TurboAttachPointPreview ap)
	{
		MinecraftModelPreview parent = ap.Parent;
		if (parent != null)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(EditorGUIUtility.IconContent("back"), GUILayout.Width(MODELLING_BUTTON_X)))
				Selection.activeObject = parent;
			GUILayout.Label($"Parent: {parent}");
			GUILayout.EndHorizontal();
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

		
		if (GUILayout.Button(EditorGUIUtility.IconContent("AvatarPivot"), GUILayout.Width(MODELLING_BUTTON_X)))
			Selection.activeObject = node;

		if(node.CanDuplicate() && parentPath.Length > 0)
			if(GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"), GUILayout.Width(MODELLING_BUTTON_X)))
				node.Duplicate();
		if (node.CanDelete() && parentPath.Length > 0)
			if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.Width(MODELLING_BUTTON_X)))
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
		// TODO
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
			bool newFoldout = EditorGUILayout.Foldout(foldout, itemTransform.Type.ToString());
			GUILayout.Label(isAppliedToRig ? $"Previewed on Rig {rig.name}" : "Not Previewed");
			if (newFoldout && !foldout)
				PoseFoldouts.Add(i);
			else if (!newFoldout && foldout)
				PoseFoldouts.Remove(i);

			int numPossiblePoses = GetNumPoses(itemTransform.Type);
			if (GUILayout.Button(EditorGUIUtility.IconContent("animationvisibilitytoggleon"), GUILayout.Width(MODELLING_BUTTON_X)))
				ApplyPose(rig, itemTransform, 0);
			if(numPossiblePoses > 1)
				if (GUILayout.Button(EditorGUIUtility.IconContent("animationvisibilitytoggleon"), GUILayout.Width(MODELLING_BUTTON_X)))
					ApplyPose(rig, itemTransform, 1);

			if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"), GUILayout.Width(MODELLING_BUTTON_X)))
				indexToDuplicate = i;
			if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.Width(MODELLING_BUTTON_X)))
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
					Undo.RegisterCompleteObjectUndo(model, $"Adjust {itemTransform.Type} pose");
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
			Undo.RegisterCompleteObjectUndo(model, $"Duplicated {model.Transforms[indexToDuplicate].Type} pose");
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
			Undo.RegisterCompleteObjectUndo(model, $"Deleted {model.Transforms[indexToDelete].Type} pose");
			model.Transforms.RemoveAt(indexToDelete);
			EditorUtility.SetDirty(model);
		}
		if(GUILayout.Button("+", GUILayout.Width(32)))
		{
			Undo.RegisterCompleteObjectUndo(model, $"Added new pose to model");
			model.Transforms.Add(new ItemTransform());
			EditorUtility.SetDirty(model);
		}			
	}
	private static string[] GetAttachTransformNames(ItemTransformType transformType)
	{
		switch(transformType)
		{
			case ItemTransformType.THIRD_PERSON_LEFT_HAND: return new string[] { "Alex_LeftHandPose", "Steve_LeftHandPose" };
			case ItemTransformType.THIRD_PERSON_RIGHT_HAND: return new string[] { "Alex_RightHandPose", "Steve_RightHandPose" };
			case ItemTransformType.FIRST_PERSON_LEFT_HAND: return new string[] { "Alex_LeftHandPose", "Steve_LeftHandPose" };
			case ItemTransformType.FIRST_PERSON_RIGHT_HAND: return new string[] { "Alex_RightHandPose", "Steve_RightHandPose" };
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

[CustomEditor(typeof(TurboAttachPointPreview))]
public class TurboAttachPointPreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboAttachPointPreview preview = (TurboAttachPointPreview)target;
		if (preview == null || preview.Parent == null || preview.Parent.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Parent.Rig)
			RigEditor = CreateEditor(preview.Parent.Rig);
		if (RigEditor is TurboRigEditor rigEditor)
		{
			rigEditor.AttachPointNode(preview);
		}

		preview.LockPartPositions = GUILayout.Toggle(preview.LockPartPositions, "Lock Positions (pieces will stay still by altering their origins)");
		preview.LockAttachPoints = GUILayout.Toggle(preview.LockAttachPoints, "Lock Attach Points (if other APs are attached to this one, they are fixed)");
	}
}

[CustomEditor(typeof(TurboRigPreview))]
public class TurboRigPreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboRigPreview preview = (TurboRigPreview)target;
		if (preview == null || preview.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Rig)
			RigEditor = CreateEditor(preview.Rig);
		if (RigEditor != null)
			RigEditor.OnInspectorGUI();
	}
}

[CustomEditor(typeof(TurboModelPreview))]
public class TurboModelPreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboModelPreview preview = (TurboModelPreview)target;
		if (preview == null || preview.Parent == null || preview.Parent.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Parent.Rig)
			RigEditor = CreateEditor(preview.Parent.Rig);
		if (RigEditor is TurboRigEditor rigEditor)
		{
			rigEditor.InitialModellingNode(preview);
		}
	}
}


[CustomEditor(typeof(TurboPiecePreview))]
public class TurboPiecePreviewEditor : Editor
{
	private Editor RigEditor = null;

	public override void OnInspectorGUI()
	{
		TurboPiecePreview preview = (TurboPiecePreview)target;
		if (preview == null || preview.Parent == null || preview.Parent.Parent == null || preview.Parent.Parent.Rig == null)
		{
			GUILayout.Label("No Model Selected");
			return;
		}

		if (RigEditor == null || RigEditor.target != preview.Parent.Parent.Rig)
			RigEditor = CreateEditor(preview.Parent.Parent.Rig);
		if (RigEditor is TurboRigEditor rigEditor)
		{
			rigEditor.InitialModellingNode(preview);
		}
	}

	/*
	public override void OnInspectorGUI()
	{
		TurboPiecePreview preview = (TurboPiecePreview)target;
		if (preview == null)
			return;

		if (preview.Piece == null)
		{
			GUILayout.Label("Invalid piece!");
			return;
		}

		preview.transform.localPosition = preview.Piece.Origin;
		preview.SetPos(EditorGUILayout.Vector3Field("Rotation Origin", preview.transform.localPosition));
		preview.SetEuler(EditorGUILayout.Vector3Field("Euler Angles", preview.transform.localEulerAngles));

		preview.Piece.Pos = EditorGUILayout.Vector3Field("Cube Offset", preview.Piece.Pos);
		preview.Piece.Dim = EditorGUILayout.Vector3Field("Dimensions", preview.Piece.Dim);


		if (preview.Piece.Offsets.Length != 8)
			preview.Piece.Offsets = new Vector3[8];

		for (int i = 0; i < 8; i++)
		{
			preview.Piece.Offsets[i] = EditorGUILayout.Vector3Field($"Offset {i}", preview.Piece.Offsets[i]);
		}

		if (GUILayout.Button("Duplicate"))
			preview.Duplicate();

		Texture2D tex = preview.GetTemporaryTexture();
		GUILayout.Label("", GUILayout.Width(tex.width * 16), GUILayout.Height(tex.height * 16));
		GUI.DrawTexture(GUILayoutUtility.GetLastRect(), tex);

		if (GUILayout.Button("Clear Texture"))
		{
			preview.ResetTexture();
		}
	}
	*/
}

[CustomEditor(typeof(TurboRig))]
public class TurboRigEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Turbo Rig Editor"); }
}

[CustomEditor(typeof(CubeModel))]
public class CubeModelEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Cube Model Editor"); }
}

[CustomEditor(typeof(BlockbenchModel))]
public class BlockbenchModelEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Imported Model Editor"); }
}

[CustomEditor(typeof(ItemModel))]
public class ItemModelEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Vanilla Item Icon Editor"); }

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		FlanStyles.HorizontalLine();
		GUILayout.Label("Item Settings", FlanStyles.BoldLabel);

		if(target is ItemModel itemModel)
		{
			ResourceLocation changedLocation = ResourceLocation.EditorObjectField<Texture2D>(itemModel.IconLocation);
			if(changedLocation != itemModel.IconLocation)
			{
				itemModel.IconLocation = changedLocation;
				itemModel.Icon = changedLocation.Load<Texture2D>();
			}
		}
	}
}
