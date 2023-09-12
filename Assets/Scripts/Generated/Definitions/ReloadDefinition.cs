using UnityEngine;

[System.Serializable]
public class ReloadDefinition
{
	[JsonField]
[Tooltip("If true, the player can press [R] to reload manually")]
	public bool manualReloadAllowed = true;
	[JsonField]
[Tooltip("If true, attempting to fire on empty will trigger a reload")]
	public bool autoReloadWhenEmpty = true;
	[JsonField]
[Tooltip("The start stage normally covers the player moving their hands into position to enact the reload")]
	public ActionGroupDefinition start = new ActionGroupDefinition();
	[JsonField]
[Tooltip("The eject stage is played once")]
	public ActionGroupDefinition eject = new ActionGroupDefinition();
	[JsonField]
[Tooltip("The loadOne stage is played once per ammo item used. This could be once per magazine, once per bullet/shell etc.")]
	public ActionGroupDefinition loadOne = new ActionGroupDefinition();
	[JsonField]
[Tooltip("The end stage should return the animations to neutral positions")]
	public ActionGroupDefinition end = new ActionGroupDefinition();
}
