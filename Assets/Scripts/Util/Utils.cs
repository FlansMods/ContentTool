using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CSVReader
{
	public string CSVData;
	public char Separator;
	public char LineEnding;


	private int ReaderPosition = 0;

	public bool ReadLine(out string[] values)
	{
		if (ReaderPosition == CSVData.Length)
		{
			values = new string[0];
			return false;
		}

		int nextLineEnding = CSVData.IndexOf(LineEnding, ReaderPosition);
		string src = (nextLineEnding == -1) ? CSVData.Substring(ReaderPosition) 
											: CSVData.Substring(ReaderPosition, nextLineEnding - ReaderPosition);
		if (src.EndsWith('\r'))
			src = src.Substring(0, src.Length - 1);
		ReaderPosition = nextLineEnding + 1;
		values = src.Split(Separator);
		return true;
	}

	public CSVReader(string csv, char separator, char lineEnding)
	{
		CSVData = csv;
		Separator = separator;
		LineEnding = lineEnding == '\r' ? '\n' : lineEnding;
	}
}

public static class Utils
{
	public static readonly char[] SLASHES = new char[] { '/', '\\' };
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
	// Use Minecraft.SanitiseID()
	public static string ToLowerWithUnderscores(string shortName)
	{
		if(shortName == null ||shortName.Length == 0)
			return "";
        return System.Text.RegularExpressions.Regex.Replace(shortName, "([a-z])([A-Z])", "$1_$2").ToLower();
	}
	// Use Minecraft.ConvertPartName
	public static string ConvertPartName(string partName)
	{
		return Minecraft.ConvertPartName(partName);
	}
    public static bool Approximately(this Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude < 0.000000001f;
    }

	public static void SetParentZero(this Transform child, Transform parent)
	{
		child.SetParent(parent);
		child.localPosition = Vector3.zero;
		child.localRotation = Quaternion.identity;
		child.localScale = Vector3.one;
	}

	public static void ZeroTransformButNotChildren(this Transform parent, bool position = true, bool rotation = true, bool scale = true)
	{
		List<Transform> children = new List<Transform>();
		foreach (Transform child in parent)
			children.Add(child);

		foreach (Transform child in children)
			child.SetParent(null, true);

		if(position)
			parent.localPosition = Vector3.zero;
		if(rotation)
			parent.localRotation = Quaternion.identity;
		if(scale)
			parent.localScale = Vector3.one;

		foreach (Transform child in children)
			child.SetParent(parent, true);
	}

	public static void TranslateButNotChildren(this Transform parent, Vector3 deltaPos)
	{
		List<Transform> children = new List<Transform>();		
		foreach(Transform child in parent)
			children.Add(child);

		foreach (Transform child in children)
			child.SetParent(null, true);

		parent.localPosition += deltaPos;

		foreach (Transform child in children)
			child.SetParent(parent, true);
	}

	public static void RotateButNotChildren(this Transform parent, Vector3 deltaEuler)
	{
		List<Transform> children = new List<Transform>();
		foreach (Transform child in parent)
			children.Add(child);

		foreach (Transform child in children)
			child.SetParent(null, true);

		parent.localEulerAngles += deltaEuler;

		foreach (Transform child in children)
			child.SetParent(parent, true);
	}
}
public static class FlanCustomButtons
{
	public static Texture2D BoxBoundsTexture = null;
	public static Texture2D ShapeboxCornersTexture = null;

	public static GUIContent BoxBoundsToolButton { get {
			return (BoxBoundsTexture != null ? new GUIContent(BoxBoundsTexture) : GUIContent.none.Clone())
			.WithTooltip("Box Bounds Tool");
		} 
	}
	public static GUIContent ShapeboxCornersToolButton { get {
			return (ShapeboxCornersTexture != null ? new GUIContent(ShapeboxCornersTexture) : GUIContent.none.Clone())
			.WithTooltip("Shapebox Corners Tool");
		} 
	}

	public static GUIContent Clone(this GUIContent content)
	{
		return new GUIContent(content);
	}
	public static GUIContent WithTooltip(this GUIContent content, string tooltip)
	{
		content.tooltip = tooltip;
		return content;
	}
}

public static class FlanStyles
{
	public static int Indent = 0;

	public static readonly GUIStyle BoldLabel = GUI.skin.label.Clone()
				.WithFontStyle(FontStyle.Bold);
    public static readonly GUIStyle GreenLabel = GUI.skin.label.Clone()
        .WithFontStyle(FontStyle.Bold)
        .WithTextColour(Color.green);
	public static readonly GUIStyle RedLabel = GUI.skin.label.Clone()
		.WithFontStyle(FontStyle.Bold)
		.WithTextColour(Color.red);

	public static GUIContent Button(string iconID, string tooltip)
	{
		return EditorGUIUtility.IconContent(iconID).Clone().WithTooltip(tooltip);
	}

	// -------------------------------------------------------------------------------------------
	// Node Editor Icons
	public static GUIContent IconForNode(Node node)
	{
		if (node is TurboRootNode) return RootNode;
		if (node is AttachPointNode) return AttachPointNode;
		if (node is SectionNode) return SectionNode;
		if (node is GeometryNode) return GeometryNode;
		if (node is ItemPoseNode) return PoseNode;

		return UnknownNode;
	}
	public static readonly GUIContent NoChildren = Button("d_FilterByLabel", "No children");
	public static readonly GUIContent UnknownNode = Button("Invalid", "???");
	public static readonly GUIContent RootNode = Button("MoveTool", "TurboRig Root");
	public static readonly GUIContent AttachPointNode = Button("Grid.MoveTool", "Attach Point");
	public static readonly GUIContent SectionNode = Button("Grid.BoxTool", "Model Section");
	public static readonly GUIContent GeometryNode = Button("FilterByType", "Geometry");
	public static readonly GUIContent PoseNode = Button("AvatarPivot", "Pose");
	public static readonly GUIContent ExpandAllNodes = Button("Toolbar Plus", "Expand all nodes");
	public static readonly GUIContent CollapseAllNodes = Button("Toolbar Minus", "Collapse all nodes");

