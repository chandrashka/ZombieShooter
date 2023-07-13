/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.Input;
using Opsive.Shared.StateSystem;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Camera
{
    /// <summary>
    ///     Handles the input for the AimAssist component.
    /// </summary>
    [RequireComponent(typeof(AimAssist))]
    public class AimAssistHandler : StateBehavior
    {
        [Tooltip("Can the targets be switched?")] [SerializeField]
        protected bool m_CanSwitchTargets;

        [Tooltip("The name of the button mapping for switching targets.")] [SerializeField]
        protected string m_SwitchTargetInputName = "Horizontal";

        [Tooltip("The minimum magnitude required to switch targets.")] [SerializeField]
        protected float m_SwitchTargetMagnitude = 0.8f;

        private AimAssist m_AimAssist;
        private bool m_AllowTargetSwitch = true;
        private PlayerInput m_PlayerInput;

        public bool CanSwitchTargets
        {
            get => m_CanSwitchTargets;
            set
            {
                m_CanSwitchTargets = value;
                if (Application.isPlaying)
                {
                    enabled = m_PlayerInput != null && m_CanSwitchTargets;
                    m_AllowTargetSwitch = true;
                }
            }
        }

        public string SwitchTargetInputName
        {
            get => m_SwitchTargetInputName;
            set => m_SwitchTargetInputName = value;
        }

        public float SwitchTargetMagnitude
        {
            get => m_SwitchTargetMagnitude;
            set => m_SwitchTargetMagnitude = value;
        }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_AimAssist = gameObject.GetComponent<AimAssist>();

            EventHandler.RegisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);

            // Enable after the character has been attached.
            enabled = false;
        }

        /// <summary>
        ///     Tries to switch targets if the input value is large enough.
        /// </summary>
        private void Update()
        {
            var value = m_PlayerInput.GetAxisRaw(m_SwitchTargetInputName);
            if (m_AllowTargetSwitch && Mathf.Abs(value) > m_SwitchTargetMagnitude)
            {
                m_AimAssist.TrySwitchTargets(value > 0);
                m_AllowTargetSwitch = false;
            }
            else if (!m_AllowTargetSwitch && Mathf.Abs(value) < 0.01f)
            {
                // Don't allow another target switch until the value is reset. This will prevent the target from quickly switching.
                m_AllowTargetSwitch = true;
            }
        }

        /// <summary>
        ///     The camera has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }

        /// <summary>
        ///     Attaches the component to the specified character.
        /// </summary>
        /// <param name="character">The handler to attach the camera to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            m_PlayerInput = character != null ? character.GetCachedComponent<PlayerInput>() : null;

            enabled = m_PlayerInput != null && m_CanSwitchTargets;
        }
    }
}