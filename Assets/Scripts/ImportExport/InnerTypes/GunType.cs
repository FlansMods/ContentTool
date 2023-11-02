using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The 1.12- implementation of Guns, used as a temporary conversion type
public class GunType : PaintableType
{
	//Gun Behaviour Variables
	/** The list of bullet types that can be used in this gun */
	public List<string> ammo = new List<string>();
	/** Whether the player can press the reload key (default R) to reload this gun */
	public bool canForceReload = true;
	/** The time (in ticks) it takes to reload this gun */
	public int reloadTime;
	/** The amount to recoil the player's view by when firing a single shot from this gun */
	public int recoil;
	/** The amount that bullets spread out when fired from this gun */
	public float bulletSpread;
	/** Damage inflicted by this gun. Multiplied by the bullet damage. */
	public float damage = 0;
	/** The damage inflicted upon punching someone with this gun */
	public float meleeDamage = 1;
	/** The speed of bullets upon leaving this gun. 0.0f means instant. */
	public float bulletSpeed = 0.0f;
	/** The number of bullet entities created by each shot */
	public int numBullets = 1;
	/** The delay between shots in ticks (1/20ths of seconds) */
	public float shootDelay = 1.0f;
	/** Number of ammo items that the gun may hold. Most guns will hold one magazine.
	* Some may hold more, such as Nerf pistols, revolvers or shotguns */
	public int numAmmoItemsInGun = 1;
	/** The firing mode of the gun. One of semi-auto, full-auto, minigun or burst */
	public ERepeatMode mode = ERepeatMode.FullAuto;
	/** The number of bullets to fire per burst in burst mode */
	public int numBurstRounds = 3;
	/** The required speed for minigun mode guns to start firing */
	public float minigunStartSpeed = 15F;
	/** Whether this gun can be used underwater */
	public bool canShootUnderwater = true;
	/** The amount of knockback to impact upon the player per shot */
	public float knockback = 0F;
	/** If true, then this gun can be dual wielded */
	public bool oneHanded = false;
	/** For one shot items like a panzerfaust */
	public bool consumeGunUponUse = false;
	/** Item to drop on shooting */
	public string dropItemOnShoot = null;

	//Information
	//Show any variables into the GUI when hovering over items.
	/** If false, then attachments wil not be listed in item GUI */
	public bool showAttachments = true;
	/** Show statistics */
	public bool showDamage = false, showRecoil = false, showSpread = false;
	/** Show reload time in seconds */
	public bool showReloadTime = false;

	//Shields
	//A shield is actually a gun without any shoot functionality (similar to knives or binoculars)
	//and a load of shield code on top. This means that guns can have in built shields (think Nerf Stampede)
	/** Whether or not this gun has a shield piece */
	public bool shield = false;
	/** Shield collision box definition. In model co-ordinates */
	public Vector3 shieldOrigin, shieldDimensions;
	/** Float between 0 and 1 denoting the proportion of damage blocked by the shield */
	public float shieldDamageAbsorption = 0F;

	//Sounds
	/** The sound played upon shooting */
	public string shootSound;
	/** The length of the sound for looping sounds */
	public int shootSoundLength;
	/** Whether to distort the sound or not. Generally only set to false for looping sounds */
	public bool distortSound = true;
	/** The sound to play upon reloading */
	public string reloadSound;

	//Looping sounds
	/** Whether the looping sounds should be used. Automatically set if the player sets any one of the following sounds */
	public bool useLoopingSounds = false;
	/** Played when the player starts to hold shoot */
	public string warmupSound;
	public int warmupSoundLength = 20;
	/** Played in a loop until player stops holding shoot */
	public string loopedSound;
	public int loopedSoundLength = 20;
	/** Played when the player stops holding shoot */
	public string cooldownSound;


	/** The sound to play upon weapon swing */
	public string meleeSound;
	/** The sound to play while holding the weapon in the hand*/
	public string idleSound;
	public int idleSoundLength;



	//Deployable Settings
	/** If true, then the bullet does not shoot when right clicked, but must instead be placed on the ground */
	//    public bool deployable = false;
	/** The deployable model */
	//    public ModelMG deployableModel;
	/** The deployable model's texture*/
	//    public String deployableTexture;
	/** Various deployable settings controlling the player view limits and standing position */
	//    public float standBackDist = 1.5F, topViewLimit = -60F, bottomViewLimit = 30F, sideViewLimit = 45F, pivotHeight = 0.375F;

	//Default Scope Settings. Overriden by scope attachments
	//In many cases, this will simply be iron sights
	/** Default scope overlay texture */
	public string defaultScopeTexture;
	/** Whether the default scope has an overlay */
	public bool hasScopeOverlay = false;
	/** The zoom level of the default scope */
	public float zoomLevel = 1.0F;
	/** The FOV zoom level of the default scope */
	public float FOVFactor = 1.5F;

	/** For guns with 3D models */
	//    public ModelGun model;

	//Various animation parameters
	//public EAnimationType animationType = EAnimationType.NONE;

	/** If true, then the gun will perform a spinning reload animation */
	public bool spinningCocking = false;

	//Attachment settings
	/** If this is true, then all attachments are allowed. Otherwise the list is checked */
	public bool allowAllAttachments = false;
	/** The list of allowed attachments for this gun */
	public List<string> allowedAttachments = new List<string>();
	/** Whether each attachment slot is available */
	public bool allowBarrelAttachments = false, allowScopeAttachments = false,
			allowStockAttachments = false, allowGripAttachments = false;
	/** The number of generic attachment slots there are on this gun */
	public int numGenericAttachmentSlots = 0;

	//Modifiers
	/** Speeds up or slows down player movement when this item is held */
	public float moveSpeedModifier = 1F;
	/** Gives knockback resistance to the player */
	public float knockbackModifier = 0F;
}