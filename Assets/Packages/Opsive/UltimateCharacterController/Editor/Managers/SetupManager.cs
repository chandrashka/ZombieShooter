﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Opsive.Shared.Audio;
using Opsive.Shared.Editor.Inspectors.Input;
using Opsive.Shared.Game;
using Opsive.Shared.StateSystem;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Editor.Utility;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera;
using Opsive.UltimateCharacterController.Utility.Builders;
using UnityEditor;
using UnityEngine;
using EditorUtility = Opsive.Shared.Editor.Utility.EditorUtility;
using Object = UnityEngine.Object;

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    /// <summary>
    ///     The SetupManager shows any project or scene related setup options.
    /// </summary>
    [OrderedEditorItem("Setup", 1)]
    public class SetupManager : Manager
    {
        private const string c_MonitorsPrefabGUID = "b5bf2e4077598914b83fc5e4ca20f2f4";
        private const string c_VirtualControlsPrefabGUID = "33d3d57ba5fc7484c8d09150e45066a4";
        private const string c_3DAudioManagerModuleGUID = "7c2f6e9d4d7571042964493904b06c50";

        [SerializeField] private bool m_CanCreateCamera = true;
        [SerializeField] private bool m_DrawSceneSetup = true;
        [SerializeField] private string m_FirstPersonViewType;

        private readonly List<Type> m_FirstPersonViewTypes = new();
        private string[] m_FirstPersonViewTypeStrings;
        [SerializeField] private Perspective m_Perspective = Perspective.None;
        private readonly string[] m_PerspectiveNames = { "First", "Third", "Both" };
        [SerializeField] private int m_ProfileIndex;
        [SerializeField] private string m_ProfileName;
        [SerializeField] private bool m_StartFirstPersonPerspective;
        [SerializeField] private StateConfiguration m_StateConfiguration;
        [SerializeField] private string m_ThirdPersonViewType;
        private readonly List<Type> m_ThirdPersonViewTypes = new();
        private string[] m_ThirdPersonViewTypeStrings;

        private readonly string[] m_ToolbarStrings = { "Scene", "Project" };

        /// <summary>
        ///     Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            // Set the default perspective based on what asset is installed.
            if (m_Perspective == Perspective.None)
            {
#if FIRST_PERSON_CONTROLLER
                m_Perspective = Perspective.First;
#elif THIRD_PERSON_CONTROLLER
                m_Perspective = Perspective.Third;
#endif
            }

            // Get a list of the available view types.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; ++i)
            {
                var assemblyTypes = assemblies[i].GetTypes();
                for (var j = 0; j < assemblyTypes.Length; ++j)
                {
                    // Must derive from ViewType.
                    if (!typeof(ViewType).IsAssignableFrom(assemblyTypes[j])) continue;

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) continue;

                    if (assemblyTypes[j].FullName.Contains("FirstPersonController"))
                        m_FirstPersonViewTypes.Add(assemblyTypes[j]);
                    else if (assemblyTypes[j].FullName.Contains("ThirdPersonController"))
                        m_ThirdPersonViewTypes.Add(assemblyTypes[j]);
                }
            }

            // Create an array of display names for the popup.
            if (m_FirstPersonViewTypes.Count > 0)
            {
                m_FirstPersonViewTypeStrings = new string[m_FirstPersonViewTypes.Count];
                for (var i = 0; i < m_FirstPersonViewTypes.Count; ++i)
                    m_FirstPersonViewTypeStrings[i] = InspectorUtility.DisplayTypeName(m_FirstPersonViewTypes[i], true);
            }

            if (m_ThirdPersonViewTypes.Count > 0)
            {
                m_ThirdPersonViewTypeStrings = new string[m_ThirdPersonViewTypes.Count];
                for (var i = 0; i < m_ThirdPersonViewTypes.Count; ++i)
                    m_ThirdPersonViewTypeStrings[i] = InspectorUtility.DisplayTypeName(m_ThirdPersonViewTypes[i], true);
            }

            // Find the state configuration.
            var stateConfiguration = ManagerUtility.FindStateConfiguration(m_MainManagerWindow);
            if (stateConfiguration != null)
                if (m_StateConfiguration == null)
                    m_StateConfiguration = stateConfiguration;
        }

        /// <summary>
        ///     Opens the Project Setup tab.
        /// </summary>
        public void OpenProjectSetup()
        {
            m_DrawSceneSetup = false;
        }

        /// <summary>
        ///     Draws the Manager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection =
                GUILayout.Toolbar(m_DrawSceneSetup ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawSceneSetup = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawSceneSetup)
                DrawSceneSetup();
            else
                DrawProjectSetup();
        }

        /// <summary>
        ///     Draws the controls for setting up the scene.
        /// </summary>
        private void DrawSceneSetup()
        {
            ManagerUtility.DrawControlBox("Manager Setup", null,
                "Adds the scene-level manager components to the scene.", true, "Add Managers", AddManagers,
                string.Empty);
            ManagerUtility.DrawControlBox("Camera Setup", DrawCameraViewTypes,
                "Sets up the camera within the scene to use the Ultimate Character Controller Camera Controller component.",
                m_CanCreateCamera, "Setup Camera", SetupCamera, string.Empty);
            ManagerUtility.DrawControlBox("UI Setup", null, "Adds the UI monitors to the scene.", true, "Add UI", AddUI,
                string.Empty);
            ManagerUtility.DrawControlBox("Virtual Controls Setup", null, "Adds the virtual controls to the scene.",
                true, "Add Virtual Controls", AddVirtualControls, string.Empty);
        }

        /// <summary>
        ///     Draws the popup for the camera view types.
        /// </summary>
        private void DrawCameraViewTypes()
        {
            // Draw the perspective.
            var selectedPerspective =
                (Perspective)EditorGUILayout.Popup("Perspective", (int)m_Perspective, m_PerspectiveNames);
            var isSupported = true;
            // Determine if the selected perspective is supported.
#if !FIRST_PERSON_CONTROLLER
            if (selectedPerspective == Perspective.First || selectedPerspective == Perspective.Both)
            {
                EditorGUILayout.HelpBox(
                    "Unable to select the First Person Controller perspective. If you'd like to use a first person perspective ensure the " +
                    "First Person Controller is imported.", MessageType.Error);
                isSupported = false;
            }
#endif
#if !THIRD_PERSON_CONTROLLER
            if (selectedPerspective == Perspective.Third || selectedPerspective == Perspective.Both) {
                EditorGUILayout.HelpBox("Unable to select the Third Person Controller perspective. If you'd like to use a third person perspective ensure the " +
                                        "Third Person Controller is imported.", MessageType.Error);
                isSupported = false;
            }
#endif
            m_Perspective = selectedPerspective;
            m_CanCreateCamera = isSupported;
            if (!isSupported) return;

            // Show the available first person ViewTypes.
            if (m_Perspective == Perspective.First || m_Perspective == Perspective.Both)
            {
                var selectedViewType = -1;
                for (var i = 0; i < m_FirstPersonViewTypes.Count; ++i)
                    if (m_FirstPersonViewTypes[i].FullName == m_FirstPersonViewType)
                    {
                        selectedViewType = i;
                        break;
                    }

                var viewType = selectedViewType == -1 ? 0 : selectedViewType;
                selectedViewType =
                    EditorGUILayout.Popup("First Person View Type", viewType, m_FirstPersonViewTypeStrings);
                if (viewType != selectedViewType || string.IsNullOrEmpty(m_FirstPersonViewType))
                    m_FirstPersonViewType = m_FirstPersonViewTypes[selectedViewType].FullName;
                if (m_Perspective != Perspective.Both) m_ThirdPersonViewType = string.Empty;
            }

            // Show the available third person ViewTypes.
            if (m_Perspective == Perspective.Third || m_Perspective == Perspective.Both)
            {
                var selectedViewType = -1;
                for (var i = 0; i < m_ThirdPersonViewTypes.Count; ++i)
                    if (m_ThirdPersonViewTypes[i].FullName == m_ThirdPersonViewType)
                    {
                        selectedViewType = i;
                        break;
                    }

                var viewType = selectedViewType == -1 ? 0 : selectedViewType;
                selectedViewType =
                    EditorGUILayout.Popup("Third Person View Type", viewType, m_ThirdPersonViewTypeStrings);
                if (viewType != selectedViewType || string.IsNullOrEmpty(m_ThirdPersonViewType))
                    m_ThirdPersonViewType = m_ThirdPersonViewTypes[selectedViewType].FullName;
                if (m_Perspective != Perspective.Both) m_FirstPersonViewType = string.Empty;
            }

            if (m_Perspective == Perspective.Both)
                m_StartFirstPersonPerspective = EditorGUILayout.Popup("Start Perspective",
                    m_StartFirstPersonPerspective ? 0 : 1, new[] { "First Person", "Third Person" }) == 0;
            else
                m_StartFirstPersonPerspective = m_Perspective == Perspective.First;
            // Show the possible base configurations.
            var updatedStateConfiguration =
                EditorGUILayout.ObjectField("State Configuration", m_StateConfiguration, typeof(StateConfiguration),
                    false) as StateConfiguration;
            if (updatedStateConfiguration != m_StateConfiguration)
            {
                if (updatedStateConfiguration != null)
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString,
                        AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(updatedStateConfiguration)));
                else
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, string.Empty);
                m_StateConfiguration = updatedStateConfiguration;
            }

            EditorGUI.indentLevel++;
            if (m_StateConfiguration != null)
            {
                var profiles =
                    m_StateConfiguration.GetProfilesForGameObject(null, StateConfiguration.Profile.ProfileType.Camera);
                // The character can be added without any profiles.
                profiles.Insert(0, "(None)");
                m_ProfileIndex = EditorGUILayout.Popup("Profile", m_ProfileIndex, profiles.ToArray());
                m_ProfileName = profiles[m_ProfileIndex];
            }

            EditorGUI.indentLevel--;
            GUILayout.Space(5);
        }

        /// <summary>
        ///     Sets up the camera if it hasn't already been setup.
        /// </summary>
        private void SetupCamera()
        {
            // Setup the camera.
            GameObject cameraGameObject;
            var addedCameraController = false;
            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                // If the main camera can't be found then use the first available camera.
                var cameras = UnityEngine.Camera.allCameras;
                if (cameras != null && cameras.Length > 0)
                {
                    // Prefer cameras that are at the root level.
                    for (var i = 0; i < cameras.Length; ++i)
                        if (cameras[i].transform.parent == null)
                        {
                            camera = cameras[i];
                            break;
                        }

                    // No cameras are at the root level. Set the first available camera.
                    if (camera == null) camera = cameras[0];
                }

                // A new camera should be created if there isn't a valid camera.
                if (camera == null)
                {
                    cameraGameObject = new GameObject("Camera");
                    cameraGameObject.tag = "MainCamera";
                    camera = cameraGameObject.AddComponent<UnityEngine.Camera>();
                    cameraGameObject.AddComponent<AudioListener>();
                }
            }

            // The near clip plane should adjusted for viewing close objects.
            camera.nearClipPlane = 0.01f;

            // Add the CameraController if it isn't already added.
            cameraGameObject = camera.gameObject;
            if (cameraGameObject.GetComponent<CameraController>() == null)
            {
                var cameraController = cameraGameObject.AddComponent<CameraController>();
                if (m_Perspective == Perspective.Both)
                    ViewTypeBuilder.AddViewType(cameraController, typeof(Transition));
                if (m_StartFirstPersonPerspective)
                {
                    if (!string.IsNullOrEmpty(m_ThirdPersonViewType))
                        ViewTypeBuilder.AddViewType(cameraController, TypeUtility.GetType(m_ThirdPersonViewType));
                    if (!string.IsNullOrEmpty(m_FirstPersonViewType))
                        ViewTypeBuilder.AddViewType(cameraController, TypeUtility.GetType(m_FirstPersonViewType));
                }
                else
                {
                    if (!string.IsNullOrEmpty(m_FirstPersonViewType))
                        ViewTypeBuilder.AddViewType(cameraController, TypeUtility.GetType(m_FirstPersonViewType));
                    if (!string.IsNullOrEmpty(m_ThirdPersonViewType))
                        ViewTypeBuilder.AddViewType(cameraController, TypeUtility.GetType(m_ThirdPersonViewType));
                }

                // Detect if a character exists in the scene. Automatically add the character if it does.
                var characters = Object.FindObjectsOfType<CharacterLocomotion>();
                if (characters != null && characters.Length == 1)
                {
                    cameraController.InitCharacterOnAwake = true;
                    cameraController.Character = characters[0].gameObject;
                }

                // Setup the components which help the Camera Controller.
                Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<CameraControllerHandler>(
                    cameraGameObject);
#if THIRD_PERSON_CONTROLLER
                if (m_Perspective != Perspective.First)
                    Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<ObjectFader>(cameraGameObject);
#endif
                addedCameraController = true;

                if (m_StateConfiguration != null)
                    if (m_ProfileIndex > 0)
                    {
                        m_StateConfiguration.AddStatesToGameObject(m_ProfileName, cameraGameObject);
                        EditorUtility.SetDirty(cameraGameObject);
                    }
            }

            if (addedCameraController)
                Debug.Log("The Camera Controller has been added.");
            else
                Debug.LogWarning(
                    "Warning: No action was performed, the Camera Controller component has already been added.");
        }

        /// <summary>
        ///     Adds the singleton manager components.
        /// </summary>
        public static void AddManagers()
        {
            // Create the "Game" components if it doesn't already exists.
            Scheduler scheduler;
            GameObject gameGameObject;
            if ((scheduler = Object.FindObjectOfType<Scheduler>()) == null)
                gameGameObject = new GameObject("Game");
            else
                gameGameObject = scheduler.gameObject;

            // Add the Singletons.
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<SurfaceManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<DecalManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<KinematicObjectManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<ObjectPool>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<Scheduler>(gameGameObject);
            var audiomanager =
                Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<AudioManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<SpawnPointManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<StateManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<LayerManager>(gameGameObject);

            if (audiomanager.AudioManagerModule == null)
            {
                var defaultAudioManagerModulePath = AssetDatabase.GUIDToAssetPath(c_3DAudioManagerModuleGUID);
                if (!string.IsNullOrEmpty(defaultAudioManagerModulePath))
                {
                    var audioManagerModule =
                        AssetDatabase.LoadAssetAtPath(defaultAudioManagerModulePath, typeof(AudioManagerModule)) as
                            AudioManagerModule;
                    audiomanager.AudioManagerModule = audioManagerModule;
                }
            }

            Debug.Log("The managers have been added.");
        }

        /// <summary>
        ///     Adds the UI to the scene.
        /// </summary>
        private void AddUI()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
                canvas = Object.FindObjectOfType<Canvas>();
            }

            // Look up based on guid.
            GameObject uiPrefab = null;
            var monitorsPath = AssetDatabase.GUIDToAssetPath(c_MonitorsPrefabGUID);
            if (!string.IsNullOrEmpty(monitorsPath))
                uiPrefab = AssetDatabase.LoadAssetAtPath(monitorsPath, typeof(GameObject)) as GameObject;

            // If the guid wasn't found try the path.
            if (uiPrefab == null)
            {
                var baseDirectory =
                    Path.GetDirectoryName(
                            AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(m_MainManagerWindow)))
                        .Replace("\\", "/").Replace("Editor/Managers", "");
                uiPrefab = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demo/Prefabs/UI/Monitors.prefab",
                    typeof(GameObject)) as GameObject;
            }

            if (uiPrefab == null)
            {
                Debug.LogError("Error: Unable to find the UI Monitors prefab.");
                return;
            }

            // Instantiate the Monitors prefab.
            var uiGameObject = PrefabUtility.InstantiatePrefab(uiPrefab) as GameObject;
            uiGameObject.name = "Monitors";
            uiGameObject.GetComponent<RectTransform>().SetParent(canvas.transform, false);
        }

        /// <summary>
        ///     Adds the UI to the scene.
        /// </summary>
        private void AddVirtualControls()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
                canvas = Object.FindObjectOfType<Canvas>();
            }

            // Look up based on guid.
            GameObject virtualControlsPrefab = null;
            var virtualControlsPath = AssetDatabase.GUIDToAssetPath(c_VirtualControlsPrefabGUID);
            if (!string.IsNullOrEmpty(virtualControlsPath))
                virtualControlsPrefab =
                    AssetDatabase.LoadAssetAtPath(virtualControlsPath, typeof(GameObject)) as GameObject;

            // If the guid wasn't found try the path.
            if (virtualControlsPrefab == null)
            {
                var baseDirectory =
                    Path.GetDirectoryName(
                            AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(m_MainManagerWindow)))
                        .Replace("\\", "/").Replace("Editor/Managers", "");
                virtualControlsPrefab =
                    AssetDatabase.LoadAssetAtPath(baseDirectory + "Demo/Prefabs/UI/VirtualControls.prefab",
                        typeof(GameObject)) as GameObject;
            }

            if (virtualControlsPrefab == null)
            {
                Debug.LogError("Error: Unable to find the UI Virtual Controls prefab.");
                return;
            }

            // Instantiate the Virtual Controls prefab.
            var virtualControls = PrefabUtility.InstantiatePrefab(virtualControlsPrefab) as GameObject;
            virtualControls.name = "VirtualControls";
            virtualControls.GetComponent<RectTransform>().SetParent(canvas.transform, false);
        }

        /// <summary>
        ///     Draws the controls for button and input setup.
        /// </summary>
        private void DrawProjectSetup()
        {
            // Show a warning if the button mappings or layers have not been updated.
            var serializedObject =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axisProperty = serializedObject.FindProperty("m_Axes");
            var hasInputs = UnityInputBuilder.FindAxisProperty(axisProperty, "Action", false) != null &&
                            UnityInputBuilder.FindAxisProperty(axisProperty, "Crouch", false) != null;

            var tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProperty = tagManager.FindProperty("layers");
            var hasLayers = layersProperty.GetArrayElementAtIndex(LayerManager.Character).stringValue == "Character";

            if (!hasInputs || !hasLayers)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.HelpBox(
                    "The default button mappings or layers have not been added. If you are just getting started you should update the button mappings and layers with the button below. " +
                    "This can be changed layer.", MessageType.Warning);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Update Buttons and Layers", GUILayout.Width(170)))
                {
                    CharacterInputBuilder.UpdateInputManager();
                    UpdateLayers();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(10);
            }

            ManagerUtility.DrawControlBox("Button Mappings", null,
                "Add the default button mappings to the Unity Input Manager. If you are using a custom button mapping or " +
                "an input integration then you do not need to update the Unity button mappings.", true,
                "Update Buttons",
                CharacterInputBuilder.UpdateInputManager, "The button mappings were successfully updated.");
            GUILayout.Space(10);

            ManagerUtility.DrawControlBox("Layers", null,
                "Update the project layers to the default character controller layers. The layers do not need to be updated " +
                "if you have already setup a custom set of layers.", true, "Update Layers", UpdateLayers,
                "The layers were successfully updated.");
        }

        /// <summary>
        ///     Updates all of the layers to the Ultimate Character Controller defaults.
        /// </summary>
        public static void UpdateLayers()
        {
            var tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProperty = tagManager.FindProperty("layers");

            // Add the layers.
            AddLayer(layersProperty, LayerManager.Enemy, "Enemy");
            AddLayer(layersProperty, LayerManager.MovingPlatform, "MovingPlatform");
            AddLayer(layersProperty, LayerManager.VisualEffect, "VisualEffect");
            AddLayer(layersProperty, LayerManager.Overlay, "Overlay");
            AddLayer(layersProperty, LayerManager.SubCharacter, "SubCharacter");
            AddLayer(layersProperty, LayerManager.Character, "Character");

            tagManager.ApplyModifiedProperties();
        }

        /// <summary>
        ///     Sets the layer index to the specified name if the string value is empty.
        /// </summary>
        public static void AddLayer(SerializedProperty layersProperty, int index, string name)
        {
            var layerElement = layersProperty.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layerElement.stringValue)) layerElement.stringValue = name;
        }

        /// <summary>
        ///     Specifies the perspective that the ViewType can change into.
        /// </summary>
        private enum Perspective
        {
            First, // The ViewType can only be in first person perspective.
            Third, // The ViewType can only be in third person perspective.
            Both, // The ViewType can be in first or third person perspective.
            None // Default value.
        }
    }
}