/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Collections.Generic;
using Opsive.Shared.Audio;
using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.Inventory;
using Opsive.Shared.StateSystem;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
using Opsive.UltimateCharacterController.Objects;
using Opsive.UltimateCharacterController.Objects.ItemAssist;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Traits.Damage;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Opsive.UltimateCharacterController.Items.Actions
{
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
using Opsive.UltimateCharacterController.Networking.Game;
#endif

    /// <summary>
    ///     Any weapon that can shoot. This includes pistols, rocket launchers, bow and arrows, etc.
    /// </summary>
    public class ShootableWeapon : UsableItem, IReloadableItem, IDamageOriginator
    {
        /// <summary>
        ///     The mode in which the weapon fires multiple shots.
        /// </summary>
        public enum FireMode
        {
            SemiAuto, // Fire discrete shots, don't continue to fire until the player fires again.
            FullAuto, // Keep firing until the ammo runs out or the player stops firing.
            Burst // Keep firing until the burst rate is zero.
        }

        /// <summary>
        ///     Specifies how the weapon should be fired.
        /// </summary>
        public enum FireType
        {
            Instant, // Fire the shot immediately.
            ChargeAndFire, // Wait for the Used callback and then fire.
            ChargeAndHold // Fire as soon as the Use button is released.
        }

        /// <summary>
        ///     Specifies when the projectile should become visible.
        /// </summary>
        public enum ProjectileVisiblityType
        {
            OnFire, // The projectile is only visible when the weapon is being fired.
            OnAim, // The projectile is visible when the character is aiming.
            OnReload, //The projectile is only visible when reloading.
            Always // The projectile is always visible when the item is equipped.
        }

        /// <summary>
        ///     Specifies how the clip should be reloaded.
        /// </summary>
        public enum ReloadClipType
        {
            Full, // Reload the entire clip.
            Single // Reload a single bullet.
        }

        [Tooltip("The ItemIdentifier that is consumed by the item.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_ConsumableItemType")]
        [SerializeField]
        protected ItemDefinitionBase m_ConsumableItemDefinition;

        [Tooltip("The mode in which the weapon fires multiple shots.")] [SerializeField]
        protected FireMode m_FireMode = FireMode.SemiAuto;

        [Tooltip("Specifies when the weapon should be fired.")] [SerializeField]
        protected FireType m_FireType = FireType.Instant;

        [Tooltip(
            "If using a charge FireType, the minimum amount of time that the weapon must be charged for in order to be fired.")]
        [SerializeField]
        protected float m_MinChargeLength;

        [Tooltip(
            "If using a charge FireType, the amount of time that the weapon must be charged for in order to be fully fired.")]
        [SerializeField]
        protected float m_FullChargeLength;

        [Tooltip("The Item Substate parameter value when charging.")] [SerializeField]
        protected int m_ChargeItemSubstateParameterValue = 10;

        [Tooltip(
            "If using a charge FireType, the minimum amount strength that the weapon will use if it isn't fully charged.")]
        [Range(0, 1)]
        [SerializeField]
        protected float m_MinChargeStrength = 0.5f;

        [Tooltip("A set of AudioClips that can be played when the weapon is charging.")] [SerializeField]
        protected AudioClipSet m_ChargeAudioClipSet = new();

        [Tooltip("The number of rounds to fire in a single shot.")] [SerializeField]
        protected int m_FireCount = 1;

        [Tooltip("If using the Burst FireMode, specifies the number of bursts the weapon can fire.")] [SerializeField]
        protected int m_BurstCount = 5;

        [Tooltip("If using the Burst FireMode, specifies the delay before the next burst can occur.")] [SerializeField]
        protected float m_BurstDelay = 0.25f;

        [Tooltip("The random spread of the bullets once they are fired.")] [Range(0, 360)] [SerializeField]
        protected float m_Spread = 0.01f;

        [Tooltip(
            "Should the weapon fire in the LookSource direction? If false the weapon will be fired in the direction of the weapon.")]
        [SerializeField]
        protected bool m_FireInLookSourceDirection = true;

        [Tooltip("Optionally specify a projectile that the weapon should use.")] [SerializeField]
        protected GameObject m_Projectile;

        [Tooltip(
            "The magnitude of the projectile velocity when fired. The direction is determined by the fire direction.")]
        [SerializeField]
        protected float m_ProjectileFireVelocityMagnitude = 10;

        [Tooltip("Specifies when the projectile should become visible.")] [SerializeField]
        protected ProjectileVisiblityType m_ProjectileVisibility = ProjectileVisiblityType.OnFire;

        [Tooltip("The layer that the projectile should occupy when initially spawned.")] [SerializeField]
        protected int m_ProjectileStartLayer = LayerManager.IgnoreRaycast;

        [Tooltip("The layer that the projectile object should change to after being fired.")] [SerializeField]
        protected int m_ProjectileFiredLayer = LayerManager.VisualEffect;

        [Tooltip("The amount of time after the object has been fired to change the layer.")] [SerializeField]
        protected float m_LayerChangeDelay = 0.1f;

        [Tooltip("The amount of time after another item has been used that the projectile should be enabled again.")]
        [SerializeField]
        protected float m_ProjectileEnableDelayAfterOtherUse = 0.4f;

        [Tooltip("The Item Substate parameter value when the weapon tries to fire but is out of ammo.")]
        [SerializeField]
        protected int m_DryFireItemSubstateParameterValue = 11;

        [Tooltip("A set of AudioClips that can be played when the weapon is out of ammo.")] [SerializeField]
        protected AudioClipSet m_DryFireAudioClipSet = new();

        [Tooltip("The delay until the hitscan is performed after being fired. Set to 0 to fire immediately.")]
        [SerializeField]
        protected float m_HitscanFireDelay;

        [Tooltip("The maximum distance in which the hitscan fire can reach.")] [SerializeField]
        protected float m_HitscanFireRange = float.MaxValue;

        [Tooltip("The maximum number of objects the hitscan cast can collide with.")] [SerializeField]
        protected int m_MaxHitscanCollisionCount = 15;

        [Tooltip("A LayerMask of the layers that can be hit when fired at.")] [SerializeField]
        protected LayerMask m_ImpactLayers = ~((1 << LayerManager.IgnoreRaycast) | (1 << LayerManager.TransparentFX) |
                                               (1 << LayerManager.UI) | (1 << LayerManager.Overlay));

        [Tooltip("Specifies if the hitscan can detect triggers.")] [SerializeField]
        protected QueryTriggerInteraction m_HitscanTriggerInteraction = QueryTriggerInteraction.Ignore;

        [Tooltip("Processes the damage dealt to a Damage Target.")] [SerializeField]
        protected DamageProcessor m_DamageProcessor;

        [Tooltip("The amount of damage to apply to the hit object.")] [SerializeField]
        protected float m_DamageAmount = 10;

        [Tooltip("The amount of force to apply to the hit object.")] [SerializeField]
        protected float m_ImpactForce = 2;

        [Tooltip("The number of frames to add the impact force to.")] [SerializeField]
        protected int m_ImpactForceFrames = 15;

        [Tooltip("The name of the state to activate upon impact.")] [SerializeField]
        protected string m_ImpactStateName;

        [Tooltip(
            "The number of seconds until the impact state is disabled. A value of -1 will require the state to be disabled manually.")]
        [SerializeField]
        protected float m_ImpactStateDisableTimer = 10;

        [Tooltip("The Surface Impact triggered when the weapon hits an object.")] [SerializeField]
        protected SurfaceImpact m_SurfaceImpact;

        [Tooltip("The number of rounds in the clip.")] [SerializeField]
        protected int m_ClipSize = 50;

        [Tooltip("Specifies when the item should be automatically reloaded.")] [SerializeField]
        protected Reload.AutoReloadType m_AutoReload = Reload.AutoReloadType.Pickup | Reload.AutoReloadType.Empty;

        [Tooltip("Specifies how the clip should be reloaded.")] [SerializeField]
        protected ReloadClipType m_ReloadType = ReloadClipType.Full;

        [Tooltip("Can the camera zoom during a reload?")] [SerializeField]
        protected bool m_ReloadCanCameraZoom = true;

        [Tooltip("Should the crosshairs spread during a recoil?")] [SerializeField]
        protected bool m_ReloadCrosshairsSpread = true;

        [Tooltip("Specifies the animator and audio state from a reload.")] [SerializeField]
        protected AnimatorAudioStateSet m_ReloadAnimatorAudioStateSet = new();

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemReload animation event or wait for the specified duration before reloading.")]
        [SerializeField]
        protected AnimationSlotEventTrigger m_ReloadEvent = new(true, 0);

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemReloadComplete animation event or wait for the specified duration before completing the reload.")]
        [SerializeField]
        protected AnimationSlotEventTrigger m_ReloadCompleteEvent = new(true, 0);

        [Tooltip("The clip that should be played after the item has finished reloading.")] [SerializeField]
        protected AudioClipSet m_ReloadCompleteAudioClipSet;

        [Tooltip("Should the weapon clip be detached and attached when reloaded?")] [SerializeField]
        protected bool m_ReloadDetachAttachClip;

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemReloadDetachClip animation event or wait for the specified duration before detaching the clip from the weapon.")]
        [SerializeField]
        protected AnimationEventTrigger m_ReloadDetachClipEvent = new(true, 0.4f);

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemReloadShowProjectile animation event or wait for the specified duration before showing the projectile.")]
        [SerializeField]
        protected AnimationEventTrigger m_ReloadShowProjectileEvent = new(true, 0.4f);

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemReloadAttachProjectile animation event or wait for the specified duration before parenting the projectile to the fire point.")]
        [SerializeField]
        protected AnimationEventTrigger m_ReloadAttachProjectileEvent = new(true, 0.6f);

        [Tooltip("The prefab that is dropped when the character is reloading.")] [SerializeField]
        protected GameObject m_ReloadDropClip;

        [Tooltip("The amount of time after the clip has been removed to change the layer.")] [SerializeField]
        protected float m_ReloadClipLayerChangeDelay = 0.1f;

        [Tooltip("The layer that the clip object should change to after being reloaded.")] [SerializeField]
        protected int m_ReloadClipTargetLayer = LayerManager.VisualEffect;

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemReloadDropClip animation event or wait for the specified duration before dropping the clip from the weapon.")]
        [SerializeField]
        protected AnimationEventTrigger m_ReloadDropClipEvent = new(true, 0.1f);

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemReloadAttachClip animation event or wait for the specified duration before attaching the clip back to the weapon.")]
        [SerializeField]
        protected AnimationEventTrigger m_ReloadAttachClipEvent = new(true, 0.4f);

        [Tooltip("The amount of positional recoil to add to the item.")] [SerializeField]
        protected MinMaxVector3 m_PositionRecoil = new(new Vector3(0, 0, -0.3f), new Vector3(0, 0.01f, -0.1f));

        [Tooltip("The amount of rotational recoil to add to the item.")] [SerializeField]
        protected MinMaxVector3 m_RotationRecoil = new(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0));

        [Tooltip("The amount of positional recoil to add to the camera.")] [SerializeField]
        protected MinMaxVector3 m_PositionCameraRecoil;

        [Tooltip("The amount of rotational recoil to add to the camera.")] [SerializeField]
        protected MinMaxVector3 m_RotationCameraRecoil = new(new Vector3(-2f, -1f, 0), new Vector3(-1f, 1f, 0));

        [Tooltip("The percent of the recoil to accumulate to the camera's rest value.")] [Range(0, 1)] [SerializeField]
        protected float m_CameraRecoilAccumulation;

        [Tooltip("Is the recoil force localized to the direct parent?")] [SerializeField]
        protected bool m_LocalizeRecoilForce;

        [Tooltip("A reference to the muzzle flash prefab.")] [SerializeField]
        protected GameObject m_MuzzleFlash;

        [Tooltip("Should the muzzle flash be pooled? If false a single muzzle flash object will be used.")]
        [SerializeField]
        protected bool m_PoolMuzzleFlash = true;

        [Tooltip("A reference to the shell prefab.")] [SerializeField]
        protected GameObject m_Shell;

        [Tooltip("The velocity that the shell should eject at.")] [SerializeField]
        protected MinMaxVector3 m_ShellVelocity = new(new Vector3(3, 0, 0), new Vector3(4, 2, 0));

        [Tooltip("The torque that the projectile should initialize with.")] [SerializeField]
        protected MinMaxVector3 m_ShellTorque = new(new Vector3(-50, -50, -50), new Vector3(50, 50, 50));

        [Tooltip("Eject the shell after the specified delay.")] [SerializeField]
        protected float m_ShellEjectDelay;

        [Tooltip("A reference to the smoke prefab.")] [SerializeField]
        protected GameObject m_Smoke;

        [Tooltip("Spawn the smoke after the specified delay.")] [SerializeField]
        protected float m_SmokeSpawnDelay;

        [Tooltip("Optionally specify a tracer that should should appear when the hitscan weapon is fired.")]
        [SerializeField]
        protected GameObject m_Tracer;

        [Tooltip("The length of the tracer if no target is hit.")] [SerializeField]
        protected float m_TracerDefaultLength = 100;

        [Tooltip("Spawn the tracer after the specified delay.")] [SerializeField]
        protected float m_TracerSpawnDelay;

        [Tooltip("Should the camera's scope camera be disabled when the character isn't aiming?")] [SerializeField]
        protected bool m_DisableScopeCameraOnNoAim;

        [Tooltip("Unity event invoked when the hitscan hits another object.")] [SerializeField]
        protected UnityFloatVector3Vector3GameObjectEvent m_OnHitscanImpactEvent;

        private int m_ActiveSharedConsumableItemIdentifierAmount;
        private bool m_Aiming;
        private ScheduledEventBase m_BurstEvent;
        private bool m_CanPlayDryFireAnimation;
        private bool m_CanPlayDryFireAudio = true;

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Transform m_CharacterTransform;
        private bool m_Charged;
        private ScheduledEventBase m_ChargeEvent;
        private int m_ClipRemaining;
        private IItemIdentifier m_ConsumableItemIdentifier;
        private bool m_DroppedClip;

        private bool m_FacingTarget;
        private bool m_FireModeCanUse = true;
        private bool m_Firing;
        private Vector3 m_FirstPersonReloadableClipLocalPosition;
        private Quaternion m_FirstPersonReloadableClipLocalRotation;
        private Transform m_FirstPersonReloadableClipParent;
        private GameObject m_FirstPersonReloadAddClip;
        private RaycastHit[] m_HitscanRaycastHits;
        private RaycastHit m_RaycastHit;
        private readonly UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer = new();
        private ScheduledEventBase m_ReloadClipEvent;
        private ScheduledEventBase m_ReloadDetachAttachClipEvent;
        private bool m_Reloading;
        private bool m_ReloadInitialized;
        private ScheduledEventBase m_ReloadProjectileEvent;
        private bool m_SharedConsumableItemIdentifier;
        private Dictionary<Item, int> m_SharedConsumableItemIdentifierAmountMap;
        private ShowProjectileStatus m_ShowReloadProjectile;
        private GameObject m_SpawnedMuzzleFlash;
        private GameObject m_SpawnedProjectile;
        private float m_StartChargeTime = -1;
        private Vector3 m_ThirdPersonReloadableClipLocalPosition;
        private Quaternion m_ThirdPersonReloadableClipLocalRotation;
        private Transform m_ThirdPersonReloadableClipParent;
        private GameObject m_ThirdPersonReloadAddClip;
        private int m_TotalClipAmount;
        private int m_TotalConsumableAmount;
        private int m_TotalReloadAmount;
        private bool m_TryStopUse;
        private int m_UseCount;
        private bool m_WaitForCharge;

        public ItemDefinitionBase ConsumableItemDefinition
        {
            get => m_ConsumableItemDefinition;
            set => m_ConsumableItemDefinition = value;
        }

        public FireMode Mode
        {
            get => m_FireMode;
            set => m_FireMode = value;
        }

        public FireType Type
        {
            get => m_FireType;
            set => m_FireType = value;
        }

        public float MinChargeLength
        {
            get => m_MinChargeLength;
            set => m_MinChargeLength = value;
        }

        public float FullChargeLength
        {
            get => m_FullChargeLength;
            set => m_FullChargeLength = value;
        }

        public int ChargeItemSubstateParameterValue
        {
            get => m_ChargeItemSubstateParameterValue;
            set => m_ChargeItemSubstateParameterValue = value;
        }

        public float MinChargeStrength
        {
            get => m_MinChargeStrength;
            set => m_MinChargeStrength = value;
        }

        public AudioClipSet ChargeAudioClipSet
        {
            get => m_ChargeAudioClipSet;
            set => m_ChargeAudioClipSet = value;
        }

        public int FireCount
        {
            get => m_FireCount;
            set => m_FireCount = value;
        }

        public int BurstCount
        {
            get => m_BurstCount;
            set => m_BurstCount = value;
        }

        public float BurstDelay
        {
            get => m_BurstDelay;
            set => m_BurstDelay = value;
        }

        public float Spread
        {
            get => m_Spread;
            set => m_Spread = value;
        }

        public bool FireInLookSourceDirection
        {
            get => m_FireInLookSourceDirection;
            set => m_FireInLookSourceDirection = value;
        }

        public GameObject Projectile
        {
            get => m_Projectile;
            set => m_Projectile = value;
        }

        public float ProjectileFireVelocityMagnitude
        {
            get => m_ProjectileFireVelocityMagnitude;
            set => m_ProjectileFireVelocityMagnitude = value;
        }

        public ProjectileVisiblityType ProjectileVisiblity
        {
            get => m_ProjectileVisibility;
            set => m_ProjectileVisibility = value;
        }

        public int ProjectileStartLayer
        {
            get { return m_ProjectileStartLayer; }
            set
            {
                m_ProjectileStartLayer = value;
                if (m_SpawnedProjectile != null)
                {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    if (m_NetworkInfo != null && !m_NetworkInfo.IsLocalPlayer()) {
                        return;
                    }
#endif
                    m_SpawnedProjectile.layer = m_ProjectileStartLayer;
                }
            }
        }

        public int ProjectileFiredLayer
        {
            get => m_ProjectileFiredLayer;
            set => m_ProjectileFiredLayer = value;
        }

        public float LayerChangeDelay
        {
            get => m_LayerChangeDelay;
            set => m_LayerChangeDelay = value;
        }

        public float ProjectileEnableDelayAfterOtherUse
        {
            get => m_ProjectileEnableDelayAfterOtherUse;
            set => m_ProjectileEnableDelayAfterOtherUse = value;
        }

        public int DryFireItemSubstateParameterValue
        {
            get => m_DryFireItemSubstateParameterValue;
            set => m_DryFireItemSubstateParameterValue = value;
        }

        public AudioClipSet DryFireAudioClipSet
        {
            get => m_DryFireAudioClipSet;
            set => m_DryFireAudioClipSet = value;
        }

        public float HitscanFireDelay
        {
            get => m_HitscanFireDelay;
            set => m_HitscanFireDelay = value;
        }

        public float HitscanFireRange
        {
            get => m_HitscanFireRange;
            set => m_HitscanFireRange = value;
        }

        public LayerMask ImpactLayers
        {
            get => m_ImpactLayers;
            set => m_ImpactLayers = value;
        }

        public QueryTriggerInteraction HitscanTriggerInteraction
        {
            get => m_HitscanTriggerInteraction;
            set => m_HitscanTriggerInteraction = value;
        }

        public DamageProcessor DamageProcessor
        {
            get => m_DamageProcessor;
            set => m_DamageProcessor = value;
        }

        public float DamageAmount
        {
            get => m_DamageAmount;
            set => m_DamageAmount = value;
        }

        public float ImpactForce
        {
            get => m_ImpactForce;
            set => m_ImpactForce = value;
        }

        public int ImpactForceFrames
        {
            get => m_ImpactForceFrames;
            set => m_ImpactForceFrames = value;
        }

        public string ImpactStateName
        {
            get => m_ImpactStateName;
            set => m_ImpactStateName = value;
        }

        public float ImpactStateDisableTimer
        {
            get => m_ImpactStateDisableTimer;
            set => m_ImpactStateDisableTimer = value;
        }

        public SurfaceImpact SurfaceImpact
        {
            get => m_SurfaceImpact;
            set => m_SurfaceImpact = value;
        }

        public int ClipSize
        {
            get => m_ClipSize;
            set => m_ClipSize = value;
        }

        public ReloadClipType ReloadType
        {
            get => m_ReloadType;
            set => m_ReloadType = value;
        }

        public bool ReloadCrosshairsSpread
        {
            get => m_ReloadCrosshairsSpread;
            set => m_ReloadCrosshairsSpread = value;
        }

        public AudioClipSet ReloadCompleteAudioClipSet
        {
            get => m_ReloadCompleteAudioClipSet;
            set => m_ReloadCompleteAudioClipSet = value;
        }

        public bool ReloadDetachAttachClip
        {
            get { return m_ReloadDetachAttachClip; }
            set
            {
                if (m_Reloading && m_ReloadDetachAttachClip && !value)
                {
#if FIRST_PERSON_CONTROLLER
                    DetachAttachClip(false, m_FirstPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, true);
#endif
                    DetachAttachClip(false, m_ThirdPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties,
                        false);
                }

                m_ReloadDetachAttachClip = value;
            }
        }

        public AnimationEventTrigger ReloadDetachClipEvent
        {
            get => m_ReloadDetachClipEvent;
            set => m_ReloadDetachClipEvent = value;
        }

        public AnimationEventTrigger ReloadShowProjectileEvent
        {
            get => m_ReloadShowProjectileEvent;
            set => m_ReloadShowProjectileEvent = value;
        }

        public AnimationEventTrigger ReloadAttachProjectileEvent
        {
            get => m_ReloadAttachProjectileEvent;
            set => m_ReloadAttachProjectileEvent = value;
        }

        public GameObject ReloadDropClip
        {
            get => m_ReloadDropClip;
            set => m_ReloadDropClip = value;
        }

        public float ReloadClipLayerChangeDelay
        {
            get => m_ReloadClipLayerChangeDelay;
            set => m_ReloadClipLayerChangeDelay = value;
        }

        public int ReloadClipTargetLayer
        {
            get => m_ReloadClipTargetLayer;
            set => m_ReloadClipTargetLayer = value;
        }

        public AnimationEventTrigger ReloadDropClipEvent
        {
            get => m_ReloadDropClipEvent;
            set => m_ReloadDropClipEvent = value;
        }

        public AnimationEventTrigger ReloadAttachClipEvent
        {
            get => m_ReloadAttachClipEvent;
            set => m_ReloadAttachClipEvent = value;
        }

        public MinMaxVector3 PositionRecoil
        {
            get => m_PositionRecoil;
            set => m_PositionRecoil = value;
        }

        public MinMaxVector3 RotationRecoil
        {
            get => m_RotationRecoil;
            set => m_RotationRecoil = value;
        }

        public MinMaxVector3 PositionCameraRecoil
        {
            get => m_PositionCameraRecoil;
            set => m_PositionCameraRecoil = value;
        }

        public MinMaxVector3 RotationCameraRecoil
        {
            get => m_RotationCameraRecoil;
            set => m_RotationCameraRecoil = value;
        }

        public float CameraRecoilAccumulation
        {
            get => m_CameraRecoilAccumulation;
            set => m_CameraRecoilAccumulation = value;
        }

        public bool LocalizeRecoilForce
        {
            get => m_LocalizeRecoilForce;
            set => m_LocalizeRecoilForce = value;
        }

        public GameObject MuzzleFlash
        {
            get => m_MuzzleFlash;
            set => m_MuzzleFlash = value;
        }

        public bool PoolMuzzleFlash
        {
            get => m_PoolMuzzleFlash;
            set => m_PoolMuzzleFlash = value;
        }

        public GameObject Shell
        {
            get => m_Shell;
            set => m_Shell = value;
        }

        public MinMaxVector3 ShellVelocity
        {
            get => m_ShellVelocity;
            set => m_ShellVelocity = value;
        }

        public MinMaxVector3 ShellTorque
        {
            get => m_ShellTorque;
            set => m_ShellTorque = value;
        }

        public float ShellEjectDelay
        {
            get => m_ShellEjectDelay;
            set => m_ShellEjectDelay = value;
        }

        public GameObject Smoke
        {
            get => m_Smoke;
            set => m_Smoke = value;
        }

        public float SmokeSpawnDelay
        {
            get => m_SmokeSpawnDelay;
            set => m_SmokeSpawnDelay = value;
        }

        public GameObject Tracer
        {
            get => m_Tracer;
            set => m_Tracer = value;
        }

        public float TracerDefaultLength
        {
            get => m_TracerDefaultLength;
            set => m_TracerDefaultLength = value;
        }

        public float TracerSpawnDelay
        {
            get => m_TracerSpawnDelay;
            set => m_TracerSpawnDelay = value;
        }

        public bool DisableScopeCameraOnNoAim
        {
            get => m_DisableScopeCameraOnNoAim;
            set => m_DisableScopeCameraOnNoAim = value;
        }

        public UnityFloatVector3Vector3GameObjectEvent OnHitscanImpactEvent
        {
            get => m_OnHitscanImpactEvent;
            set => m_OnHitscanImpactEvent = value;
        }

        public int ClipRemaining
        {
            get => m_ClipRemaining;
            private set
            {
                m_ClipRemaining = value;
                EventHandler.ExecuteEvent(m_Character, "OnItemUseConsumableItemIdentifier", m_Item,
                    m_ConsumableItemIdentifier, m_ClipRemaining);
            }
        }

        public IShootableWeaponPerspectiveProperties ShootableWeaponPerspectiveProperties { get; private set; }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_CharacterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_CharacterTransform = m_Character.transform;

            if (m_ConsumableItemDefinition != null)
                m_ConsumableItemIdentifier = m_ConsumableItemDefinition.CreateItemIdentifier();

            m_ReloadAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_CharacterLocomotion);
            m_ReloadAnimatorAudioStateSet.Awake(m_Item.gameObject);
            ShootableWeaponPerspectiveProperties =
                m_ActivePerspectiveProperties as IShootableWeaponPerspectiveProperties;
            m_HitscanRaycastHits = new RaycastHit[m_MaxHitscanCollisionCount];

            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
            EventHandler.RegisterEvent<Item>(m_Character, "OnInventoryAddItem", OnAddItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryUnequipItem", OnUnequipItem);

#if FIRST_PERSON_CONTROLLER && !FIRST_PERSON_SHOOTER
            Debug.LogError("Error: The first person perspective is imported but the first person shooter weapons do not exist. Ensure the First Person Controller or UFPS is imported.");
#endif
        }

        /// <summary>
        ///     Initializes any values that require on other components to first initialize.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (ShootableWeaponPerspectiveProperties == null)
            {
                ShootableWeaponPerspectiveProperties =
                    m_ActivePerspectiveProperties as IShootableWeaponPerspectiveProperties;

                if (ShootableWeaponPerspectiveProperties == null)
                    Debug.LogError(
                        $"Error: The First/Third Person Shootable Weapon Properties component cannot be found for the Item {name}." +
                        $"Ensure the component exists and the component's Action ID matches the Action ID of the Item ({m_ID}).");
            }
        }

        /// <summary>
        ///     The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_ReloadAnimatorAudioStateSet.OnDestroy();
            EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
            EventHandler.UnregisterEvent<Item>(m_Character, "OnInventoryAddItem", OnAddItem);
            EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryUnequipItem", OnUnequipItem);
        }

        public GameObject Owner => m_Character;
        public GameObject OriginatingGameObject => m_GameObject;

        public Reload.AutoReloadType AutoReload
        {
            get => m_AutoReload;
            set => m_AutoReload = value;
        }

        public bool CanCameraZoom
        {
            get => m_ReloadCanCameraZoom;
            set => m_ReloadCanCameraZoom = value;
        }

        public AnimatorAudioStateSet ReloadAnimatorAudioStateSet
        {
            get => m_ReloadAnimatorAudioStateSet;
            set => m_ReloadAnimatorAudioStateSet = value;
        }

        public AnimationSlotEventTrigger ReloadEvent
        {
            get => m_ReloadEvent;
            set => m_ReloadEvent = value;
        }

        public AnimationSlotEventTrigger ReloadCompleteEvent
        {
            get => m_ReloadCompleteEvent;
            set => m_ReloadCompleteEvent = value;
        }

        /// <summary>
        ///     Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        public override int GetItemSubstateIndex()
        {
            // A positive value should always be returned when the item is being fired.
            if (m_Firing)
            {
                if (m_ConsumableItemIdentifier != null && m_CanPlayDryFireAnimation)
                    return m_DryFireItemSubstateParameterValue;
                if (m_FireType == FireType.Instant)
                {
                    return Mathf.Max(1, base.GetItemSubstateIndex());
                }

                if (m_StartChargeTime != -1)
                {
                    if (m_WaitForCharge) return m_ChargeItemSubstateParameterValue;
                    return Mathf.Max(1, base.GetItemSubstateIndex());
                }
            }

            return -1;
        }

        /// <summary>
        ///     Returns the ItemIdentifier which can be reloaded by the item.
        /// </summary>
        /// <returns>The ItemIdentifier which can be reloaded by the item.</returns>
        public IItemIdentifier GetReloadableItemIdentifier()
        {
            return m_ConsumableItemIdentifier;
        }

        /// <summary>
        ///     Can the item be reloaded?
        /// </summary>
        /// <param name="sharedCheck">Should extra checks be performed ensuring the reload is valid for shared items?</param>
        /// <returns>True if the item can be reloaded.</returns>
        public bool CanReloadItem(bool sharedCheck)
        {
            // Don't reload if the clip size is infinitely large or at capacity.
            if (m_ClipSize == int.MaxValue || ClipRemaining == m_ClipSize) return false;

            // Don't reload if the inventory is out of ammo.
            if (m_Inventory.GetItemIdentifierAmount(m_ConsumableItemIdentifier) == 0) return false;

            // Don't reload if the consumed item type is shared and the item isn't equipped. This will prevent an unequipped shootable weapon from taking ammo.
            if (sharedCheck && m_SharedConsumableItemIdentifier &&
                m_Inventory.GetActiveItem(m_Item.SlotID) != m_Item) return false;

            // Can't reload if the weapon hasn't been added to the inventory yet.
            if (m_Inventory.GetItemIdentifierAmount(m_Item.ItemIdentifier) == 0) return false;

            return true;
        }

        /// <summary>
        ///     Starts to reload the item.
        /// </summary>
        public void StartItemReload()
        {
            m_Reloading = true;
            m_DroppedClip = false;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
                if (m_NetworkCharacter != null) {
                    m_NetworkCharacter.StartItemReload(this);
                }
            }
