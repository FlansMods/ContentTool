using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CubeModel))]
public class CubeModelEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Cube Model Editor"); }
}