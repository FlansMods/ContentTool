using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;

public class FlansModToolbox : EditorWindow
{
    [MenuItem ("Flan's Mod/Toolbox")]
    public static void  ShowWindow () 
	{
        EditorWindow.GetWindow(typeof(FlansModToolbox));
    }

	private ModelPreivewer ModelPreviewerInst = null;
	private DefinitionImporter DefinitionImporter = null;

	private string SelectedContentPackName = "";

	private string recipeFolder = "";
	private string copyFromMat = "iron";
	private string copyToMat = "aluminium";

    void OnGUI()
	{
		recipeFolder = GUILayout.TextField(recipeFolder);
		copyFromMat = GUILayout.TextField(copyFromMat);
		copyToMat = GUILayout.TextField(copyToMat);

		if(GUILayout.Button("Run"))
		{
			foreach(string file in Directory.EnumerateFiles(recipeFolder))
			{
				
				if(file.Contains($"part_fab_{copyFromMat}"))
				{
					string jsonText = File.ReadAllText(file);
					jsonText = jsonText.Replace(copyFromMat, copyToMat);
					File.WriteAllText(file.Replace(copyFromMat, copyToMat), jsonText);
					Debug.Log($"Writing to {file.Replace(copyFromMat, copyToMat)}");
				}
			}
		}

		

		if(ModelPreviewerInst == null)
		{
			ModelPreviewerInst = FindObjectOfType<ModelPreivewer>();
		}
		if(DefinitionImporter == null)
		{
			DefinitionImporter = FindObjectOfType<DefinitionImporter>();
		}

		if(DefinitionImporter != null)
		{
			// Pack selector
			GUILayout.Label("Select Content Pack");
			List<string> packNames = new List<string>();
			packNames.Add("None");
			int selectedPackIndex = 0;
			for(int i = 0; i < DefinitionImporter.Packs.Count; i++)
			{
				ContentPack pack = DefinitionImporter.Packs[i];
				packNames.Add(pack.ModName);
				if(pack.ModName == SelectedContentPackName)
				{
					selectedPackIndex = i + 1;
				}
			}
			selectedPackIndex = EditorGUILayout.Popup(selectedPackIndex, packNames.ToArray());
			SelectedContentPackName = selectedPackIndex == 0 ? "" : DefinitionImporter.Packs[selectedPackIndex - 1].ModName;
			GUILayout.Label(" ------------ ");
			// ---


			if(selectedPackIndex >= 1)
			{
				ContentPack pack = DefinitionImporter.Packs[selectedPackIndex - 1];
				
			}

			if (GUILayout.Button("Update Models"))
			{
				foreach (ContentPack pack in DefinitionImporter.Packs)
				{
					string modelsPath = $"{DefinitionImporter.ASSET_ROOT}/{pack.ModName}/models";
					if (!Directory.Exists(modelsPath))
						Directory.CreateDirectory(modelsPath);
					foreach (Definition def in pack.Content)
					{
						if(def.Model != null)
						{
							MinecraftModel updatedModel = UpdateModel(def.Model);
							if(updatedModel != null)
							{
								updatedModel.name = def.Model.name;
								if(updatedModel.name == null || updatedModel.name.Length == 0)
								{
									updatedModel.name = def.name;
								}
								if (updatedModel.name == null || updatedModel.name.Length == 0)
								{
									updatedModel.name = "unknown";
								}
								AssetDatabase.CreateAsset(updatedModel, $"{modelsPath}/{updatedModel.name}.asset");
							}
						}
					}
				}
			}
		}

		if(ModelPreviewerInst != null)
		{
			GUILayout.Label("Test");
		}

    }

	private MinecraftModel UpdateModel(Model model)
	{
		switch (model.Type)
		{
			case Model.ModelType.TurboRig:
			{
				TurboRig rig = CreateInstance<TurboRig>();
				foreach(Model.Section modelSection in model.sections)
				{
					TurboModel section = new TurboModel();
					section.partName = modelSection.partName;
					section.pieces = new TurboPiece[modelSection.pieces.Length];
					for (int i = 0; i < modelSection.pieces.Length; i++)
						section.pieces[i] = modelSection.pieces[i].CopyAsTurbo();
					rig.Sections.Add(section);
				}
				foreach (Model.AnimationParameter animParam in model.animations)
					rig.AnimationParameters.Add(new AnimationParameter(animParam.key, animParam.isVec3, animParam.floatValue, animParam.vec3Value));
				foreach (Model.AttachPoint attachPoint in model.attachPoints)
					rig.AttachPoints.Add(new AttachPoint(attachPoint.name, attachPoint.attachedTo, attachPoint.position));
				rig.TextureX = model.textureX;
				rig.TextureY = model.textureY;
				return rig;
			}
			case Model.ModelType.Block:
			{
				CubeModel cube = CreateInstance<CubeModel>();
				cube.north = new ResourceLocation(model.north);
				cube.east = new ResourceLocation(model.east);
				cube.south = new ResourceLocation(model.south);
				cube.west = new ResourceLocation(model.west);
				cube.top = new ResourceLocation(model.top);
				cube.bottom = new ResourceLocation(model.bottom);
				cube.particle = new ResourceLocation(model.north);
				return cube;
			}
			case Model.ModelType.Custom:
			{
				BlockbenchModel bbModel = CreateInstance<BlockbenchModel>();
				bbModel.ID = new ResourceLocation(model.customModelLocation);
				return bbModel;
			}
			case Model.ModelType.Item:
			{
				ItemModel itemModel = CreateInstance<ItemModel>();
				itemModel.ID = new ResourceLocation(model.icon);
				return itemModel;
			}
		}
		return null;
	}
}
