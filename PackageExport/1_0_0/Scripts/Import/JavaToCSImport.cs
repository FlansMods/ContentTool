using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
	public const string ManualOverridesDir = "Assets/Scripts/Generated/ManualOverrides";

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

			if (File.Exists($"{ManualOverridesDir}/{className}.cs"))
			{
				Debug.Log($"Skipping cs import of {className} due to manual override");
				continue;
			}

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
			"using UnityEngine.Serialization;",
			"",
			"[System.Serializable]"
		};
		if(!isElement)
			output.Add($"[CreateAssetMenu(menuName = \"Flans Mod/{className}\")]");
		int classLine = output.Count;
		if(isElement)
			output.Add($"public class {className} : Element");
		else
			output.Add($"public class {className} : Definition");
		output.Add("{");


		List<string> serializeFixers = new List<string>();
		List<string> serializeArrayFixers = new List<string>();

		for(int i = 0; i < input.Length; i++)
		{
			if(input[i].Contains("@JsonField"))
			{
				if (input[i].Contains("AssetPathHint = \""))
				{
					string assetPathString = input[i].Substring(input[i].IndexOf("AssetPathHint = \"") + "AssetPathHint = \"".Length);
					assetPathString = assetPathString.Substring(0, assetPathString.IndexOf("\""));
					output.Add($"	[JsonField(AssetPathHint = \"{assetPathString}\")]");
				}
				else 
					output.Add("	[JsonField]");

				if(input[i].Contains("Docs = \""))
				{
					string docString = input[i].Substring(input[i].IndexOf("Docs = \"") + 8);
					docString = docString.Substring(0, docString.IndexOf("\""));
					output.Add($"	[Tooltip(\"{docString}\")]");
				}

				int lineToInclude = i+1;
				while(input[lineToInclude].Contains("@"))
					lineToInclude++;

				ImportLine(input[lineToInclude], output, serializeFixers, serializeArrayFixers);
			}
		}

		if(serializeFixers.Count > 0 || serializeArrayFixers.Count > 0)
		{
			output[classLine] += ", ISerializationCallbackReceiver";
			output.Add("	public void OnBeforeSerialize() {}");
			output.Add("	public void OnAfterDeserialize() {");
			foreach (string varName in serializeFixers)
			{
				output.Add($"		if({varName} == ResourceLocation.InvalidLocation)");
				output.Add($"			{varName} = new ResourceLocation(_{varName});");
			}
			foreach(string arrayName in serializeArrayFixers)
			{
				output.Add($"		if({arrayName}.Length == 0) {{");
				output.Add($"		{arrayName} = new ResourceLocation[_{arrayName}.Length];");
				output.Add($"		for(int i = 0; i < _{arrayName}.Length; i++)");
				output.Add($"			{arrayName}[i] = new ResourceLocation(_{arrayName}[i]);");
				output.Add($"		}}");
			}
			output.Add("	}");
		}

		output.Add("}");
		
		File.WriteAllLines(csPath, output);
	}

	private static readonly Regex resLocRegex = new Regex("public ResourceLocation\\s*([a-z0-9_z-]*)\\s*=");
	private static readonly Regex arrayResLocRegex = new Regex("public ResourceLocation\\[\\]\\s*([a-z0-9_z-]*)\\s*=");
	private void ImportLine(string java, List<string> output, List<string> serializeFixers, List<string> serializeArrayFixers)
	{

		output.Add(ConvertToCS(java));

		Match match = resLocRegex.Match(java);
		if(match.Success)
		{
			string varName = match.Groups[1].Value;
			output.Add($"	[FormerlySerializedAs(\"{varName}\")]");
			output.Add("	[HideInInspector]");
			output.Add($"	public string _{varName};");
			serializeFixers.Add(varName);
		}
		Match array = arrayResLocRegex.Match(java);
		if(array.Success)
		{
			string arrayName = array.Groups[1].Value;
			output.Add($"	[FormerlySerializedAs(\"{arrayName}\")]");
			output.Add("	[HideInInspector]");
			output.Add($"	public string[] _{arrayName};");
			serializeArrayFixers.Add(arrayName);
		}
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
