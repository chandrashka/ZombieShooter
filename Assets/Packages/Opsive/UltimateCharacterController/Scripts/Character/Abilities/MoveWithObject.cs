﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Game;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    ///     Moves with the specified object. See this page for more info on the setup required:
    ///     https://opsive.com/support/documentation/ultimate-character-controller/character/abilities/included-abilities/move-with-object/
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    public class MoveWithObject : Ability
    {
        [Tooltip("The object that the character should move with.")] [SerializeField]
        protected Transform m_Target;

        public Transform Target
        {
            get => m_Target;
            set
            {
                var prevTarget = m_Target;
                m_Target = value;

                if (m_Target != null && m_Target.GetComponent<KinematicObject>() == null)
                {
                    Debug.Log("Error: The target " + Target.name +
                              " does not have the Kinematic Object component. See the Move With Object documentation for more information.");
                    m_Target = null;
                }

                if (IsActive && prevTarget != null && m_Target != prevTarget)
                    m_CharacterLocomotion.SetPlatform(m_Target);
            }
        }

        public override bool IsConcurrent => true;

        /// <summary>
        ///     Initailize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Set the property so it goes through the error check.
            if (m_Target != null) Target = m_Target;
        }

        /// <summary>
        ///     Can the ability be started?
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (m_Target == null) return false;

            return base.CanStartAbility();
        }

        /// <summary>
        ///     The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_CharacterLocomotion.SetPlatform(m_Target);
        }

        /// <summary>
        ///     Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            if (m_Target != null) return false;

            return base.CanStopAbility();
        }

        /// <summary>
        ///     The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_CharacterLocomotion.SetPlatform(null, false);
        }
    }
}