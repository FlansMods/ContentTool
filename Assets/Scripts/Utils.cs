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
        string path = AssetDatabase.GetAssetPath(asset);
        Match match = ResourceLocator.Match(path);
        if(match.Success)
        {
            string modName = match.Groups[1].Value;
            string assetName = match.Groups[2].Value;
            return new ResourceLocation(modName, assetName);
        }
        Debug.LogWarning($"Could not resolve the path {path} as a resource location");
        return new ResourceLocation("minecraft", "null");
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
}
