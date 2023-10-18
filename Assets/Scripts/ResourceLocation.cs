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
	private static List<string> namespaces = null;
	public static ResourceLocation EditorField(ResourceLocation src)
    {
		if (namespaces == null)
        {
			namespaces = new List<string>
			{
				"minecraft",
				"flansmod"
			};
			DefinitionImporter importer = Object.FindObjectOfType<DefinitionImporter>();
            if(importer != null)
            {
                foreach (ContentPack pack in importer.Packs)
                    namespaces.Add(pack.ModName);
            }
        }

        GUILayout.BeginHorizontal();
        string editedNamespace = EditorGUILayout.DelayedTextField(src.Namespace, GUILayout.MinWidth(64));
        if(editedNamespace != src.Namespace && !namespaces.Contains(editedNamespace))
        {
            bool register = EditorUtility.DisplayDialog(
                "New namespace",
                $"Do you want to register {editedNamespace} as a new namespace?", 
                "Yes", "No");

            if(register)
            {
                namespaces.Add(editedNamespace);
                src.Namespace = editedNamespace;
            }
            else
            {
                // Do anything?
            }

		}

        int index = namespaces.IndexOf(editedNamespace);
        int editedSelection = EditorGUILayout.Popup(index, namespaces.ToArray(), GUILayout.MinWidth(16));
        if(editedSelection != -1 && editedSelection != index)
        {
            src.Namespace = namespaces[editedSelection];
        }

        src.ID = GUILayout.TextField(src.ID, GUILayout.MinWidth(64)).ToLower().Replace(" ", "_");
        GUILayout.EndHorizontal();

        return src;
	}
#endif
}
