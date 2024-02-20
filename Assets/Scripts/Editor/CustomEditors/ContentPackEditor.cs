using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContentPack))]
public class ContentPackEditor : Editor
{
	public bool VerificationFoldout = false;
	public bool DebugFoldout = false;
	public bool QuickActionsFoldout = true;

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
			foreach(RootNode model in pack.AllModels)
			{
				List<Verification> verifications = new List<Verification>();
				model.GetVerifications(verifications);
				if (verifications.Count == 0)
					verifications.Add(Verification.Success($"Model {model.name} has no outstanding issues."));
				multiVerify.Add(model, verifications);
			}

			GUILayout.BeginHorizontal();
			VerificationFoldout = EditorGUILayout.Foldout(VerificationFoldout, "Verification");
			if(!VerificationFoldout)
			{
				GUIVerify.VerificationIcon(multiVerify);
			}
			GUILayout.EndHorizontal();
			if (VerificationFoldout)
			{
				GUIVerify.VerificationsBox(multiVerify);
			}

			//QuickActionsFoldout = EditorGUILayout.Foldout(QuickActionsFoldout, "Quick Actions");
			if (QuickActionsFoldout)
			{
				
			}

			if (GUILayout.Button("Import Balacing CSV [!]"))
			{
				SpreadsheetImportExport.ImportFromCSV(pack);
			}
			if (GUILayout.Button("Export Balacing CSV"))
			{
				SpreadsheetImportExport.ExportToCSV(pack);
			}

			if (GUILayout.Button("Export"))
			{
				ContentManager importExport = FindObjectOfType<ContentManager>();
				if(importExport != null)
				{
					importExport.ExportPack(pack.name, false, ContentManager.GetFreshExportLogger($"Export {pack.name}"));
				}
			}
		}

		DebugFoldout = EditorGUILayout.Foldout(DebugFoldout, "Debug View");
		if (DebugFoldout)
		{
			base.OnInspectorGUI();
		}

	}
}
