﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using System.Collections.Generic;
using Opsive.Shared.Camera;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Demo.Objects;
using Opsive.UltimateCharacterController.Demo.UI;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    ///     The DemoManager will control the objects in the demo scene as well as the text shown.
    /// </summary>
    public class DemoManager : MonoBehaviour
    {
        /// <summary>
        ///     Container for each zone within the demo scene.
        /// </summary>
        [Serializable]
        public class DemoZone
        {
            [Tooltip("The header text.")] [SerializeField]
            protected string m_Header;

            [Tooltip("The description text.")] [SerializeField]
            protected string m_Description;

            [Tooltip("The text that appears beneath the header requiring action.")] [SerializeField]
            protected string m_Action;

            [Tooltip("The trigger that enables the header and description text.")] [SerializeField]
            protected DemoZoneTrigger m_DemoZoneTrigger;

            [Tooltip("The objects that the trigger should enable.")] [SerializeField]
            protected MonoBehaviour[] m_EnableObjects;

            [Tooltip("The objects that the trigger should activate/deactivate.")] [SerializeField]
            protected GameObject[] m_ToggleObjects;

            [Tooltip("The objects that should be disabled when the current state is active.")] [SerializeField]
            protected GameObject[] m_InverseToggleObjects;

            public string Header => m_Header;
            public string Description => m_Description;
            public string Action => m_Action;
            public DemoZoneTrigger DemoZoneTrigger => m_DemoZoneTrigger;
            public MonoBehaviour[] EnableObjects => m_EnableObjects;
            public GameObject[] ToggleObjects => m_ToggleObjects;
            public GameObject[] InverseToggleObjects => m_InverseToggleObjects;
            public int Index { get; private set; }

            /// <summary>
            ///     Initializes the zone.
            /// </summary>
            /// <param name="index">The index of the DemoZone.</param>
            public void Initialize(int index)
            {
                Index = index;

                // Assign the spawn point so the character will know where to spawn upon death.
                var spawnPoints = m_DemoZoneTrigger.GetComponentsInChildren<SpawnPoint>();
                for (var i = 0; i < spawnPoints.Length; ++i) spawnPoints[i].Grouping = index;

                // The toggled objects should start disabled.
                for (var i = 0; i < m_ToggleObjects.Length; ++i) m_ToggleObjects[i].SetActive(false);
            }
        }

        [Tooltip("A reference to the character.")] [SerializeField]
        protected GameObject m_Character;

        [Tooltip("Is the character allowed to free roam the scene at the very start?")] [SerializeField]
        protected bool m_FreeRoam;

        [Tooltip("A reference used to determine the character's perspective selection at the start.")] [SerializeField]
        protected GameObject m_PerspectiveSelection;

        [Tooltip("A reference to the panel which shows the demo text.")] [SerializeField]
        protected GameObject m_TextPanel;

        [Tooltip("A reference to the Text component which shows the demo header text.")] [SerializeField]
        protected Text m_Header;

        [Tooltip("A reference to the Text component which shows the demo description text.")] [SerializeField]
        protected Text m_Description;

        [Tooltip("A reference to the Text component which shows the demo action text.")] [SerializeField]
        protected Text m_Action;

        [Tooltip("A reference to the GameObject which shows the next zone arrow.")] [SerializeField]
        protected GameObject m_NextZoneArrow;

        [Tooltip("A reference to the GameObject which shows the previous zone arrow.")] [SerializeField]
        protected GameObject m_PreviousZoneArrow;

        [Tooltip("A reference to the quit button GameObject. This is only shown for add-ons.")] [SerializeField]
        protected GameObject m_QuitButton;

        [Tooltip("A list of all of the zones within the scene.")] [SerializeField]
        protected DemoZone[] m_DemoZones;

        [Tooltip("Should the ItemIdentifiers be picked up when the character spawns within free roam mode?")]
        [SerializeField]
        protected bool m_FreeRoamPickupItemDefinitions = true;

        [Tooltip("An array of ItemIdentifiers to be picked up when free roaming.")]
        [FormerlySerializedAs("m_FreeRoamItemTypeCounts")]
        [FormerlySerializedAs("m_FreeRoamItemIdentifierAmounts")]
        [SerializeField]
        protected ItemDefinitionAmount[] m_FreeRoamItemDefinitionAmounts;

        [Tooltip("The title that should be displayed when the character is not in a zone.")] [SerializeField]
        protected string m_NoZoneTitle;

        [Tooltip("The description that should be displayed when the character is not in a zone.")] [SerializeField]
        protected string m_NoZoneDescription;

        [Tooltip("Is this manager part of an add-on?")] [FormerlySerializedAs("m_AddonDemoManager")] [SerializeField]
        protected bool m_AddOnDemoManager;
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        [Tooltip("Specifies the perspective that the character should start in if there is no perspective selection GameObject.")]
        [SerializeField] protected bool m_DefaultFirstPersonStart = true;
#endif

        public GameObject Character => m_Character;

        public bool FreeRoam
        {
            get => m_FreeRoam;
            set => m_FreeRoam = value;
        }

        public GameObject PerspectiveSelection
        {
            get => m_PerspectiveSelection;
            set => m_PerspectiveSelection = value;
        }

        public DemoZone[] DemoZones => m_DemoZones;

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Health m_CharacterHealth;
        private Respawner m_CharacterRespawner;
        private readonly Dictionary<DemoZoneTrigger, DemoZone> m_DemoZoneTriggerDemoZoneMap = new();
        private readonly List<int> m_ActiveZoneIndices = new();
        private bool m_CursorActionTextVisible;
        private int m_LastZoneIndex = -1;
        private readonly List<Door> m_Doors = new();
        private int m_EnterFrame;
        private bool m_FullAccess;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
            var demoZones = new List<DemoZone>(m_DemoZones);
            for (var i = demoZones.Count - 1; i > -1; --i)
                // The demo zone may belong to the other perspective.
                if (demoZones[i].DemoZoneTrigger == null)
                    demoZones.RemoveAt(i);
            m_DemoZones = demoZones.ToArray();
#endif
            for (var i = 0; i < m_DemoZones.Length; ++i)
            {
                if (m_DemoZones[i].DemoZoneTrigger == null) continue;

                m_DemoZones[i].Initialize(i);
                m_DemoZoneTriggerDemoZoneMap.Add(m_DemoZones[i].DemoZoneTrigger, m_DemoZones[i]);
            }

            // Enable the UI after the character has spawned.
            if (m_TextPanel != null) m_TextPanel.SetActive(false);
            if (m_PreviousZoneArrow != null) m_PreviousZoneArrow.SetActive(false);
            if (m_NextZoneArrow != null) m_NextZoneArrow.SetActive(false);
            if (m_Action != null) m_Action.enabled = false;
            if (m_QuitButton != null) m_QuitButton.SetActive(false);

            // The controller updates within Update. Limit the update rate.
            Application.targetFrameRate = 60;
        }

        /// <summary>
        ///     Initializes the character.
        /// </summary>
        protected virtual void Start()
        {
            InitializeCharacter(m_Character, true, true);
        }

        /// <summary>
        ///     Initializes the Demo Manager with the specified character.
        /// </summary>
        /// <param name="character">The character that should be initialized/</param>
        /// <param name="selectStartingPerspective">Should the starting perspective be selected?</param>
        /// <param name="teleport">Should the character be teleported to the first demo zone?</param>
        protected void InitializeCharacter(GameObject character, bool selectStartingPerspective, bool teleport)
        {
            m_Character = character;

            if (m_Character == null) return;

            m_CharacterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
            m_CharacterHealth = m_Character.GetComponent<Health>();
            m_CharacterRespawner = m_Character.GetComponent<Respawner>();

            // Disable the demo components if the character is null. This allows for free roaming within the demo scene.
            if (m_FreeRoam)
            {
                m_FullAccess = true;
                if (m_PerspectiveSelection != null) m_PerspectiveSelection.SetActive(false);

                var uiZones = GetComponentsInChildren<UIZone>();
                for (var i = 0; i < uiZones.Length; ++i) uiZones[i].enabled = false;

                // All of the doors should be opened with free roam.
                for (var i = 0; i < m_Doors.Count; ++i)
                {
                    m_Doors[i].CloseOnTriggerExit = false;
                    m_Doors[i].OpenClose(true, true, false);
                }

                // The enable objects should be enabled.
                for (var i = 0; i < m_DemoZones.Length; ++i)
                for (var j = 0; j < m_DemoZones[i].EnableObjects.Length; ++j)
                    m_DemoZones[i].EnableObjects[j].enabled = true;

                // The character needs to be assigned to the camera.
                var foundCamera = CameraUtility.FindCamera(null);
                var cameraController = foundCamera.GetComponent<CameraController>();
                cameraController.SetPerspective(m_CharacterLocomotion.FirstPersonPerspective, true);

                // The character doesn't start out with any items.
                if (m_FreeRoamItemDefinitionAmounts != null && m_FreeRoamPickupItemDefinitions)
                {
                    var inventory = m_Character.GetComponent<InventoryBase>();
                    if (inventory != null)
                        for (var i = 0; i < m_FreeRoamItemDefinitionAmounts.Length; ++i)
                        {
                            if (m_FreeRoamItemDefinitionAmounts[i].ItemDefinition == null) continue;
                            inventory.Pickup(m_FreeRoamItemDefinitionAmounts[i].ItemIdentifier,
                                m_FreeRoamItemDefinitionAmounts[i].Amount, -1, true, false);
                        }
                }

                cameraController.Character = m_Character;
                if (m_Character.activeInHierarchy) EventHandler.ExecuteEvent(m_Character, "OnCharacterSnapAnimator");
                return;
            }

            // The cursor needs to be visible.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!selectStartingPerspective) return;
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            // Show the perspective selection menu.
            if (m_PerspectiveSelection != null) {
                // The character should be disabled until the perspective is set.
                m_Character.SetActive(false);

                m_PerspectiveSelection.SetActive(true);
            } else {
                SelectStartingPerspective(m_DefaultFirstPersonStart, teleport);
            }
#elif FIRST_PERSON_CONTROLLER
            SelectStartingPerspective(true, teleport);
#else
            SelectStartingPerspective(false, teleport);
#endif
        }

        /// <summary>
        ///     Updates the cursor and action text.
        /// </summary>
        private void Update()
        {
            if (m_LastZoneIndex == -1)
            {
                // Keep the mouse visible when the perspective screen is active.
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (!m_AddOnDemoManager)
            {
                if (Cursor.visible
#if THIRD_PERSON_CONTROLLER
                    && !(m_CharacterLocomotion.ActiveMovementType is TopDown ||
                         m_CharacterLocomotion.ActiveMovementType is Pseudo3D ||
                         m_CharacterLocomotion.ActiveMovementType is PointClick)
#endif
                   )
                {
                    // Show the additional action item if the cursor is visible.
                    m_Action.enabled = true;
                    m_Action.text = "PRESS ESCAPE AGAIN TO OPEN MENU";
                    m_CursorActionTextVisible = true;
                }
                else if (m_CursorActionTextVisible)
                {
                    // Revert to the default if the cursor is not visible.
                    if (!string.IsNullOrEmpty(m_DemoZones[m_LastZoneIndex].Action))
                        m_Action.text = m_DemoZones[m_LastZoneIndex].Action;
                    else
                        m_Action.enabled = false;
                    m_CursorActionTextVisible = false;
                }
#if !UNITY_EDITOR && !UNITY_WEBGL
            } else { // Add-on.
                m_QuitButton.SetActive(Cursor.visible);
#endif
            }
        }

        /// <summary>
        ///     Registers the door with the DemoManager.
        /// </summary>
        /// <param name="door">The door that should be registered.</param>
        public void RegisterDoor(Door door)
        {
            m_Doors.Add(door);
        }

        /// <summary>
        ///     The character has entered a trigger zone.
        /// </summary>
        /// <param name="demoZoneTrigger">The trigger zone that the character entered.</param>
        /// <param name="other">The GameObject that entered the trigger.</param>
        /// <returns>Did the character successfully enter the trigger?</returns>
        public bool EnteredTriggerZone(DemoZoneTrigger demoZoneTrigger, GameObject other)
        {
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null || characterLocomotion.gameObject != m_Character) return false;

            if (!m_DemoZoneTriggerDemoZoneMap.TryGetValue(demoZoneTrigger, out var demoZone)) return false;

            if (m_CharacterHealth != null && m_CharacterHealth.Value == 0) return false;

            return ActivateDemoZone(demoZone, false);
        }

        /// <summary>
        ///     Activates the specified demo zone.
        /// </summary>
        /// <param name="demoZone">The demo zone to active.</param>
        /// <param name="teleport">Should the character be teleported to the demo zone?</param>
        /// <returns>Did the character successfully enter the trigger?</returns>
        private bool ActivateDemoZone(DemoZone demoZone, bool teleport)
        {
            if (demoZone.Index == m_LastZoneIndex) return false;

            if (m_ActiveZoneIndices.Count == 0 || m_ActiveZoneIndices[m_ActiveZoneIndices.Count - 1] != demoZone.Index)
                m_ActiveZoneIndices.Add(demoZone.Index);
            m_LastZoneIndex = demoZone.Index;
            ShowText(demoZone.Header, demoZone.Description, demoZone.Action);
            if (m_PreviousZoneArrow != null) m_PreviousZoneArrow.SetActive(demoZone.Index != 0);
            if (m_NextZoneArrow != null) m_NextZoneArrow.SetActive(demoZone.Index != m_DemoZones.Length - 1);
            m_EnterFrame = Time.frameCount;
            for (var i = 0; i < demoZone.EnableObjects.Length; ++i) demoZone.EnableObjects[i].enabled = true;
            for (var i = 0; i < demoZone.ToggleObjects.Length; ++i) demoZone.ToggleObjects[i].SetActive(true);
            for (var i = 0; i < demoZone.InverseToggleObjects.Length; ++i)
            {
                if (demoZone.InverseToggleObjects[i] == null) continue;
                demoZone.InverseToggleObjects[i].SetActive(false);
            }

            // When the character reaches the outside section all doors should be unlocked.
            if (!m_AddOnDemoManager && !m_FullAccess && demoZone.Index >= m_DemoZones.Length - 6)
            {
                for (var i = 0; i < m_Doors.Count; ++i)
                {
                    m_Doors[i].CloseOnTriggerExit = false;
                    m_Doors[i].OpenClose(true, true, false);
                }

                m_FullAccess = true;
            }

            if (teleport)
            {
                var position = Vector3.zero;
                var rotation = Quaternion.identity;
                SpawnPointManager.GetPlacement(m_Character, demoZone.Index, ref position, ref rotation);
                m_CharacterLocomotion.SetPositionAndRotation(position, rotation, true);
            }

            // Set the group after the state so the default state doesn't override the grouping value.
            m_CharacterRespawner.Grouping = demoZone.Index;
            return true;
        }

        /// <summary>
        ///     The character has exited a trigger zone.
        /// </summary>
        /// <param name="demoZoneTrigger">The trigger zone that the character exited.</param>
        public void ExitedTriggerZone(DemoZoneTrigger demoZoneTrigger)
        {
            DemoZone demoZone;
            if (!m_DemoZoneTriggerDemoZoneMap.TryGetValue(demoZoneTrigger, out demoZone)) return;
            for (var i = 0; i < demoZone.ToggleObjects.Length; ++i) demoZone.ToggleObjects[i].SetActive(false);
            for (var i = 0; i < demoZone.InverseToggleObjects.Length; ++i)
            {
                if (demoZone.InverseToggleObjects[i] == null) continue;
                demoZone.InverseToggleObjects[i].SetActive(true);
            }

            m_ActiveZoneIndices.Remove(demoZone.Index);

            // Show standard text if the demo zone isn't the last demo zone.
            if (m_ActiveZoneIndices.Count == 0 && (m_AddOnDemoManager || demoZone.Index != m_DemoZones.Length - 1) &&
                m_EnterFrame != Time.frameCount)
                ShowText(m_NoZoneTitle.Replace("{AssetName}", AssetInfo.Name), m_NoZoneDescription, string.Empty);
            else if (m_ActiveZoneIndices.Count > 0 &&
                     m_LastZoneIndex != m_ActiveZoneIndices[m_ActiveZoneIndices.Count - 1])
                ActivateDemoZone(m_DemoZones[m_ActiveZoneIndices[m_ActiveZoneIndices.Count - 1]], false);
        }

        /// <summary>
        ///     Teleports the character to the next or pervious zone.
        /// </summary>
        /// <param name="next">Should the character be teleported to the next zone? If false the previous zone will be used.</param>
        public void Teleport(bool next)
        {
            var targetIndex = Mathf.Clamp(m_LastZoneIndex + (next ? 1 : -1), 0, m_DemoZones.Length - 1);
            if (m_ActiveZoneIndices.Count > 0 &&
                targetIndex == m_ActiveZoneIndices[m_ActiveZoneIndices.Count - 1]) return;

            ActivateDemoZone(m_DemoZones[targetIndex], true);
        }

        /// <summary>
        ///     Sets the starting perspective on the character.
        /// </summary>
        /// <param name="firstPersonPerspective">Should the character start in a first person perspective?</param>
        public virtual void SelectStartingPerspective(bool firstPersonPerspective)
        {
            SelectStartingPerspective(firstPersonPerspective, true);
        }

        /// <summary>
        ///     Sets the starting perspective on the character.
        /// </summary>
        /// <param name="firstPersonPerspective">Should the character start in a first person perspective?</param>
        /// <param name="teleport">Should the character be teleported to the demo zone?</param>
        protected void SelectStartingPerspective(bool firstPersonPerspective, bool teleport)
        {
            // Set the starting position.
            m_LastZoneIndex = -1;
            ActivateDemoZone(m_DemoZones[0], teleport);
            // The character should be activated after positioned so the fall surface impacts don't play.
            m_Character.SetActive(true);
            EventHandler.ExecuteEvent(m_Character, "OnEnableGameplayInput", true);

            // Set the perspective on the camera.
            var foundCamera = CameraUtility.FindCamera(null);
            var cameraController = foundCamera.GetComponent<CameraController>();
            // Ensure the camera starts with the correct view type.
            cameraController.FirstPersonViewTypeFullName = GetViewTypeFullName(true);
            cameraController.ThirdPersonViewTypeFullName = GetViewTypeFullName(false);
            cameraController.SetPerspective(firstPersonPerspective, true);
            cameraController.Character = m_Character;

            // The cursor should be hidden to start the demo.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // The character and camera are ready to go - disable the perspective selection panel.
            if (m_PerspectiveSelection != null) m_PerspectiveSelection.SetActive(false);
        }

        /// <summary>
        ///     Returns the full name of the view type for the specified perspective.
        /// </summary>
        /// <param name="firstPersonPerspective">Should the first person perspective be returned?</param>
        /// <returns>The full name of the view type for the specified perspective.</returns>
        protected virtual string GetViewTypeFullName(bool firstPersonPerspective)
        {
            return firstPersonPerspective
                ? "Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes.Combat"
                : "Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes.Adventure";
        }

        /// <summary>
        ///     Shows the text in the UI with the specified header and description.
        /// </summary>
        /// <param name="header">The header that should be shown.</param>
        /// <param name="description">The description that should be shown.</param>
        /// <param name="action">The action that should be shown.</param>
        private void ShowText(string header, string description, string action)
        {
            if (m_TextPanel == null) return;

            if (string.IsNullOrEmpty(header))
            {
                m_TextPanel.SetActive(false);
                return;
            }

            m_TextPanel.SetActive(true);
            m_Header.text = "--- " + header + " ---";
            m_Description.text = description.Replace("{AssetName}", AssetInfo.Name);
            if (m_Action != null)
            {
                m_Action.text = action;
                m_Action.enabled = !string.IsNullOrEmpty(action);
            }
        }

        /// <summary>
        ///     Quits the project.
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Application.targetFrameRate = -1;
        }
    }
}