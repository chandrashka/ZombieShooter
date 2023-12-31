﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Collections.Generic;
using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.Inventory;
using Opsive.Shared.StateSystem;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Opsive.UltimateCharacterController.Items
{
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking;
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_VR
    using Opsive.UltimateCharacterController.VR;
#endif

    /// <summary>
    ///     An item represents anything that can be picked up by the character.
    /// </summary>
    public class Item : StateBehavior
    {
        [Tooltip("A reference to the object used to identify the item.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_ItemType")]
        [SerializeField]
        protected ItemDefinitionBase m_ItemDefinition;

        [Tooltip("Specifies the inventory slot/spawn location of the item.")] [SerializeField]
        protected int m_SlotID;

        [Tooltip("Unique ID used for item identification within the animator.")] [SerializeField]
        protected int m_AnimatorItemID;

        [Tooltip("The movement set ID used for within the animator.")] [SerializeField]
        protected int m_AnimatorMovementSetID;

        [Tooltip("Does the item control the movement and the UI shown?")] [SerializeField]
        protected bool m_DominantItem = true;

        [Tooltip("Does the item belong to a unique ItemSet?")] [SerializeField]
        protected bool m_UniqueItemSet = true;

        [Tooltip("Can the camera zoom when the item is equipped?")] [SerializeField]
        protected bool m_AllowCameraZoom = true;

        [Tooltip("The GameObject that is dropped when the item is removed from the character.")] [SerializeField]
        protected GameObject m_DropPrefab;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
        [Tooltip("The multiplier to apply to the velocity when the item is dropped.")]
        [SerializeField] protected float m_DropVelocityMultiplier = 4;
#endif
        [Tooltip(
            "When the item is dropped should the entire item be dropped? Throwable Items will want this option enabled.")]
        [SerializeField]
        protected bool m_FullInventoryDrop;

        [Tooltip("Should the consumable items be dropped along with the current item?")] [SerializeField]
        protected bool m_DropConsumableItems = true;

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemEquip animation event or wait for the specified duration before equipping.")]
        [SerializeField]
        protected AnimationSlotEventTrigger m_EquipEvent = new(true, 0.3f);

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemEquipComplete animation event or wait for the specified duration before stopping the equip ability.")]
        [SerializeField]
        protected AnimationSlotEventTrigger m_EquipCompleteEvent = new(false, 0f);

        [Tooltip("Specifies the animator and audio state from an equip.")] [SerializeField]
        protected AnimatorAudioStateSet m_EquipAnimatorAudioStateSet = new();

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemUnequip animation event or wait for the specified duration before unequipping.")]
        [SerializeField]
        protected AnimationSlotEventTrigger m_UnequipEvent = new(true, 0.3f);

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemUnequipComplete animation event or wait for the specified duration before stopping the unequip ability.")]
        [SerializeField]
        protected AnimationSlotEventTrigger m_UnequipCompleteEvent = new(false, 0);

        [Tooltip("Specifies the animator and audio state from an unequip.")] [SerializeField]
        protected AnimatorAudioStateSet m_UnequipAnimatorAudioStateSet = new();

        [Tooltip("The ID of the UI Monitor that the item should use.")] [SerializeField]
        protected int m_UIMonitorID;

        [Tooltip("The sprite representing the icon.")] [SerializeField]
        protected Sprite m_Icon;

        [Tooltip("Should the crosshairs be shown when the item aims?")] [SerializeField]
        protected bool m_ShowCrosshairsOnAim = true;

        [Tooltip("The sprite used for the center crosshairs image.")] [SerializeField]
        protected Sprite m_CenterCrosshairs;

        [Tooltip("The offset of the quadrant crosshairs sprites.")] [SerializeField]
        protected float m_QuadrantOffset = 5f;

        [Tooltip("The max spread of the quadrant crosshairs sprites caused by a recoil or reload.")] [SerializeField]
        protected float m_MaxQuadrantSpread = 10f;

        [Tooltip("The amount of damping to apply to the spread offset.")] [SerializeField]
        protected float m_QuadrantSpreadDamping = 0.05f;

        [Tooltip("The sprite used for the left crosshairs image.")] [SerializeField]
        protected Sprite m_LeftCrosshairs;

        [Tooltip("The sprite used for the top crosshairs image.")] [SerializeField]
        protected Sprite m_TopCrosshairs;

        [Tooltip("The sprite used for the right crosshairs image.")] [SerializeField]
        protected Sprite m_RightCrosshairs;

        [Tooltip("The sprite used for the bottom crosshairs image.")] [SerializeField]
        protected Sprite m_BottomCrosshairs;

        [Tooltip("Should the item's full screen UI be shown?")] [SerializeField]
        protected bool m_ShowFullScreenUI;

        [Tooltip("The ID of the full screen UI. This must match the ID within the FullScreenItemUIMonitor.")]
        [SerializeField]
        protected int m_FullScreenUIID = -1;

        [Tooltip("Unity event that is invoked when the item is picked up.")] [SerializeField]
        protected UnityEvent m_PickupItemEvent;

        [Tooltip("Unity event that is invoked when the item is equipped.")] [SerializeField]
        protected UnityEvent m_EquipItemEvent;

        [Tooltip("Unity event that is invoked when the item is unequipped.")] [SerializeField]
        protected UnityEvent m_UnequipItemEvent;

        [Tooltip("Unity event that is invoked when the item is dropped.")] [SerializeField]
        protected UnityEvent m_DropItemEvent;

        [NonSerialized]
        public ItemDefinitionBase ItemDefinition
        {
            get => m_ItemDefinition;
            set => m_ItemDefinition = value;
        }

        [NonSerialized]
        public int SlotID
        {
            get => m_SlotID;
            set => m_SlotID = value;
        }

        [NonSerialized]
        public int AnimatorItemID
        {
            get => m_AnimatorItemID;
            set => m_AnimatorItemID = value;
        }

        [NonSerialized]
        public int AnimatorMovementSetID
        {
            get => m_AnimatorMovementSetID;
            set => m_AnimatorMovementSetID = value;
        }

        public bool DominantItem
        {
            get => m_DominantItem;
            set
            {
                m_DominantItem = value;
                if (Application.isPlaying)
                    EventHandler.ExecuteEvent(m_Character, "OnItemUpdateDominantItem", this, m_DominantItem);
            }
        }

        public bool UniqueItemSet
        {
            get => m_UniqueItemSet;
            set => m_UniqueItemSet = value;
        }

        public bool AllowCameraZoom
        {
            get => m_AllowCameraZoom;
            set => m_AllowCameraZoom = value;
        }

        public GameObject DropPrefab
        {
            get => m_DropPrefab;
            set => m_DropPrefab = value;
        }
#if ULTIMATE_CHARACTER_CONTROLLER_VR
        public float DropVelocityMultiplier { get { return m_DropVelocityMultiplier; } set { m_DropVelocityMultiplier =
 value; } }
#endif
        public bool FullInventoryDrop
        {
            get => m_FullInventoryDrop;
            set => m_FullInventoryDrop = value;
        }

        public bool DropConsumableItems
        {
            get => m_DropConsumableItems;
            set => m_DropConsumableItems = value;
        }

        public AnimationSlotEventTrigger EquipEvent
        {
            get => m_EquipEvent;
            set => m_EquipEvent = value;
        }

        public AnimationSlotEventTrigger EquipCompleteEvent
        {
            get => m_EquipCompleteEvent;
            set => m_EquipCompleteEvent = value;
        }

        public AnimatorAudioStateSet EquipAnimatorAudioStateSet
        {
            get => m_EquipAnimatorAudioStateSet;
            set => m_EquipAnimatorAudioStateSet = value;
        }

        public AnimationSlotEventTrigger UnequipEvent
        {
            get => m_UnequipEvent;
            set => m_UnequipEvent = value;
        }

        public AnimationSlotEventTrigger UnequipCompleteEvent
        {
            get => m_UnequipCompleteEvent;
            set => m_UnequipCompleteEvent = value;
        }

        public AnimatorAudioStateSet UnequipAnimatorAudioStateSet
        {
            get => m_UnequipAnimatorAudioStateSet;
            set => m_UnequipAnimatorAudioStateSet = value;
        }

        public int UIMonitorID
        {
            get => m_UIMonitorID;
            set => m_UIMonitorID = value;
        }

        public Sprite Icon
        {
            get => m_Icon;
            set => m_Icon = value;
        }

        public bool ShowCrosshairsOnAim
        {
            get => m_ShowCrosshairsOnAim;
            set => m_ShowCrosshairsOnAim = value;
        }

        public Sprite CenterCrosshairs => m_CenterCrosshairs;

        public float QuadrantOffset
        {
            get => m_QuadrantOffset;
            set => m_QuadrantOffset = value;
        }

        public float MaxQuadrantSpread
        {
            get => m_MaxQuadrantSpread;
            set => m_MaxQuadrantSpread = value;
        }

        public float QuadrantSpreadDamping
        {
            get => m_QuadrantSpreadDamping;
            set => m_QuadrantSpreadDamping = value;
        }

        public Sprite LeftCrosshairs => m_LeftCrosshairs;
        public Sprite TopCrosshairs => m_TopCrosshairs;
        public Sprite RightCrosshairs => m_RightCrosshairs;
        public Sprite BottomCrosshairs => m_BottomCrosshairs;

        public bool ShowFullScreenUI
        {
            get => m_ShowFullScreenUI;
            set
            {
                m_ShowFullScreenUI = value;
                if (Application.isPlaying && DominantItem && IsActive())
                    EventHandler.ExecuteEvent(m_Character, "OnItemShowFullScreenUI", m_FullScreenUIID,
                        m_ShowFullScreenUI);
            }
        }

        [NonSerialized]
        public int FullScreenUIID
        {
            get => m_FullScreenUIID;
            set => m_FullScreenUIID = value;
        }

        public UnityEvent PickupItemEvent
        {
            get => m_PickupItemEvent;
            set => m_PickupItemEvent = value;
        }

        public UnityEvent EquipItemEvent
        {
            get => m_EquipItemEvent;
            set => m_EquipItemEvent = value;
        }

        public UnityEvent UnequipItemEvent
        {
            get => m_UnequipItemEvent;
            set => m_UnequipItemEvent = value;
        }

        public UnityEvent DropItemEvent
        {
            get => m_DropItemEvent;
            set => m_DropItemEvent = value;
        }

        private GameObject m_GameObject;
        protected GameObject m_Character;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected InventoryBase m_Inventory;
        protected PerspectiveItem m_ActivePerspectiveItem;
#if FIRST_PERSON_CONTROLLER
        private PerspectiveItem m_FirstPersonPerspectiveItem;
        private GameObject[] m_FirstPersonObjects;
        private ChildAnimatorMonitor[] m_FirstPersonObjectsAnimatorMonitor;
        private ChildAnimatorMonitor m_FirstPersonPerspectiveItemAnimatorMonitor;
#endif
        private ChildAnimatorMonitor m_ThirdPersonItemAnimatorMonitor;
        private Dictionary<int, ItemAction> m_IDItemActionMap;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_VR
        private IVRHandHandler m_HandHandler;
        public IVRHandHandler HandHandler { get { return m_HandHandler; } }
#endif

        private bool m_Started;

        public GameObject Character => m_Character;
        public UltimateCharacterLocomotion CharacterLocomotion => m_CharacterLocomotion;
        public PerspectiveItem ActivePerspectiveItem => m_ActivePerspectiveItem;

        public PerspectiveItem FirstPersonPerspectiveItem
        {
            get
            {
#if FIRST_PERSON_CONTROLLER
                return m_FirstPersonPerspectiveItem;
#else
                return null;
#endif
            }
        }

        public IItemIdentifier ItemIdentifier { get; set; }

        public PerspectiveItem ThirdPersonPerspectiveItem { get; private set; }

        public ItemAction[] ItemActions { get; private set; }

        public bool VisibleObjectActive { get; private set; }

        public int UnequipDropAmount { get; set; }

        public Vector3 UnequpDropPosition { get; set; }

        public Quaternion UnequipDropRotation { get; set; }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            m_GameObject = gameObject;
            m_CharacterLocomotion = m_GameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_Character = m_CharacterLocomotion.gameObject;
            m_Inventory = m_Character.GetCachedComponent<InventoryBase>();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_Character.GetCachedComponent<INetworkInfo>();
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            m_HandHandler = m_Character.GetCachedComponent<IVRHandHandler>();
#endif

            // Item Definitions can be assigned after the item is active.
            if (m_ItemDefinition != null && ItemIdentifier == null)
                ItemIdentifier = m_ItemDefinition.CreateItemIdentifier();

            base.Awake();

            // Find the PerspectiveItems/ItemActions.
            var perspectiveItems = GetComponents<PerspectiveItem>();
            for (var i = 0; i < perspectiveItems.Length; ++i)
            {
                // Initialize the perspective item manually to ensure an Object GameObject exists. This is important because the Item component will execute
                // before the FirstPersonPerspectiveItem component, but the FirstPersonPerspectiveItem component may not be completely initialized.
                // The FirstPersonPerspectiveItem component must be initialized after Item so Item.Start can be called and add the item to the inventory.
                if (!perspectiveItems[i].Initialize(m_Character)) continue;

                if (perspectiveItems[i].FirstPersonItem)
                {
#if FIRST_PERSON_CONTROLLER
                    var firstPersonPerspectiveItem =
 perspectiveItems[i] as FirstPersonController.Items.FirstPersonPerspectiveItem;
                    if (firstPersonPerspectiveItem.Object != null) {
                        var baseAnimatorMonitor =
 firstPersonPerspectiveItem.Object.GetComponent<ChildAnimatorMonitor>();
                        if (baseAnimatorMonitor != null) {
                            m_FirstPersonObjects =
 new GameObject[firstPersonPerspectiveItem.AdditionalControlObjects.Length + 1];
                            m_FirstPersonObjectsAnimatorMonitor =
 new ChildAnimatorMonitor[firstPersonPerspectiveItem.AdditionalControlObjects.Length + 1];
                            m_FirstPersonObjects[0] = baseAnimatorMonitor.gameObject;
                            m_FirstPersonObjectsAnimatorMonitor[0] = baseAnimatorMonitor;
                            for (int j = 0; j < firstPersonPerspectiveItem.AdditionalControlObjects.Length; ++j) {
                                m_FirstPersonObjects[j + 1] = firstPersonPerspectiveItem.AdditionalControlObjects[j];
                                m_FirstPersonObjectsAnimatorMonitor[j + 1] =
 firstPersonPerspectiveItem.AdditionalControlObjects[j].GetComponent<ChildAnimatorMonitor>();
                            }
                        }
                    } else {
                        // The character doesn't have a first person perspective setup.
                        continue;
                    }
                    m_FirstPersonPerspectiveItem = perspectiveItems[i];

                    var visibleItem = firstPersonPerspectiveItem.VisibleItem;
                    if (visibleItem != null) {
                        m_FirstPersonPerspectiveItemAnimatorMonitor = visibleItem.GetComponent<ChildAnimatorMonitor>();
                    }
#endif
                }
                else
                {
                    ThirdPersonPerspectiveItem = perspectiveItems[i];
                    m_ThirdPersonItemAnimatorMonitor = perspectiveItems[i].Object != null
                        ? perspectiveItems[i].Object.GetComponent<ChildAnimatorMonitor>()
                        : null;
                }
            }

            ItemActions = GetComponents<ItemAction>();
            if (ItemActions.Length > 1)
            {
                m_IDItemActionMap = new Dictionary<int, ItemAction>();
                for (var i = 0; i < ItemActions.Length; ++i) m_IDItemActionMap.Add(ItemActions[i].ID, ItemActions[i]);
            }

            m_EquipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(this, m_CharacterLocomotion);
            m_UnequipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(this, m_CharacterLocomotion);
            m_EquipAnimatorAudioStateSet.Awake(m_GameObject);
            m_UnequipAnimatorAudioStateSet.Awake(m_GameObject);

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        ///     Adds the item to the inventory and initializes the non-local network player.
        /// </summary>
        private void Start()
        {
            // Start may have already been called within Pickup.
            if (m_Started) return;
            m_Started = true;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            var remotePlayer = false;
            // Perform any initialization for a non-local network player.
            if (m_NetworkInfo != null && !m_NetworkInfo.IsLocalPlayer()) {
#if FIRST_PERSON_CONTROLLER
                // First person items do not need to be updated for remote players.
                if (m_FirstPersonPerspectiveItem != null) {
                    m_FirstPersonPerspectiveItem = null;
                    m_FirstPersonObjects = null;
                    m_FirstPersonObjectsAnimatorMonitor = null;
                    m_FirstPersonPerspectiveItemAnimatorMonitor = null;
                }
#endif

                // Remote players should always be in the third person view.
                OnChangePerspectives(false);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
                remotePlayer = true;
            }
#endif

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.ItemStarted();
            }
#endif
            if (ThirdPersonPerspectiveItem != null) ThirdPersonPerspectiveItem.ItemStarted();
            SetVisibleObjectActive(false, m_Inventory.GetItemIdentifierAmount(ItemIdentifier) > 0);

            // Set the correct visible object for the current perspective.
            if (m_CharacterLocomotion.FirstPersonPerspective
#if FIRST_PERSON_CONTROLLER
                && m_FirstPersonPerspectiveItem != null
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                && !remotePlayer
#endif
               )
            {
#if FIRST_PERSON_CONTROLLER
                m_ActivePerspectiveItem = m_FirstPersonPerspectiveItem;
#endif
            }
            else
            {
                m_ActivePerspectiveItem = ThirdPersonPerspectiveItem;
            }

            // The character should ignore any of the item's colliders.
            var colliders = GetComponents<Collider>();
            for (var i = 0; i < colliders.Length; ++i) m_CharacterLocomotion.AddIgnoredCollider(colliders[i]);

            // The item may have already been added at runtime.
            if (!m_Inventory.HasItem(this)) m_Inventory.AddItem(this, true, false);
        }

        /// <summary>
        ///     The item has been picked up by the character.
        /// </summary>
        public virtual void Pickup()
        {
            // The item will not be started if the item is picked up at runtime.
            if (!m_Started) Start();

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Pickup();
            }
#endif

            if (ThirdPersonPerspectiveItem != null) ThirdPersonPerspectiveItem.Pickup();

            if (ItemActions != null)
                for (var i = 0; i < ItemActions.Length; ++i)
                    ItemActions[i].Pickup();
            if (m_PickupItemEvent != null) m_PickupItemEvent.Invoke();
        }

        /// <summary>
        ///     Returns the ItemAction based on the ID.
        /// </summary>
        /// <param name="id">The ID of the ItemAction to retrieve.</param>
        /// <returns>The ItemAction that corresponds to the specified ID.</returns>
        public ItemAction GetItemAction(int id)
        {
            if (ItemActions == null || ItemActions.Length == 0) return null;

            if (ItemActions.Length == 1)
            {
                // The ID must match.
                if (ItemActions[0].ID == id || id == -1) return ItemActions[0];
                return null;
            }

            // Multiple actions exist - look up the action based on the ID.
            ItemAction itemAction;
            if (m_IDItemActionMap.TryGetValue(id, out itemAction)) return itemAction;

            // The action with the specified ID wasn't found.
            return null;
        }

        /// <summary>
        ///     The item will be equipped.
        /// </summary>
        public void WillEquip()
        {
            if (ItemActions != null)
                for (var i = 0; i < ItemActions.Length; ++i)
                    ItemActions[i].WillEquip();
        }

        /// <summary>
        ///     Starts to equip the item.
        /// </summary>
        /// <param name="immediateEquip">
        ///     Is the item being equipped immediately? Immediate equips will occur from the default
        ///     loadout or quickly switching to the item.
        /// </param>
        public void StartEquip(bool immediateEquip)
        {
            if (immediateEquip)
            {
                SetVisibleObjectActive(true, true);
            }
            else
            {
                // The equip AnimatorAudioState is starting.
                m_EquipAnimatorAudioStateSet.StartStopStateSelection(true);
                m_EquipAnimatorAudioStateSet.NextState();
            }

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.StartEquip(immediateEquip);
            }
