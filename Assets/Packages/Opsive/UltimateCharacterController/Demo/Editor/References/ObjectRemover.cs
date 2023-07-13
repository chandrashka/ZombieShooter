/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Demo.Objects;
using Opsive.UltimateCharacterController.Demo.References;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Magic.BeginEndActions;
using Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions;
using Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using EditorUtility = Opsive.Shared.Editor.Utility.EditorUtility;

#if !ULTIMATE_CHARACTER_CONTROLLER_EXTENSION_DEBUG

namespace Opsive.UltimateCharacterController.Editor.References
{
#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
    using Shared.Utility;
    using Camera.ViewTypes;
    using Character.MovementTypes;
    using Character.Abilities;
    using Character.Abilities.Items;
    using Managers;
    using System.Collections.Generic;
#endif

    [InitializeOnLoad]
    public class ObjectRemover
    {
        private static Scene s_ActiveScene;

        /// <summary>
        ///     Registers for the scene change callback.
        /// </summary>
        static ObjectRemover()
        {
            EditorApplication.update += Update;
        }

        /// <summary>
        ///     The scene has been changed.
        /// </summary>
        private static void Update()
        {
            var scene = SceneManager.GetActiveScene();

            if (scene == s_ActiveScene || Application.isPlaying) return;
            s_ActiveScene = scene;

            // Only the add-ons and integrations demo scene should be affected.
            var scenePath = s_ActiveScene.path.Replace("\\", "/");
            if (!scenePath.Contains("UltimateCharacterController/Add-Ons") &&
                !scenePath.Contains("UltimateCharacterController/Integrations")) return;

            // Find the object which contains the objects that should be removed.
            var objectReferences = Object.FindObjectOfType<ObjectReferences>();
            ProcessObjectReferences(objectReferences, true);
        }

        /// <summary>
        ///     Removes the objects specified by the object references object.
        /// </summary>
        private static void ProcessObjectReferences(ObjectReferences objectReferences, bool fromScene)
        {
            if (objectReferences == null) return;

            RemoveObjects(objectReferences.RemoveObjects);
            objectReferences.RemoveObjects = null;
#if !FIRST_PERSON_CONTROLLER
            RemoveObjects(objectReferences.FirstPersonObjects);
            objectReferences.FirstPersonObjects = null;
#endif
#if !THIRD_PERSON_CONTROLLER
            RemoveObjects(objectReferences.ThirdPersonObjects);
            objectReferences.ThirdPersonObjects = null;
#endif
#if !ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            RemoveObjects(objectReferences.ShooterObjects);
            objectReferences.ShooterObjects = null;
#endif
#if !ULTIMATE_CHARACTER_CONTROLLER_MELEE
            RemoveObjects(objectReferences.MeleeObjects);
            objectReferences.MeleeObjects = null;
#endif

#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
            // Remove any view types and states that are no longer valid.
            CameraController cameraController;
            if (fromScene)
                cameraController = Object.FindObjectOfType<CameraController>();
            else
                cameraController = objectReferences.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.DeserializeViewTypes();
                var viewTypes = new List<ViewType>(cameraController.ViewTypes);
                for (var i = viewTypes.Count - 1; i > -1; --i)
                {
                    if (viewTypes[i] == null)
                    {
                        viewTypes.RemoveAt(i);
                        continue;
                    }

                    viewTypes[i].States = RemoveUnusedStates(viewTypes[i].States);
                }

                cameraController.ViewTypeData = Serialization.Serialize<ViewType>(viewTypes);
                cameraController.ViewTypes = viewTypes.ToArray();
                EditorUtility.SetDirty(cameraController);
            }

