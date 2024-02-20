using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class Definition : ScriptableObject, IVerifiableAsset
{
	public virtual void GetVerifications(List<Verification> verifications)
	{
		try
		{

			ResourceLocation resLoc = this.GetLocation();

			if (LocalisedNames.Count < 1 && this is not FlanimationDefinition)
				verifications.Add(Verification.Failure($"{name} has no localised name in any language"));
			if (name != Utils.ToLowerWithUnderscores(name))
			{
				verifications.Add(Verification.Failure(
					$"Definition {name} does not have a Minecraft-compliant name",
					() =>
					{
						name = Utils.ToLowerWithUnderscores(name);
					})
				);
			}

			VerifyModifiers(verifications);

			if (this is GunDefinition gunDef)
			{
				foreach (ActionGroupDefinition group in gunDef.actionGroups)
				{
					bool hasADSAction = false;
					foreach (ActionDefinition actionDef in group.actions)
					{
						if (actionDef.actionType == EActionType.AimDownSights || actionDef.actionType == EActionType.Scope)
						{
							hasADSAction = true;
						}
					}
					if (hasADSAction && group.repeatMode != ERepeatMode.Toggle)
					{
						verifications.Add(Verification.Neutral(
							$"Aim down sights action is not a toggle",
							() =>
							{
								group.repeatMode = ERepeatMode.Toggle;
							})
						);
					}
				}

				for (int i = 0; i < gunDef.paints.paintjobs.Length; i++)
				{
					PaintjobDefinition paintjob = gunDef.paints.paintjobs[i];
					bool hasLoc = false;
					foreach (LocalisedExtra loc in gunDef.LocalisedExtras)
					{
						if (loc.Unlocalised == $"paintjob.{resLoc.Namespace}.{paintjob.textureName}")
						{
							hasLoc = true;
							break;
						}
					}
					if (!hasLoc)
					{
						verifications.Add(Verification.Neutral(
							$"Paintjob with key {paintjob.textureName} in paintable def {gunDef.name}, has no localised name",
							() =>
							{
								gunDef.LocalisedExtras.Add(new LocalisedExtra()
								{
									Unlocalised = $"paintjob.{resLoc.Namespace}.{paintjob.textureName}",
									Localised = paintjob.textureName.Replace("_", " "),
									Lang = ELang.en_us,
								});
							}));
					}
				}
			}

			if (ExpectsModel())
			{

				string modelPath = $"Assets/Content Packs/{resLoc.Namespace}/models/{GetModelFolder()}/{resLoc.IDWithoutPrefixes()}.prefab";
				RootNode rootNode = AssetDatabase.LoadAssetAtPath<RootNode>(modelPath);
				if (rootNode == null)
					verifications.Add(Verification.Failure(
						$"Definition {name} does not have a matching model at {modelPath}",
						() =>
						{
							rootNode = ConvertToNodes.CreateEmpty(name);
							rootNode.AddDefaultTransforms();
							PrefabUtility.SaveAsPrefabAsset(rootNode.gameObject, modelPath);
							DestroyImmediate(rootNode.gameObject);
						}));
				else
				{
					if (this is GunDefinition gun)
					{
						foreach (NamedTexture iconTexture in rootNode.Icons)
						{
							if (iconTexture.Key != "default")
							{
								bool existsInDef = false;
								foreach (PaintjobDefinition paint in gun.paints.paintjobs)
								{
									if (paint.textureName == iconTexture.Key)
										existsInDef = true;
								}

								if (!existsInDef)
								{
									verifications.Add(Verification.Neutral(
										$"Found icon {iconTexture.Key} in model {modelPath}, not referenced in paintable def {gun.name}",
										() =>
										{
											List<PaintjobDefinition> paintjobs = new List<PaintjobDefinition>(gun.paints.paintjobs);
											paintjobs.Add(new PaintjobDefinition()
											{
												textureName = iconTexture.Key,
												paintBucketsRequired = 1,
											});
											gun.paints.paintjobs = paintjobs.ToArray();
										}));
								}
							}
						}

						for (int i = 0; i < gun.paints.paintjobs.Length; i++)
						{
							PaintjobDefinition paintjob = gun.paints.paintjobs[i];
							bool existsInModel = false;
							foreach (NamedTexture skinTexture in rootNode.Textures)
							{
								if (paintjob.textureName == skinTexture.Key)
									existsInModel = true;
							}
							foreach (NamedTexture iconTexture in rootNode.Icons)
							{
								if (paintjob.textureName == iconTexture.Key)
									existsInModel = true;
							}
							if (!existsInModel)
							{
								int index = i;
								verifications.Add(Verification.Neutral(
										$"Found icon {paintjob.textureName} in paintable def {gun.name}, not referenced in model {rootNode.name}",
										() =>
										{
											List<PaintjobDefinition> paintjobs = new List<PaintjobDefinition>(gun.paints.paintjobs);
											paintjobs.RemoveAt(index);
											gun.paints.paintjobs = paintjobs.ToArray();
										}));
							}
						}
					}

					foreach(ItemPoseNode poseNode in rootNode.GetChildNodes<ItemPoseNode>())
					{
						if (WillRenderIconInPose(poseNode.TransformType))
						{
							// Icon rendering has "suggested defaults"

						}
					}
				}
			}

			ItemDefinition itemSettings = GetItemSettings();
			if (itemSettings != null)
			{
				VerifyItemSettings(itemSettings, verifications);
			}
		}
		catch(Exception e)
		{
			verifications?.Add(Verification.Exception(e, () => { Selection.activeObject = this; }));
		}
	}

	private void VerifyItemSettings(ItemDefinition itemSettings, List<Verification> verifications)
	{
		for (int i = 0; i < itemSettings.tags.Length; i++)
		{
			ResourceLocation tagLoc = itemSettings.tags[i];
			if (tagLoc.Namespace.Length == 0)
			{
				verifications.Add(Verification.Failure($"Tag {tagLoc} has no namespace set"));
			}
			else if (tagLoc.ID.Length == 0)
			{
				verifications.Add(Verification.Failure($"Tag {tagLoc} has no ID set"));
			}
			else
			{
				string prefixes = tagLoc.GetPrefixes();
				if (prefixes.Length == 0)
				{

				}
				else if (prefixes.StartsWith("blocks") || prefixes.StartsWith("items"))
				{
					int index = i;
					verifications.Add(Verification.Failure($"Tag {tagLoc} has an automatically inferred prefix attached",
					() =>
					{
						ResourceLocation innerTagLoc = itemSettings.tags[index];
						itemSettings.tags[index] = new ResourceLocation($"{innerTagLoc.Namespace}:{innerTagLoc.IDWithSpecificPrefixesStripped("blocks", "items")}");
					}));
				}
				else
				{

				}
			}
		}
	}

	public ItemDefinition GetItemSettings()
	{
		if (this is GunDefinition gunDef)
			return gunDef.itemSettings;
		else if (this is BulletDefinition bulletDef)
			return bulletDef.itemSettings;
		else if (this is AttachmentDefinition attachmentDef)
			return attachmentDef.itemSettings;
		else if (this is ArmourDefinition armourDef)
			return armourDef.itemSettings;
		else if (this is GrenadeDefinition grenadeDef)
			return grenadeDef.itemSettings;
		else if (this is ToolDefinition toolDef)
			return toolDef.itemSettings;
		else if (this is VehicleDefinition vehicleDef)
			return vehicleDef.itemSettings;
		else if (this is PartDefinition partDef)
			return partDef.itemSettings;
		else if (this is WorkbenchDefinition workbenchDef)
			return workbenchDef.itemSettings;
		return null;
	}

	public string GetTagExportFolder()
	{
		if (this is WorkbenchDefinition)
			return "blocks";
		return "items";
	}

	public bool IsBlock()
	{
		return this is WorkbenchDefinition;
	}

	public void VerifyModifiers(List<Verification> verifications)
	{
		if (this is GunDefinition gunDef)
			foreach (ActionGroupDefinition actionGroup in gunDef.actionGroups)
				foreach (ModifierDefinition modifier in actionGroup.modifiers)
					VerifyModifier(modifier, verifications);
		else if (this is AttachmentDefinition attachmentDef)
			foreach (ModifierDefinition modifier in attachmentDef.modifiers)
				VerifyModifier(modifier, verifications);
		else if (this is MagazineDefinition magDef)
			foreach (ModifierDefinition modifier in magDef.modifiers)
				VerifyModifier(modifier, verifications);
	}

	public void VerifyModifier(ModifierDefinition modDef, List<Verification> verifications)
	{
		if (!Constants.STAT_SUGGESTIONS.Contains(modDef.stat))
			verifications.Add(Verification.Neutral($"Unknown modifier {modDef.stat}"));
	}

	public bool WillRenderIconInPose(ItemDisplayContext pose)
	{
		if (this is GunDefinition) return pose == ItemDisplayContext.GUI;
		return true;
	}

	public bool ExpectsModel()
	{
		if (this is FlanimationDefinition
		|| this is ClassDefinition
		|| this is LoadoutPoolDefinition
		|| this is MagazineDefinition
		|| this is MaterialDefinition
		|| this is NpcDefinition
		|| this is TeamDefinition
		|| this is CraftingTraitDefinition)
			return false;

		return true;
	}
	public string GetModelFolder()
	{
		if (this is WorkbenchDefinition)
			return "block";
		return "item";
	}

	public enum ELang
	{
		en_us,
		en_gb,
		de_de,
		es_es,
		es_mx,
		fr_fr,
		fr_ca,
		it_it,
		ja_jp,
		ko_kr,
		pt_br,
		pt_pt,
		ru_ru,
		zh_cn,
		zh_tw,
		nl_nl,
		bg_bg,
		cs_cz,
		da_dk,
		el_gr,
		fi_fi,
		hu_hu,
		id_id,
		nb_no,
		pl_pl,
		sk_sk,
		sv_se,
		tr_tr,
		uk_ua,
	}

	public static class Langs
	{
		public static int NUM_LANGS = (int)ELang.uk_ua + 1;
	}

	[System.Serializable]
	public class LocalisedName
	{
		public ELang Lang = ELang.en_us;
		public string Name = "";
	}

	[System.Serializable]
	public class LocalisedExtra
	{
		public string Unlocalised = "";
		public ELang Lang = ELang.en_us;
		public string Localised = "";
	}

	public List<LocalisedName> LocalisedNames = new List<LocalisedName>();
	public List<LocalisedExtra> LocalisedExtras = new List<LocalisedExtra>();
}
