using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContentManager))]
public class ContentManagerEditor : Editor
{	
	private bool hasDoneInit = false;
	private enum Tab
	{
		Import,
		Export,
	}
	private string[] TabNames = new string[] {
		"Import",
		"Export"
	};
	private Tab CurrentTab = Tab.Import;
	private int SelectedImportPackIndex = 0;
	private List<string> ImportFoldouts = new List<string>();

	public override void OnInspectorGUI()
	{
		ContentManager instance = (ContentManager)target;
		if (instance != null)
		{
			instance.Refresh();
			CurrentTab = (Tab)GUILayout.Toolbar((int)CurrentTab, TabNames);
			switch (CurrentTab)
			{
				case Tab.Import:
					ImportTab(instance);
					break;
				case Tab.Export:
					ExportTab(instance);
					break;
			}
		}
	}

	public void ImportTab(ContentManager instance)
	{
		foreach (string sourcePack in instance.GetPreImportPackNames())
		{
			string packFoldoutPath = $"{sourcePack}";
			GUILayout.BeginHorizontal();
			bool packFoldout = NestedFoldout(packFoldoutPath, sourcePack);

			// --- Import Status ---
			bool alreadyImported = instance.FindContentPack(sourcePack) != null;
			if (alreadyImported)
				GUILayout.Label("[Imported]", FlanStyles.GreenLabel, GUILayout.Width(120));
			else
				GUILayout.Label("[Not Imported]", FlanStyles.BoldLabel, GUILayout.Width(120));

			// --- Asset Summary ---
			int assetCount = instance.GetNumAssetsInPack(sourcePack);
			bool hasFullImportMapForAllTypes = instance.HasFullImportMap(sourcePack);
			if (hasFullImportMapForAllTypes)
			{
				if (instance.TryGetFullImportCount(sourcePack, out int inputCount, out int outputCount))
				{
					GUILayout.Label($"[{inputCount}] Input Assets", FlanStyles.GreenLabel, GUILayout.Width(120));
					GUILayout.Label($"[{outputCount}] Output Assets", FlanStyles.GreenLabel, GUILayout.Width(120));
				}
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Button(EditorGUIUtility.IconContent("d_UnityEditor.HistoryWindow"), GUILayout.Width(32));
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				GUILayout.Label($"[?] Input Assets", FlanStyles.BoldLabel, GUILayout.Width(120));
				GUILayout.Label($"[?] Output Assets", FlanStyles.BoldLabel, GUILayout.Width(120));
				if (GUILayout.Button(EditorGUIUtility.IconContent("d_UnityEditor.HistoryWindow"), GUILayout.Width(32)))
				{
					instance.GenerateFullImportMap(sourcePack);
				}
			}

			// --- Import (New Only) Button! ---
			if (GUILayout.Button(EditorGUIUtility.IconContent("Customized"), GUILayout.Width(32)))
			{
				List<Verification> errors = new List<Verification>();
				instance.ImportPack(sourcePack, errors, false);
			}
			// --- Import Button! ---
			if (GUILayout.Button(EditorGUIUtility.IconContent("Download-Available"), GUILayout.Width(32)))
			{
				List<Verification> errors = new List<Verification>();
				instance.ImportPack(sourcePack, errors, true);
			}
			GUILayout.EndHorizontal();

			if (packFoldout)
			{
				EditorGUI.indentLevel++;
				for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
				{
					EDefinitionType defType = (EDefinitionType)i;
					int count = instance.GetNumAssetsInPack(sourcePack, defType);
					if (count > 0)
					{
						string typeFoldoutPath = $"{packFoldoutPath}/{defType}";
						GUILayout.BeginHorizontal();
						bool typeFoldout = NestedFoldout(typeFoldoutPath, defType.ToString());
						GUILayout.FlexibleSpace();

						GUILayout.Label($"[{count}] .txt", GUILayout.Width(100));
						bool hasFullImportMap = instance.HasFullImportMap(sourcePack, defType);
						if (hasFullImportMap)
						{
							// Print an import summary
							if (instance.TryGetFullImportCount(sourcePack, defType, out int inputCount, out int outputCount))
							{
								GUILayout.Label($"[{inputCount}] Input Assets", GUILayout.Width(120));
								GUILayout.Label($"[{outputCount}] Output Assets", GUILayout.Width(120));
								EditorGUI.BeginDisabledGroup(true);
								GUILayout.Button(EditorGUIUtility.IconContent("d_UnityEditor.HistoryWindow"), GUILayout.Width(32));
								EditorGUI.EndDisabledGroup();
							}
						}
						else
						{
							GUILayout.Label($"[?] Input Assets", GUILayout.Width(120));
							GUILayout.Label($"[?] Output Assets", GUILayout.Width(120));
							if (GUILayout.Button(EditorGUIUtility.IconContent("d_UnityEditor.HistoryWindow"), GUILayout.Width(32)))
							{
								instance.HasFullImportMap(sourcePack, defType, true);
							}
						}
						if (GUILayout.Button(EditorGUIUtility.IconContent("Download-Available"), GUILayout.Width(32)))
						{

						}
						GUILayout.EndHorizontal();


						if (typeFoldout)
						{
							EditorGUI.indentLevel++;
							foreach (string txtFileName in instance.GetAssetNamesInPack(sourcePack, defType))
							{
								string txtFileFoldoutPath = $"{typeFoldoutPath}/{txtFileName}";
								bool txtFileFoldout = NestedFoldout(txtFileFoldoutPath, Utils.ToLowerWithUnderscores(txtFileName.Split(".")[0]));
								if (txtFileFoldout)
								{
									EditorGUI.indentLevel++;
									GUILayout.BeginHorizontal();
									{
										List<string> inputMap, outputMap;
										instance.TryGetImportMap(sourcePack, defType, txtFileName, out inputMap, out outputMap);
										string targetAsset = instance.GetTargetAssetPathFor(sourcePack, defType, txtFileName);

										GUILayout.Label(GUIContent.none, GUILayout.Width(15 * EditorGUI.indentLevel));

										GUILayout.BeginVertical();
										GUILayout.Label("File Name: ");
										if (File.Exists(targetAsset))
											GUILayout.Label("Already exists at: ", FlanStyles.GreenLabel);
										else
											GUILayout.Label("Will import to: ");
										foreach (string input in inputMap)
											GUILayout.Label("Extra Input: ");
										foreach (string output in outputMap)
										{
											if (File.Exists(output))
												GUILayout.Label("Extra already imported at: ", FlanStyles.GreenLabel);
											else
												GUILayout.Label("Extra Output: ");
										}
										GUILayout.EndVertical();

										GUILayout.BeginVertical();
										GUILayout.Label(txtFileName);

										if (File.Exists(targetAsset))
											GUILayout.Label(targetAsset, FlanStyles.GreenLabel);
										else
											GUILayout.Label(targetAsset);
										foreach (string input in inputMap)
											GUILayout.Label(input);
										foreach (string output in outputMap)
										{
											if (File.Exists(output))
												GUILayout.Label(output, FlanStyles.GreenLabel);
											else
												GUILayout.Label(output);
										}
										GUILayout.EndVertical();
									}
									GUILayout.EndHorizontal();
									EditorGUI.indentLevel--;
								}
							}
							EditorGUI.indentLevel--;
						}
					}
				}
				EditorGUI.indentLevel--;
			}
		}
	}

