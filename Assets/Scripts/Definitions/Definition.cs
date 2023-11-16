using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Definition : ScriptableObject, IVerifiableAsset
{
	public virtual void GetVerifications(List<Verification> verifications)
	{
		if (LocalisedNames.Count < 1 && this is not AnimationDefinition)
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

		if(this is GunDefinition gunDef)
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
		}

		if(ExpectsModel())
		{
			ResourceLocation resLoc = this.GetLocation();
			string modelPath = $"Assets/Content Packs/{resLoc.Namespace}/models/{GetModelFolder()}/{resLoc.IDWithoutPrefixes()}.asset";
			MinecraftModel mcModel = AssetDatabase.LoadAssetAtPath<MinecraftModel>(modelPath);
			if (mcModel == null)
				verifications.Add(Verification.Failure(
					$"Definition {name} does not have a matching model at {modelPath}",
					() => {
						TurboRig turboRig = CreateInstance<TurboRig>();
						turboRig.name = $"{resLoc.IDWithoutPrefixes()}";
						turboRig.AddDefaultTransforms();
						AssetDatabase.CreateAsset(turboRig, modelPath);
					}));
			else
			{
				if(this is GunDefinition gun && mcModel is TurboRig rig)
				{
					foreach(MinecraftModel.NamedTexture iconTexture in rig.Icons)
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
						foreach (MinecraftModel.NamedTexture skinTexture in rig.Textures)
						{
							if (paintjob.textureName == skinTexture.Key)
								existsInModel = true;
						}
						foreach (MinecraftModel.NamedTexture iconTexture in rig.Icons)
						{
							if (paintjob.textureName == iconTexture.Key)
								existsInModel = true;
						}
						if (!existsInModel)
						{
							int index = i;
							verifications.Add(Verification.Neutral(
									$"Found icon {paintjob.textureName} in paintable def {gun.name}, not referenced in model {rig.name}",
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
	}

	public bool ExpectsModel()
	{
		if (this is AnimationDefinition
		|| this is ClassDefinition
		|| this is LoadoutPoolDefinition
		|| this is MagazineDefinition
		|| this is MaterialDefinition
		|| this is NpcDefinition
		|| this is TeamDefinition)
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
