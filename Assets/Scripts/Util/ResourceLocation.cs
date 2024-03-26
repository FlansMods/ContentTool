using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[System.Serializable]
public struct ResourceLocation
{
    public static readonly ResourceLocation InvalidLocation = new ResourceLocation() { Namespace = "minecraft", ID = "null" };

	public string Namespace;
    public string ID;

    public string ResolveWithSubdir(string subdir)
    {
        if (ID.Contains($"{subdir}/"))
            return $"{Namespace}:{ID}";
        else
            return $"{Namespace}:{subdir}/{ID}";
    }
	public ResourceLocation(string input)
    {
        if(input == null)
        {
            Namespace = InvalidLocation.Namespace;
            ID = InvalidLocation.ID;
        }
        else if (input.Contains(":"))
        {
            Namespace = input.Split(":")[0];
			ID = input.Split(":")[1];
		}
        else
        {
            Namespace = "";
            ID = input;
        }
    }
    public ResourceLocation(string ns, string id)
    {
        Namespace = ns;
        ID = id;
	}

	// --------------------------------------------------------------------------------------------
    #if UNITY_EDITOR
    // Editor tools for creating locations from paths
	// --------------------------------------------------------------------------------------------
	private static Regex ResourceLocator = new Regex(".*Assets\\/Content Packs\\/([a-zA-Z0-9]*)\\/([a-zA-Z0-9_\\/]*)\\.([a-z]*)");
	public static bool TryGetFromAssetPath(string assetPath, out ResourceLocation location)
    {
		Match match = ResourceLocator.Match(assetPath);
		if (match.Success)
		{
			string modName = match.Groups[1].Value;
			string assetName = match.Groups[2].Value;
			location = new ResourceLocation(modName, assetName);
			return true;
		}
        location = InvalidLocation;
        return false;
	}
    #endif

    public string UnityFolderPath()
    {
        return $"Assets/Content Packs/{Namespace}/{ID.Substring(0, ID.LastIndexOfAny(Utils.SLASHES))}";
    }
	// --------------------------------------------------------------------------------------------

	public ResourceLocation Clone()
    {
        return new ResourceLocation(Namespace, ID);
    }

	public override string ToString() { return $"{Namespace}:{ID}";  }
	public override int GetHashCode() { return Namespace.GetHashCode() ^ ID.GetHashCode(); }
	public bool IsValid() { return Namespace.Length > 0 && ID.Length > 0; }
	public bool IsContentPack() { return IsValid() && Namespace != "minecraft" && Namespace != "flansmod"; }
	public string ExportAsModelPath()
    {
        return $"{Namespace}:{IDWithSpecificPrefixStripped("models")}";
    }
	public string ExportAsTexturePath()
	{
		return $"{Namespace}:{IDWithSpecificPrefixStripped("textures")}";
	}
	public string ExportAsSoundPath()
	{
		return $"{Namespace}:{IDWithSpecificPrefixStripped("sounds")}";
	}
	public string IDWithSpecificPrefixStripped(string prefix)
    {
        if (ID.StartsWith(prefix))
            return ID.Substring(prefix.Length + 1);
        return ID;
    }
	public string IDWithSpecificPrefixesStripped(params string[] prefixes)
	{
        foreach(string prefix in prefixes)
		    if (ID.StartsWith(prefix))
			    return ID.Substring(prefix.Length + 1);
		return ID;
	}
	public string IDWithoutPrefixes()
    {
        int lastSlash = ID.LastIndexOf('/');
        if (lastSlash == -1)
            return ID;
        return ID.Substring(lastSlash+1);
    }
    public string GetPrefixes()
    {
		int lastSlash = ID.LastIndexOf('/');
		if (lastSlash == -1)
			return "";
		return ID.Substring(0, lastSlash);
	}

	public override bool Equals(object obj)
	{
		if(obj is ResourceLocation resLoc)
        {
            return resLoc.Namespace == Namespace && resLoc.ID == ID;
        }
        return false;
	}

    public static bool operator==(ResourceLocation a, ResourceLocation b)
    {
        return a.Namespace == b.Namespace && a.ID == b.ID;
    }
	public static bool operator !=(ResourceLocation a, ResourceLocation b)
	{
		return a.Namespace != b.Namespace || a.ID != b.ID;
	}

    public static bool IsSameObjectGroup(ResourceLocation a, ResourceLocation b)
    {
        return a.Namespace == b.Namespace && a.IDWithoutPrefixes() == b.IDWithoutPrefixes();
	}

#if UNITY_EDITOR
    public bool TryLoad<T>(out T result, string subfolder = "") where T : Object
    {
		if (Namespace == NEW_NAMESPACE || Namespace == "minecraft")
		{
            result = null;
            return false;
		}

		string extension = "asset";
		if (typeof(T) == typeof(Texture2D))
			extension = "png";
        if (typeof(T) == typeof(AudioClip))
            extension = "ogg";
           
		string path = $"Assets/Content Packs/{Namespace}/{subfolder}/{IDWithoutPrefixes()}.{extension}";
		//if (ID.Contains(subfolder))
		//	path = $"Assets/Content Packs/{Namespace}/{ID}.{extension}";

		result = AssetDatabase.LoadAssetAtPath<T>(path);
        return result != null;
	}

	public T Load<T>(string subfolder = "") where T : Object
    {
        if (ID == "null" || Namespace == "minecraft")
            return null;

        if (TryLoad(out T result, subfolder))
            return result;

        Debug.LogError($"Failed to load {this} as {typeof(T)}");
        return null;
    }

    public const string NEW_NAMESPACE = ContentManager.NEW_NAMESPACE;
	public static List<string> Namespaces { get { return ContentManager.inst.Namespaces; } }

