using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public enum VerifyType
{
	Pass,
	Neutral,
	Fail,
}

[System.Serializable]
public class Verification
{
	public delegate void QuickFixFunc();
	public QuickFixFunc Func = null;
    public string Message;
    public VerifyType Type;

	public static Verification Success(string msg, QuickFixFunc func = null)
	{
		return new Verification()
		{
			Type = VerifyType.Pass,
			Func = func,
			Message = msg,
		};
	}
	public static Verification Neutral(string msg, QuickFixFunc func = null)
	{
		return new Verification()
		{
			Type = VerifyType.Neutral,
			Func = func,
			Message = msg
		};
	}
	public static Verification Failure(string msg, QuickFixFunc func = null)
	{
		return new Verification()
		{
			Type = VerifyType.Fail,
			Func = func,
			Message = msg
		};
	}
	public static Verification Exception(Exception exception, string msg, QuickFixFunc func = null)
	{
		return new Verification()
		{
			Type = VerifyType.Fail,
			Func = func,
			Message = $"'{msg}' due to Exception[{exception.Message} @ {exception.StackTrace}]",
		};
	}
	public static Verification Exception(Exception exception, QuickFixFunc func = null)
	{
		return new Verification()
		{
			Type = VerifyType.Fail,
			Func = func,
			Message = $"'{exception.Message}' @ {exception.StackTrace}"
		};
	}

	public static VerifyType GetWorstState(Dictionary<UnityEngine.Object, List<Verification>> multiVerify)
	{
		VerifyType result = VerifyType.Pass;
		foreach (var kvp in multiVerify)
		{
			foreach (Verification v in kvp.Value)
			{
				if (v.Type == VerifyType.Fail)
					return VerifyType.Fail;
				if (v.Type == VerifyType.Neutral)
					result = VerifyType.Neutral;
			}
		}
		return result;
	}

	public static VerifyType GetWorstState(List<Verification> verifications)
	{
		VerifyType result = VerifyType.Pass;
		foreach (Verification v in verifications)
		{
			if (v.Type == VerifyType.Fail)
				return VerifyType.Fail;
			if (v.Type == VerifyType.Neutral)
				result = VerifyType.Neutral;
		}
		return result;
	}

	public static int CountSuccesses(List<Verification> verifications) { return CountType(verifications, VerifyType.Pass); }
	public static int CountNeutrals(List<Verification> verifications) { return CountType(verifications, VerifyType.Neutral); }
	public static int CountFailures(List<Verification> verifications) { return CountType(verifications, VerifyType.Fail); }

	public static int CountType(List<Verification> verifications, VerifyType type)
	{
		int count = 0;
		foreach (Verification v in verifications)
			if (v.Type == type)
				count++;
		return count;
	}

	public static int CountQuickFixes(List<Verification> verifications)
	{
		int count = 0;
		foreach (Verification v in verifications)
			if (v.Func != null)
				count++;
		return count;
	}

	public static void ApplyAllQuickFixes(List<Verification> verifications)
	{
		foreach (Verification v in verifications)
			if (v.Func != null)
				v.Func();
	}
	public static int CountQuickFixes(Dictionary<UnityEngine.Object, List<Verification>> multiVerify)
	{
		int count = 0;
		foreach(var kvp in multiVerify)
			foreach (Verification v in kvp.Value)
				if (v.Func != null)
					count++;
		return count;
	}

	public static void ApplyAllQuickFixes(Dictionary<UnityEngine.Object, List<Verification>> multiVerify)
	{
		foreach (var kvp in multiVerify)
			foreach (Verification v in kvp.Value)
				if (v.Func != null)
					v.Func();
	}

	public override string ToString()
	{
		return $"[{Type}] {Message}";
	}
}

public interface IVerifiableContainer
{
	UnityEngine.Object GetUnityObject();
	IEnumerable<IVerifiableAsset> GetAssets();
}

public interface IVerifiableAsset
{
	void GetVerifications(List<Verification> verifications);
}

public static class GUIVerify
{
	private static bool ShowSuccess = true;
	private static bool ShowNeutral = true;
	private static bool ShowFail = true;

	public static Texture2D TickTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Resources/tick.png");
	public static Texture2D NeutralTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Resources/neutral.png");
	public static Texture2D CrossTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Resources/cross.png");

