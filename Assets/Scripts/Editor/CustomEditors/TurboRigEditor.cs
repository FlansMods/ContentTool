using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TurboRig))]
public class TurboRigEditor : MinecraftModelEditor
{
	protected override void Header() { FlanStyles.BigHeader("Turbo Rig Editor"); }
}