#endif

            DetermineTotalReloadAmount();
            var detachClip = false;
            IShootableWeaponPerspectiveProperties shootableWeaponPerspectiveProperties;
#if FIRST_PERSON_CONTROLLER
            shootableWeaponPerspectiveProperties =
 m_FirstPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties;
            if (shootableWeaponPerspectiveProperties != null) {
                detachClip = shootableWeaponPerspectiveProperties.ReloadableClip != null;
            }
#endif
            if (!detachClip)
            {
                shootableWeaponPerspectiveProperties =
                    m_ThirdPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties;
                if (shootableWeaponPerspectiveProperties != null)
                    detachClip = shootableWeaponPerspectiveProperties.ReloadableClip != null;
            }

            if (detachClip)
                if (!m_ReloadDetachClipEvent.WaitForAnimationEvent)
                    m_ReloadDetachAttachClipEvent =
                        SchedulerBase.ScheduleFixed(m_ReloadDetachClipEvent.Duration, DetachClip);

            // The reload AnimatorAudioState is starting.
            m_ReloadAnimatorAudioStateSet.StartStopStateSelection(true);
            m_ReloadAnimatorAudioStateSet.NextState();

            // Optionally play a reload sound based upon the reload animation.
            if (m_ReloadType == ReloadClipType.Full)
                m_ReloadAnimatorAudioStateSet.PlayAudioClip(m_Item.GetVisibleObject());

            // The crosshairs should be set to the max spread.
            if (m_ReloadCrosshairsSpread) EventHandler.ExecuteEvent(m_Character, "OnAddCrosshairsSpread", true, false);

            // The projectile may become visible when the item is reloaded.
            if (m_ProjectileVisibility == ProjectileVisiblityType.OnReload)
                if (!m_ReloadShowProjectileEvent.WaitForAnimationEvent)
                    m_ReloadProjectileEvent =
                        SchedulerBase.ScheduleFixed(m_ReloadShowProjectileEvent.Duration, ShowReloadProjectile);
        }

        /// <summary>
        ///     Reloads the item.
        /// </summary>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        public void ReloadItem(bool fullClip)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
                if (m_NetworkCharacter != null) {
                    m_NetworkCharacter.ReloadItem(this, fullClip);
                }
            }
