using System.Collections;
using System.Collections.Generic;
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

	public override string ToString()
	{
        return $"{Namespace}:{ID}";
    }

    public bool IsValid()
    {
        return Namespace.Length > 0 && ID.Length > 0;
    }
}
