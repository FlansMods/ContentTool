using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class Utils
{
        public static int[] ParseInts(int numInts, string text)
    {
        int[] ret = new int[numInts];
        int current = 0;
        bool bNeg = false;

        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == ',')
            {
                if (bNeg)
                {
                    ret[current] = -ret[current];
                    bNeg = false;
                }
                current++;
                if (current >= numInts)
                {
                    return ret;
                }
            }
            else if (chars[i] == '-')
            {
                bNeg = true;
            }
            else if (chars[i] >= '0' && chars[i] <= '9')
            {
                int iVal = chars[i] - '0';
                ret[current] = ret[current] * 10 + iVal;
            }
            // Skip ALL unsupported chars!
            //else if (chars[i] == ' ') continue;
        }

        Debug.Assert(current == numInts - 1, $"Failed to parse {numInts} ints from {text}");
        return ret;
    }

    public static float[] ParseFloats(int numFloats, string text)
    {
		text = text.Replace("(float)", "");
		text = text.Replace("Math.PI", "3.14159265f");

        float[] ret = new float[numFloats];
        int current = 0;
        bool bNeg = false;
        bool bCommentMode = false;
        int iDecimalPlace = 0;
        float fNumerator = 0.0f;
        bool bDivideMode = false;

        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            /* Comment mode */
            if (chars[i] == '/' && chars[i + 1] == '*')
            {
                bCommentMode = true;
                continue;
            }
            if (bCommentMode && (chars[i] == '/' && chars[i - 1] == '*'))
            {
                bCommentMode = false;
                continue;
            }
            if (bCommentMode) continue;
            /* Go no further */

            if (chars[i] == ';')
            {
                if (bNeg)
                {
                    ret[current] = -ret[current];
                }
                if (bDivideMode)
                {
                    ret[current] = fNumerator / ret[current];
                }
                return ret;
            }
            else if (chars[i] == ',')
            {
                if (bNeg)
                {
                    ret[current] = -ret[current];
                }
                if(bDivideMode)
                {
                    ret[current] = fNumerator / ret[current];
                }
                // Reset
                {
                    bNeg = false; iDecimalPlace = 0;
                    bDivideMode = false;
                }
                current++; // Move to next
                if (current >= numFloats) // Return if done
                {
                    return ret;
                }
            }
            else if (chars[i] == '-')
            {
                bNeg = true;
            }
            else if (chars[i] == '.')
            {
                iDecimalPlace = 1;
            }
            else if (chars[i] >= '0' && chars[i] <= '9')
            {
                int iVal = chars[i] - '0';
                if (iDecimalPlace > 0)
                {
                    ret[current] += Mathf.Pow(0.1f, iDecimalPlace) * iVal;
                    iDecimalPlace++;
                }
                else ret[current] = ret[current] * 10.0f + iVal;
            }
            else if(chars[i] == '/')
            {
                fNumerator = ret[current];
                ret[current] = 0.0f;
                bDivideMode = true;
                iDecimalPlace = 0;
            }
            // Skip ALL unsupported chars!
            //else if (chars[i] == ' ') continue;
        }

        Debug.Assert(current == numFloats - 1, "Failed to parse ints");
        return ret;
    }

	public static string[] ToLowerWithUnderscores(string[] names)
	{
		string[] results = new string[names.Length];
		for(int i = 0; i < names.Length; i++)
		{
			results[i] = ToLowerWithUnderscores(names[i]);
		}
		return results;
 	}

	public static string ToLowerWithUnderscores(string shortName)
	{
		if(shortName == null ||shortName.Length == 0)
			return "";
        return System.Text.RegularExpressions.Regex.Replace(shortName, "([a-z])([A-Z])", "$1_$2").ToLower();
	}
	public static string ConvertPartName(string partName)
	{
        if (partName.EndsWith("Model"))
            partName = partName.Substring(0, partName.Length - 5);
		else if (partName.EndsWith("Models"))
			partName = partName.Substring(0, partName.Length - 6);

		switch (partName)
		{
			case "gun": return "body";
			case "pump": return "pump";
			case "ammo": return "ammo_0";
			case "defaultGrip": return "grip";
			case "defaultBarrel": return "barrel";
			case "defaultScope": return "sights";
			case "defaultStock": return "stock";
			case "revolverBarrel": return "revolver";
			case "breakAction": return "break_action";
			default: return partName;
		}
	}

    private static Regex ResourceLocator = new Regex(".*Assets\\/Content Packs\\/([a-zA-Z0-9]*)\\/([a-zA-Z0-9_\\/]*)\\.([a-z]*)");
    public static ResourceLocation GetLocation(this Object asset)
    {
        if(!TryGetLocation(asset, out ResourceLocation location))
        {
			Debug.LogWarning($"Could not resolve the path to {asset} as a resource location");
		}
		return location;
	}
    public static bool TryGetLocation(this Object asset, out ResourceLocation location)
    {
		string path = AssetDatabase.GetAssetPath(asset);
		Match match = ResourceLocator.Match(path);
		if (match.Success)
		{
			string modName = match.Groups[1].Value;
			string assetName = match.Groups[2].Value;
			location = new ResourceLocation(modName, assetName);
            return true;
		}
		location = new ResourceLocation("minecraft", "null");
        return false;
	}

    public static Transform FindRecursive(this Transform t, string name)
    {
		if (t.name == name)
			return t;
		foreach (Transform child in t)
        {
            Transform childSearch = child.FindRecursive(name);
            if (childSearch != null)
                return childSearch;
        }
        return null;
    }
    public static bool Approximately(this Vector3 a, Vector3 b)
    {
        return Mathf.Approximately((a - b).sqrMagnitude, 0f);
    }
}
public static class FlanStyles
{
	public static readonly GUIStyle BoldLabel = GUI.skin.label.Clone()
				.WithFontStyle(FontStyle.Bold);
    public static readonly GUIStyle GreenLabel = GUI.skin.label.Clone()
        .WithFontStyle(FontStyle.Bold)
        .WithTextColour(Color.green);