	public void ExportTab(ContentManager instance)
	{
		FolderSelector("Export Location", instance.ExportRoot, "Assets/Export");

	}

	private string FolderSelector(string label, string folder, string defaultLocation)
	{
		GUILayout.Label(label);
		GUILayout.BeginHorizontal();
		folder = EditorGUILayout.DelayedTextField(folder);
		if (GUILayout.Button(EditorGUIUtility.IconContent("d_Profiler.Open")))
			folder = EditorUtility.OpenFolderPanel("Select resources root folder", folder, "");
		if (GUILayout.Button(EditorGUIUtility.IconContent("d_preAudioLoopOff")))
			folder = defaultLocation;
		GUILayout.EndHorizontal();
		return folder;
	}

	private bool NestedFoldout(string path, string label)
	{
		bool oldFoldout = ImportFoldouts.Contains(path);
		bool newFoldout = EditorGUILayout.Foldout(oldFoldout, label);
		if (newFoldout && !oldFoldout)
			ImportFoldouts.Add(path);
		else if (oldFoldout && !newFoldout)
			ImportFoldouts.Remove(path);
		return newFoldout;
	}



	private void p()
	{ 
		{
			ContentManager instance = (ContentManager)target;
			//base.OnInspectorGUI();

			if (!hasDoneInit)
			{
				instance.Refresh();
				hasDoneInit = true;
			}

			instance.ExportRoot = EditorGUILayout.DelayedTextField(instance.ExportRoot);
			if(GUILayout.Button("Choose Export Folder"))
			{
				instance.ExportRoot  = EditorUtility.OpenFolderPanel("Select resources root folder", "", "");
				
			}
			

			GUILayout.Label("Imported Packs");

			List<string> packNames = new List<string>();

			foreach(ContentPack pack in instance.Packs)
			{
				if(pack != null)
					packNames.Add(pack.name);
			}

			foreach(string packName in packNames)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"> {packName}");
				//if(GUILayout.Button("Re-Import"))
					//instance.ImportPack(packName);
				if(GUILayout.Button("Export"))
					instance.ExportPack(packName);
				GUILayout.EndHorizontal();
			}

			GUILayout.Label("Unimported Packs");
			
			//foreach(string packName in instance.UnimportedPacks)
			//{
			//	GUILayout.BeginHorizontal();
			//	if(GUILayout.Button("Import"))
			//	{
			//		instance.ImportPack(packName);
			//	}
			//	GUILayout.Label($"> {packName}");
			//	GUILayout.EndHorizontal();
			//}

		}
	}
}
