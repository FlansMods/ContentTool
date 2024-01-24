using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JavaToCSImport : MonoBehaviour
{
	[System.Serializable]
	public class JavaCSClassMapping 
	{
		public string JavaPackage = "com.flansmod.common.types";
		public string JavaClassName;
	}

	public string RootJavaDir;
	public const string RootCSDir = "Assets/Scripts/Generated";

	public string ScanDir;

	public List<string> AutoMappings;

	public List<JavaCSClassMapping> Mappings;

	public void Process()
	{
		if(!Directory.Exists(RootCSDir))
			Directory.CreateDirectory(RootCSDir);
		foreach(JavaCSClassMapping mapping in Mappings)
		{
			Import($"{RootJavaDir}/{mapping.JavaPackage.Replace('.', '/')}/{mapping.JavaClassName}.java", 
				$"{RootCSDir}/Definitions/{mapping.JavaClassName}.cs",
				mapping.JavaClassName,
				false);
		}
		foreach(string autoMapping in AutoMappings)
		{
			string className = new FileInfo(autoMapping).Name.Split(".")[0];
			Import(autoMapping,
				$"{RootCSDir}/Definitions/{className}.cs",
				className,
				autoMapping.Contains("elements"));
		}
	}
	
	private void Import(string javaPath, string csPath, string className, bool isElement)
	{
		string[] input = File.ReadAllLines(javaPath);
		List<string> output = new List<string>
		{
			"using UnityEngine;",
			"using static ResourceLocation;",
			"",
			"[System.Serializable]"
		};
		if(!isElement)
			output.Add($"[CreateAssetMenu(menuName = \"Flans Mod/{className}\")]");
		if(isElement)
			output.Add($"public class {className}");
		else
			output.Add($"public class {className} : Definition");
		output.Add("{");

		for(int i = 0; i < input.Length; i++)
		{
			if(input[i].Contains("@JsonField"))
			{
				output.Add("	[JsonField]");
				if(input[i].Contains("Docs = \""))
				{
					string docString = input[i].Substring(input[i].IndexOf("Docs = \"") + 8);
					docString = docString.Substring(0, docString.IndexOf("\""));
					output.Add($"[Tooltip(\"{docString}\")]");
				}

				int lineToInclude = i+1;
				while(input[lineToInclude].Contains("@"))
					lineToInclude++;
				output.Add(ConvertToCS(input[lineToInclude]));
			}
		}
		output.Add("}");
		
		File.WriteAllLines(csPath, output);
	}

	private string ConvertToCS(string java)
	{
		return java
		.Replace("Vec3.ZERO", "Vector3.zero")
		.Replace("Vec3", "Vector3")
		.Replace("Vector3f", "Vector3")
		.Replace("boolean", "bool")
		.Replace(" String", " string");
	}
}