    public void MatchToAsset(Object asset)
    {
        if (asset != null)
        {
            ResourceLocation resLoc = asset.GetLocation();
            if (resLoc.Namespace != Namespace)
                Debug.LogError($"ResourceLocation {this} does not match asset {asset} at location {resLoc}");
            else if(resLoc.ID != ID)
            {
                if(resLoc.ID.Contains(ID))
                {
                    // We weren't fully specific, that's okay
                    ID = resLoc.ID;
                }
                else
                {
                    // Something different came up, that's odd
                    Debug.LogWarning($"Partial ResourceLocation mismatch, {this} does not match asset {asset} at location {resLoc}");
                    ID = resLoc.ID;
                }
            }
        }
    }

    public static ResourceLocation EditorObjectField<T>(ResourceLocation src, string subfolder = "") where T : Object
    {
        src.TryLoad(out T result, subfolder);
        return EditorObjectField<T>(src, result, subfolder);
    }

    public static string NamespaceField(string ns)
    {
		if (ns == NEW_NAMESPACE)
		{
			string editedNamespace = EditorGUILayout.DelayedTextField("");
			if (editedNamespace.Length > 0)
			{
				//bool register = EditorUtility.DisplayDialog(
				//	"New namespace",
				//	$"Do you want to register {editedNamespace} as a new namespace?",
				//	"Yes", "No");

				if (!Namespaces.Contains(editedNamespace))
					Namespaces.Insert(Namespaces.Count - 1, editedNamespace);
				ns = editedNamespace;
			}
		}
		else
		{
			int index = Namespaces.IndexOf(ns);
			int editedSelection = EditorGUILayout.Popup(index, Namespaces.ToArray());
			if (editedSelection != -1 && editedSelection != index)
			{
				ns = Namespaces[editedSelection];
			}
		}
        return ns;
	}

    public static string IDField(string ns, string id, string subfolder = "")
    {
        if (Namespaces.Contains(ns) && ns != "minecraft" && ns != NEW_NAMESPACE)
        {
            List<string> possibleIDs = new List<string>();
            ContentPack pack = ContentManager.inst.FindContentPack(ns);
            int selectedIndex = -1;
			if (pack != null)
            {
                if (subfolder.Length == 0)
                {
                    possibleIDs.AddRange(pack.AllIDs);
                    selectedIndex = possibleIDs.IndexOf(id);

				}
                else
                {
                    GUILayout.Label($"{subfolder}/");
                    foreach (string option in pack.AllIDs)
                        if (option.Contains(subfolder))
                        {
                            string trunc = option.Substring(option.IndexOf(subfolder) + subfolder.Length + 1);
                            possibleIDs.Add(trunc);
                            if (option == id)
                                selectedIndex = possibleIDs.Count - 1;
						}
                }
            }

			selectedIndex = EditorGUILayout.Popup(selectedIndex, possibleIDs.ToArray());
            if (selectedIndex < 0)
                return id;
            if (subfolder.Length > 0)
                return $"{subfolder}/{possibleIDs[selectedIndex]}";
            return possibleIDs[selectedIndex];
        }
		return GUILayout.TextField(id).ToLower().Replace(" ", "_");
	}

	public static ResourceLocation EditorObjectField<T>(ResourceLocation src, T currentObject, string subfolder = "") where T : Object
    {
        ResourceLocation result = src.Clone();

        result.MatchToAsset(currentObject);
		
		GUILayout.BeginHorizontal();

        // Namespace
        result.Namespace = NamespaceField(result.Namespace);
        GUILayout.Label(":", GUILayout.Width(8));
        // ID
        result.ID = IDField(result.Namespace, result.ID, subfolder);

		// Interactive field
		if (result.Namespace != "minecraft" && result.Namespace != NEW_NAMESPACE)
        {
            Object changedObject = EditorGUILayout.ObjectField(currentObject, typeof(T), false);
            if(changedObject != currentObject)
            {
                result = changedObject.GetLocation();
            }
        }
		GUILayout.EndHorizontal();
        return result;
	}
	public static ResourceLocation EditorField(ResourceLocation src, string subfolder = "")
    {
		ResourceLocation result = src.Clone();
        GUILayout.BeginHorizontal();
        result.Namespace = NamespaceField(result.Namespace); 
        result.ID = IDField(result.Namespace, result.ID, subfolder);
		GUILayout.EndHorizontal();

        return result;
	}
#endif
}

public static class ResourceLocationUtils
{
    public static ResourceLocation GetLocation(this Object asset)
    {
        if (TryGetLocation(asset, out ResourceLocation result))
            return result;

		Debug.LogWarning($"Could not resolve the path to {asset} as a resource location. Is it in a Content Pack?");
		return new ResourceLocation();
    }
    public static bool TryGetLocation(this Object asset, out ResourceLocation location)
    {
        if (asset is Component component)
            return TryGetLocation(component, out location);
#if UNITY_EDITOR
		string path = AssetDatabase.GetAssetPath(asset);
		if (path.Length > 0)
			return ResourceLocation.TryGetFromAssetPath(path, out location);
#endif
		location = new ResourceLocation();
		return false;
	}
    public static bool TryGetLocation(this Component component, out ResourceLocation location)
    {
#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfAnyPrefab(component))
        {
			return ResourceLocation.TryGetFromAssetPath(
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(component), 
                out location);
        }

        PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(component.gameObject);
        if(prefabStage != null)
        {
            return ResourceLocation.TryGetFromAssetPath(
                prefabStage.assetPath, out location);
        }
#endif
        location = new ResourceLocation();
        return false;
    }
}