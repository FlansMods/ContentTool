using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public abstract class Definition : ScriptableObject, IVerifiableAsset
{
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

	public bool NeedsName()
	{
		return !(this is FlanimationDefinition);
	}
	public bool ExpectsModel()
	{
		return !(this is FlanimationDefinition
		|| this is ClassDefinition
		|| this is LoadoutPoolDefinition
		|| this is MagazineDefinition
		|| this is MaterialDefinition
		|| this is NpcDefinition
		|| this is TeamDefinition
		|| this is CraftingTraitDefinition);
	}
	public bool IsBlock()
	{
		return this is WorkbenchDefinition;
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

	public bool WillRenderIconInPose(ItemDisplayContext pose)
	{
		if (this is GunDefinition) return pose == ItemDisplayContext.GUI;
		return true;
	}

	public string GetTagExportFolder()
	{
		if (this is WorkbenchDefinition)
			return "blocks";
		return "items";
	}
	public string GetModelFolder()
	{
		if (this is WorkbenchDefinition)
			return "block";
		return "item";
	}

	// -------------------------------------------------------------------------------------------------------------
	#region Verification
	// -------------------------------------------------------------------------------------------------------------
	public virtual void GetVerifications(List<Verification> verifications)
	{
		try
		{
			ResourceLocation resLoc = this.GetLocation();

			VerifyNames(resLoc, this, verifications);
			VerifyItemSettings(resLoc, this, verifications);
			VerifyModel(resLoc, this, verifications);

			if (this is GunDefinition gun)
				VerifyGun(resLoc, gun, verifications);
			else if (this is AttachmentDefinition attachment)
				VerifyAttachment(resLoc, attachment, verifications);
			else if (this is MagazineDefinition mag)
				VerifyMagazine(resLoc, mag, verifications);
		}
		catch (Exception e)
		{
			verifications?.Add(Verification.Exception(e, () => 
			{ 
				Selection.activeObject = this;
				return null;
			}));
		}
	}

	private static void VerifyNames(ResourceLocation resLoc, Definition def, List<Verification> verifications)
	{
		if (def.LocalisedNames.Count < 1 && def.NeedsName())
			verifications.Add(Verification.Failure($"{def.name} has no localised name in any language"));
		if (def.name != Utils.ToLowerWithUnderscores(def.name))
		{
			verifications.Add(Verification.Failure(
				$"Definition {def.name} does not have a Minecraft-compliant name",
				() =>
				{
					def.name = Utils.ToLowerWithUnderscores(def.name);
					return def;
				})
			);
		}
	}

	private static void VerifyAttachment(ResourceLocation resLoc, AttachmentDefinition attachment, List<Verification> verifications)
	{
		foreach (ActionGroupDefinition group in attachment.actionOverrides)
			VerifyActionGroup(resLoc, attachment, group, verifications);

		foreach(ModifierDefinition mod in attachment.modifiers)
			VerifyModifier(resLoc, mod, verifications);
	}

	private static void VerifyMagazine(ResourceLocation resLoc, MagazineDefinition mag, List<Verification> verifications)
	{
		foreach (ModifierDefinition mod in mag.modifiers)
			VerifyModifier(resLoc, mod, verifications);
	}

	private static void VerifyGun(ResourceLocation resLoc, GunDefinition gunDef, List<Verification> verifications)
	{
		foreach (ActionGroupDefinition group in gunDef.actionGroups)
		{
			VerifyActionGroup(resLoc, gunDef, group, verifications);
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
					return gunDef;
				}));
			}
		}
	}

	private static void VerifyModel(ResourceLocation resLoc, Definition def, List<Verification> verifications)
	{
		if (def.ExpectsModel())
		{

			string modelPath = $"Assets/Content Packs/{resLoc.Namespace}/models/{def.GetModelFolder()}/{resLoc.IDWithoutPrefixes()}.prefab";
			RootNode rootNode = AssetDatabase.LoadAssetAtPath<RootNode>(modelPath);
			if (rootNode == null)
				verifications.Add(Verification.Failure(
					$"Definition {def.name} does not have a matching model at {modelPath}",
					() =>
					{
						rootNode = ConvertToNodes.CreateEmpty(def.name);
						rootNode.AddDefaultTransforms();
						PrefabUtility.SaveAsPrefabAsset(rootNode.gameObject, modelPath);
						DestroyImmediate(rootNode.gameObject);
						return def;
					}));
			else
			{
				if (def is GunDefinition gun)
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
										return def;
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
										return def;
									}));
						}
					}
				}

				foreach (ItemPoseNode poseNode in rootNode.GetChildNodes<ItemPoseNode>())
				{
					if (def.WillRenderIconInPose(poseNode.TransformType))
					{
						// Icon rendering has "suggested defaults"

					}
				}
			}
		}
	}

	private static void VerifyItemSettings(ResourceLocation resLoc, Definition def, List<Verification> verifications)
	{
		ItemDefinition itemSettings = def.GetItemSettings();
		if (itemSettings != null)
		{
			for (int i = 0; i < itemSettings.tags.Length; i++)
			{
				int index = i;
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
						
						verifications.Add(Verification.Failure($"Tag {tagLoc} has an automatically inferred prefix attached",
						() =>
						{
							ResourceLocation innerTagLoc = itemSettings.tags[index];
							itemSettings.tags[index] = new ResourceLocation($"{innerTagLoc.Namespace}:{innerTagLoc.IDWithSpecificPrefixesStripped("blocks", "items")}");
							return def;
						}));
					}
					else if(prefixes == "ingot")
					{
						verifications.Add(Verification.Failure($"Tag {tagLoc} uses 'ingot' instead of Forge tag 'ingots'",
						() => 
						{
							ResourceLocation innerTagLoc = itemSettings.tags[index];
							itemSettings.tags[index] = new ResourceLocation($"{innerTagLoc.Namespace}:{innerTagLoc.ID.Replace("ingot", "ingots")}");
							return def;
						}));
					}
					else if (prefixes == "nugget")
					{
						verifications.Add(Verification.Failure($"Tag {tagLoc} uses 'nugget' instead of Forge tag 'nuggets'",
						() =>
						{
							ResourceLocation innerTagLoc = itemSettings.tags[index];
							itemSettings.tags[index] = new ResourceLocation($"{innerTagLoc.Namespace}:{innerTagLoc.ID.Replace("nugget", "nuggets")}");
							return def;
						}));
					}
					else if (prefixes == "plate")
					{
						verifications.Add(Verification.Failure($"Tag {tagLoc} uses 'plate' instead of Forge tag 'plates'",
						() =>
						{
							ResourceLocation innerTagLoc = itemSettings.tags[index];
							itemSettings.tags[index] = new ResourceLocation($"{innerTagLoc.Namespace}:{innerTagLoc.ID.Replace("plate", "plates")}");
							return def;
						}));
					}
					else if (prefixes == "storage_block")
					{
						verifications.Add(Verification.Failure($"Tag {tagLoc} uses 'storage_block' instead of Forge tag 'storage_blocks'",
						() =>
						{
							ResourceLocation innerTagLoc = itemSettings.tags[index];
							itemSettings.tags[index] = new ResourceLocation($"{innerTagLoc.Namespace}:{innerTagLoc.ID.Replace("storage_block", "storage_blocks")}");
							return def;
						}));
					}
				}
			}
		}
	}
	public static void VerifyActionGroup(ResourceLocation resLoc, Definition def, ActionGroupDefinition actionGroup, List<Verification> verifications)
	{	
		bool hasADSAction = false;
		Dictionary<string, Func<ModifierDefinition>> expected = new Dictionary<string, Func<ModifierDefinition>>();
		foreach(ActionDefinition action in actionGroup.actions)
		{
			switch (action.actionType)
			{
				case EActionType.AimDownSights:
				case EActionType.Scope:
					hasADSAction = true;
					expected.Add(Constants.STAT_ZOOM_FOV_FACTOR, () => { return new ModifierDefinition() {
						stat = Constants.STAT_ZOOM_FOV_FACTOR,
						accumulators = new StatAccumulatorDefinition[] { new StatAccumulatorDefinition() {
							value = 1.25f,
						} } }; });
					break;
			}
		}
		if (hasADSAction && actionGroup.repeatMode != ERepeatMode.Toggle)
		{
			verifications.Add(Verification.Neutral(
				$"Aim down sights action is not a toggle",
				() =>
				{
					actionGroup.repeatMode = ERepeatMode.Toggle;
					return def;
				})
			);
		}

		foreach (ModifierDefinition modifier in actionGroup.modifiers)
		{
			VerifyModifier(resLoc, modifier, verifications);
			if (expected.ContainsKey(modifier.stat))
				expected.Remove(modifier.stat);
		}

		foreach(var kvp in expected)
		{
			verifications.Add(Verification.Neutral($"Expected an entry for {kvp.Key} stat in ActionGroup {actionGroup.key}", () => {
				List<ModifierDefinition> list = new List<ModifierDefinition>(actionGroup.modifiers);
				list.Add(kvp.Value.Invoke());
				actionGroup.modifiers = list.ToArray();
				return def;
			}));
		}
	}
	public static void VerifyModifier(ResourceLocation resLoc, ModifierDefinition modDef, List<Verification> verifications)
	{
		if (!Constants.STAT_SUGGESTIONS.Contains(modDef.stat))
			verifications.Add(Verification.Neutral($"{resLoc} has unknown modifier {modDef.stat}"));
	}
	#endregion
	// -------------------------------------------------------------------------------------------------------------
}
