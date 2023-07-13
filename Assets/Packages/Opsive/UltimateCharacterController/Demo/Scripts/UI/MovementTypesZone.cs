/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Camera;
using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.UI
{
    /// <summary>
    ///     Manages the movement types zone. Allows switching between movement types.
    /// </summary>
    public class MovementTypesZone : UIZone
    {
        public enum MovementType
        {
            FirstPersonCombat,
            FirstPersonFreeLook,
            ThirdPersonAdventure,
            ThirdPersonCombat,
            ThirdPersonRPG,
            None
        }

        private CameraController m_CameraController;
        private GameObject m_Character;
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        private MovementType m_MovementType = MovementType.FirstPersonCombat;
        private MovementTypeSwitcher m_MovementTypeSwitcher;
        private bool m_MovementTypeSwitcherEnabled;
        private MovementType m_PerspectiveSwitchMovementType = MovementType.None;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_MovementTypeSwitcher = FindObjectOfType<MovementTypeSwitcher>();
        }

        /// <summary>
        ///     Initializes the default values.
        /// </summary>
        private void Start()
        {
            var foundCamera = CameraUtility.FindCamera(null);
            m_CameraController = foundCamera.GetComponent<CameraController>();
            m_Character = FindObjectOfType<DemoManager>().Character;
            m_CharacterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();

            EventHandler.RegisterEvent<UltimateCharacterController.Character.MovementTypes.MovementType, bool>(
                m_Character, "OnCharacterChangeMovementType", OnMovementTypeChanged);
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<UltimateCharacterController.Character.MovementTypes.MovementType, bool>(
                m_Character, "OnCharacterChangeMovementType", OnMovementTypeChanged);
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        ///     Change the movement type to the specified type.
        /// </summary>
        /// <param name="type">The type to change the value to.</param>
        public void ChangeMovementType(int type)
        {
            if (m_PerspectiveSwitchMovementType != MovementType.None) return;

            // Switch the camera's perspective if the movement type changes perspective.
            var movementType = (MovementType)type;
            if (IsFirstPersonType(movementType) != IsFirstPersonType(m_MovementType))
            {
                m_PerspectiveSwitchMovementType = movementType;
                m_CameraController.SetPerspective(IsFirstPersonType(movementType));
                return;
            }

            ChangeMovementType(movementType, true);
        }

        /// <summary>
        ///     Change the movement type to the specified type.
        /// </summary>
        /// <param name="type">The type to change the value to.</param>
        /// <param name="updateSwitcher">Should the type be set with the Movement Type Switcher?</param>
        private void ChangeMovementType(MovementType type, bool updateSwitcher)
        {
            // Revert the old.
            if (m_ButtonImages[(int)m_MovementType] != null) SetButtonColor((int)m_MovementType, m_NormalColor);

            m_MovementType = type;
            SetButtonColor((int)m_MovementType, m_PressedColor);

            // Set the new movement type with the Movement Type Switcher.
            if (updateSwitcher)
            {
                if (IsFirstPersonType(type))
                    m_MovementTypeSwitcher.UpdateMovementType(true, (int)type);
                else
                    m_MovementTypeSwitcher.UpdateMovementType(false, (int)type - 2);
            }

            EnableInput();
            m_PerspectiveSwitchMovementType = MovementType.None;
        }

        /// <summary>
        ///     Returns true if the type is a first person type.
        /// </summary>
        /// <param name="type">The Movement Type to compare against.</param>
        /// <returns>True if the type is a first person type.</returns>
        private bool IsFirstPersonType(MovementType type)
        {
            return type == MovementType.FirstPersonCombat || type == MovementType.FirstPersonFreeLook;
        }

        /// <summary>
        ///     The character has entered from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that entered the zone.</param>
        protected override void CharacterEnter(UltimateCharacterLocomotion characterLocomotion)
        {
            UpdateMovementType();

            // The buttons will change the movement types.
            m_MovementTypeSwitcherEnabled = m_MovementTypeSwitcher.enabled;
            m_MovementTypeSwitcher.enabled = false;
        }

        /// <summary>
        ///     The movement type has changed.
        /// </summary>
        /// <param name="movementType">The movement type that was changed.</param>
        /// <param name="activated">Was the specified movement type activated?</param>
        private void OnMovementTypeChanged(
            UltimateCharacterController.Character.MovementTypes.MovementType movementType, bool activated)
        {
            if (!activated || m_ActiveCharacter != null) return;

            UpdateMovementType();
        }

        /// <summary>
        ///     The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person view?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            if (m_PerspectiveSwitchMovementType != MovementType.None)
            {
                ChangeMovementType(m_PerspectiveSwitchMovementType, true);
                m_PerspectiveSwitchMovementType = MovementType.None;
            }
        }

        /// <summary>
        ///     Updates the MovementType enum to the character's current movement type.
        /// </summary>
        private void UpdateMovementType()
        {
            var movementType = m_CharacterLocomotion.ActiveMovementType;
#if FIRST_PERSON_CONTROLLER
            if (movementType is FirstPersonController.Character.MovementTypes.Combat) {
                ChangeMovementType(MovementType.FirstPersonCombat, false);
            } else if (movementType is FirstPersonController.Character.MovementTypes.FreeLook) {
                ChangeMovementType(MovementType.FirstPersonFreeLook, false);
            }
#endif
#if THIRD_PERSON_CONTROLLER
            if (movementType is Adventure)
                ChangeMovementType(MovementType.ThirdPersonAdventure, false);
            else if (movementType is Combat)
                ChangeMovementType(MovementType.ThirdPersonCombat, false);
            else if (movementType is RPG) ChangeMovementType(MovementType.ThirdPersonRPG, false);
#endif
        }

        /// <summary>
        ///     The character has exited from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that exited the zone.</param>
        protected override void CharacterExit(UltimateCharacterLocomotion characterLocomotion)
        {
            if (m_MovementTypeSwitcherEnabled) m_MovementTypeSwitcher.enabled = true;
        }
    }
}