#endif
            if (!fullClip) EventHandler.ExecuteEvent(m_Character, "OnAddCrosshairsSpread", false, false);

            var consumableItemIdentifierAmount = m_Inventory.GetItemIdentifierAmount(m_ConsumableItemIdentifier);
            if (consumableItemIdentifierAmount == 0) return;

            int reloadAmount;
            if (m_ActiveSharedConsumableItemIdentifierAmount > 0)
            {
                if (!m_Reloading) DetermineTotalReloadAmount();
                if (m_TotalReloadAmount > m_TotalConsumableAmount)
                {
                    var totalAmount = m_TotalConsumableAmount + m_TotalClipAmount;

                    // If there are multiple active consumable ItemIdentifiers then the reloaded count is shared across all of the ItemIdentifiers.
                    var targetAmount = fullClip || m_ReloadType == ReloadClipType.Full
                        ? Mathf.CeilToInt(totalAmount / (float)(m_ActiveSharedConsumableItemIdentifierAmount + 1))
                        : ClipRemaining + 1;
                    reloadAmount = Mathf.Min(consumableItemIdentifierAmount,
                        Mathf.Min(m_ClipSize - ClipRemaining, targetAmount - ClipRemaining));
                }
                else
                {
                    // The Consumable ItemIdentifier doesn't need to be shared if there is plenty of ammo for all weapons.
                    reloadAmount = Mathf.Min(m_TotalConsumableAmount,
                        fullClip || m_ReloadType == ReloadClipType.Full ? m_ClipSize - ClipRemaining : 1);
                }
            }
            else
            {
                // The consumable ItemIdentifier doesn't share with any other objects.
                reloadAmount = Mathf.Min(consumableItemIdentifierAmount,
                    fullClip || m_ReloadType == ReloadClipType.Full ? m_ClipSize - ClipRemaining : 1);
            }

            ClipRemaining += reloadAmount;
            m_UseCount = 0;
            m_Inventory.AdjustItemIdentifierAmount(m_ConsumableItemIdentifier, -reloadAmount);

            if (!fullClip && m_ReloadType == ReloadClipType.Single)
            {
                m_ReloadAnimatorAudioStateSet.PlayAudioClip(m_Item.GetVisibleObject());

                // If the item cannot be reloaded any more then the complete animation will play.
                if (CanReloadItem(false)) m_ReloadAnimatorAudioStateSet.NextState();
            }

            if (!fullClip && m_ReloadDropClip != null)
            {
                // When the item is reloaded the clip should also be replaced.
#if FIRST_PERSON_CONTROLLER
                AddRemoveReloadableClip(true, m_FirstPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, true);
#endif
                AddRemoveReloadableClip(true,
                    m_ThirdPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, false);
            }

            if (m_ShowReloadProjectile != ShowProjectileStatus.NotShown)
            {
                m_ShowReloadProjectile = ShowProjectileStatus.NotShown;
                DetermineVisibleProjectile(false);
            }
        }

        /// <summary>
        ///     The item has finished reloading.
        /// </summary>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        public void ItemReloadComplete(bool success, bool immediateReload)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
                if (m_NetworkCharacter != null) {
                    m_NetworkCharacter.ItemReloadComplete(this, success, immediateReload);
                }
            }
