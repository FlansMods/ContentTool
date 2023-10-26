using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContentPack))]
public class ContentPackEditor : Editor
{
    public override void OnInspectorGUI()
	{
		ContentPack pack = (ContentPack)target;
		if(pack != null)
		{
			Dictionary<Object, List<Verification>> multiVerify = new Dictionary<Object, List<Verification>>();
			List<Verification> packIssues = new List<Verification>();
			pack.GetVerifications(packIssues);
			multiVerify.Add(pack, packIssues);
			foreach(Definition def in pack.AllContent)
			{
				List<Verification> verifications = new List<Verification>();
				def.GetVerifications(verifications);
				if (verifications.Count == 0)
					verifications.Add(Verification.Success($"{def.name} has no outstanding issues."));
				multiVerify.Add(def, verifications);
			}
			GUIVerify.VerificationsBox(multiVerify);

			if(GUILayout.Button("Export"))
			{
				DefinitionImporter importExport = FindObjectOfType<DefinitionImporter>();
				if(importExport != null)
				{
					importExport.ExportPack(pack.name);
				}
			}
		}
		base.OnInspectorGUI();


	}
}