	public static GUILayoutOption[] OBJECT_FIELD_OPTIONS = new GUILayoutOption[] { GUILayout.Width(128) };
	public static GUILayoutOption[] STATUS_ICON_OPTIONS = new GUILayoutOption[] { GUILayout.Width(24) };
	public static GUILayoutOption[] DESCRIPTION_OPTIONS { get { return new GUILayoutOption[] { GUILayout.MaxWidth(Screen.width - 128 - 16 - 96 - 8) }; } }
	public static GUILayoutOption[] QUICK_FIX_BUTTON_OPTIONS = new GUILayoutOption[] { GUILayout.Width(96) };

	private static Dictionary<IVerifiableAsset, List<Verification>> CachedVerifications = new Dictionary<IVerifiableAsset, List<Verification>>();
	private static Dictionary<IVerifiableContainer, List<IVerifiableAsset>> CachedContainers = new Dictionary<IVerifiableContainer, List<IVerifiableAsset>>();
	private static List<Verification> GetCachedVerifications(IVerifiableAsset asset, bool forceRefresh)
	{
		if (asset == null)
			return new List<Verification>();

		if (forceRefresh || !CachedVerifications.TryGetValue(asset, out List<Verification> verifications))
		{
			verifications = new List<Verification>();
			asset.GetVerifications(verifications);
			CachedVerifications.Add(asset, verifications);
		}
		return verifications;
	}
	private static List<IVerifiableAsset> GetCachedAssetList(IVerifiableContainer container, bool forceRefresh)
	{
		if (container == null)
			return new List<IVerifiableAsset>();

		if (forceRefresh || !CachedContainers.TryGetValue(container, out List<IVerifiableAsset> assets))
		{
			assets = new List<IVerifiableAsset>();
			foreach (IVerifiableAsset asset in container.GetAssets())
				assets.Add(asset);
			CachedContainers.Add(container, assets);
		}
		return assets;
	}
	private static List<Verification> GetCachedVerifications(IVerifiableContainer container, bool forceRefresh)
	{
		List<Verification> verifications = new List<Verification>();
		foreach (IVerifiableAsset asset in GetCachedAssetList(container, forceRefresh))
			verifications.AddRange(GetCachedVerifications(asset, forceRefresh));
		if (container is IVerifiableAsset containerAsset)
			verifications.AddRange(GetCachedVerifications(containerAsset, forceRefresh));
		return verifications;
	}
	public static void InvalidateCaches()
	{
		CachedVerifications.Clear();
		CachedContainers.Clear();
	}

	// ----------------------------------------------------------------------------------------------
	#region Cached Verification Boxes - Much faster
	// ----------------------------------------------------------------------------------------------
	// For IVerifiableContainers
	public static bool CachedVerificationHeader(IVerifiableContainer container, bool forceRefresh = false)
	{
		return VerificationHeader(GetCachedVerifications(container, forceRefresh));		
	}
	public static bool CachedVerificationsBox(IVerifiableContainer container, bool forceRefresh = false)
	{
		Dictionary<UnityEngine.Object, List<Verification>> multiVerify = new Dictionary<UnityEngine.Object, List<Verification>>();
		foreach (IVerifiableAsset asset in GetCachedAssetList(container, forceRefresh))
		{
			if (asset is UnityEngine.Object childUnityObject)
				multiVerify[childUnityObject] = GetCachedVerifications(asset, forceRefresh);
			else
				multiVerify[container.GetUnityObject()] = GetCachedVerifications(asset, forceRefresh);
		}

		return VerificationsBox(multiVerify);
	}
	public static void CachedApplyQuickFixes(IVerifiableContainer container, bool forceRefresh = false)
	{
		Verification.ApplyAllQuickFixes(GetCachedVerifications(container, forceRefresh));
	}
	public static int CachedCountQuickFixes(IVerifiableContainer container, bool forceRefresh = false)
	{
		return Verification.CountQuickFixes(GetCachedVerifications(container, forceRefresh));
	}