#endif

            if (!success)
            {
                // The weapon will not be successfully reloaded if the Reload ability stopped early.
                AttachClip();
                if (m_ReloadClipEvent != null)
                {
                    SchedulerBase.Cancel(m_ReloadClipEvent);
                    m_ReloadClipEvent = null;
                }

                if (m_ReloadProjectileEvent != null)
                {
                    SchedulerBase.Cancel(m_ReloadProjectileEvent);
                    m_ReloadProjectileEvent = null;
                }

                m_ShowReloadProjectile = ShowProjectileStatus.NotShown;
            }
            else if (!immediateReload)
            {
                m_ReloadCompleteAudioClipSet.PlayAudioClip(m_Item.GetVisibleObject());
            }

            DetermineVisibleProjectile(false);
            m_Reloading = false;

            // The item has been reloaded - inform the state set.
            m_ReloadAnimatorAudioStateSet.StartStopStateSelection(false);
            m_CanPlayDryFireAudio = true;
        }

        /// <summary>
        ///     The item will be equipped.
        /// </summary>
        public override void WillEquip()
        {
            var returnedConsumable = false;
            // When the weapon is equipped for the first time it needs to reload so it doesn't need to be reloaded manually.
            if (!m_ReloadInitialized)
            {
                ItemSetManagerBase itemSetManager;
                // Consumable ItemIdentifiers that are shared may need to have their amount redistrubuted for on the first equip. This will allow all items to have an equal
                // amount on the first run.
                if (m_SharedConsumableItemIdentifier &&
                    (itemSetManager = m_Inventory.GetComponent<ItemSetManagerBase>()) != null)
                    for (var i = 0; i < m_Inventory.SlotCount; ++i)
                    {
                        // Use the ItemSetManager to determine which ItemIdentifier will be equipped. The current slot cannot be used from the inventory because not all
                        // items may have been equipped yet.
                        var itemIdentifier = itemSetManager.GetEquipItemIdentifier(i);
                        if (itemIdentifier == null) continue;

                        // Skip if the item is null or matches the current item. The current item can't give ammo to itself.
                        var item = m_Inventory.GetItem(itemIdentifier, i);
                        if (item == null || item == m_Item) continue;

                        // Find any ShootableWeapons that may be sharing the same Consumable ItemIdentifier.
                        var itemActions = item.ItemActions;
                        for (var j = 0; j < itemActions.Length; ++j)
                        {
                            var shootableWeapon = itemActions[j] as ShootableWeapon;
                            // The ShootableWeapon has to share the Consumable ItemIdentifier. The other shootable weapon must have ammo in the clip otherwise it isn't of any use.
                            if (shootableWeapon == null || shootableWeapon.GetConsumableItemIdentifier() !=
                                m_ConsumableItemIdentifier ||
                                shootableWeapon.ClipRemaining == 0)
                                continue;

                            // The Consumable ItemIdentifier doesn't need to be shared if there is plenty of ammo for all weapons.
                            var totalInventoryAmount = m_Inventory.GetItemIdentifierAmount(m_ConsumableItemIdentifier);
                            if (shootableWeapon.ClipSize + m_ClipSize < totalInventoryAmount) continue;

                            // The ShootableWeapon needs to share the ammo. Take half of the ammo and return it to the inventory. When the current item is reloaded it will 
                            // then take the Consumable ItemIdentifier that was returned to the inventory.
                            var totalConsumable =
                                shootableWeapon.ClipRemaining + m_ClipRemaining + totalInventoryAmount;
                            var returnConsumable = Mathf.FloorToInt(shootableWeapon.ClipRemaining -
                                                                    (totalConsumable - totalConsumable / 2));
                            if (returnConsumable > 0)
                            {
                                shootableWeapon.ClipRemaining -= returnConsumable;
                                m_Inventory.Pickup(m_ConsumableItemIdentifier, returnConsumable, -1, false, false,
                                    false);
                                returnedConsumable = true;
                            }
                        }
                    }

                if (CanReloadItem(false))
                {
                    // If part of the consumable was returned then the weapon should take as much as it can from the remaining amount. 
                    var activeCount = m_ActiveSharedConsumableItemIdentifierAmount;
                    if (returnedConsumable) m_ActiveSharedConsumableItemIdentifierAmount--;
                    ReloadItem(true);
                    m_ActiveSharedConsumableItemIdentifierAmount = activeCount;
                }

                m_ReloadInitialized = true;
            }
        }

        /// <summary>
        ///     The item has been equipped by the character.
        /// </summary>
        public override void Equip()
        {
            base.Equip();

            DetermineVisibleProjectile(false);
            DetermineVisibleScopeCamera();
            m_CanPlayDryFireAnimation = ClipRemaining == 0;

            EventHandler.RegisterEvent<IUsableItem, bool>(m_Character, "OnItemStartUse", ItemStartUse);
            EventHandler.RegisterEvent(m_Character, "OnAnimatorItemReloadDetachClip", DetachClip);
            EventHandler.RegisterEvent(m_Character, "OnAnimatorItemReloadAttachClip", AttachClip);
            EventHandler.RegisterEvent(m_Character, "OnAnimatorItemReloadDropClip", DropClip);
            EventHandler.RegisterEvent(m_Character, "OnAnimatorItemReloadShowProjectile", ShowReloadProjectile);
            EventHandler.RegisterEvent(m_Character, "OnAnimatorItemReloadAttachProjectile", AttachReloadProjectile);
        }

        /// <summary>
        ///     Set the ItemIdentifier which can be consumed by the item.
        /// </summary>
        /// <param name="itemIdentifier">The new ItemIdentifier which can be consumed by the item.</param>
        public override void SetConsumableItemIdentifier(IItemIdentifier itemIdentifier)
        {
            if (m_ConsumableItemIdentifier == itemIdentifier) return;

            // Add back the previous consumable item to the inventory.
            m_Inventory.AdjustItemIdentifierAmount(m_ConsumableItemIdentifier, ClipRemaining);

            m_ConsumableItemIdentifier = itemIdentifier;
            m_ConsumableItemDefinition = itemIdentifier.GetItemDefinition();
            // Set the ClipRemaining to 0 so the new consumable item can be loaded from the inventory.
            ClipRemaining = 0;

            ReloadItem(false);
        }

        /// <summary>
        ///     Returns the ItemIdentifier which can be used by the item.
        /// </summary>
        /// <returns>The ItemIdentifier which can be used by the item.</returns>
        public override IItemIdentifier GetConsumableItemIdentifier()
        {
            return m_ConsumableItemIdentifier;
        }

        /// <summary>
        ///     Sets the UsableItemIdentifier amount on the UsableItem.
        /// </summary>
        /// <param name="amount">The amount to set the UsableItemIdentifier to.</param>
        public override void SetConsumableItemIdentifierAmount(int amount)
        {
            ClipRemaining = amount;
        }

        /// <summary>
        ///     Returns the amout of UsableItemIdentifier which has been consumed by the UsableItem.
        /// </summary>
        /// <returns>The amount consumed of the UsableItemIdentifier.</returns>
        public override int GetConsumableItemIdentifierAmount()
        {
            return ClipRemaining;
        }

        /// <summary>
        ///     Removes the amout of UsableItemIdentifier which has been loaded by the UsableItem.
        /// </summary>
        public override void RemoveConsumableItemIdentifierAmount()
        {
            ClipRemaining = 0;
        }

        /// <summary>
        ///     Can the item be used?
        /// </summary>
        /// <param name="itemAbility">The itemAbility that is trying to use the item.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanUseItem(ItemAbility itemAbility, UseAbilityState abilityState)
        {
            if (!base.CanUseItem(itemAbility, abilityState, false)) return false;

            // The weapon cannot be used if the fire mode prevents it.
            if (!m_FireModeCanUse) return false;

            // The weapon can't be used if it is reloading and out of ammo.
            if (m_Reloading && (m_ReloadType == ReloadClipType.Full || m_ClipRemaining == 0)) return false;

            // The weapon can't be used if it is already being used.
            if (abilityState == UseAbilityState.Start && m_Firing) return false;

            // The weapon can be used.
            return true;
        }

        /// <summary>
        ///     Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public override void StartItemUse(ItemAbility itemAbility)
        {
            base.StartItemUse(itemAbility);

            // An Animator Audio State Set may prevent the item from being used.
            if (!IsItemInUse()) return;

            m_FacingTarget = false;
            m_UseCount = 0;
            m_TryStopUse = false;
            m_Charged = false;
            m_WaitForCharge = false;
            m_StartChargeTime = -1;
            DetermineVisibleProjectile(false);
            BurstReset();
            m_CanPlayDryFireAnimation = ClipRemaining == 0;
            m_CanPlayDryFireAudio = true;
        }

        /// <summary>
        ///     Allows the item to update while it is being used.
        /// </summary>
        public override void UseItemUpdate()
        {
            base.UseItemUpdate();

            if (m_FacingTarget) return;

            // When the weapon is being used it will rotate to face the look source direction. Wait to fire until the weapon is looking at the look source.
            if (ShootableWeaponPerspectiveProperties.CanFire(true,
                    m_FireInLookSourceDirection && !m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(true)))
            {
                m_FacingTarget = true;
                m_Firing = true;
                m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
            }
        }

        /// <summary>
        ///     Uses the item.
        /// </summary>
        public override void UseItem()
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && !m_NetworkInfo.IsLocalPlayer()) {
                return;
            }