#endif
            if (ThirdPersonPerspectiveItem != null) ThirdPersonPerspectiveItem.StartEquip(immediateEquip);

            if (immediateEquip) SnapAnimator();
        }

        /// <summary>
        ///     The item has been equipped within the inventory.
        /// </summary>
        /// <param name="immediateEquip">
        ///     Is the item being equipped immediately? Immediate equips will occur from the default
        ///     loadout or quickly switching to the item.
        /// </param>
        public void Equip(bool immediateEquip)
        {
            // The item will not be started if the item is picked up at runtime.
            if (!m_Started) Pickup();

            SetVisibleObjectActive(true, true);

            if (!immediateEquip)
            {
                if (m_DominantItem)
                    // Optionally play an equip sound based upon the equipping animation.
                    m_EquipAnimatorAudioStateSet.PlayAudioClip(m_Character);

                // The item has been equipped- inform the state set.
                m_EquipAnimatorAudioStateSet.StartStopStateSelection(false);
            }

            if (ItemActions != null)
                for (var i = 0; i < ItemActions.Length; ++i)
                    ItemActions[i].Equip();

            if (m_EquipItemEvent != null) m_EquipItemEvent.Invoke();

            // Update the full screen UI property to handle the case when the preset has already been applied.
            ShowFullScreenUI = m_ShowFullScreenUI;
        }

        /// <summary>
        ///     Moves the item according to the horizontal and vertical movement, as well as the character velocity.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        public void Move(float horizontalMovement, float verticalMovement)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Move(horizontalMovement, verticalMovement);
            }
