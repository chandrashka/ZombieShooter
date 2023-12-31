﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Opsive.UltimateCharacterController.Demo.UI
{
    /// <summary>
    ///     Toggles the UI that shows the help for the control mappings.
    /// </summary>
    public class ToggleControlHelp : MonoBehaviour
    {
        [Tooltip("The UI that should be toggled.")] [SerializeField]
        protected GameObject m_ControlUI;

        [Tooltip("The keycode that should enable the UI.")] [SerializeField]
        protected KeyCode m_EnableKeyCode = KeyCode.Escape;

        [Tooltip("A reference to the keyboard controls sprite.")] [SerializeField]
        protected GameObject m_KeyboardUI;

        [Tooltip("A reference to the controller controls sprite.")] [SerializeField]
        protected GameObject m_ControllerUI;

        [Tooltip("A reference to the Resume button.")] [SerializeField]
        protected Button m_ResumeButton;

        [Tooltip("A reference to the Quit button.")] [SerializeField]
        protected Button m_QuitButton;

        [Tooltip("A reference to the displays that contain button selections.")] [SerializeField]
        protected GameObject[] m_ButtonMenus;

        private bool m_Active;

        private GameObject m_Character;
        private bool m_InUIZone;
        private float m_PrevTimeScale;
        private UnityInput m_UnityInput;
        private bool m_VisibleCursor;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_ControlUI.SetActive(m_Active);
            var demoManager = GetComponent<DemoManager>();
            m_Character = demoManager.Character;
            m_UnityInput = m_Character.GetComponent<UnityInput>();

            m_ResumeButton.onClick.AddListener(Resume);
#if UNITY_STANDALONE || UNITY_EDITOR
            m_QuitButton.onClick.AddListener(Quit);
#else
            // The application can't quit on the target platform.
            m_QuitButton.gameObject.SetActive(false);

            // Center the Resume button.
            var rectTransform = m_ResumeButton.GetComponent<RectTransform>();
            var position = rectTransform.localPosition;
            position.x = rectTransform.rect.width / 2; 
            rectTransform.localPosition = position;
#endif

            OnControllerConnected(m_UnityInput.ControllerConnected);
            EventHandler.RegisterEvent<bool>(m_Character, "OnInputControllerConnected", OnControllerConnected);
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterEnterUIZone", OnEnterUIZone);
        }

        /// <summary>
        ///     Toggles the UI.
        /// </summary>
        private void Update()
        {
            if (m_Active)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Resume();
                    return;
                }

                // Keep the cursor visible while the UI is active.
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if ((m_VisibleCursor && Input.GetKeyDown(m_EnableKeyCode)) ||
                     Input.GetKeyDown(KeyCode.Joystick1Button6))
            {
                ShowControls();
            }

            // Store the cursor state so the controls don't appear on the same frame that the cursor was enabled.
            m_VisibleCursor = Cursor.visible;
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(m_Character, "OnInputControllerConnected", OnControllerConnected);
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterEnterUIZone", OnEnterUIZone);
        }

        /// <summary>
        ///     Resumes the game.
        /// </summary>
        public void Resume()
        {
            m_Active = false;
            m_ControlUI.SetActive(false);
            Time.timeScale = m_PrevTimeScale;

            EventHandler.ExecuteEvent(m_Character, "OnEnableGameplayInput", true);
            Cursor.lockState = m_InUIZone ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = m_InUIZone;
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        /// <summary>
        ///     Quits the game.
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
#endif

        /// <summary>
        ///     Shows the controls.
        /// </summary>
        public void ShowControls()
        {
            m_Active = true;
            EventHandler.ExecuteEvent(m_Character, "OnEnableGameplayInput", false);
            m_ControlUI.SetActive(true);
            m_PrevTimeScale = Time.timeScale;
            Time.timeScale = 0;
            m_KeyboardUI.SetActive(!m_UnityInput.ControllerConnected);
            m_ControllerUI.SetActive(m_UnityInput.ControllerConnected);

            EventSystem.current.SetSelectedGameObject(m_ResumeButton.gameObject);
        }

        /// <summary>
        ///     A controller has been connected or disconnected.
        /// </summary>
        /// <param name="controllerConnected">True if a controller has been connected.</param>
        private void OnControllerConnected(bool controllerConnected)
        {
            if (!m_Active) return;

            m_KeyboardUI.SetActive(controllerConnected);
            m_ControllerUI.SetActive(controllerConnected);
        }

        /// <summary>
        ///     The character has entered or left a zone which should show the UI and cursor.
        /// </summary>
        /// <param name="inUIZone">Did the character enter the UI zone?</param>
        private void OnEnterUIZone(bool inUIZone)
        {
            m_InUIZone = inUIZone;
        }
    }
}