#endif

            // The weapon may not have any more ammo left. Play a dry fire sound and return immediately so the weapon is not fired.
            if ((m_ConsumableItemIdentifier != null && ClipRemaining == 0) || InvalidUseAttributes())
            {
                if (!m_Reloading && m_FireModeCanUse && m_CanPlayDryFireAudio)
                {
                    // If it wasn't for being out of ammo the item would have been able to be used - play a dry fire sound.
                    m_DryFireAudioClipSet.PlayAudioClip(m_Item.GetVisibleObject());
                    m_CanPlayDryFireAnimation = true;
                    m_CanPlayDryFireAudio = false;
                }

                m_NextAllowedUseTime = Time.time + m_UseRate;
                m_FireModeCanUse = m_FireMode == FireMode.FullAuto;
                return;
            }

            if (m_FireType == FireType.Instant)
            {
                Fire(1);
            }
            else if (!m_WaitForCharge)
            {
                // The FireType requires charging.
                m_StartChargeTime = Time.time;
                m_Charged = false;
                m_WaitForCharge = true;
                m_ChargeAudioClipSet.PlayAudioClip(m_Item.GetVisibleObject());
                m_ChargeEvent = SchedulerBase.ScheduleFixed(m_FullChargeLength, OnCharged);
                m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
            }
        }

        /// <summary>
        ///     Is the item waiting to be used? This will return true if the item is waiting to be charged or pulled back.
        /// </summary>
        /// <returns>Returns true if the item is waiting to be used.</returns>
        public override bool IsItemUsePending()
        {
            if (m_FireType == FireType.Instant || !m_WaitForCharge) return false;
            // The item is waiting to be charged.
            return true;
        }

        /// <summary>
        ///     The weapon has finished charging.
        /// </summary>
        protected virtual void OnCharged()
        {
            m_Charged = true;
            m_ChargeEvent = null;
            if (m_FireType == FireType.ChargeAndFire)
            {
                m_WaitForCharge = false;
                m_ChargeAudioClipSet.Stop(m_Item.GetVisibleObject());
                Fire(1);
            }
        }

        /// <summary>
        ///     Fires the weapon.
        /// </summary>
        /// <param name="strength">(0 - 1) value indicating the amount of strength to apply to the shot.</param>
        public virtual void Fire(float strength)
        {
            if (!ShootableWeaponPerspectiveProperties.CanFire(true,
                    m_FireInLookSourceDirection && !m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(true)))
            {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
#endif
                return;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                }
#endif
            }

            // Call base.UseItem within Fire since this is where the weapon use actually occurs.
            base.UseItem();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
                if (m_NetworkCharacter != null) {
                    m_NetworkCharacter.Fire(this, strength);
                }
#endif
            m_Firing = true;
            m_UseCount++;
            if (m_FireMode == FireMode.Burst)
            {
                // The weapon can continue to be fired if the current burst count is greater than the total burst count.
                m_FireModeCanUse = m_UseCount < m_BurstCount;

                // If the weapon can't be used that means the burst count needs to be reset after the specified delay.
                if (!m_FireModeCanUse) m_BurstEvent = SchedulerBase.ScheduleFixed(m_BurstDelay, BurstReset);
            }
            else
            {
                // Prevent the weapon from continuously firing if it not a fully automatic.
                m_FireModeCanUse = m_FireMode == FireMode.FullAuto;
            }

            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            }
#endif
            // Update the clips remaining after the parameters so dry fire doesn't play when there was still a single shot left.
            if (m_ConsumableItemIdentifier != null) ClipRemaining--;

            // Fire as many projectiles or hitscan bullets as the fire count specifies.
            for (var i = 0; i < m_FireCount; ++i)
                if (m_Projectile)
                    ProjectileFire(strength);
                else
                    SchedulerBase.Schedule(m_HitscanFireDelay, HitscanFire, strength);
            // The weapon has been fired - add any effects.
            ApplyFireEffects();
        }

        /// <summary>
        ///     Spawns a projectile which will move in the firing direction.
        /// </summary>
        /// <param name="strength">(0 - 1) value indicating the amount of strength to apply to the shot.</param>
        private void ProjectileFire(float strength)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // Do not spawn the projectile unless on the server. The server will manage the projectile.
            if (m_NetworkInfo != null && !m_NetworkInfo.IsServer()) {
                if (m_SpawnedProjectile != null) {
                    ObjectPoolBase.Destroy(m_SpawnedProjectile);
                    m_SpawnedProjectile = null;
                }
                return;
            }
#endif

            var firePoint = ShootableWeaponPerspectiveProperties.FirePointLocation.position;
            var fireDirection = FireDirection(firePoint);
            var rotation = Quaternion.LookRotation(fireDirection);
            // The projectile will already be spawned if it is always visible.
            if (m_SpawnedProjectile == null)
            {
                m_SpawnedProjectile =
                    ObjectPoolBase.Instantiate(m_Projectile, firePoint, rotation * m_Projectile.transform.rotation);
            }
            else
            {
                // The projectile may be on the other side of an object (especially in the case of separate arms for the first person perspective). Perform a linecast
                // to ensure the projectile doesn't go through any objects.
                if (m_FireInLookSourceDirection &&
                    !m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(false) &&
                    Physics.Linecast(m_LookSource.LookPosition(true), m_SpawnedProjectile.transform.position,
                        out m_RaycastHit, m_ImpactLayers, QueryTriggerInteraction.Ignore))
                {
                    // The cast should not hit the character that it belongs to.
                    var updatePosition = true;
                    var hitGameObject = m_RaycastHit.transform.gameObject;
                    var parentCharacterLocomotion =
                        hitGameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
                    if (parentCharacterLocomotion != null && parentCharacterLocomotion == m_CharacterLocomotion)
                        updatePosition = false;
#if FIRST_PERSON_CONTROLLER
                    // The cast should not hit any colliders who are a child of the camera.
                    if (updatePosition && hitGameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                        updatePosition = false;
                    }
#endif
                    if (updatePosition) m_SpawnedProjectile.transform.position = m_RaycastHit.point;
                    EventHandler.ExecuteEvent(m_Character, "OnShootableWeaponShowProjectile", m_SpawnedProjectile,
                        false);
                }

                m_SpawnedProjectile.transform.parent = null;
            }

            // Optionally change the layer after the object has been fired. This allows the object to change from the first person Overlay layer
            // to the Default layer after it has cleared the first person weapon.
            if (m_ProjectileStartLayer != m_ProjectileFiredLayer)
                SchedulerBase.ScheduleFixed(m_LayerChangeDelay, ChangeFiredLayer, m_SpawnedProjectile);
            var projectile = m_SpawnedProjectile.GetCachedComponent<Projectile>();
            projectile.Initialize(
                m_CharacterLocomotion.Velocity - m_CharacterLocomotion.LocomotionVelocity +
                rotation * (m_ProjectileFireVelocityMagnitude * strength * Vector3.forward),
                Vector3.zero, m_DamageProcessor, m_DamageAmount, m_ImpactForce, m_ImpactForceFrames, m_ImpactLayers,
                m_ImpactStateName, m_ImpactStateDisableTimer, m_SurfaceImpact, m_Character);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkCharacter != null) {
                NetworkObjectPool.NetworkSpawn(m_Projectile, projectile.gameObject, true);
            }
