﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Camera;
using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Camera;
using UnityEngine;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    ///     The CameraMonitor component allows for UI elements to mapped to a specific character (allowing for split screen and
    ///     coop).
    /// </summary>
    public abstract class CharacterMonitor : StateBehavior
    {
        [Tooltip("The character that uses the UI represents. Can be null.")] [SerializeField]
        protected GameObject m_Character;

        [Tooltip("Is the UI visible?")] [SerializeField]
        protected bool m_Visible = true;

        protected GameObject m_CameraGameObject;

        protected bool m_ShowUI = true;

        public GameObject Character
        {
            get => m_Character;
            set => OnAttachCharacter(value);
        }

        public bool Visible
        {
            get => m_Visible;
            set
            {
                m_Visible = value;
                ShowUI(m_ShowUI);
            }
        }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            AssignCamera();

            if (m_Character != null)
            {
                var character = m_Character;
                m_Character = null;
                OnAttachCharacter(character);
            }
        }

        /// <summary>
        ///     Starts the UI.
        /// </summary>
        protected virtual void Start()
        {
            // If the camera GameObject is null then the camera may have been spawned after Awake.
            if (m_CameraGameObject == null) AssignCamera();

            // Disable the UI if it shouldn't be shown.
            if (!CanShowUI()) enabled = false;
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_CameraGameObject != null)
                EventHandler.UnregisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter",
                    OnAttachCharacter);
            OnAttachCharacter(null);
        }

        /// <summary>
        ///     Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            if (m_Character == character) return;

            if (m_Character != null)
            {
                StateManager.LinkGameObjects(m_Character, gameObject, false);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            m_Character = character;

            if (m_Character != null)
            {
                StateManager.LinkGameObjects(m_Character, gameObject, true);
                EventHandler.RegisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            // The monitor may be in the process of being destroyed.
            enabled = m_Character != null;
        }

        /// <summary>
        ///     Assigns the camera to the UI Monitor.
        /// </summary>
        private void AssignCamera()
        {
            var foundCamera = CameraUtility.FindCamera(m_Character);
            if (foundCamera != null)
            {
                m_CameraGameObject = foundCamera.gameObject;
                if (m_Character == null)
                    m_Character = m_CameraGameObject.GetCachedComponent<CameraController>().Character;
                EventHandler.RegisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter",
                    OnAttachCharacter);
            }
        }

        /// <summary>
        ///     Shows or hides the UI.
        /// </summary>
        /// <param name="show">Should the UI be shown?</param>
        protected virtual void ShowUI(bool show)
        {
            m_ShowUI = show;
            gameObject.SetActive(CanShowUI());
        }

        /// <summary>
        ///     Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected virtual bool CanShowUI()
        {
            return m_ShowUI && m_Visible && m_Character != null;
        }
    }
}