            // Remove any movement types and states that are no longer valid.
            UltimateCharacterLocomotion characterLocomotion;
            if (fromScene)
                characterLocomotion = Object.FindObjectOfType<UltimateCharacterLocomotion>();
            else
                characterLocomotion = objectReferences.GetComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion != null)
            {
                characterLocomotion.DeserializeMovementTypes();
                var movementTypes = new List<MovementType>(characterLocomotion.MovementTypes);
                for (var i = movementTypes.Count - 1; i > -1; --i)
                {
                    if (movementTypes[i] == null)
                    {
                        movementTypes.RemoveAt(i);
                        continue;
                    }

                    movementTypes[i].States = RemoveUnusedStates(movementTypes[i].States);
                }

                characterLocomotion.MovementTypeData = Serialization.Serialize<MovementType>(movementTypes);
                characterLocomotion.MovementTypes = movementTypes.ToArray();
#if FIRST_PERSON_CONTROLLER
                characterLocomotion.SetMovementType(Shared.Utility.TypeUtility.GetType(characterLocomotion.FirstPersonMovementTypeFullName));
#else
                characterLocomotion.SetMovementType(
                    TypeUtility.GetType(characterLocomotion.ThirdPersonMovementTypeFullName));
#endif

                // Ensure the animator is pointing to the correct reference.
                var animator = characterLocomotion.GetComponent<Animator>();
                if (animator != null && animator.runtimeAnimatorController == null)
                {
                    animator.runtimeAnimatorController = ManagerUtility.FindAnimatorController(null);
                    EditorUtility.SetDirty(animator);
                }

                // Check for unused ability states.
                var abilities = new List<Ability>(characterLocomotion.GetSerializedAbilities());
                for (var i = abilities.Count - 1; i > -1; --i)
                {
                    if (abilities[i] == null)
                    {
                        abilities.RemoveAt(i);
                        continue;
                    }

                    abilities[i].States = RemoveUnusedStates(abilities[i].States);
                }

                characterLocomotion.AbilityData = Serialization.Serialize<Ability>(abilities);
                characterLocomotion.Abilities = abilities.ToArray();

                // Check for unused item ability states.
                var itemAbilities = new List<ItemAbility>(characterLocomotion.GetSerializedItemAbilities());
                for (var i = itemAbilities.Count - 1; i > -1; --i)
                {
                    if (itemAbilities[i] == null)
                    {
                        itemAbilities.RemoveAt(i);
                        continue;
                    }

                    itemAbilities[i].States = RemoveUnusedStates(itemAbilities[i].States);
                }

                characterLocomotion.ItemAbilityData = Serialization.Serialize<ItemAbility>(itemAbilities);
                characterLocomotion.ItemAbilities = itemAbilities.ToArray();
                EditorUtility.SetDirty(characterLocomotion);

                // Update the inventory.
                var inventory = characterLocomotion.GetComponent<Inventory.Inventory>();
                if (inventory != null)
                {
                    var loadout = new List<ItemDefinitionAmount>(inventory.DefaultLoadout);
                    for (var i = loadout.Count - 1; i > -1; --i)
                        if (loadout[i].ItemDefinition == null)
                            loadout.RemoveAt(i);
                    inventory.DefaultLoadout = loadout.ToArray();
                    EditorUtility.SetDirty(inventory);
                }

                var itemSetManager = characterLocomotion.GetComponent<ItemSetManager>();
                if (itemSetManager != null)
                {
                    var categoryItemSets = itemSetManager.CategoryItemSets;
                    for (var i = 0; i < categoryItemSets.Length; ++i)
                    for (var j = categoryItemSets[i].ItemSetList.Count - 1; j > -1; --j)
                    {
                        var nullItemIdentifier = true;
                        for (var k = 0; k < categoryItemSets[i].ItemSetList[j].Slots.Length; ++k)
                            if (categoryItemSets[i].ItemSetList[j].Slots[k] != null)
                            {
                                nullItemIdentifier = false;
                                break;
                            }

                        if (nullItemIdentifier) categoryItemSets[i].ItemSetList.RemoveAt(j);
                    }

                    ;
                    EditorUtility.SetDirty(itemSetManager);
                }
            }

#if !THIRD_PERSON_CONTROLLER
            // Set the shadow caster for the first person only objects.
            var shadowCaster = ManagerUtility.FindInvisibleShadowCaster(null);
            if (shadowCaster != null) {
                for (int i = 0; i < objectReferences.ShadowCasterObjects.Length; ++i) {
                    if (objectReferences.ShadowCasterObjects[i] == null) {
                        continue;
                    }

                    var renderers = objectReferences.ShadowCasterObjects[i].GetComponentsInChildren<Renderer>();
                    for (int j = 0; j < renderers.Length; ++j) {
                        var materials = renderers[j].sharedMaterials;
                        for (int k = 0; k < materials.Length; ++k) {
                            materials[k] = shadowCaster;
                        }
                        renderers[j].sharedMaterials = materials;
                        Shared.Editor.Utility.EditorUtility.SetDirty(renderers[j]);
                    }
                }
            }
#endif

            var items = objectReferences.GetComponentsInChildren<Item>();
            for (var i = 0; i < items.Length; ++i) CheckItem(items[i].gameObject);

            // Ensure all of the states point to a preset
            StateBehavior[] stateBehaviors;
            if (fromScene)
                stateBehaviors = Object.FindObjectsOfType<StateBehavior>();
            else
                stateBehaviors = objectReferences.GetComponentsInChildren<StateBehavior>(true);
            if (stateBehaviors != null)
                for (var i = 0; i < stateBehaviors.Length; ++i)
                {
                    stateBehaviors[i].States = RemoveUnusedStates(stateBehaviors[i].States);
                    EditorUtility.SetDirty(stateBehaviors[i]);
                }
#endif
            // Some doors should be locked.
#if !FIRST_PERSON_CONTROLLER
            LockDoors(objectReferences.FirstPersonDoors);
#endif
#if !THIRD_PERSON_CONTROLLER
            LockDoors(objectReferences.ThirdPersonDoors);
#endif

