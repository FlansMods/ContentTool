using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

	public static Texture2D TickTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/tick.png");
	public static Texture2D NeutralTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/neutral.png");
	public static Texture2D CrossTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/cross.png");
	public static bool VerificationsBox(IVerifiableAsset asset)
	{
		List<Verification> verifications = new List<Verification>();
		asset.GetVerifications(verifications);
		return VerificationsBox(verifications);
	}

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

	
	public static bool VerificationsBox(Dictionary<Object, List<Verification>> multiVerifications)
	{
		bool allSucceeded = true;
		bool pressedAnyQuickFix = false;
		Internal_VerificationsHeader();
		foreach (var kvp in multiVerifications)
		{
			Internal_VerificationsBox(kvp.Value, 
				out bool instanceSuccess, 
				out bool instancePressedAny, 
				() => { EditorGUILayout.ObjectField(kvp.Key, kvp.Key.GetType(), false, GUILayout.Width(128)); }
			);
			if (!instanceSuccess)
				allSucceeded = false;
			if (instancePressedAny)
				pressedAnyQuickFix = true;
		}
		Internal_VerificationsSummary(allSucceeded);
		return pressedAnyQuickFix;
	}

	public static void VerificationIcon(List<Verification> verifications)
	{
		VerifyType type = Verification.GetWorstState(verifications);
		switch(type)
		{
			case VerifyType.Pass: GUILayout.Label(TickTexture, GUILayout.Width(16)); break;
			case VerifyType.Neutral: GUILayout.Label(NeutralTexture, GUILayout.Width(16)); break;
			case VerifyType.Fail: GUILayout.Label(CrossTexture, GUILayout.Width(16)); break;
		}
	}

	private static void Internal_VerificationsHeader()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Filters:");
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
				case VerifyType.Pass: GUILayout.Label(TickTexture, GUILayout.Width(16)); break;
				case VerifyType.Neutral: GUILayout.Label(NeutralTexture, GUILayout.Width(16)); break;
				case VerifyType.Fail:
					{
						
						GUILayout.Label(CrossTexture, GUILayout.Width(16));
						break;
					}
			}
			GUILayout.Label(verification.Message);
			if (verification.Func != null)
			{
				if (GUILayout.Button("Quick-Fix", GUILayout.Width(96)))
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
		GUILayout.Label(pass ? TickTexture : CrossTexture, GUILayout.Width(16));
		GUILayout.Label(pass ? "All verifications passed." : "Verification issues!");
		GUILayout.EndHorizontal();
	}

}