	// For a single IVerifiableAsset
	public static bool CachedVerificationsBoxAsset(IVerifiableAsset asset, bool forceRefresh = false)
	{
		return VerificationsBox(GetCachedVerifications(asset, forceRefresh));
	}
	public static void CachedVerificationsIcon(IVerifiableAsset asset, bool forceRefresh = false)
	{
		VerificationIcon(GetCachedVerifications(asset, forceRefresh));
	}
	public static void CachedApplyQuickFixes(IVerifiableAsset asset, bool forceRefresh = false)
	{
		Verification.ApplyAllQuickFixes(GetCachedVerifications(asset, forceRefresh));
	}
	public static int CachedCountQuickFixes(IVerifiableAsset asset, bool forceRefresh = false)
	{
		return Verification.CountQuickFixes(GetCachedVerifications(asset, forceRefresh));
	}
	#endregion
	// ----------------------------------------------------------------------------------------------

	// ----------------------------------------------------------------------------------------------
	#region Directly draw a verification box with a list of verifications
	// ----------------------------------------------------------------------------------------------
	// An in-line header, best for using on the same line as a foldout
	public static bool VerificationHeader(List<Verification> verifications)
	{
		VerifyType verificationSummary = Verification.GetWorstState(verifications);
		int failCount = Verification.CountFailures(verifications);
		int quickFixCount = Verification.CountQuickFixes(verifications);

		if (quickFixCount > 0)
		{
			GUILayout.Label($"{quickFixCount} Quick-Fixes Available");
			if (GUILayout.Button("Apply"))
			{
				Verification.ApplyAllQuickFixes(verifications);
			}
		}

		if (failCount > 0)
		{
			GUILayout.Label($"{failCount} Errors");
			VerificationIcon(verifications);
			return false;
		}
		else
		{
			VerificationIcon(verifications);
			return true;
		}
	}
	public static bool VerificationsResultsPanel(bool expanded, string label,List<Verification> verifications)
	{
		FlanStyles.ThinLine();
		FlanStyles.BigHeader($"Results ({label})");
		GUILayout.BeginHorizontal();
		expanded = EditorGUILayout.Foldout(expanded, $"{Verification.GetWorstState(verifications)}");
		GUILayout.FlexibleSpace();
		VerificationHeader(verifications);
		GUILayout.EndHorizontal();
		if (expanded)
		{
			VerificationsBox(verifications);
		}
		FlanStyles.ThinLine();
		return expanded;
	}

	// Deprecated
	public static bool VerificationsBox(IVerifiableAsset asset, List<Verification> verifications)
	{
		asset.GetVerifications(verifications);
		return VerificationsBox(verifications);
	}
	public static bool VerificationsBox(List<Verification> verifications)
	{
		bool noFails = true;
		bool pressedAnyQuickFix = false;
		Internal_VerificationsHeader();
		Internal_VerificationsBox(verifications, out noFails, out pressedAnyQuickFix);
		Internal_VerificationsSummary(noFails);
		return pressedAnyQuickFix;
	}
	public static bool VerificationsBox(Dictionary<UnityEngine.Object, List<Verification>> multiVerifications)
	{
		bool allSucceeded = true;
		bool pressedAnyQuickFix = false;
		Internal_VerificationsHeader();
		foreach (var kvp in multiVerifications)
		{
			Internal_VerificationsBox(kvp.Value, 
				out bool instanceSuccess, 
				out bool instancePressedAny, 
				() => { EditorGUILayout.ObjectField(kvp.Key, kvp.Key.GetType(), false, OBJECT_FIELD_OPTIONS); }
			);
			if (!instanceSuccess)
				allSucceeded = false;
			if (instancePressedAny)
				pressedAnyQuickFix = true;
		}
		Internal_VerificationsSummary(allSucceeded);
		return pressedAnyQuickFix;
	}
	public static void VerificationIcon(VerifyType type)
	{
		switch (type)
		{
			case VerifyType.Pass: GUILayout.Label(TickTexture, STATUS_ICON_OPTIONS); break;
			case VerifyType.Neutral: GUILayout.Label(NeutralTexture, STATUS_ICON_OPTIONS); break;
			case VerifyType.Fail: GUILayout.Label(CrossTexture, STATUS_ICON_OPTIONS); break;
		}
	}
	public static void VerificationIcon(List<Verification> verifications)
	{
		VerifyType type = Verification.GetWorstState(verifications);
		int quickFixes = Verification.CountQuickFixes(verifications);
		Texture2D tex = TickTexture;
		switch (type)
		{
			case VerifyType.Pass: tex = TickTexture; break;
			case VerifyType.Neutral: tex = NeutralTexture; break;
			case VerifyType.Fail: tex = CrossTexture; break;
		}
		StringBuilder sb = new StringBuilder();
		foreach (Verification v in verifications)
		{
			sb.AppendLine(v.Message);
		}
		if (quickFixes > 0)
		{
			if (GUILayout.Button(new GUIContent(tex).WithTooltip(sb.ToString()), STATUS_ICON_OPTIONS))
				Verification.ApplyAllQuickFixes(verifications);
		}
		else
		{
			GUILayout.Label(new GUIContent(tex).WithTooltip(sb.ToString()), STATUS_ICON_OPTIONS);
		}
	}
	public static void VerificationIcon(Dictionary<UnityEngine.Object, List<Verification>> multiVerify)
	{
		VerifyType type = Verification.GetWorstState(multiVerify);
		int quickFixes = Verification.CountQuickFixes(multiVerify);
		Texture2D tex = TickTexture;
		switch (type)
		{
			case VerifyType.Pass: tex = TickTexture; break;
			case VerifyType.Neutral: tex = NeutralTexture; break;
			case VerifyType.Fail: tex = CrossTexture; break;
		}
		if (quickFixes > 0)
		{
			if (GUILayout.Button(tex, STATUS_ICON_OPTIONS))
				Verification.ApplyAllQuickFixes(multiVerify);
		}
		else
		{
			GUILayout.Label(tex, STATUS_ICON_OPTIONS);
		}
	}
	#endregion
	// ----------------------------------------------------------------------------------------------