    public static readonly GUIContent ExportSingleAsset = 
        EditorGUIUtility.IconContent("SaveAs").Clone()
        .WithTooltip("Export this asset");
	public static readonly GUIContent ExportSingleAssetOverwrite =
		EditorGUIUtility.IconContent("Warning").Clone()
		.WithTooltip("Export this asset, overwriting the existing asset!");
	public static readonly GUIContent ExportPackNewOnly =
		EditorGUIUtility.IconContent("SaveAs").Clone()
		.WithTooltip("Export all new assets in this pack");
	public static readonly GUIContent ExportPackOverwrite =
        EditorGUIUtility.IconContent("Warning").Clone()
        .WithTooltip("Export ALL assets in this pack, overwritng existing assets.");
	public static readonly GUIContent ExportError =
        EditorGUIUtility.IconContent("Error").Clone()
        .WithTooltip("Fix all verification errors before exporting.");

	public static GUIContent Clone(this GUIContent content)
	{
		return new GUIContent(content);
	}
    public static GUIContent WithTooltip(this GUIContent content, string tooltip)
    {
        content.tooltip = tooltip;
        return content;
    }
	public static GUIStyle Clone(this GUIStyle style)
	{
		return new GUIStyle(style);
	}
	public static GUIStyle WithFontSize(this GUIStyle style, int fontSize)
	{
		style.fontSize = fontSize;
		return style;
	}
	public static GUIStyle WithFontStyle(this GUIStyle style, FontStyle fontStyle)
	{
		style.fontStyle = fontStyle;
		return style;
	}
	public static GUIStyle WithTextColour(this GUIStyle style, Color colour)
	{
		style.normal.textColor = colour;
		return style;
	}
	public static GUIStyle WithAlignment(this GUIStyle style, TextAnchor alignment)
	{
		style.alignment = alignment;
		return style;
	}
	public static GUIStyle WithMargin(this GUIStyle style, RectOffset rectOffset)
	{
		style.margin = rectOffset;
		return style;
	}
	public static GUIStyle WithPressedStyle(this GUIStyle style)
	{
		style.hover = style.active;
		style.normal = style.active;
		return style;
	}

	// Spacer styles
	public static GUIStyle horizontalLine = new GUIStyle()
	{
		normal = new GUIStyleState()
		{
			background = EditorGUIUtility.whiteTexture,
		},
		margin = new RectOffset(0, 0, 4, 4),
		fixedHeight = 3,
	};

	public static void BigSpacer()
	{
		GUILayout.Space(12.0f);
		FlanStyles.HorizontalLine();
		GUILayout.Space(12.0f);
	}

	public static void HorizontalLine()
	{
		GUILayout.Box(GUIContent.none, horizontalLine);
	}


    public static readonly GUIStyle BorderlessButton = GUI.skin.button.Clone()
            .WithMargin(new RectOffset(0, 0, 0, 0));
	public static readonly GUIStyle SelectedTextStyle = GUI.skin.label.Clone()
			.WithFontStyle(FontStyle.Bold);
    public static void SelectedLabel(string text, params GUILayoutOption[] options)
    {
		Color old = GUI.color;
		GUI.color = Color.green;
        GUILayout.Label(text, SelectedTextStyle, options);
		GUI.color = old;
	}
	public static GUIStyle BigHeaderBGStyle = new GUIStyle()
	{
		normal = new GUIStyleState()
		{
			background = EditorGUIUtility.whiteTexture,
		},
		margin = new RectOffset(0, 0, 0, 0),
		fixedHeight = 32,
	};
	public static readonly GUIStyle BigHeaderTextStyle = GUI.skin.label.Clone()
				.WithFontStyle(FontStyle.Bold)
                .WithFontSize(24);
	public static void BigHeader(string text)
    {
        Color old = GUI.color;
        GUI.color = Color.gray;
		GUILayout.Box(GUIContent.none, BigHeaderBGStyle);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.color = Color.white;
        GUI.Label(lastRect, text, BigHeaderTextStyle);
        GUI.color = old;
	}

	private static readonly GUILayoutOption Vec3_Name_Width = GUILayout.Width(128);
	public static Vector3 CompactVector3Field(string label, Vector3 value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, Vec3_Name_Width);
        Vector3 ret = EditorGUILayout.Vector3Field("", value);
		GUILayout.EndHorizontal();
        return ret;
    }
}
