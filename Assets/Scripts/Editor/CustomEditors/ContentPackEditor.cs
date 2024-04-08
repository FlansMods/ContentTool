using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContentPack))]
public class ContentPackEditor : Editor
{
	public static Tab CurrentTab = Tab.Verification;

	public enum Tab
	{
		Import,
		Verification,
		Balancing,
		Debug,
	}

	public static readonly string[] TabNames = new string[] {
		"Import",
		"Verification and Export",
		"Balancing and .CSV Sheets",
		"Debug View"
	};


	public override void OnInspectorGUI()
	{
		ContentPack pack = (ContentPack)target;
		if (pack != null)
		{
			CurrentTab = (Tab)GUILayout.Toolbar((int)CurrentTab, TabNames);
			switch (CurrentTab)
			{
				case Tab.Import:
					//ImportTab();
					break;

				case Tab.Verification:
					VerificationTab(pack);
					break;
				case Tab.Balancing:
					BalancingTab(pack);
					break;
				case Tab.Debug:
					DebugTab(pack);
					break;
			}
		}
	}

	public void VerificationTab(ContentPack pack)
	{
		Dictionary<Object, List<Verification>> multiVerify = new Dictionary<Object, List<Verification>>();
		IVerificationLogger packIssues = new VerificationList($"Verify Tab {pack}");
		pack.GetVerifications(packIssues);
		multiVerify.Add(pack, packIssues.AsList());
		foreach (Definition def in pack.AllContent)
		{
			IVerificationLogger verifications = new VerificationList($"Verify Content {def} in Pack {pack}");
			def.GetVerifications(verifications);
			if (verifications.AsList().Count == 0)
				verifications.Success($"{def.name} has no outstanding issues.");
			multiVerify.Add(def, verifications.AsList());
		}
		foreach (RootNode model in pack.AllModels)
		{
			IVerificationLogger verifications = new VerificationList($"Verify Model {model} in Pack {pack}");
			model.GetVerifications(verifications);
			if (verifications.AsList().Count == 0)
				verifications.Success($"Model {model.name} has no outstanding issues.");
			multiVerify.Add(model, verifications.AsList());
		}
		GUIVerify.VerificationsBox(multiVerify);

		if (GUILayout.Button("Export"))
		{
			FlansModExport.ExportPack(pack, false);
		}
	}

	public void BalancingTab(ContentPack pack)
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Import All Balacing CSVs [!]"))
		{
			if (EditorUtility.DisplayDialog(
				"Import and Overwrite Stats?",
				$"Importing these .csv files will overwrite the values found within your Definitions. It is recommended to use Git or have a backup of your 'Assets/Content Packs/{pack.name}' folder.",
				"Overwrite",
				"Cancel"))
			{
				SpreadsheetImportExport.ImportFromCSV(pack);
			}
		}
		if (GUILayout.Button("Export to Balacing CSVs"))
		{
			SpreadsheetImportExport.ExportToCSV(pack);
		}
		GUILayout.EndHorizontal();

		FlanStyles.HorizontalLine();

		foreach (ENewDefinitionType defType in SpreadsheetImportExport.ExportableTypes)
		{
			GUILayout.BeginHorizontal();
			int contentCount = pack.GetContentCount(defType);
			EditorGUI.BeginDisabledGroup(contentCount == 0);
			{
				GUILayout.Label($"{defType} (x{contentCount}) Balancing:");
				GUILayout.FlexibleSpace();
				EditorGUI.BeginDisabledGroup(!SpreadsheetImportExport.CanImportCSV(pack, defType));
				if (GUILayout.Button("Import from CSV [!]"))
				{
					if (EditorUtility.DisplayDialog(
						"Import and Overwrite Stats?",
						$"Importing this .csv file will overwrite the values found within your {defType} Definitions. It is recommended to use Git or have a backup of your 'Assets/Content Packs/{pack.name}/{defType.OutputFolder()}' folder.",
						"Overwrite",
						"Cancel"))
					{
						SpreadsheetImportExport.ImportFromCSV(pack, defType);
					}
				}
				EditorGUI.EndDisabledGroup();
				if (GUILayout.Button("Export to CSV"))
				{
					SpreadsheetImportExport.ExportToCSV(pack, defType);
				}
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
		}
	}

	public void DebugTab(ContentPack pack)
	{
		base.OnInspectorGUI();
	}
}
