using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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
				List<Verification> errors = new List<Verification>();
				instance.ImportPack(sourcePack, errors, false);
			}
			// --- Import Button! ---
			if (GUILayout.Button(FlanStyles.ImportPackOverwrite, GUILayout.Width(32)))
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
		string changedLoc = FolderSelector("Export Location", instance.ExportRoot, "Assets/Export");
		if(changedLoc != instance.ExportRoot)
		{
			instance.ExportRoot = changedLoc;
			EditorUtility.SetDirty(instance);
		}

		foreach(ContentPack pack in instance.Packs)
		{
			string packFoldoutPath = $"{pack.ModName}";
			List<Verification> verifications = new List<Verification>();
			pack.GetVerifications(verifications);
			foreach (Definition def in pack.AllContent)
				def.GetVerifications(verifications);
			foreach (MinecraftModel model in pack.AllModels)
				model.GetVerifications(verifications);
			VerifyType verificationSummary = Verification.GetWorstState(verifications);
			int failCount = Verification.CountFailures(verifications);
			int quickFixCount = Verification.CountQuickFixes(verifications);

			// Foldout / summary view
			GUILayout.BeginHorizontal();
			bool packFoldout = NestedFoldout(packFoldoutPath, pack.ModName);
			GUILayout.FlexibleSpace();
			if(quickFixCount > 0)
			{
				GUILayout.Label($"{quickFixCount} Quick-Fixes Available");
				if(GUILayout.Button("Apply"))
				{
					Verification.ApplyAllQuickFixes(verifications);
				}
			}
			if (failCount > 0)
			{
				GUILayout.Label($"{failCount} Errors");
				GUIVerify.VerificationIcon(verifications);
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Button(FlanStyles.ExportError, GUILayout.Width(68));
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				GUIVerify.VerificationIcon(verifications);
				if (GUILayout.Button(FlanStyles.ExportPackNewOnly, GUILayout.Width(32)))
				{
					instance.ExportPack(pack.ModName, false);
				}
				if (GUILayout.Button(FlanStyles.ExportPackOverwrite, GUILayout.Width(32)))
				{
					List<Verification> exportVerifications = new List<Verification>();
					instance.ExportPack(pack.ModName, true, exportVerifications);
					foreach (Verification verification in exportVerifications)
					{
						if (verification.Type == VerifyType.Fail)
						{
							Debug.LogError($"Verification Failure: {verification.Message}");
						}
					}
				}
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

	private void AssetListFoldout(ContentManager instance, ContentPack pack, string foldoutPath, IEnumerable<Object> assets)
	{
		foreach (Object asset in assets)
		{
			List<Verification> verifications = new List<Verification>();
			if (asset is IVerifiableAsset verifiable)
			{
				verifiable.GetVerifications(verifications);
			}

			GUILayout.BeginHorizontal();
			VerifyType verificationSummary = Verification.GetWorstState(verifications);
			int quickFixCount = Verification.CountQuickFixes(verifications);
			string assetFoldoutPath = $"{foldoutPath}/{asset.name}";
			bool assetFoldout = NestedFoldout(assetFoldoutPath, asset.name);
			bool alreadyExported = instance.ExportedAssetAlreadyExists(pack.ModName, asset);
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
						instance.ExportAsset(pack.ModName, asset);
					}
					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup(!alreadyExported);
					if (GUILayout.Button(FlanStyles.ExportSingleAssetOverwrite, GUILayout.Width(32)))
					{
						instance.ExportAsset(pack.ModName, asset);
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
							instance.ExportAsset(pack.ModName, asset);
						}
						EditorGUI.EndDisabledGroup();
						EditorGUI.BeginDisabledGroup(!alreadyExported);
						if (GUILayout.Button(FlanStyles.ExportSingleAssetOverwrite, GUILayout.Width(32)))
						{
							instance.ExportAsset(pack.ModName, asset);
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
