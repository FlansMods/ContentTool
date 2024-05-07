using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

/*
[ScriptedImporter(1, "bbmodel")]
public class BBModelImporter : ScriptedImporter
{
	public float m_Scale = 1;

	public override void OnImportAsset(AssetImportContext ctx)
	{
		BlockbenchModel bbModel = ScriptableObject.CreateInstance<BlockbenchModel>();
		using (StringReader reader = new StringReader(File.ReadAllText(ctx.assetPath)))
		using (JsonTextReader jReader = new JsonTextReader(reader))
		{
			JObject jRoot = JObject.Load(jReader);
			bbModel.name = jRoot.ContainsKey("name") ? jRoot.GetValue("name").ToString() : ctx.assetPath;
			if(jRoot.ContainsKey("elements"))
			{
				
			}

		}
	}
}
*/