using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentType : PaintableType
{
    /** The type of attachment. Each gun can have one barrel, one scope, one grip, one stock and some number of generics up to a limit set by the gun */
    public EAttachmentType type = EAttachmentType.Generic;

    /** This variable controls whether or not bullet sounds should be muffled */
    public bool silencer = false;
    /** If true, then this attachment will act like a flashlight */
    public bool flashlight = false;
    /** Flashlight range. How far away it lights things up */
    public float flashlightRange = 10F;
    /** Flashlight strength between 0 and 15 */
    public int flashlightStrength = 12;

    //Gun behaviour modifiers
    /** These stack between attachments and apply themselves to the gun's default spread */
    public float spreadMultiplier = 1F;
    /** Likewise these stack and affect recoil */
    public float recoilMultiplier = 1F;
    /** Another stacking variable for damage */
    public float damageMultiplier = 1F;
    /** Melee damage modifier */
    public float meleeDamageMultiplier = 1F;
    /** Bullet speed modifier */
    public float bulletSpeedMultiplier = 1F;
    /** This modifies the reload time, which is then rounded down to the nearest tick */
    public float reloadTimeMultiplier = 1F;
    /** If set to anything other than null, then this attachment will override the weapon's default firing mode */
    public EFireMode modeOverride = EFireMode.BurstFire;

    //Scope variables (These variables only come into play for scope attachments)
    /** The zoomLevel of this scope */
    public float zoomLevel = 1F;
    /** The FOV zoom level of this scope */
    public float FOVZoomLevel = 1F;
    /** The overlay to render when using this scope */
    public string zoomOverlay;
    /** Whether to overlay a texture or not */
    public bool hasScopeOverlay = false;


    /** Model. Only applicable when the attachment is added to 3D guns */
    //    public ModelAttachment model;

    /** Some model thing that I don't think I added */
    public float fRenderOffset = 0.0f;

    //Some more mundane variables
    /** The max stack size in the inventory */
    public int maxStackSize = 1;
}
