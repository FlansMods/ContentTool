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
    public T Load<T>(string subfolder = "") where T : Object
    {
        if (Namespace == "minecraft")
        {
            Debug.Log($"Did not load {this} asset from minecraft namespace.");
            return null;
        }

        string extension = "asset";
        if (typeof(T) == typeof(Texture2D))
            extension = "png";
        string path = $"Assets/Content Packs/{Namespace}/{subfolder}/{ID}.{extension}";

        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        Debug.LogError($"Failed to load {path} as {typeof(T)}");
        return null;
    }

    // Namespace caching
	private static List<string> _Namespaces = null;
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
			}
            return _Namespaces;
		} 
    }


    public static ResourceLocation EditorObjectField<T>(ResourceLocation src) where T : Object
    {
        return EditorObjectField<T>(src, src.Load<T>());
    }

	public static ResourceLocation EditorObjectField<T>(ResourceLocation src, T currentObject) where T : Object
    {
        ResourceLocation result = src.Clone();
		

		GUILayout.BeginHorizontal();
		string editedNamespace = EditorGUILayout.DelayedTextField(result.Namespace, GUILayout.MinWidth(64));
		if (editedNamespace != result.Namespace && !Namespaces.Contains(editedNamespace))
		{
			bool register = EditorUtility.DisplayDialog(
				"New namespace",
				$"Do you want to register {editedNamespace} as a new namespace?",
				"Yes", "No");

			if (register)
			{
				Namespaces.Add(editedNamespace);
				result.Namespace = editedNamespace;
			}
			else
			{
				// Do anything?
			}

		}

		int index = Namespaces.IndexOf(editedNamespace);
		int editedSelection = EditorGUILayout.Popup(index, Namespaces.ToArray(), GUILayout.MinWidth(16));
		if (editedSelection != -1 && editedSelection != index)
		{
			result.Namespace = Namespaces[editedSelection];
		}

        if(result.Namespace == "minecraft")
        {
			result.ID = GUILayout.TextField(result.ID, GUILayout.MinWidth(64)).ToLower().Replace(" ", "_");
		}
        else 
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
	public static ResourceLocation EditorField(ResourceLocation src)
    {
		ResourceLocation result = src.Clone();
        GUILayout.BeginHorizontal();
        string editedNamespace = EditorGUILayout.DelayedTextField(result.Namespace, GUILayout.MinWidth(64));
        if(editedNamespace != result.Namespace && !Namespaces.Contains(editedNamespace))
        {
            bool register = EditorUtility.DisplayDialog(
                "New namespace",
                $"Do you want to register {editedNamespace} as a new namespace?", 
                "Yes", "No");

            if(register)
            {
				Namespaces.Add(editedNamespace);
				result.Namespace = editedNamespace;
            }
            else
            {
                // Do anything?
            }

		}

        int index = Namespaces.IndexOf(editedNamespace);
        int editedSelection = EditorGUILayout.Popup(index, Namespaces.ToArray(), GUILayout.MinWidth(16));
        if(editedSelection != -1 && editedSelection != index)
        {
			result.Namespace = Namespaces[editedSelection];
        }

		result.ID = GUILayout.TextField(result.ID, GUILayout.MinWidth(64)).ToLower().Replace(" ", "_");
        GUILayout.EndHorizontal();

        return result;
	}
#endif
}
