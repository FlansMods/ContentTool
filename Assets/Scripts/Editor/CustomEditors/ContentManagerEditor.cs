using System;
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


	public bool ExpandImportResults = false;
	public enum ImportSubTab 
	{
		ImportPacks,
		ImportModels,
	}
	public static readonly GUIContent[] ImportSubTabTitles = new GUIContent[] {
		new GUIContent("Import Packs"),
		new GUIContent("Import Models"),
	};
	public ImportSubTab SelectedImportTab = ImportSubTab.ImportPacks;
	public void ImportTab(ContentManager instance)
	{
		if (ContentManager.LastImportOperation != null)
		{
			ExpandExportResults = GUIVerify.VerificationsResultsPanel(
				ExpandExportResults,
				ContentManager.LastImportOperation.GetOpName(),
				ContentManager.LastImportOperation.AsList());
		}

		SelectedImportTab = (ImportSubTab)GUILayout.Toolbar((int)SelectedImportTab, ImportSubTabTitles);
		switch(SelectedImportTab)
		{
			case ImportSubTab.ImportPacks:
				ImportTab_Packs(instance);
				break;
			case ImportSubTab.ImportModels:
				ImportTab_Models(instance);
				break;
		}
		
	}

	public FlanStyles.FoldoutTree ModelFoldout = new FlanStyles.FoldoutTree();
	private GUILayoutOption EntryHeight = GUILayout.MinHeight(20);
	public void ImportTab_Models(ContentManager instance)
	{
		foreach(var kvp in instance.GetPreImportModelList())
		{
			GUILayout.BeginHorizontal();
			bool foldout = ModelFoldout.Foldout(new GUIContent(kvp.Key), kvp.Key);
			GUILayout.Label(" maps to... ", FlanStyles.BoldLabel);
			kvp.Value.PackName = EditorGUILayout.DelayedTextField(kvp.Value.PackName);
			List<string> packNames = new List<string>();
			int selectedPackIndex = -1;
			for (int i = 0; i < instance.Packs.Count; i++)
			{
				packNames.Add(instance.Packs[i].name);
				if (kvp.Value.PackName == instance.Packs[i].name)
					selectedPackIndex = i;
			}
			int changedIndex = EditorGUILayout.Popup(selectedPackIndex, packNames.ToArray());
			if (changedIndex != selectedPackIndex && changedIndex != -1)
				kvp.Value.PackName = packNames[changedIndex];

			// Import all button

			GUILayout.EndHorizontal();


			bool packNameSelected = kvp.Value.PackName.Length > 0;
			if (foldout)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Space(15);

				// Column 1 - Model Name
				GUILayout.BeginVertical();
				foreach (string modelName in kvp.Value.Models)
					GUILayout.Label(modelName, EntryHeight);
				GUILayout.EndVertical();

				GUILayout.FlexibleSpace();

				

				// Column 2 - buttons!
				GUILayout.BeginVertical();
				foreach (var modelImportMapping in kvp.Value.ImportMappings)
				{
					GUILayout.BeginHorizontal();
					string from = modelImportMapping.Key;
					string to = modelImportMapping.Value;
					bool outputExists = File.Exists(to);
					bool doImport = false;

					EditorGUI.BeginDisabledGroup(!outputExists);
					if (GUILayout.Button(FlanStyles.ImportSingleAssetOverwrite, EntryHeight))
						doImport = true;
					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup(outputExists);
					if (GUILayout.Button(FlanStyles.ImportSingleAssetNewOnly, EntryHeight))
						doImport = true;
					EditorGUI.EndDisabledGroup();

					if(doImport)
					{
						TurboRootNode rootNode = JavaModelImporter.ImportJavaModel(from, ContentManager.GetFreshImportLogger($"Importing {from}"));
						rootNode.AddDefaultTransforms();
						string folderPath = to.Substring(0, to.LastIndexOf('/'));
						if (!Directory.Exists(folderPath))
							Directory.CreateDirectory(folderPath);

						PrefabUtility.SaveAsPrefabAsset(rootNode.gameObject, to);
                        UnityEngine.Object.DestroyImmediate(rootNode.gameObject);			
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				// Column 3 - maps to...
				GUILayout.BeginVertical();
				foreach (var modelImportMapping in kvp.Value.ImportMappings)
				{
					if (packNameSelected)
					{
						bool fileExists = File.Exists(modelImportMapping.Value);
						if (fileExists)
							GUILayout.Label($"maps to {modelImportMapping.Value} (already exists)", FlanStyles.RedLabel, EntryHeight);
						else
							GUILayout.Label($"maps to {modelImportMapping.Value}", EntryHeight);
					}
					else
						GUILayout.Label($"<Select an export folder>", EntryHeight);
				}
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
			}
			
		}
	}

	public void ImportTab_Packs(ContentManager instance)
	{
		foreach (var kvp in instance.GetImportPackMap())
		{
			string sourcePack = kvp.Key.PackName;
			GUILayout.BeginHorizontal();
			bool packFoldout = NestedFoldout(sourcePack, sourcePack);

			// --- Import Status ---
			bool alreadyImported = kvp.Value != null;
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
				GUILayout.Button(FlanStyles.RefreshImportInfo, GUILayout.Width(32));
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				GUILayout.Label($"[?] Input Assets", FlanStyles.BoldLabel, GUILayout.Width(120));
				GUILayout.Label($"[?] Output Assets", FlanStyles.BoldLabel, GUILayout.Width(120));
				if (GUILayout.Button(FlanStyles.RefreshImportInfo, GUILayout.Width(32)))
				{
					instance.GenerateFullImportMap(sourcePack);
				}
			}

			// --- Import (New Only) Button! ---
			if (GUILayout.Button(FlanStyles.ImportPackNewOnly, GUILayout.Width(32)))
			{
				instance.ImportPack(sourcePack, false, ContentManager.GetFreshImportLogger($"Import new assets only for {sourcePack}"));
			}
			// --- Import Button! ---
			if (GUILayout.Button(FlanStyles.ImportPackOverwrite, GUILayout.Width(32)))
			{
				instance.ImportPack(sourcePack, true, ContentManager.GetFreshImportLogger($"Import ALL assets for {sourcePack}"));
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
						string typeFoldoutPath = $"{sourcePack}/{defType}";
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
								GUILayout.Button(FlanStyles.RefreshImportInfo, GUILayout.Width(32));
								EditorGUI.EndDisabledGroup();
							}
						}
						else
						{
							GUILayout.Label($"[?] Input Assets", GUILayout.Width(120));
							GUILayout.Label($"[?] Output Assets", GUILayout.Width(120));
							if (GUILayout.Button(FlanStyles.RefreshImportInfo, GUILayout.Width(32)))
							{
								instance.HasFullImportMap(sourcePack, defType, true);
							}
						}
						if (GUILayout.Button(FlanStyles.ImportPackOverwrite, GUILayout.Width(32)))
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
										{
											if (File.Exists(input))
												GUILayout.Label(input);
											else
												GUILayout.Label(new GUIContent(input).WithTooltip("Input file not found"), FlanStyles.RedLabel);
										}
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

	public bool ExpandExportResults = false;
	public void ExportTab(ContentManager instance, Action<ContentPack> viewDetailsFunc = null)
	{ 
		if (FlansModExport.LastExportOperation != null)
		{
			ExpandExportResults = GUIVerify.VerificationsResultsPanel(
				ExpandExportResults,
				FlansModExport.LastExportOperation.GetOpName(),
				FlansModExport.LastExportOperation.AsList());
		}

		if(GUILayout.Button("Force refresh verifications"))
		{
			GUIVerify.InvalidateCaches();
		}

		string changedLoc = FolderSelector("Export Location", instance.ExportRoot, "Assets/Export");
		if(changedLoc != instance.ExportRoot)
		{
			instance.ExportRoot = changedLoc;
			EditorUtility.SetDirty(instance);
		}

		foreach(ContentPack pack in instance.Packs)
		{
			string packFoldoutPath = $"{pack.ModName}";
	
			// Foldout / summary view
			GUILayout.BeginHorizontal();
			bool packFoldout = NestedFoldout(packFoldoutPath, pack.ModName);
			GUILayout.FlexibleSpace();

			bool passedVerification = GUIVerify.CachedVerificationHeader(pack);
			if(passedVerification)
			{
				if (GUILayout.Button(FlanStyles.ExportPackNewOnly, GUILayout.Width(32)))
				{
					FlansModExport.ExportPack(pack, false);
				}
				if (GUILayout.Button(FlanStyles.ExportPackOverwrite, GUILayout.Width(32)))
				{
					FlansModExport.ExportPack(pack, true);
				}
			}
			else
			{
				if (viewDetailsFunc != null)
				{
					if (GUILayout.Button("View Details"))
					{
						viewDetailsFunc.Invoke(pack);
					}
				}
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Button(FlanStyles.ExportError, GUILayout.Width(68));
				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();

			if (packFoldout)
			{
				EditorGUI.indentLevel++;
				string contentFoldoutPath = $"{packFoldoutPath}/content";
				bool contentFoldout = NestedFoldout(contentFoldoutPath, "Content");
				if (contentFoldout)
				{
					EditorGUI.indentLevel++;
					AssetListFoldout(instance, pack, contentFoldoutPath, pack.AllContent);
					EditorGUI.indentLevel--;
				}

				string modelsFoldoutPath = $"{packFoldoutPath}/models";
				bool modelsFoldout = NestedFoldout(modelsFoldoutPath, "Models");
				if(modelsFoldout)
				{
					EditorGUI.indentLevel++;
					AssetListFoldout(instance, pack, modelsFoldoutPath, pack.AllModels);
					EditorGUI.indentLevel--;
				}

				string texturesFoldoutPath = $"{packFoldoutPath}/textures";
				bool texturesFoldout = NestedFoldout(texturesFoldoutPath, "Textures");
				if (texturesFoldout)
				{
					EditorGUI.indentLevel++;
					AssetListFoldout(instance, pack, texturesFoldoutPath, pack.AllTextures);
					EditorGUI.indentLevel--;
				}
				EditorGUI.indentLevel--;
			}
		}
	}

	private void AssetListFoldout(ContentManager instance, ContentPack pack, string foldoutPath, IEnumerable<UnityEngine.Object> assets)
	{
		foreach (UnityEngine.Object asset in assets)
		{
			IVerificationLogger verifications = new VerificationList($"Verify asset {asset}");
			if (asset is IVerifiableAsset verifiable)
			{
				verifiable.GetVerifications(verifications);
			}

			GUILayout.BeginHorizontal();
			VerifyType verificationSummary = Verification.GetWorstState(verifications);
			int quickFixCount = Verification.CountQuickFixes(verifications);
			string assetFoldoutPath = $"{foldoutPath}/{asset.name}";
			bool assetFoldout = NestedFoldout(assetFoldoutPath, asset.name);
			bool alreadyExported = FlansModExport.ExportedAssetAlreadyExists(pack.ModName, asset);
			if (!assetFoldout)
			{
				GUILayout.FlexibleSpace();
				if (quickFixCount > 0)
				{
					GUILayout.Label($"{quickFixCount} Quick-Fixes Available");
					if (GUILayout.Button("Apply"))
					{
						Verification.ApplyAllQuickFixes(verifications);
					}
				}
				GUIVerify.VerificationIcon(verifications);
				if (verificationSummary == VerifyType.Fail)
				{
					EditorGUI.BeginDisabledGroup(true);
					GUILayout.Button(FlanStyles.ExportError, GUILayout.Width(68));
					EditorGUI.EndDisabledGroup();
				}
				else
				{
					EditorGUI.BeginDisabledGroup(alreadyExported);
					if (GUILayout.Button(FlanStyles.ExportSingleAsset, GUILayout.Width(32)))
					{
						FlansModExport.ExportSingleAsset(asset, false);
					}
					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup(!alreadyExported);
					if (GUILayout.Button(FlanStyles.ExportSingleAssetOverwrite, GUILayout.Width(32)))
					{
						FlansModExport.ExportSingleAsset(asset, true);
					}
					EditorGUI.EndDisabledGroup();
				}				
			}
			GUILayout.EndHorizontal();
			if (assetFoldout)
			{
				EditorGUI.indentLevel++;

				EditorGUILayout.ObjectField(asset, asset.GetType(), false);
				GUILayout.BeginHorizontal();
				GUILayout.Label(GUIContent.none, GUILayout.Width(15 * EditorGUI.indentLevel));
				{
					GUILayout.BeginVertical();
					GUILayout.Label(asset.GetType().ToString());
					GUIVerify.VerificationsBox(verifications);

					if (quickFixCount > 0)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label($"{quickFixCount} Quick-Fixes Available");
						if (GUILayout.Button("Apply"))
						{
							Verification.ApplyAllQuickFixes(verifications);
						}
						GUILayout.EndHorizontal();
					}

					GUILayout.BeginHorizontal();
					if(verificationSummary == VerifyType.Fail)
					{
						EditorGUI.BeginDisabledGroup(true);
						GUILayout.Button(FlanStyles.ExportError, GUILayout.Width(68));
						EditorGUI.EndDisabledGroup();
					}
					else
					{
						EditorGUI.BeginDisabledGroup(alreadyExported);
						if (GUILayout.Button(FlanStyles.ExportSingleAsset, GUILayout.Width(32)))
						{
							FlansModExport.ExportSingleAsset(asset, false);
						}
						EditorGUI.EndDisabledGroup();
						EditorGUI.BeginDisabledGroup(!alreadyExported);
						if (GUILayout.Button(FlanStyles.ExportSingleAssetOverwrite, GUILayout.Width(32)))
						{
							FlansModExport.ExportSingleAsset(asset, true);
						}
						EditorGUI.EndDisabledGroup();
					}
					
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();

				EditorGUI.indentLevel--;
			}
		}
	}

	private string FolderSelector(string label, string folder, string defaultLocation)
	{
		GUILayout.Label(label);
		GUILayout.BeginHorizontal();
		folder = EditorGUILayout.DelayedTextField(folder);
		if (GUILayout.Button(FlanStyles.SelectFolder))
			folder = EditorUtility.OpenFolderPanel("Select resources root folder", folder, "");
		if (GUILayout.Button(FlanStyles.ResetToDefault))
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
}
