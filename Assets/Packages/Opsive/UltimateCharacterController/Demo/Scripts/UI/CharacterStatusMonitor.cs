﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Camera;
using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using UnityEngine;
using UnityEngine.UI;

namespace Opsive.UltimateCharacterController.Demo.UI
{
    /// <summary>
    ///     Monitors the text components which show the active perspective and movement type.
    /// </summary>
    public class CharacterStatusMonitor : MonoBehaviour
    {
        [Tooltip("A reference to the perspective UI text.")] [SerializeField]
        protected Text m_PerspectiveText;

        [Tooltip("A reference to the movement type UI text.")] [SerializeField]
        protected Text m_MovementTypeText;

        private UltimateCharacterLocomotion m_CharacterLocomotion;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Start()
        {
            var foundCamera = CameraUtility.FindCamera(null);
            var character = foundCamera.GetComponent<CameraController>().Character;
            m_CharacterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();

            m_PerspectiveText.text = GetPerspectiveText();
            m_PerspectiveText.enabled = true;
            m_MovementTypeText.text = GetMovementTypeText();
            m_MovementTypeText.enabled = true;

            EventHandler.RegisterEvent<MovementType, bool>(character, "OnCharacterChangeMovementType",
                OnMovementTypeChanged);
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_CharacterLocomotion != null)
                EventHandler.UnregisterEvent<MovementType, bool>(m_CharacterLocomotion.gameObject,
                    "OnCharacterChangeMovementType", OnMovementTypeChanged);
        }

        /// <summary>
        ///     Returns the perspective text.
        /// </summary>
        /// <returns>The perspective text.</returns>
        private string GetPerspectiveText()
        {
            return m_CharacterLocomotion.FirstPersonPerspective ? "FIRST PERSON" : "THIRD PERSON";
        }

        /// <summary>
        ///     Returns the movement type text.
        /// </summary>
        /// <returns>The movement type text.</returns>
        private string GetMovementTypeText()
        {
            var movementType = m_CharacterLocomotion.ActiveMovementType.GetType().Name.ToUpper();
            if (movementType == "PSEUDO3D") return "2.5D";
            return movementType;
        }

        /// <summary>
        ///     The movement type has changed - update the UI.
        /// </summary>
        /// <param name="movementType">The movement type that was changed.</param>
        /// <param name="activated">Was the specified movement type activated?</param>
        private void OnMovementTypeChanged(MovementType movementType, bool activated)
        {
            if (!activated || m_CharacterLocomotion == null) return;

            m_PerspectiveText.text = GetPerspectiveText();
            m_MovementTypeText.text = GetMovementTypeText();
        }
    }
}