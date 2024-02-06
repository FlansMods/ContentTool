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
						() => {
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

		if(ExpectsModel())
		{
			
			string modelPath = $"Assets/Content Packs/{resLoc.Namespace}/models/{GetModelFolder()}/{resLoc.IDWithoutPrefixes()}.prefab";
			RootNode rootNode = AssetDatabase.LoadAssetAtPath<RootNode>(modelPath);
			if (rootNode == null)
				verifications.Add(Verification.Failure(
					$"Definition {name} does not have a matching model at {modelPath}",
					() => {
						rootNode = ConvertToNodes.CreateEmpty(name);
						rootNode.AddDefaultTransforms();
						PrefabUtility.SaveAsPrefabAsset(rootNode.gameObject, modelPath);
						DestroyImmediate(rootNode.gameObject);
					}));
			else
			{
				if(this is GunDefinition gun)
				{
					foreach(NamedTexture iconTexture in rootNode.Icons)
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
									() => {
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

					for(int i = 0; i < gun.paints.paintjobs.Length; i++)
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
									() => {
										List<PaintjobDefinition> paintjobs = new List<PaintjobDefinition>(gun.paints.paintjobs);
										paintjobs.RemoveAt(index);
										gun.paints.paintjobs = paintjobs.ToArray();
									}));
						}
					}
				}
			}
		}

		ItemDefinition itemSettings = GetItemSettings();
		if(itemSettings != null)
		{
			VerifyItemSettings(itemSettings, verifications);
		}
	}

	private void VerifyItemSettings(ItemDefinition itemSettings, List<Verification> verifications)
	{
		for (int i = 0; i < itemSettings.tags.Length; i++)
		{
			ResourceLocation tagLoc = new ResourceLocation(itemSettings.tags[i]);
			string prefixes = tagLoc.GetPrefixes();
			if(prefixes.Length == 0)
			{
				//int index = i;
				//verifications.Add(Verification.Failure($"Tag {tagLoc} is in the root tag folder?",
				//() =>
				//{
				//	ResourceLocation innerTagLoc = new ResourceLocation(itemSettings.tags[index]);
				//	string newLoc = innerTagLoc.ResolveWithSubdir(this.IsBlock() ? "blocks" : "items");
				//	itemSettings.tags[index] = newLoc;
				//}));
			}
			else if(prefixes.StartsWith("blocks") || prefixes.StartsWith("items"))
			{
				int index = i;
				verifications.Add(Verification.Failure($"Tag {tagLoc} has an automatically inferred prefix attached",
				() =>
				{
					ResourceLocation innerTagLoc = new ResourceLocation(itemSettings.tags[index]);
					string newLoc = $"{innerTagLoc.Namespace}:{innerTagLoc.IDWithSpecificPrefixesStripped("blocks", "items")}";
					itemSettings.tags[index] = newLoc;
				}));
			}
			else
			{
				
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
				foreach (ActionDefinition action in actionGroup.actions)
					foreach (ModifierDefinition modifier in action.modifiers)
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
		if (!ValidModifierKeys.Contains(modDef.Stat))
			verifications.Add(Verification.Neutral($"Unknown modifier {modDef.Stat}"));
	}

	private static List<string> ValidModifierKeys = new List<string>()
	{
		"repeat_mode",
		"repeat_delay",
		"repeat_count",
		"spin_up_duration",
		"loudness",
		"spread",
		"vertical_recoil",
		"horizontal_recoil",
		"speed",
		"bullet_count",
		"penetration_power",
		"spread_pattern",
		"impact_damage",
		"potion_effect_on_target",
		"knockback",
		"multiplier_vs_players",
		"multiplier_vs_vehicles",
		"splash_damage",
		"splash_damage_radius",
		"splash_damage_falloff",
		"potion_effect_on_splash",
		"set_fire_to_target",
		"fire_spread_radius",
		"fire_spread_amount",
		"explosion_radius",
		"melee_damage",
		"reach",
		"tool_level",
		"harvest_speed",
		"fov_factor",
		"scope_overlay",
		"anim",
		"block_id",
		"duration",
		"heal_amount",
		"feed_amount",
		"feed_saturation",
		"pitch",
		"flashlight_strength",
		"flashlight_range",
		"entity_tag",
		"entity_id",
		"action_key",
	};

	public bool ExpectsModel()
	{
		if (this is FlanimationDefinition
		|| this is ClassDefinition
		|| this is LoadoutPoolDefinition
		|| this is MagazineDefinition
		|| this is MaterialDefinition
		|| this is NpcDefinition
		|| this is TeamDefinition
		|| this is AbilityDefinition)
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
