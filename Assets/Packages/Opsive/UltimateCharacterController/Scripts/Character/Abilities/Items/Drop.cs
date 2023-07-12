﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The Drop ItemAbility will drop the currently equipped item.
    /// </summary>
    [AllowDuplicateTypes]
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Drop")]
    [DefaultItemStateIndex(6)]
    public class Drop : ItemAbility
    {
        [Tooltip("The slot that should be dropped. -1 will drop all of the slots.")]
        [SerializeField] protected int m_SlotID = -1;
        [Tooltip("The ItemIdentifiers that cannot be dropped.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_NoDropItemTypes")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_NoDropItemIdentifiers")]
        [SerializeField] protected ItemDefinitionBase[] m_NoDropItemDefinitions;
        [Tooltip("Should the item wait to be dropped until it is unequipped?")]
        [SerializeField] protected bool m_WaitForUnequip;
        [Tooltip("Specifies if the item should be dropped when the OnAnimatorDropItem event is received or wait for the specified duration before dropping the item.")]
        [SerializeField] protected AnimationEventTrigger m_DropEvent;

        public int SlotID { get { return m_SlotID; } set { m_SlotID = value; } }
        public ItemDefinitionBase[] NoDropItemDefinitions { get { return m_NoDropItemDefinitions; } set { m_NoDropItemDefinitions = value; } }
        public bool WaitForUnequip { get { return m_WaitForUnequip; } set { m_WaitForUnequip = value; } }
        public AnimationEventTrigger DropEvent { get { return m_DropEvent; } set { m_DropEvent = value; } }

        private ItemSetManager m_ItemSetManager;
        private Item[] m_Items;
        private EquipUnequip[] m_EquipUnequipAbilities;

#if UNITY_EDITOR
        public override string AbilityDescription { get { if (m_SlotID != -1) { return "Slot " + m_SlotID; } return string.Empty; } }
#endif

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_ItemSetManager = m_GameObject.GetCachedComponent<ItemSetManager>();
            m_Items = new Item[m_SlotID == -1 ? m_Inventory.SlotCount : 1];

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorDropItem", DropItem);
            if (m_ItemSetManager != null) {
                EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            }
        }

        /// <summary>
        /// Initialize the equip unequip abilities.
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_EquipUnequipAbilities = m_CharacterLocomotion.GetAbilities<EquipUnequip>();
        }

        /// <summary>
        /// Can the item be dropped?
        /// </summary>
        /// <returns>True if the item can be dropped.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // If the SlotID is -1 then the ability should drop every equipped item at the same time. If only one slot has a item then the 
            // ability can start. If the SlotID is not -1 then the ability should drop the item in the specified slot.
            var canDrop = false;
            if (m_SlotID == -1) {
                for (int i = 0; i < m_Items.Length; ++i) {
                    m_Items[i] = m_Inventory.GetActiveItem(i);
                    if (m_Items[i] == null) {
                        continue;
                    }
                    // Certain ItemIdentifiers cannot be dropped.
                    if (m_NoDropItemDefinitions != null) {
                        var skipItemIdentifier = false;
                        for (int j = 0; j < m_NoDropItemDefinitions.Length; ++j) {
                            if (m_Items[i].ItemIdentifier.GetItemDefinition() == m_NoDropItemDefinitions[j]) {
                                skipItemIdentifier = true;
                                break;
                            }
                        }
                        if (skipItemIdentifier) {
                            continue;
                        }
                    }
                    // The item can be droppped.
                    canDrop = true;
                }
            } else {
                m_Items[0] = m_Inventory.GetActiveItem(m_SlotID);
                // Certain ItemIdentifiers cannot be dropped.
                var skipItemIdentifier = false;
                if (m_NoDropItemDefinitions != null) {
                    for (int j = 0; j < m_NoDropItemDefinitions.Length; ++j) {
                        if (ReferenceEquals(m_Items[0].ItemIdentifier.GetItemDefinition(), m_NoDropItemDefinitions[j])) {
                            skipItemIdentifier = true;
                            break;
                        }
                    }
                }
                canDrop = !skipItemIdentifier && m_Items[0] != null;
            }

            return canDrop;
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            if (base.ShouldBlockAbilityStart(startingAbility)) {
                return true;
            }
            if (startingAbility is Use
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                || startingAbility is Reload
#endif
                ) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (activeAbility is Use
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                || activeAbility is Reload
#endif
                ) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            var waitForUnequip = false;
            // The ItemSetManager will be null when the items are managed by Slot ID rather than ItemSet (such as for first person VR).
            if (m_ItemSetManager != null && m_WaitForUnequip) {
                for (int i = 0; i < m_Items.Length; ++i) {
                    if (m_Items[i] != null) {
                        for (int j = 0; j < m_EquipUnequipAbilities.Length; ++j) {
                            if (m_ItemSetManager.IsCategoryMember(m_Items[i].ItemIdentifier.GetItemDefinition(), m_EquipUnequipAbilities[j].ItemSetCategoryIndex)) {
                                m_EquipUnequipAbilities[j].StartEquipUnequip(m_ItemSetManager.GetDefaultItemSetIndex(m_EquipUnequipAbilities[j].ItemSetCategoryIndex));
                                waitForUnequip = true;
                            }
                        }
                    }
                }
            }

            if (!waitForUnequip && !m_DropEvent.WaitForAnimationEvent) {
                SchedulerBase.ScheduleFixed(m_DropEvent.Duration, DropItem);
            }
        }

        /// <summary>
        /// Drops the actual item and stops the ability.
        /// </summary>
        private void DropItem()
        {
            // DropItem may be triggered by the animation event even when the ability isn't active.
            if (!IsActive) {
                return;
            }

            // Drop each item. If a drop prefab is specified then the item will be dropped.
            for (int i = 0; i < m_Items.Length; ++i) {
                if (m_Items[i] != null) {
                    m_Inventory.RemoveItem(m_Items[i].ItemIdentifier, m_Items[i].SlotID, 1, true);
                    m_Items[i] = null;
                }
            }

            StopAbility();
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (!IsActive || item != m_Items[slotID]) {
                return;
            }

            // Once the item has been unequipped it can be removed from the inventory. This will trigger the drop.
            m_Inventory.RemoveItem(m_Items[slotID].ItemIdentifier, m_Items[slotID].SlotID, 1, true);
            m_Items[slotID] = null;

            // The ability can be stopped as soon as all items are removed.
            var stopAbility = true;
            for (int i = 0; i < m_Items.Length; ++i) {
                if (m_Items[i] != null) {
                    stopAbility = false;
                }
            }

            if (stopAbility) {
                StopAbility();
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorDropItem", DropItem);
            if (m_ItemSetManager != null) {
                EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            }
        }
    }
}