#endif
            if (ThirdPersonPerspectiveItem != null)
                ThirdPersonPerspectiveItem.Move(horizontalMovement, verticalMovement);
        }

        /// <summary>
        ///     Starts to unequip the item.
        /// </summary>
        /// <param name="immediateUnequip">
        ///     Is the item being unequipped immediately? Immediate unequips will occur when the
        ///     character dies.
        /// </param>
        public void StartUnequip(bool immediateUnequip)
        {
            if (!immediateUnequip)
            {
                // The unequip AnimatorAudioState is starting.
                m_UnequipAnimatorAudioStateSet.StartStopStateSelection(true);
                m_UnequipAnimatorAudioStateSet.NextState();
                if (m_DominantItem) m_UnequipAnimatorAudioStateSet.PlayAudioClip(m_Character);
            }

            // Notify any item actions of the unequip.
            if (ItemActions != null)
                for (var i = 0; i < ItemActions.Length; ++i)
                    ItemActions[i].StartUnequip();

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.StartUnequip();
            }
#endif
            if (ThirdPersonPerspectiveItem != null) ThirdPersonPerspectiveItem.StartUnequip();
        }

        /// <summary>
        ///     The item has been unequipped within the item.
        /// </summary>
        public void Unequip()
        {
            // If the item isn't a dominant item then it doesn't move the transform or set the animator parameters.
            if (m_DominantItem) SetItemIDParameter(m_SlotID, 0);

            // The item has been unequipped- inform the state set.
            m_UnequipAnimatorAudioStateSet.StartStopStateSelection(false);

            // Execute the FullScreenUI event directly without setting the variable so the varaible doesn't get reset when it is being equipped.
            if (m_ShowFullScreenUI && m_DominantItem)
                EventHandler.ExecuteEvent(m_Character, "OnItemShowFullScreenUI", m_FullScreenUIID, false);
            // When the item is unequipped it is no longer visible.
            SetVisibleObjectActive(false, m_Inventory.GetItemIdentifierAmount(ItemIdentifier) > 0);

            // Notify any item actions of the unequip.
            if (ItemActions != null)
                for (var i = 0; i < ItemActions.Length; ++i)
                    ItemActions[i].Unequip();

            // Notify the perspective items of the unequip.
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Unequip();
            }
