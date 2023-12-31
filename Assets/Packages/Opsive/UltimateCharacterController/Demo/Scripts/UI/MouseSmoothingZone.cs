﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Character;

namespace Opsive.UltimateCharacterController.Demo.UI
{
    /// <summary>
    ///     Manages the mouse smoothing zone. Allows switching betweening mouse input types.
    /// </summary>
    public class MouseSmoothingZone : UIZone
    {
        private SmoothingType m_SmoothingType = SmoothingType.Smoothing;

        /// <summary>
        ///     Change the smoothing type to the specified type.
        /// </summary>
        /// <param name="type">The type to change the value to.</param>
        public void ChangeSmoothingType(int type)
        {
            ChangeInputType((SmoothingType)type);
        }

        /// <summary>
        ///     Change the smoothing type to the specified type.
        /// </summary>
        /// <param name="type">The type to change the value to.</param>
        private void ChangeInputType(SmoothingType type)
        {
            // Revert the old.
            SetButtonColor((int)m_SmoothingType, m_NormalColor);
            StateManager.SetState(m_ActiveCharacter, Enum.GetName(typeof(SmoothingType), m_SmoothingType), false);

            // Set the new smoothing type.
            m_SmoothingType = type;
            SetButtonColor((int)m_SmoothingType, m_PressedColor);
            StateManager.SetState(m_ActiveCharacter, Enum.GetName(typeof(SmoothingType), type), true);

            EnableInput();
        }

        /// <summary>
        ///     The character has entered from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that entered the zone.</param>
        protected override void CharacterEnter(UltimateCharacterLocomotion characterLocomotion)
        {
            // The smoothing type is the standard type.
            ChangeInputType(SmoothingType.Smoothing);
        }

        /// <summary>
        ///     The character has exited from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that exited the zone.</param>
        protected override void CharacterExit(UltimateCharacterLocomotion characterLocomotion)
        {
            // The smoothing type should activate when leaving the zone.
            ChangeInputType(SmoothingType.Smoothing);
        }

        private enum SmoothingType
        {
            Raw,
            Smoothing,
            LowSensitivity,
            LowSensitivityAcceleration
        }
    }
}