            for (var i = 0; i < objectReferences.NestedReferences.Length; ++i)
            {
                var nestedObject = objectReferences.NestedReferences[i];
                if (nestedObject == null) continue;
                GameObject nestedRoot = null;
                if (PrefabUtility.IsPartOfPrefabAsset(nestedObject))
                {
                    nestedRoot =
                        PrefabUtility.LoadPrefabContents(
                            AssetDatabase.GetAssetPath(objectReferences.NestedReferences[i]));
                    nestedObject = nestedRoot.GetComponent<ObjectReferences>();
                }

                ProcessObjectReferences(nestedObject, false);
                if (nestedRoot != null)
                {
                    PrefabUtility.SaveAsPrefabAsset(nestedRoot,
                        AssetDatabase.GetAssetPath(objectReferences.NestedReferences[i]));
                    PrefabUtility.UnloadPrefabContents(nestedRoot);
                }
            }

            UnpackPrefab(objectReferences);
            Object.DestroyImmediate(objectReferences, true);
        }

        /// <summary>
        ///     Removes the specified objects.
        /// </summary>
        private static void RemoveObjects(Object[] objects)
        {
            if (objects == null) return;

            for (var i = objects.Length - 1; i > -1; --i)
            {
                if (objects[i] == null ||
                    PrefabUtility.GetPrefabAssetType(objects[i]) == PrefabAssetType.MissingAsset) continue;

                if (objects[i] is GameObject && (objects[i] as GameObject).transform.parent == null &&
                    AssetDatabase.GetAssetPath(objects[i]).Length > 0 &&
                    PrefabUtility.GetPrefabAssetType(objects[i]) == PrefabAssetType.Regular)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(objects[i]));
                }
                else
                {
                    UnpackPrefab(objects[i]);
                    Object.DestroyImmediate(objects[i], true);
                }
            }
        }

#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
        /// <summary>
        ///     Ensure the item only has the valid states.
        /// </summary>
        private static void CheckItem(GameObject gameObject)
        {
            if (gameObject == null || gameObject.GetComponent<Item>() == null) return;

            var magicItems = gameObject.GetComponents<MagicItem>();
            for (var i = 0; i < magicItems.Length; ++i)
            {
                magicItems[i].DeserializeBeginActions(false);
                var beginActions = magicItems[i].BeginActions;
                if (beginActions != null)
                {
                    for (var j = 0; j < beginActions.Length; ++j)
                        beginActions[j].States = RemoveUnusedStates(beginActions[j].States);
                    magicItems[i].BeginActionData = Serialization.Serialize<BeginEndAction>(beginActions);
                }

                magicItems[i].DeserializeCastActions(false);
                var castActions = magicItems[i].CastActions;
                if (castActions != null)
                {
                    for (var j = 0; j < castActions.Length; ++j)
                        castActions[j].States = RemoveUnusedStates(castActions[j].States);
                    magicItems[i].CastActionData = Serialization.Serialize<CastAction>(castActions);
                }

                magicItems[i].DeserializeImpactActions(false);
                var impactActions = magicItems[i].ImpactActions;
                if (impactActions != null)
                {
                    for (var j = 0; j < impactActions.Length; ++j)
                        impactActions[j].States = RemoveUnusedStates(impactActions[j].States);
                    magicItems[i].ImpactActionData = Serialization.Serialize<ImpactAction>(impactActions);
                }

                magicItems[i].DeserializeEndActions(false);
                var endActions = magicItems[i].EndActions;
                if (endActions != null)
                {
                    for (var j = 0; j < endActions.Length; ++j)
                        endActions[j].States = RemoveUnusedStates(endActions[j].States);
                    magicItems[i].EndActionData = Serialization.Serialize<BeginEndAction>(endActions);
                }

                EditorUtility.SetDirty(magicItems[i]);
            }
        }
#endif

        /// <summary>
        ///     Unpacks the prefab root.
        /// </summary>
        /// <param name="obj">The object that should be unpacked.</param>
        private static void UnpackPrefab(Object obj)
        {
            if (obj != null && PrefabUtility.IsPartOfAnyPrefab(obj))
            {
                var root = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                if (root != null)
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
            }
        }

#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
        /// <summary>
        ///     Removes any states whose preset will be exlcluded.
        /// </summary>
        private static State[] RemoveUnusedStates(State[] stateArray)
        {
            var states = new List<State>(stateArray);
            var stateRemovals = new HashSet<string>();
            for (var i = states.Count - 2; i > -1; --i)
            {
                var preset = states[i].Preset;
                if (preset == null)
                {
                    stateRemovals.Add(states[i].Name);
                    states.RemoveAt(i);
                }
            }

            for (var i = 0; i < states.Count; ++i)
            {
                if (states[i].BlockList == null) continue;
                var blockList = new List<string>(states[i].BlockList);
                for (var j = blockList.Count - 1; j > -1; --j)
                    if (stateRemovals.Contains(blockList[j]))
                        blockList.RemoveAt(j);
                states[i].BlockList = blockList.ToArray();
            }

            return states.ToArray();
        }

        /// <summary>
        ///     Locks the unused doors.
        /// </summary>
        private static void LockDoors(GameObject[] doors)
        {
            if (doors == null) return;

            for (var i = 0; i < doors.Length; ++i)
            {
                if (doors[i] == null) continue;
                var door = doors[i].GetComponent<Door>();
                if (door == null) continue;
                door.PermanentlyLocked = true;
                EditorUtility.SetDirty(door);
            }
        }
#endif
    }
}
#endif