#endif
            if (ThirdPersonPerspectiveItem != null) ThirdPersonPerspectiveItem.Unequip();

            if (m_UnequipItemEvent != null) m_UnequipItemEvent.Invoke();

            // Drop could have been called before the item was unequipped. Now that the item is unequipped it can be dropped.
            if (UnequipDropAmount > 0) Drop(UnequipDropAmount, false);
        }

        /// <summary>
        ///     Activates or deactivates the visible objects.
        /// </summary>
        /// <param name="active">Should the visible object be activated?</param>
        /// <param name="hasItem">Does the inventory contain the item?</param>
        public void SetVisibleObjectActive(bool active, bool hasItem)
        {
            if (!m_Started) return;

            var change = VisibleObjectActive != active;
            VisibleObjectActive = active;

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.SetActive(active, hasItem);
            }
#endif
            if (ThirdPersonPerspectiveItem != null) ThirdPersonPerspectiveItem.SetActive(active, hasItem);

            // The ItemActions can execute within Update so also set the enabled state based on the active state.
            for (var i = 0; i < ItemActions.Length; ++i) ItemActions[i].enabled = active;

            if (change && active && m_DominantItem) SnapAnimator();
        }

        /// <summary>
        ///     Returns the current PerspectiveItem object.
        /// </summary>
        /// <returns>The current PerspectiveItem object.</returns>
        public virtual GameObject GetVisibleObject()
        {
            return m_ActivePerspectiveItem.GetVisibleObject();
        }

        /// <summary>
        ///     Is the item active?
        /// </summary>
        /// <returns>True if the item is active.</returns>
        public bool IsActive()
        {
            return VisibleObjectActive && m_ActivePerspectiveItem.IsActive();
        }

        /// <summary>
        ///     Returns true if the camera can zoom.
        /// </summary>
        /// <returns>True if the camera can zoom.</returns>
        public virtual bool CanCameraZoom()
        {
            return m_AllowCameraZoom;
        }

        /// <summary>
        ///     Synchronizes the item Animator paremeters with the character's Animator.
        /// </summary>
        public void SnapAnimator()
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SnapAnimator();
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SnapAnimator();
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SnapAnimator();
        }

        /// <summary>
        ///     Sets the Animator's Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetHorizontalMovementParameter(float value, float timeScale, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetHorizontalMovementParameter(value, timeScale, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetHorizontalMovementParameter(value, timeScale, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetHorizontalMovementParameter(value, timeScale, dampingTime);
        }

        /// <summary>
        ///     Sets the Animator's Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetForwardMovementParameter(float value, float timeScale, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetForwardMovementParameter(value, timeScale, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetForwardMovementParameter(value, timeScale, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetForwardMovementParameter(value, timeScale, dampingTime);
        }

        /// <summary>
        ///     Sets the Animator's Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetPitchParameter(float value, float timeScale, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetPitchParameter(value, timeScale, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetPitchParameter(value, timeScale, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetPitchParameter(value, timeScale, dampingTime);
        }

        /// <summary>
        ///     Sets the Animator's Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetYawParameter(float value, float timeScale, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetYawParameter(value, timeScale, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetYawParameter(value, timeScale, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetYawParameter(value, timeScale, dampingTime);
        }

        /// <summary>
        ///     Sets the Animator's Moving parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetMovingParameter(bool value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetMovingParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetMovingParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetMovingParameter(value);
        }

        /// <summary>
        ///     Sets the Animator's Aiming parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAimingParameter(bool value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAimingParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAimingParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetAimingParameter(value);
        }

        /// <summary>
        ///     Sets the Animator's Movement Set ID parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetMovementSetIDParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetMovementSetIDParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetMovementSetIDParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetMovementSetIDParameter(value);
        }

        /// <summary>
        ///     Sets the Animator's Ability Index parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAbilityIndexParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAbilityIndexParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAbilityIndexParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetAbilityIndexParameter(value);
        }

        /// <summary>
        ///     Sets the Animator's Int Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAbilityIntDataParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAbilityIntDataParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAbilityIntDataParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetAbilityIntDataParameter(value);
        }

        /// <summary>
        ///     Sets the Animator's Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetAbilityFloatDataParameter(float value, float timeScale, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAbilityFloatDataParameter(value, timeScale, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAbilityFloatDataParameter(value, timeScale, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetAbilityFloatDataParameter(value, timeScale, dampingTime);
        }

        /// <summary>
        ///     Sets the Animator's Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetSpeedParameter(float value, float timeScale, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetSpeedParameter(value, timeScale, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetSpeedParameter(value, timeScale, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetSpeedParameter(value, timeScale, dampingTime);
        }

        /// <summary>
        ///     Sets the Animator's Height parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetHeightParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetHeightParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetHeightParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetHeightParameter(value);
        }

        /// <summary>
        ///     Sets the Animator's Item ID parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public void SetItemIDParameter(int slotID, int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetItemIDParameter(slotID, value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetItemIDParameter(slotID, value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetItemIDParameter(slotID, value);
        }

        /// <summary>
        ///     Sets the Animator's Item State Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public void SetItemStateIndexParameter(int slotID, int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetItemStateIndexParameter(slotID, value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetItemStateIndexParameter(slotID, value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetItemStateIndexParameter(slotID, value);
        }

        /// <summary>
        ///     Sets the Animator's Item Substate Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot of that item that should be set.</param>
        /// <param name="value">The new value.</param>
        public void SetItemSubstateIndexParameter(int slotID, int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetItemSubstateIndexParameter(slotID, value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetItemSubstateIndexParameter(slotID, value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && ThirdPersonPerspectiveItem.IsActive())
                m_ThirdPersonItemAnimatorMonitor.SetItemSubstateIndexParameter(slotID, value);
        }

        /// <summary>
        ///     The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        protected virtual void OnChangePerspectives(bool firstPersonPerspective)
        {
            // The object isn't active if it isn't equipped. OnChangePerspective will be sent to all items regardless of whether or not they are equipped.
            var isActive = m_ActivePerspectiveItem != null && m_ActivePerspectiveItem.IsActive();
            if (firstPersonPerspective)
            {
#if FIRST_PERSON_CONTROLLER
                m_ActivePerspectiveItem = m_FirstPersonPerspectiveItem;
#endif
            }
            else
            {
                m_ActivePerspectiveItem = ThirdPersonPerspectiveItem;
            }

            if (isActive)
            {
                var hasItem = m_Inventory.GetItemIdentifierAmount(ItemIdentifier) > 0;
                var active = false;
                var activeItem = m_Inventory.GetActiveItem(m_SlotID);
                if (activeItem != null) active = activeItem.ItemIdentifier == ItemIdentifier;
                m_ActivePerspectiveItem.SetActive(active, hasItem);
            }
        }

        /// <summary>
        ///     Drop the item from the character.
        /// </summary>
        /// <param name="amount">The amount of ItemIdentifier that should be dropped.</param>
        /// <param name="forceDrop">Should the item be dropped even if the inventory doesn't contain any count for the item?</param>
        /// <returns>The instance of the dropped item (can be null).</returns>
        public GameObject Drop(int amount, bool forceDrop)
        {
            return Drop(amount, forceDrop, true);
        }

        /// <summary>
        ///     Drop the item from the character.
        /// </summary>
        /// <param name="forceDrop">Should the item be dropped even if the inventory doesn't contain any count for the item?</param>
        /// <param name="amount">The amount of ItemIdentifier that should be dropped.</param>
        /// <param name="remove">Should the item be removed after it is dropped?</param>
        /// <returns>The instance of the dropped item (can be null).</returns>
        public GameObject Drop(int amount, bool forceDrop, bool remove)
        {
            return m_Inventory.DropItem(this, amount, forceDrop, remove);
        }

        /// <summary>
        ///     Removes the item from the character.
        /// </summary>
        public void Remove()
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Remove();
            }