#endif
            m_SpawnedProjectile = null;
        }

        /// <summary>
        ///     Changes the fired projectile to the fired layer.
        /// </summary>
        /// <param name="projectileObject">The projectile that was fired.</param>
        private void ChangeFiredLayer(GameObject projectileObject)
        {
            projectileObject.transform.SetLayerRecursively(m_ProjectileFiredLayer);
        }

        /// <summary>
        ///     Enables or disables the visibile projectile.
        /// </summary>
        /// <param name="forceDisable">Should the projectile be disabled?</param>
        private void DetermineVisibleProjectile(bool forceDisable)
        {
            if (m_Projectile == null || m_ProjectileVisibility == ProjectileVisiblityType.OnFire) return;

            // The projectile should be visible if:
            // - The item isn't being unequipped and is active.
            // - The projectile is always visible.
            // - The projectile is visible upon aim and the aim ability is active.
            // - The projectile is visible when reloading and the character is reloading.
            var enable = !forceDisable && m_Item.IsActive() &&
                         (m_ProjectileVisibility == ProjectileVisiblityType.Always ||
                          (m_Aiming && m_ProjectileVisibility == ProjectileVisiblityType.OnAim) ||
                          (m_ShowReloadProjectile != ShowProjectileStatus.NotShown &&
                           m_ProjectileVisibility == ProjectileVisiblityType.OnReload));

            // The projectile can't be shown if there are no projectiles remaining.
            if (enable && m_ClipRemaining == 0 && m_ShowReloadProjectile == ShowProjectileStatus.NotShown) return;

            if (enable)
            {
                if (m_SpawnedProjectile == null)
                {
                    m_SpawnedProjectile = ObjectPoolBase.Instantiate(m_Projectile);
                    m_SpawnedProjectile.transform.SetLayerRecursively(m_ProjectileStartLayer);
                    // If the character is reloading the projectile should be shown 
                    if (m_ShowReloadProjectile == ShowProjectileStatus.AttachmentLocation)
                    {
                        m_SpawnedProjectile.transform.SetParentOrigin(ShootableWeaponPerspectiveProperties
                            .ReloadProjectileAttachment);
                        m_SpawnedProjectile.layer = ShootableWeaponPerspectiveProperties.ReloadProjectileAttachment
                            .gameObject.layer;
                    }
                    else
                    {
                        m_SpawnedProjectile.transform.SetParentOrigin(ShootableWeaponPerspectiveProperties
                            .FirePointLocation);
                    }
                }
                else if (m_ShowReloadProjectile == ShowProjectileStatus.FirePointLocation)
                {
                    m_SpawnedProjectile.transform.parent = ShootableWeaponPerspectiveProperties.FirePointLocation;
                }

                EventHandler.ExecuteEvent(m_Character, "OnShootableWeaponShowProjectile", m_SpawnedProjectile, true);
            }
            else if (m_SpawnedProjectile != null)
            {
                EventHandler.ExecuteEvent(m_Character, "OnShootableWeaponShowProjectile", m_SpawnedProjectile, false);
                ObjectPoolBase.Destroy(m_SpawnedProjectile);
                m_SpawnedProjectile = null;
            }
        }

        /// <summary>
        ///     Determines if the scope camera should be visible.
        /// </summary>
        private void DetermineVisibleScopeCamera()
        {
            if (ShootableWeaponPerspectiveProperties != null &&
                ShootableWeaponPerspectiveProperties.ScopeCamera != null)
                ShootableWeaponPerspectiveProperties.ScopeCamera.SetActive((!m_DisableScopeCameraOnNoAim || m_Aiming) &&
                                                                           m_Inventory.GetActiveItem(m_Item.SlotID) ==
                                                                           m_Item);
        }

        /// <summary>
        ///     The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) return;
            m_Aiming = aim;

            DetermineVisibleProjectile(false);
            DetermineVisibleScopeCamera();
        }

        /// <summary>
        ///     Fire by casting a ray in the specified direction. If an object was hit then apply damage, apply a force, add a
        ///     decal, etc.
        /// </summary>
        /// <param name="strength">(0 - 1) value indicating the amount of strength to apply to the shot.</param>
        private void HitscanFire(float strength)
        {
            // The hitscan should be fired from the center of the camera so the hitscan will always hit the correct crosshairs location.
            var useLookPosition = m_FireInLookSourceDirection &&
                                  !m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(false);
            var firePoint = useLookPosition
                ? m_LookSource.LookPosition(true)
                : ShootableWeaponPerspectiveProperties.FirePointLocation.position;
            var fireDirection = FireDirection(firePoint);
            var fireRay = new Ray(firePoint, fireDirection);
            // Prevent the ray between the character and the look source from causing a false collision.
            if (useLookPosition && !m_CharacterLocomotion.FirstPersonPerspective)
            {
                var direction = m_CharacterTransform.InverseTransformPoint(firePoint);
                direction.y = 0;
                fireRay.origin = fireRay.GetPoint(direction.magnitude);
            }

            var hitCount = Physics.RaycastNonAlloc(fireRay, m_HitscanRaycastHits, m_HitscanFireRange * strength,
                m_ImpactLayers.value, m_HitscanTriggerInteraction);
            var hasHit = false;

#if UNITY_EDITOR
            if (hitCount == m_MaxHitscanCollisionCount)
                Debug.LogWarning(
                    $"Warning: The maximum number of colliders have been hit by {m_GameObject.name}. Consider increasing the Max Hitscan Collision Count value.",
                    this);
#endif

            for (var i = 0; i < hitCount; ++i)
            {
                var closestRaycastHit = QuickSelect.SmallestK(m_HitscanRaycastHits, hitCount, i, m_RaycastHitComparer);
                var hitGameObject = closestRaycastHit.transform.gameObject;
                // The character can't shoot themself.
                if (hitGameObject.transform.IsChildOf(m_CharacterTransform)
#if FIRST_PERSON_CONTROLLER
                    // The cast should not hit any colliders who are a child of the camera.
                    || hitGameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>() != null
#endif
                   )
                    continue;

                // The shield can absorb some (or none) of the damage from the hitscan.
                var damageAmount = m_DamageAmount;
#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
                ShieldCollider shieldCollider;
                if ((shieldCollider = hitGameObject.GetCachedComponent<ShieldCollider>()) != null)
                    damageAmount = shieldCollider.Shield.Damage(this, damageAmount);
#endif

                // Allow a custom event to be received.
                EventHandler.ExecuteEvent<float, Vector3, Vector3, GameObject, object, Collider>(
                    closestRaycastHit.transform.gameObject, "OnObjectImpact", damageAmount, closestRaycastHit.point,
                    m_ImpactForce * strength * fireDirection, m_Character, this, closestRaycastHit.collider);
                if (m_OnHitscanImpactEvent != null)
                    m_OnHitscanImpactEvent.Invoke(damageAmount, closestRaycastHit.point,
                        m_ImpactForce * strength * fireDirection, m_Character);

                // If the shield didn't absorb all of the damage then it should be applied to the character.
                if (damageAmount > 0)
                {
                    var damageTarget = DamageUtility.GetDamageTarget(hitGameObject);
                    if (damageTarget != null)
                    {
                        var pooledDamageData = GenericObjectPool.Get<DamageData>();
                        pooledDamageData.SetDamage(this, damageAmount, closestRaycastHit.point, fireDirection,
                            m_ImpactForce * strength, m_ImpactForceFrames, 0, closestRaycastHit.collider);
                        var damageProcessorModule =
                            m_CharacterLocomotion.gameObject.GetCachedComponent<DamageProcessorModule>();
                        if (damageProcessorModule != null)
                        {
                            damageProcessorModule.ProcessDamage(m_DamageProcessor, damageTarget, pooledDamageData);
                        }
                        else
                        {
                            if (m_DamageProcessor == null) m_DamageProcessor = DamageProcessor.Default;
                            m_DamageProcessor.Process(damageTarget, pooledDamageData);
                        }

                        GenericObjectPool.Return(pooledDamageData);
                    }
                    else
                    {
                        // If the damage target exists it will apply a force to the rigidbody in addition to procesing the damage. Otherwise just apply the force to the rigidbody. 
                        if (m_ImpactForce > 0 && closestRaycastHit.rigidbody != null &&
                            !closestRaycastHit.rigidbody.isKinematic)
                            closestRaycastHit.rigidbody.AddForceAtPosition(
                                m_ImpactForce * strength * MathUtility.RigidbodyForceMultiplier * fireDirection,
                                closestRaycastHit.point);
                    }
                }

                // Spawn a tracer which moves to the hit point.
                if (m_Tracer != null)
                    SchedulerBase.ScheduleFixed(m_TracerSpawnDelay, AddHitscanTracer, closestRaycastHit.point);

                // An optional state can be activated on the hit object.
                if (!string.IsNullOrEmpty(m_ImpactStateName))
                {
                    var characterLocomotion = hitGameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
                    StateManager.SetState(characterLocomotion != null ? characterLocomotion.gameObject : hitGameObject,
                        m_ImpactStateName, true);
                    // If the timer isn't -1 then the state should be disabled after a specified amount of time. If it is -1 then the state
                    // will have to be disabled manually.
                    if (m_ImpactStateDisableTimer != -1)
                        StateManager.DeactivateStateTimer(hitGameObject, m_ImpactStateName, m_ImpactStateDisableTimer);
                }

                // The surface manager will apply effects based on the type of bullet hit.
                SurfaceManager.SpawnEffect(closestRaycastHit, m_SurfaceImpact, m_CharacterLocomotion.GravityDirection,
                    m_CharacterLocomotion.TimeScale, m_Item.GetVisibleObject());

                hasHit = true;
                break;
            }

            // A tracer should still be spawned if no object was hit.
            if (!hasHit && m_Tracer != null)
                SchedulerBase.ScheduleFixed(m_TracerSpawnDelay, AddHitscanTracer,
                    MathUtility.TransformPoint(firePoint, Quaternion.LookRotation(fireDirection),
                        new Vector3(0, 0, m_TracerDefaultLength)));
        }

        /// <summary>
        ///     Adds a tracer to the hitscan weapon.
        /// </summary>
        /// <param name="position">The position that the tracer should move towards.</param>
        protected virtual void AddHitscanTracer(Vector3 position)
        {
            var tracerLocation = ShootableWeaponPerspectiveProperties.TracerLocation;
            var tracerObject = ObjectPoolBase.Instantiate(m_Tracer, tracerLocation.position, tracerLocation.rotation);
            var tracer = tracerObject.GetCachedComponent<Tracer>();
            if (tracer != null) tracer.Initialize(position);
        }

        /// <summary>
        ///     Determines the direction to fire.
        /// </summary>
        /// <returns>The direction to fire.</returns>
        private Vector3 FireDirection(Vector3 firePoint)
        {
            var direction = m_FireInLookSourceDirection
                ? m_LookSource.LookDirection(firePoint, false, m_ImpactLayers, true, true)
                : ShootableWeaponPerspectiveProperties.FirePointLocation.forward;

            // Add the spread in a random direction.
            if (m_Spread > 0)
                direction += Quaternion.AngleAxis(Random.Range(0, 360), direction) * m_CharacterLocomotion.Up *
                             Random.Range(0, m_Spread / 360);

            return direction;
        }

        /// <summary>
        ///     Adds any effects (muzzle flash, shell, recoil, etc) to the fire position.
        /// </summary>
        protected virtual void ApplyFireEffects()
        {
            // Spawn a muzzle flash.
            if (m_MuzzleFlash != null) SpawnMuzzleFlash();

            // Spawn a shell.
            if (m_Shell != null) SchedulerBase.Schedule(m_ShellEjectDelay, EjectShell);

            // Spawn the smoke.
            if (m_Smoke != null) SchedulerBase.Schedule(m_SmokeSpawnDelay, SpawnSmoke);

            // Apply the weapon recoil.
            EventHandler.ExecuteEvent(m_Character, "OnAddSecondaryForce", m_Item.SlotID, m_PositionRecoil.RandomValue,
                m_RotationRecoil.RandomValue, !m_LocalizeRecoilForce);
            EventHandler.ExecuteEvent(m_Character, "OnAddSecondaryCameraForce", m_PositionCameraRecoil.RandomValue,
                m_RotationCameraRecoil.RandomValue, m_CameraRecoilAccumulation);
            EventHandler.ExecuteEvent(m_Character, "OnAddCrosshairsSpread", true, true);
        }

        /// <summary>
        ///     Spawns the muzzle flash.
        /// </summary>
        private void SpawnMuzzleFlash()
        {
            if (m_PoolMuzzleFlash || m_SpawnedMuzzleFlash == null)
                m_SpawnedMuzzleFlash = ObjectPoolBase.Instantiate(m_MuzzleFlash, Vector3.zero, Quaternion.identity);
            var muzzleFlashLocation = ShootableWeaponPerspectiveProperties.MuzzleFlashLocation;
            m_SpawnedMuzzleFlash.transform.parent = muzzleFlashLocation;
            m_SpawnedMuzzleFlash.transform.localScale = m_MuzzleFlash.transform.localScale;
            m_SpawnedMuzzleFlash.transform.position = muzzleFlashLocation.position + m_MuzzleFlash.transform.position;
            // Choose a random z rotation angle.
            var eulerAngles = m_MuzzleFlash.transform.eulerAngles;
            eulerAngles.z = Random.Range(0, 360);
            m_SpawnedMuzzleFlash.transform.localRotation = Quaternion.Euler(eulerAngles);

            var muzzleFlashObj = m_SpawnedMuzzleFlash.GetCachedComponent<MuzzleFlash>();
            if (muzzleFlashObj != null) muzzleFlashObj.Show(m_Item, m_ID, m_PoolMuzzleFlash, m_CharacterLocomotion);
        }

        /// <summary>
        ///     Ejects the shell.
        /// </summary>
        private void EjectShell()
        {
            var shellLocation = ShootableWeaponPerspectiveProperties.ShellLocation;
            var shell = ObjectPoolBase.Instantiate(m_Shell, shellLocation.position, shellLocation.rotation);
            var shellObj = shell.GetCachedComponent<Shell>();
            if (shellObj != null)
            {
                var visibleObject = m_Item.ActivePerspectiveItem.GetVisibleObject();
                shellObj.Initialize(visibleObject.transform.TransformDirection(m_ShellVelocity.RandomValue),
                    m_ShellTorque.RandomValue, m_Character);
            }
        }

        /// <summary>
        ///     Spawns the shell.
        /// </summary>
        private void SpawnSmoke()
        {
            var smokeLocation = ShootableWeaponPerspectiveProperties.SmokeLocation;
            var smoke = ObjectPoolBase.Instantiate(m_Smoke, smokeLocation.position, smokeLocation.rotation);
            var smokeObj = smoke.GetCachedComponent<Smoke>();
            if (smokeObj != null) smokeObj.Show(m_Item, m_ID, m_CharacterLocomotion);
        }

        /// <summary>
        ///     Resets the use count which will allow burst to start again.
        /// </summary>
        private void BurstReset()
        {
            m_BurstEvent = null;

            if (!m_TryStopUse)
            {
                m_UseCount = 0;
                m_FireModeCanUse = true;
            }
        }

        /// <summary>
        ///     The item has been used.
        /// </summary>
        public override void ItemUseComplete()
        {
            base.ItemUseComplete();
            m_Firing = false;

            // When the clip is empty the weapon should be reloaded if specified.
            if (ClipRemaining == 0 && (m_AutoReload & Reload.AutoReloadType.Empty) != 0)
                EventHandler.ExecuteEvent(m_Character, "OnItemTryReload", m_Item.SlotID, m_ConsumableItemIdentifier,
                    false, false);
        }

        /// <summary>
        ///     Tries to stop the item use. The weapon may be fired here if the FireType is not instant.
        /// </summary>
        public override void TryStopItemUse()
        {
            base.TryStopItemUse();
            m_TryStopUse = true;

            if (m_FireType != FireType.Instant && m_FireModeCanUse)
            {
                m_ChargeAudioClipSet.Stop(m_Item.GetVisibleObject());
                m_WaitForCharge = false;
                if (!m_Charged)
                {
                    // If the weapon has been stopped before it has been fully charged then the item should be fired with a reduced velocity.
                    if (m_StartChargeTime != -1 && Time.time > m_StartChargeTime + m_MinChargeLength)
                    {
                        var strength = Mathf.Clamp01((Time.time - m_StartChargeTime) /
                                                     (m_FullChargeLength - m_MinChargeLength));
                        strength = m_MinChargeStrength + (1 - m_MinChargeStrength) * strength;
                        Fire(strength);
                    }
                    else
                    {
                        m_StartChargeTime = -1;
                        m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
                    }

                    SchedulerBase.Cancel(m_ChargeEvent);
                    m_ChargeEvent = null;
                }
                else if (m_FireType == FireType.ChargeAndHold)
                {
                    Fire(1);
                }
            }
        }

        /// <summary>
        ///     Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            // An instant fire type must fire at least once before it can be stopped.
            if (m_FireType == FireType.Instant && (m_Firing || !m_FacingTarget)) return false;

            // The full burst amount should complete before the item is stopped.
            if (m_FireMode == FireMode.Burst && m_UseCount < m_BurstCount &&
                m_Inventory.GetItemIdentifierAmount(m_ConsumableItemIdentifier) > 0) return false;

            return base.CanStopItemUse();
        }

        /// <summary>
        ///     Stops the item use.
        /// </summary>
        public override void StopItemUse()
        {
            base.StopItemUse();

            m_FireModeCanUse = true;
            m_Firing = false;
            if (m_BurstEvent != null)
            {
                SchedulerBase.Cancel(m_BurstEvent);
                m_BurstEvent = null;
            }
        }

        /// <summary>
        ///     Determines the total amount of the consumable ItemIdentifier that needs to be reloaded.
        ///     This method only needs to be used if there are multiple active shared consumable ItemIdentifiers.
        /// </summary>
        private void DetermineTotalReloadAmount()
        {
            m_TotalReloadAmount = m_ClipSize - ClipRemaining;
            m_TotalClipAmount = ClipRemaining;
            if (m_ActiveSharedConsumableItemIdentifierAmount > 0)
                for (var i = 0; i < m_Inventory.SlotCount; ++i)
                {
                    var item = m_Inventory.GetActiveItem(i);
                    if (item != null)
                    {
                        var shootableWeapons = item.gameObject.GetCachedComponents<ShootableWeapon>();
                        for (var j = 0; j < shootableWeapons.Length; ++j)
                        {
                            if (shootableWeapons[j] == this) continue;

                            m_TotalReloadAmount += shootableWeapons[j].ClipSize - shootableWeapons[j].ClipRemaining;
                            m_TotalClipAmount += shootableWeapons[j].ClipRemaining;
                        }
                    }
                }

            m_TotalConsumableAmount = m_Inventory.GetItemIdentifierAmount(m_ConsumableItemIdentifier);
        }

        /// <summary>
        ///     The clip has been detached from the weapon.
        /// </summary>
        private void DetachClip()
        {
            if (!m_Reloading) return;

            // Attach the clip to the attachment transform. Attach both first and third person in case there is a perspective switch.
#if FIRST_PERSON_CONTROLLER
            DetachAttachClip(true, m_FirstPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, true);
#endif
            DetachAttachClip(true, m_ThirdPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, false);

            // Clean up from the detach event.
            if (m_ReloadDetachAttachClipEvent != null)
            {
                SchedulerBase.Cancel(m_ReloadDetachAttachClipEvent);
                m_ReloadDetachAttachClipEvent = null;
            }

            // The clip can actually be dropped.
            if (m_ReloadDropClip != null)
                if (!m_ReloadDropClipEvent.WaitForAnimationEvent)
                    m_ReloadClipEvent = SchedulerBase.ScheduleFixed(m_ReloadDropClipEvent.Duration, DropClip);

            // Schedule the event which will attach clip back to the weapon.
            if (!m_ReloadAttachClipEvent.WaitForAnimationEvent)
                m_ReloadDetachAttachClipEvent =
                    SchedulerBase.ScheduleFixed(m_ReloadAttachClipEvent.Duration, AttachClip);
        }

        /// <summary>
        ///     Detaches or attaches the clip from the weapon.
        /// </summary>
        /// <param name="detach">Should the clip be detached? If false the clip will be attached.</param>
        /// <param name="shootableWeaponPerspectiveProperties">
        ///     A reference to the perspective's
        ///     IShootableWeaponPerspectiveProperties.
        /// </param>
        /// <param name="firstPerson">Is the first person perspective being affected?</param>
        private void DetachAttachClip(bool detach,
            IShootableWeaponPerspectiveProperties shootableWeaponPerspectiveProperties, bool firstPerson)
        {
            // If the perspective properties is null then that perspective isn't setup for the character.
            if (shootableWeaponPerspectiveProperties == null) return;

            // Don't do anything if the clip doesn't exist or the clip shouldn't be detached.
            var clip = shootableWeaponPerspectiveProperties.ReloadableClip;
            if (clip == null || (!m_ReloadDetachAttachClip && detach)) return;

            // If detaching then set the clip's parent from the weapon to the attachment object. Attaching will set the clip's parent from the attachment
            // object back to the weapon.
            if (detach)
            {
                if (firstPerson)
                {
                    if (m_FirstPersonReloadableClipParent == null)
                    {
                        m_FirstPersonReloadableClipParent = clip.parent;
                        m_FirstPersonReloadableClipLocalPosition = clip.localPosition;
                        m_FirstPersonReloadableClipLocalRotation = clip.localRotation;
                    }
                }
                else
                {
                    if (m_ThirdPersonReloadableClipParent == null)
                    {
                        m_ThirdPersonReloadableClipParent = clip.parent;
                        m_ThirdPersonReloadableClipLocalPosition = clip.localPosition;
                        m_ThirdPersonReloadableClipLocalRotation = clip.localRotation;
                    }
                }

                clip.parent = shootableWeaponPerspectiveProperties.ReloadableClipAttachment;
            }
            else
            {
                if (firstPerson)
                {
                    if (m_FirstPersonReloadableClipParent != null)
                    {
                        clip.parent = m_FirstPersonReloadableClipParent;
                        clip.localPosition = m_FirstPersonReloadableClipLocalPosition;
                        clip.localRotation = m_FirstPersonReloadableClipLocalRotation;
                        m_FirstPersonReloadableClipParent = null;
                    }
                }
                else
                {
                    if (m_ThirdPersonReloadableClipParent != null)
                    {
                        clip.parent = m_ThirdPersonReloadableClipParent;
                        clip.localPosition = m_ThirdPersonReloadableClipLocalPosition;
                        clip.localRotation = m_ThirdPersonReloadableClipLocalRotation;
                        m_ThirdPersonReloadableClipParent = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Drops the weapon's clip.
        /// </summary>
        private void DropClip()
        {
            if (!m_Reloading || m_ReloadDropClip == null || m_DroppedClip) return;

            // Hide the existing clip and drop a new prefab.
            var clip = (m_ActivePerspectiveProperties as IShootableWeaponPerspectiveProperties).ReloadableClip;
            if (clip == null) return;

            if (m_ReloadDetachAttachClip) clip.gameObject.SetActive(false);

            var dropClip = ObjectPoolBase.Instantiate(m_ReloadDropClip, clip.position, clip.rotation);
            // The first person perspective requires the clip to be on the overlay layer so the fingers won't render in front of the clip.
            dropClip.transform.SetLayerRecursively(m_CharacterLocomotion.FirstPersonPerspective
                ? LayerManager.Overlay
                : clip.gameObject.layer);
            SchedulerBase.Schedule(m_ReloadClipLayerChangeDelay, UpdateDropClipLayer, dropClip);
            m_ReloadClipEvent = null;

            // If the clip has a trajectory object attached then it needs to be initialized.
            var trajectoryClipObject = dropClip.GetCachedComponent<TrajectoryObject>();
            if (trajectoryClipObject != null) trajectoryClipObject.Initialize(Vector3.zero, Vector3.zero, m_Character);

            // Cleanup from the event.
            if (m_ReloadClipEvent != null)
            {
                SchedulerBase.Cancel(m_ReloadClipEvent);
                m_ReloadClipEvent = null;
            }

            m_DroppedClip = true;
        }

        /// <summary>
        ///     Updates the dropped clip layer.
        /// </summary>
        /// <param name="dropClip">The object that should have its layer updated.</param>
        private void UpdateDropClipLayer(GameObject dropClip)
        {
            dropClip.transform.SetLayerRecursively(m_ReloadClipTargetLayer);
        }

        /// <summary>
        ///     The clip has been detached form the weapon.
        /// </summary>
        private void AttachClip()
        {
            if (!m_Reloading) return;

            // Attach the clip back to the original transform. Attach both first and third person in case there is a perspective switch.
#if FIRST_PERSON_CONTROLLER
            DetachAttachClip(false, m_FirstPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, true);
            AddRemoveReloadableClip(false, m_FirstPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, true);
#endif
            DetachAttachClip(false, m_ThirdPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties, false);
            AddRemoveReloadableClip(false, m_ThirdPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties,
                false);

            // Clean up from the event.
            if (m_ReloadDetachAttachClipEvent != null)
            {
                SchedulerBase.Cancel(m_ReloadDetachAttachClipEvent);
                m_ReloadDetachAttachClipEvent = null;
            }
        }

        /// <summary>
        ///     Shows the reload projectile at the attachment location.
        /// </summary>
        private void ShowReloadProjectile()
        {
            m_ShowReloadProjectile = ShowProjectileStatus.AttachmentLocation;
            DetermineVisibleProjectile(false);

            // The projectile will be attached to the fire point in the future.
            if (!ReloadAttachProjectileEvent.WaitForAnimationEvent)
                m_ReloadProjectileEvent =
                    SchedulerBase.ScheduleFixed(ReloadAttachProjectileEvent.Duration, AttachReloadProjectile);
            else
                m_ReloadProjectileEvent = null;
        }

        /// <summary>
        ///     Attaches the reload projectile to the fire point.
        /// </summary>
        private void AttachReloadProjectile()
        {
            m_ShowReloadProjectile = ShowProjectileStatus.FirePointLocation;
            DetermineVisibleProjectile(false);
        }

        /// <summary>
        ///     Adds or removes the instantiated reloadable clip.
        /// </summary>
        /// <param name="add">Should the reloadable clip be instantiated?</param>
        /// <param name="shootableWeaponPerspectiveProperties">
        ///     A reference to the perspective's
        ///     IShootableWeaponPerspectiveProperties.
        /// </param>
        /// <param name="firstPerson">Is the first person perspective being affected?</param>
        private void AddRemoveReloadableClip(bool add,
            IShootableWeaponPerspectiveProperties shootableWeaponPerspectiveProperties, bool firstPerson)
        {
            // If the perspective properties is null then that perspective isn't setup for the character.
            if (shootableWeaponPerspectiveProperties == null || m_ReloadDropClip == null) return;

            // If the clip can't be detached then the weapon's clip shouldn't be disabled.
            if (!m_ReloadDetachAttachClip) return;

            if (add)
            {
                var clip = ObjectPoolBase.Instantiate(m_ReloadDropClip,
                    shootableWeaponPerspectiveProperties.ReloadableClip.position,
                    shootableWeaponPerspectiveProperties.ReloadableClip.rotation);
                var remover = clip.GetCachedComponent<Remover>();
                if (remover != null) remover.CancelRemoveEvent();
                // The first person perspective requires the clip to be on the overlay layer so the fingers won't render in front of the clip.
                clip.transform.SetLayerRecursively(firstPerson ? LayerManager.Overlay : clip.layer);
                clip.transform.SetParentOrigin(shootableWeaponPerspectiveProperties.ReloadableClipAttachment);
                clip.transform.SetPositionAndRotation(shootableWeaponPerspectiveProperties.ReloadableClip.position,
                    shootableWeaponPerspectiveProperties.ReloadableClip.rotation);
                if (firstPerson)
                {
                    m_FirstPersonReloadAddClip = clip;
                    m_FirstPersonReloadAddClip.SetActive(m_CharacterLocomotion.FirstPersonPerspective);
                }
                else
                {
                    m_ThirdPersonReloadAddClip = clip;
                    m_ThirdPersonReloadAddClip.SetActive(!m_CharacterLocomotion.FirstPersonPerspective);
                }
            }
            else
            {
                var clip = firstPerson ? m_FirstPersonReloadAddClip : m_ThirdPersonReloadAddClip;
                if (clip != null)
                {
                    ObjectPoolBase.Destroy(clip);
                    if (firstPerson)
                        m_FirstPersonReloadAddClip = null;
                    else
                        m_ThirdPersonReloadAddClip = null;
                }

                shootableWeaponPerspectiveProperties.ReloadableClip.gameObject.SetActive(true);
            }
        }

        /// <summary>
        ///     An item has started or stopped being used. If the projectile is visible then it may need to be disabled.
        /// </summary>
        /// <param name="usableItem">The item that is being used.</param>
        /// <param name="start">True if the item is starting to be used.</param>
        private void ItemStartUse(IUsableItem usableItem, bool start)
        {
            if (usableItem as ItemAction == this) return;

            if (!start)
                SchedulerBase.ScheduleFixed(m_ProjectileEnableDelayAfterOtherUse, DetermineVisibleProjectile, start);
            else
                DetermineVisibleProjectile(start);
        }

        /// <summary>
        ///     The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        protected override void OnChangePerspectives(bool firstPersonPerspective)
        {
            base.OnChangePerspectives(firstPersonPerspective);

            if (m_ActivePerspectiveProperties != null)
            {
                ShootableWeaponPerspectiveProperties =
                    m_ActivePerspectiveProperties as IShootableWeaponPerspectiveProperties;
                // The FirePointLocation cannot be null.
                if (ShootableWeaponPerspectiveProperties.FirePointLocation == null)
                    Debug.LogError(
                        $"Error: The FirePointLocation is null on the ShootableWeaponPerspectiveProperties of the {name}.");

                DetermineVisibleScopeCamera();
            }

            if (m_SpawnedProjectile != null)
            {
                if (m_ShowReloadProjectile != ShowProjectileStatus.NotShown)
                {
                    if (m_ShowReloadProjectile == ShowProjectileStatus.AttachmentLocation)
                    {
                        m_SpawnedProjectile.transform.SetParentOrigin(ShootableWeaponPerspectiveProperties
                            .ReloadProjectileAttachment);
                        m_SpawnedProjectile.layer = ShootableWeaponPerspectiveProperties.ReloadProjectileAttachment
                            .gameObject.layer;
                    }
                    else
                    {
                        // Keep the projectile in the same relative location.
                        var localPosition = m_SpawnedProjectile.transform.localPosition;
                        var localRotation = m_SpawnedProjectile.transform.localRotation;
                        m_SpawnedProjectile.transform.parent = ShootableWeaponPerspectiveProperties.FirePointLocation;
                        m_SpawnedProjectile.transform.localPosition = localPosition;
                        m_SpawnedProjectile.transform.localRotation = localRotation;
                    }
                }
                else
                {
                    m_SpawnedProjectile.transform.SetParentOrigin(
                        ShootableWeaponPerspectiveProperties.FirePointLocation);
                }
            }

            // If the clip has been dropped then the opposite perspective add clip should be disabled.
            if (m_FirstPersonReloadAddClip != null && m_ThirdPersonReloadAddClip != null)
            {
                m_FirstPersonReloadAddClip.SetActive(firstPersonPerspective);
                m_ThirdPersonReloadAddClip.SetActive(!firstPersonPerspective);
            }

            if (!firstPersonPerspective)
                // The scope camera exists doesn't need to render while in third person view.
                if (m_FirstPersonPerspectiveProperties != null)
                {
                    var firstPersonShootableWeaponPerspectiveProperties =
                        m_FirstPersonPerspectiveProperties as IShootableWeaponPerspectiveProperties;
                    if (firstPersonShootableWeaponPerspectiveProperties.ScopeCamera != null)
                        firstPersonShootableWeaponPerspectiveProperties.ScopeCamera.SetActive(false);
                }
        }

        /// <summary>
        ///     The item has started to be unequipped by the character.
        /// </summary>
        public override void StartUnequip()
        {
            base.StartUnequip();

            if (!m_Aiming) DetermineVisibleProjectile(true);
        }

        /// <summary>
        ///     The item has been unequipped by the character.
        /// </summary>
        public override void Unequip()
        {
            base.Unequip();

            DetermineVisibleProjectile(true);
            EventHandler.UnregisterEvent<IUsableItem, bool>(m_Character, "OnItemStartUse", ItemStartUse);
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorItemReloadDetachClip", DetachClip);
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorItemReloadAttachClip", AttachClip);
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorItemReloadDropClip", DropClip);
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorItemReloadShowProjectile", ShowReloadProjectile);
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorItemReloadAttachProjectile", AttachReloadProjectile);
        }

        /// <summary>
        ///     The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(Item item)
        {
            CheckForSharedConsumableItemIdentifiers(item.GetComponents<ShootableWeapon>());
        }

        /// <summary>
        ///     Determines if the ShootableWeapon is sharing the consumable ItemIdentifier with another ShootableWeapon.
        /// </summary>
        /// <param name="shootableWeapons">The array of ShootableWeapons which have been added to the character.</param>
        private void CheckForSharedConsumableItemIdentifiers(ShootableWeapon[] shootableWeapons)
        {
            if (m_ConsumableItemIdentifier == null) return;

            for (var i = 0; i < shootableWeapons.Length; ++i)
            {
                if (shootableWeapons[i] == this) continue;

                // Increase the count if the ItemIdentifiers match. The current ShootableWeapon is sharing ItemIdentifiers.
                if (shootableWeapons[i].GetConsumableItemIdentifier() == m_ConsumableItemIdentifier)
                {
                    if (!m_SharedConsumableItemIdentifier)
                    {
                        m_SharedConsumableItemIdentifier = true;
                        m_SharedConsumableItemIdentifierAmountMap = new Dictionary<Item, int>();
                    }

                    var count = 0;
                    m_SharedConsumableItemIdentifierAmountMap.TryGetValue(shootableWeapons[i].Item, out count);
                    count++;
                    m_SharedConsumableItemIdentifierAmountMap[shootableWeapons[i].Item] = count;
                }
            }
        }

        /// <summary>
        ///     An item has been equipped.
        /// </summary>
        /// <param name="item">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Item item, int slotID)
        {
            if (!m_SharedConsumableItemIdentifier) return;

            int amount;
            m_SharedConsumableItemIdentifierAmountMap.TryGetValue(item, out amount);
            m_ActiveSharedConsumableItemIdentifierAmount += amount;
        }

        /// <summary>
        ///     An item has been unequipped.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (!m_SharedConsumableItemIdentifier) return;

            int amount;
            m_SharedConsumableItemIdentifierAmountMap.TryGetValue(item, out amount);
            m_ActiveSharedConsumableItemIdentifierAmount -= amount;
        }

        /// <summary>
        ///     The item has been removed by the character.
        /// </summary>
        public override void Remove()
        {
            m_ReloadInitialized = false;
        }

        /// <summary>
        ///     Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            base.StateChange();

            if (m_ActivePerspectiveProperties != null) DetermineVisibleProjectile(false);
        }

        /// <summary>
        ///     Specifies the current status of the shown projectile.
        /// </summary>
        private enum ShowProjectileStatus
        {
            NotShown, // The projectile is not currently shown.
            AttachmentLocation, // The projectile is parented to the attachment location.
            FirePointLocation // The projectile is parented to the fire point location.
        }
    }
}