	private static void Internal_VerificationsHeader()
	{
		FlanStyles.ThinLine();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Verification Summary", FlanStyles.BoldLabel);
		ShowSuccess = GUILayout.Toggle(ShowSuccess, TickTexture);
		ShowNeutral = GUILayout.Toggle(ShowNeutral, NeutralTexture);
		ShowFail = GUILayout.Toggle(ShowFail, CrossTexture); 
		GUILayout.EndHorizontal();
	}

	private delegate void PrefixGUIFunc();
	private static void Internal_VerificationsBox(List<Verification> verifications, out bool noFails, out bool pressedAnyQuickFix, PrefixGUIFunc prefixFunc = null)
	{
		noFails = true;
		pressedAnyQuickFix = false;
		int numSkipped = 0;
		foreach (Verification verification in verifications)
		{
			if(verification.Type == VerifyType.Fail)
				noFails = false;

			if ((!ShowSuccess && verification.Type == VerifyType.Pass)
			|| (!ShowNeutral && verification.Type == VerifyType.Neutral)
			|| (!ShowFail && verification.Type == VerifyType.Fail))
			{
				numSkipped++;
				continue;
			}

			GUILayout.BeginHorizontal();
			if(prefixFunc != null)
			{
				prefixFunc.Invoke();
			}
			switch (verification.Type)
			{
				case VerifyType.Pass: GUILayout.Label(TickTexture, STATUS_ICON_OPTIONS); break;
				case VerifyType.Neutral: GUILayout.Label(NeutralTexture, STATUS_ICON_OPTIONS); break;
				case VerifyType.Fail:
					{
						
						GUILayout.Label(CrossTexture, STATUS_ICON_OPTIONS);
						break;
					}
			}
			GUILayout.Label(verification.Message, DESCRIPTION_OPTIONS);
			if (verification.Func != null)
			{
				if (GUILayout.Button("Quick-Fix", QUICK_FIX_BUTTON_OPTIONS))
				{
					verification.Func.Invoke();
					pressedAnyQuickFix = true;
				}
			}
			GUILayout.EndHorizontal();
		}
	}
	private static void Internal_VerificationsSummary(bool pass)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(pass ? TickTexture : CrossTexture, STATUS_ICON_OPTIONS);
		GUILayout.Label(pass ? "All verifications passed." : "Verification issues!", DESCRIPTION_OPTIONS);
		GUILayout.EndHorizontal();
		FlanStyles.ThinLine();
	}

}
