using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
		foreach (Definition def in pack.Content)
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

	public override void OnInspectorGUI()
	{
		if (target is MinecraftModel mcModel)
		{
			GUILayout.Label("Minecraft Model Settings", FlanStyles.BoldLabel);
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

					if (GUILayout.Button("Edit Model (Opens in Scene View)"))
					{
						ModelEditingRig modelEditingRig = GetCurrentRig();
						if (modelEditingRig != null)
						{
							modelEditingRig.OpenModel(mcModel);
						}
					}

					EditorGUI.BeginChangeCheck();
					FlanStyles.HorizontalLine();
					GUILayout.Label("Default Inspector", FlanStyles.BoldLabel);
					base.OnInspectorGUI();
					if (EditorGUI.EndChangeCheck())
						RefreshMatches(pack, resLoc, mcModel);
				}
				else GUILayout.Label($"Could not locate {resLoc.Namespace} content pack.");
			}
			else GUILayout.Label("Could not locate DefinitionImporter");
		}
		else GUILayout.Label($"Invalid target {target} for this editor");
	}

	protected static ModelEditingRig GetCurrentRig()
	{
		ModelEditingRig[] rigs = Object.FindObjectsOfType<ModelEditingRig>();
		return rigs.Length > 0 ? rigs[0] : null;
	}
}

[CustomEditor(typeof(TurboRig))]
public class TurboRigEditor : MinecraftModelEditor
{	
}

[CustomEditor(typeof(CubeModel))]
public class CubeModelEditor : MinecraftModelEditor
{
}

[CustomEditor(typeof(BlockbenchModel))]
public class BlockbenchModelEditor : MinecraftModelEditor
{
}

[CustomEditor(typeof(ItemModel))]
public class ItemModelEditor : MinecraftModelEditor
{
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
