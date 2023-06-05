using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentPack : ScriptableObject
{
	public List<Definition> Content = new List<Definition>();
	public string ModName = "";

	public bool HasContent(string shortName)
	{
		foreach(Definition def in Content)
			if(def.name == shortName)
				return true;
		return false;
	}

}
