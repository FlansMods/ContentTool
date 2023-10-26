using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class ResourceLocation
{
    public string Namespace = "";
    public string ID = "";

    public string ResolveWithSubdir(string subdir)
    {
        if (ID.Contains($"{subdir}/"))
            return $"{Namespace}:{ID}";
        else
            return $"{Namespace}:{subdir}/{ID}";
    }
    public ResourceLocation()
    {
        Namespace = "minecraft";
        ID = "null";
    }
	public ResourceLocation(string input)
    {
        if (input.Contains(":"))
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

    public ResourceLocation Clone()
    {
        return new ResourceLocation(Namespace, ID);
    }

	public override string ToString()
	{
        return $"{Namespace}:{ID}";
    }
	public override int GetHashCode()
	{
        return Namespace.GetHashCode() ^ ID.GetHashCode();
	}

	public bool IsValid()
    {
        return Namespace.Length > 0 && ID.Length > 0;
    }

    public string IDWithoutPrefixes()
    {
        int lastSlash = ID.LastIndexOf('/');
        if (lastSlash == -1)
            return ID;
        return ID.Substring(lastSlash+1);
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
           
		string path = $"Assets/Content Packs/{Namespace}/{subfolder}/{ID}.{extension}";
		if (ID.Contains(subfolder))
			path = $"Assets/Content Packs/{Namespace}/{ID}.{extension}";

		result = AssetDatabase.LoadAssetAtPath<T>(path);
        return result != null;
	}

	public T Load<T>(string subfolder = "") where T : Object
    {
        if (TryLoad(out T result, subfolder))
            return result;

        Debug.LogError($"Failed to load {this} as {typeof(T)}");
        return null;
    }

    // Namespace caching
	private static List<string> _Namespaces = null;
    private const string NEW_NAMESPACE = "new ...";
    public static List<string> Namespaces 
    { 
        get 
        {
			if (_Namespaces == null)
			{
				_Namespaces = new List<string>
			    {
				    "minecraft",
				    "flansmod"
			    };
				DefinitionImporter importer = Object.FindObjectOfType<DefinitionImporter>();
				if (importer != null)
				{
					foreach (ContentPack pack in importer.Packs)
						_Namespaces.Add(pack.ModName);
				}
                _Namespaces.Add(NEW_NAMESPACE);
			}
            return _Namespaces;
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
            ContentPack pack = DefinitionImporter.inst.FindContentPack(ns);
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