	// -------------------------------------------------------------------------------------------
	// Import Buttons
	public static readonly GUIContent RefreshImportInfo = Button("d_UnityEditor.HistoryWindow", 
		"Refresh this pack's estimated import summary");
	public static readonly GUIContent ImportPackOverwrite = Button("Download-Available", 
		"Import this entire pack, overwriting existing assets!");
	public static readonly GUIContent ImportPackNewOnly = Button("Customized",
		"Import new assets in this pack, without overwriting existing assets");
	public static readonly GUIContent ImportSingleAssetNewOnly = Button("Customized",
		"Import this asset, unless it already exists");
	public static readonly GUIContent ImportSingleAssetOverwrite = Button("Download-Available",
		"Import this asset, overwriting existing");

	// -------------------------------------------------------------------------------------------
	// Export Buttons
	public static readonly GUIContent ExportSingleAsset = Button("SaveAs", "Export this asset");
	public static readonly GUIContent ExportSingleAssetOverwrite = Button("Warning", "Export this asset, overwriting the existing asset!");
	public static readonly GUIContent ExportPackNewOnly = Button("SaveAs", "Export all new assets in this pack");
	public static readonly GUIContent ExportPackOverwrite = Button("Warning", "Export ALL assets in this pack, overwritng existing assets.");
	public static readonly GUIContent ExportError = Button("Error", "Fix all verification errors before exporting.");

	// -------------------------------------------------------------------------------------------
	// Navigation Buttons
	public static readonly GUIContent NavigateBack = Button("back", "Return to parent");
	public static readonly GUIContent GoToEntry = Button("AvatarPivot", "Focus in Scene View");
	public static readonly GUIContent DuplicateEntry = Button("TreeEditor.Duplicate", "Duplicate");
	public static readonly GUIContent DeleteEntry = Button("TreeEditor.Trash", "Delete");
	public static readonly GUIContent AddEntry = Button("CreateAddNew", "Add Entry");

	// -------------------------------------------------------------------------------------------
	// Folder Selector Buttons
	public static readonly GUIContent SelectFolder = Button("d_Profiler.Open", "Select Folder");
	public static readonly GUIContent ResetToDefault = Button("d_preAudioLoopOff", "Reset to Default");
	
	// -------------------------------------------------------------------------------------------
	// Content pack selection buttons
	public static readonly GUIContent NavigateToContentPack = Button("Package Manager", "Select Content Pack");
	public static readonly GUIContent NotInAnyContentPack = Button("Error", "Not in any Content Pack!");

	// -------------------------------------------------------------------------------------------
	// Animation Buttons
	public static readonly GUIContent ApplyPose = Button("animationvisibilitytoggleon", "Apply Pose to Rig");
	public static readonly GUIContent ViewPose = Button("d_SceneViewCamera", "Align to Pose in Scene View"); 
    public static readonly GUIContent Play = Button("PlayButton", "Play");
	public static readonly GUIContent Pause = Button("PauseButton", "Pause"); 
    public static readonly GUIContent ReturnToStart = Button("Animation.PrevKey", "Return to start");

	// -------------------------------------------------------------------------------------------
	// Helper class for a tree of foldouts with nested levels
	public class FoldoutTree
    {
        public List<string> FoldoutPaths = new List<string>();

        public bool Foldout(GUIContent label, string path)
        {
			bool foldout = FoldoutPaths.Contains(path);
            bool updatedFolout = EditorGUILayout.Foldout(foldout, label);
			if (updatedFolout && !foldout)
				FoldoutPaths.Add(path);
			else if (!updatedFolout && foldout)
				FoldoutPaths.Remove(path);
			return updatedFolout;			
		}
		public void ForceExpand(string path)
		{
			if (!FoldoutPaths.Contains(path))
				FoldoutPaths.Add(path);
		}
		public void ForceCollapse()
		{
			FoldoutPaths.Clear();
		}
	}
	// -------------------------------------------------------------------------------------------



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
	public static GUIStyle thinLine = new GUIStyle()
	{
		normal = new GUIStyleState()
		{
			background = EditorGUIUtility.whiteTexture,
		},
		margin = new RectOffset(0, 0, 1, 1),
		fixedHeight = 2,
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
	public static void ThinLine()
	{
		GUILayout.Box(GUIContent.none, thinLine);
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

	private static int TextureZoomLevel = 16;
	public static void RenderTextureAutoWidth(Texture texture)
	{
		if (TextureZoomLevel == 0)
		{
			float scale = (float)(Screen.width - 10) / texture.width;
			GUILayout.Label(GUIContent.none,
							GUILayout.Width(texture.width * scale),
							GUILayout.Height(texture.height * scale));
		}
		else
		{
			GUILayout.Label(GUIContent.none,
							GUILayout.Width(texture.width * TextureZoomLevel),
							GUILayout.Height(texture.height * TextureZoomLevel));
		}
		GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
	}
}
