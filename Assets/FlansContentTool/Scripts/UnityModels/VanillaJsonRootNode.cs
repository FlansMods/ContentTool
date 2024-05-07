using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class VanillaJsonRootNode : RootNode
{
	public string Json = "{}";

	// -----------------------------------------------------------------------------------
	#region Operations
	// -----------------------------------------------------------------------------------
	public override bool SupportsTranslate() { return false; }
	public override bool SupportsRotate() { return false; }
	public override bool SupportsMirror() { return false; }
	public override bool SupportsRename() { return false; }
	public override bool SupportsDelete() { return false; }
	public override bool SupportsDuplicate() { return false; }
	#endregion
	// -----------------------------------------------------------------------------------

	protected override bool NeedsIcon() { return false; }

	#if UNITY_EDITOR
	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		Json = EditorGUILayout.TextArea(Json, GUILayout.ExpandHeight(true));

		if(GUILayout.Button("Auto-Format"))
		{
			JObject jObject = JObject.Parse(Json);
			if(jObject != null)
			{
				Json = JsonReadWriteUtils.WriteFormattedJson(jObject);
			}
		}

	}
	#endif


}
