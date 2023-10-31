using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlockbenchModel))]
public class BlockbenchModelEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Imported Model Editor"); }
}
