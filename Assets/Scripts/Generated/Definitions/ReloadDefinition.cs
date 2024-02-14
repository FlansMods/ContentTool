using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ReloadDefinition : Element
{
	[JsonField]
	[Tooltip("This should match the action group key")]
	public string key = "primary";
	[JsonField]
	[Tooltip("If true, the player can press [R] to reload manually")]
	public bool manualReloadAllowed = true;
	[JsonField]
	[Tooltip("If true, attempting to fire on empty will trigger a reload")]
	public bool autoReloadWhenEmpty = true;
	[JsonField]
	public string startActionKey = "primary_reload_start";
	[JsonField]
	public string ejectActionKey = "primary_reload_eject";
	[JsonField]
	public string loadOneActionKey = "primary_reload_load_one";
	[JsonField]
	public string endActionKey = "primary_reload_end";
}
