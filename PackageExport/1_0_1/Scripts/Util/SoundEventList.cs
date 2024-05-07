using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flans Mod/Sound Event List")]
public class SoundEventList : ScriptableObject
{
    public List<SoundEventEntry> SoundEvents = new List<SoundEventEntry>();
}
