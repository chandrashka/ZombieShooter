﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
using Opsive.UltimateCharacterController.Objects;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    ///     Extends the ThrowableItem to allow a pin to be removed.
    /// </summary>
    public class GrenadeItem : ThrowableItem
    {
        [Tooltip("Is the pin removal animated?")] [SerializeField]
        protected bool m_AnimatePinRemoval = true;

        [Tooltip(
            "Specifies if the item should wait for the OnAnimatorItemRemovePin animation event or wait for the specified duration before removing the pin from the object?")]
        [SerializeField]
        protected AnimationEventTrigger m_RemovePinEvent = new(true, 0.4f);

        private Grenade m_InstantiatedGrenade;

        public bool AnimatePinRemoval
        {
            get => m_AnimatePinRemoval;
            set => m_AnimatePinRemoval = value;
        }

        public AnimationEventTrigger RemovePinEvent
        {
            get => m_RemovePinEvent;
            set => m_RemovePinEvent = value;
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

            // Grenades can be cooked (and explode while still in the character's hands).
            m_InstantiatedGrenade = m_InstantiatedTrajectoryObject as Grenade;
            m_InstantiatedGrenade.StartCooking(m_Character);

            // If a pin is specified then it can optionally be removed when the grenade is being thrown.
            if (m_InstantiatedGrenade.Pin != null)
                if (m_AnimatePinRemoval && !m_DisableVisibleObject)
                {
                    if (m_RemovePinEvent.WaitForAnimationEvent)
                        EventHandler.RegisterEvent(m_Character, "OnAnimatorItemRemovePin", RemovePin);
                    else
                        SchedulerBase.ScheduleFixed(m_RemovePinEvent.Duration, RemovePin);
                }
        }

        /// <summary>
        ///     The pin has been removed from the grenade.
        /// </summary>
        private void RemovePin()
        {
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorItemRemovePin", RemovePin);

            // Attach the pin to the attachment transform. Attach both first and third person in case there is a perspective switch.
            var activeGrenadeItemPerspectiveProperties =
                m_ActivePerspectiveProperties as IGrenadeItemPerspectiveProperties;
            m_InstantiatedGrenade.DetachAttachPin(activeGrenadeItemPerspectiveProperties.PinAttachmentLocation);
        }
    }
}