#endif
            if (ThirdPersonPerspectiveItem != null) ThirdPersonPerspectiveItem.Remove();
            if (ItemActions != null)
                for (var i = 0; i < ItemActions.Length; ++i)
                    ItemActions[i].Remove();
            SetVisibleObjectActive(false, false);
        }

        /// <summary>
        ///     The GameObject has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            m_EquipAnimatorAudioStateSet.OnDestroy();
            m_UnequipAnimatorAudioStateSet.OnDestroy();
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        ///     Spawns the item under the specified character.
        /// </summary>
        /// <param name="character">The character that should parent the item.</param>
        /// <param name="item">The item that should be spawned.</param>
        /// <returns>The spawned item GameObject.</returns>
        public static Item SpawnItem(GameObject character, Item item)
        {
            // Spawn the item under the character's ItemPlacement GameObject.
            var itemPlacement = character.GetComponentInChildren<ItemPlacement>(true);
            if (itemPlacement == null)
            {
                Debug.LogError($"Error: ItemPlacement doesn't exist under the character {character.name}.");
                return null;
            }

            var additionalPoolKey = character.GetInstanceID();
            var itemGameObject = ObjectPoolBase.Instantiate(item.gameObject, additionalPoolKey, Vector3.zero,
                Quaternion.identity, itemPlacement.transform);
            itemGameObject.name = item.name;
            itemGameObject.transform.localPosition = Vector3.zero;
            itemGameObject.transform.localRotation = Quaternion.identity;

            var instancedItem = itemGameObject.GetComponent<Item>();

            //Initialize the perspectives in case the item was previously returned to the pool.
            var perspectiveItems = instancedItem.GetComponents<PerspectiveItem>();
            for (var i = 0; i < perspectiveItems.Length; ++i)
                // Initialize the perspective item manually to ensure an Object GameObject exists. This is important because the Item component will execute
                // before the FirstPersonPerspectiveItem component, but the FirstPersonPerspectiveItem component may not be completely initialized.
                // The FirstPersonPerspectiveItem component must be initialized after Item so Item.Start can be called and add the item to the inventory.
                perspectiveItems[i].Initialize(character);

            return instancedItem;
        }

        /// <summary>
        ///     Resets the Item back to the initial values.
        /// </summary>
        /// <param name="item">The item that should be reset.</param>
        public static void ResetInitialization(Item item)
        {
#if FIRST_PERSON_CONTROLLER
            if (item.FirstPersonPerspectiveItem != null) {
                item.FirstPersonPerspectiveItem.ResetInitialization();
            }
#endif
            if (item.ThirdPersonPerspectiveItem != null) item.ThirdPersonPerspectiveItem.ResetInitialization();

            item.ItemIdentifier = null;

            if (!ObjectPoolBase.IsPooledObject(item.gameObject))
            {
                // The item is not pooled.
                Destroy(item.gameObject);
                return;
            }

            ObjectPoolBase.Destroy(item.gameObject);
        }
    }
}