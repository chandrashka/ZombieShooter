﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Collections.Generic;
using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Identifiers;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using UnityEngine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character
{
    /// <summary>
    ///     Manages the objects that are changed between a first and third person perspective.
    /// </summary>
    public class PerspectiveMonitor : StateBehavior
    {
        /// <summary>
        ///     Specifies which objects should be visible when the character dies in a first person view.
        /// </summary>
        public enum ObjectDeathVisiblity
        {
            AllInvisible, // The entire rig will be invisible upon death.
            ThirdPersonObjectDetermined, // Only the objects marked with the ThirdPersonObject and ThirdPersonObject.InvisibleOnDeath set to true will be invisible upon death.
            AllVisible // No objects will be invisible upon death.
        }

        [Tooltip("The material used to make the object invisible but still cast shadows.")] [SerializeField]
        protected Material m_InvisibleMaterial;

        [Tooltip("Specifies which objects should be visible when the character dies in a first person view.")]
        [SerializeField]
        protected ObjectDeathVisiblity m_DeathVisibility = ObjectDeathVisiblity.AllVisible;

        private UltimateCharacterLocomotion m_CharacterLocomotion;

        private bool m_FirstPersonPerspective;

        private GameObject m_GameObject;
        private InventoryBase m_Inventory;
        private readonly List<Material[]> m_InvisibleMaterials = new();
        private readonly List<Material[]> m_OriginalMaterials = new();
        private readonly HashSet<Renderer> m_RegisteredRenderers = new();
        private readonly List<Renderer> m_Renderers = new();
        private readonly List<ThirdPersonObject> m_RendererThirdPersonObjects = new();
        private readonly List<int> m_ThirdPersonRenderers = new();

        public Material InvisibleMaterial
        {
            get => m_InvisibleMaterial;
            set => m_InvisibleMaterial = value;
        }

        public ObjectDeathVisiblity DeathVisiblity
        {
            get => m_DeathVisibility;
            set => m_DeathVisibility = value;
        }

        /// <summary>
        ///     Registeres for any interested events.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_CharacterLocomotion = m_GameObject.GetCachedComponent<UltimateCharacterLocomotion>();

            // The third person objects will be hidden with the invisible shadow caster while in first person view.
            var characterRenderers = m_GameObject.GetComponentsInChildren<Renderer>(true);
            if (characterRenderers != null)
                for (var i = 0; i < characterRenderers.Length; ++i)
                {
                    var characterRenderer = characterRenderers[i];
                    if (m_RegisteredRenderers.Contains(characterRenderer)) continue;

                    CacheRendererMaterials(characterRenderer);
                }

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCameraChangePerspectives", OnChangePerspectives);
            EventHandler.RegisterEvent<Item>(m_GameObject, "OnInventoryAddItem", OnAddItem);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnWillRespawn", OnWillRespawn);
        }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Start()
        {
            m_Inventory = m_GameObject.GetCachedComponent<InventoryBase>();
            m_FirstPersonPerspective = m_CharacterLocomotion.FirstPersonPerspective;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            var networkInfo = m_GameObject.GetCachedComponent<Networking.INetworkInfo>();
            if (networkInfo != null && !networkInfo.IsLocalPlayer()) {
                // Remote players should always be in the third person view.
                OnChangePerspectives(false);
                EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCameraChangePerspectives", OnChangePerspectives);
            }
#endif
        }

        /// <summary>
        ///     The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCameraChangePerspectives", OnChangePerspectives);
            EventHandler.UnregisterEvent<Item>(m_GameObject, "OnInventoryAddItem", OnAddItem);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnWillRespawn", OnWillRespawn);
        }

        /// <summary>
        ///     Caches the materials attached to the renderer so they can be switched with the invisible material.
        /// </summary>
        /// <param name="materialRenderer">The renderer to cache.</param>
        private void CacheRendererMaterials(Renderer materialRenderer)
        {
            var thirdPersonobject = materialRenderer.gameObject.GetCachedInactiveComponentInParent<ThirdPersonObject>();
            if (thirdPersonobject != null) m_ThirdPersonRenderers.Add(m_Renderers.Count);

            m_Renderers.Add(materialRenderer);
            m_RendererThirdPersonObjects.Add(thirdPersonobject);
            m_RegisteredRenderers.Add(materialRenderer);
            m_OriginalMaterials.Add(materialRenderer.materials);
            var invisibleMaterials = new Material[materialRenderer.materials.Length];
            for (var i = 0; i < materialRenderer.materials.Length; ++i) invisibleMaterials[i] = m_InvisibleMaterial;
            m_InvisibleMaterials.Add(invisibleMaterials);
        }

        /// <summary>
        ///     The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            m_FirstPersonPerspective = firstPersonPerspective;

            if (!m_GameObject.activeSelf) return;

            UpdateThirdPersonMaterials(false);

            // The FirstPersonObjects GameObject must be changed first to prevent activation errors/warnings. After the GameObject has been changed the 
            // character components can safely receive the event.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangePerspectives", firstPersonPerspective);
        }

        /// <summary>
        ///     Updates the materials of the third person objects.
        /// </summary>
        public void UpdateThirdPersonMaterials(bool forceThirdPerson)
        {
            for (var i = 0; i < m_ThirdPersonRenderers.Count; ++i)
            {
                var thirdPersonIndex = m_ThirdPersonRenderers[i];
                m_Renderers[thirdPersonIndex].materials =
                    m_FirstPersonPerspective && !m_RendererThirdPersonObjects[thirdPersonIndex].ForceVisible &&
                    !forceThirdPerson
                        ? m_InvisibleMaterials[thirdPersonIndex]
                        : m_OriginalMaterials[thirdPersonIndex];
            }
        }

        /// <summary>
        ///     The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(Item item)
        {
            // The Third Person's PerspectiveItem object will contain a reference to the ThirdPersonObject component.
            var perspectiveItems = item.GetComponents<PerspectiveItem>();
            PerspectiveItem thirdPersonPerspectiveItem = null;
            for (var i = 0; i < perspectiveItems.Length; ++i)
                if (!perspectiveItems[i].FirstPersonItem)
                {
                    thirdPersonPerspectiveItem = perspectiveItems[i];
                    break;
                }

            if (thirdPersonPerspectiveItem != null && thirdPersonPerspectiveItem.Object != null)
            {
                var thirdPersonObject = thirdPersonPerspectiveItem.Object.GetComponent<ThirdPersonObject>();
                // If the third person object exists then it should be added to the materials list.
                if (thirdPersonObject != null)
                {
                    var renderers = thirdPersonObject.GetComponentsInChildren<Renderer>(true);
                    for (var i = 0; i < renderers.Length; ++i)
                        if (!m_RegisteredRenderers.Contains(renderers[i]))
                        {
                            CacheRendererMaterials(renderers[i]);

                            // If the first person perspective is active then any third person item materials should use the invisible material.
                            if (m_CharacterLocomotion.FirstPersonPerspective)
                                renderers[i].materials = m_InvisibleMaterials[m_InvisibleMaterials.Count - 1];
                        }
                }
            }
        }

        /// <summary>
        ///     The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            TryUpdateDeathMaterials(true);
        }

        /// <summary>
        ///     Tries to switch to the materials used when the character died.
        /// </summary>
        /// <param name="fromDeathEvent">Is the method being called from the OnDeath event?</param>
        /// <returns>True if the materials were updated.</returns>
        private bool TryUpdateDeathMaterials(bool fromDeathEvent)
        {
            // Ensure no first person weapons are equipped before enabling the third person objects.
            if (m_CharacterLocomotion.FirstPersonPerspective)
                for (var i = 0; i < m_Inventory.SlotCount; ++i)
                    if (m_Inventory.GetActiveItem(i) != null)
                    {
                        // If an item is still equipped then the material shouldn't be switched until after it is no longer equipped.
                        if (fromDeathEvent)
                            EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnInventoryUnequipItem",
                                OnUnequipItem);
                        return false;
                    }

            // All items are unequipped. Update the renderers.
            for (var i = 0; i < m_Renderers.Count; ++i)
            {
                var invisibleObject = false;
                if (m_DeathVisibility == ObjectDeathVisiblity.AllInvisible)
                {
                    invisibleObject = m_CharacterLocomotion.FirstPersonPerspective;
                }
                else if (m_DeathVisibility == ObjectDeathVisiblity.ThirdPersonObjectDetermined)
                {
                    var thirdPersonObject = m_RendererThirdPersonObjects[i];
                    invisibleObject = m_CharacterLocomotion.FirstPersonPerspective &&
                                      thirdPersonObject != null && !thirdPersonObject.FirstPersonVisibleOnDeath &&
                                      !thirdPersonObject.ForceVisible;
                }

                m_Renderers[i].materials = invisibleObject ? m_InvisibleMaterials[i] : m_OriginalMaterials[i];
            }

            return true;
        }

        /// <summary>
        ///     The specified item has been unequipped.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        /// <param name="slotID"></param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (TryUpdateDeathMaterials(false))
                EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnInventoryUnequipItem", OnUnequipItem);
        }

        /// <summary>
        ///     The character will respawn. This should be performed before the MaterialSwapper's Respawn method is called.
        /// </summary>
        private void OnWillRespawn()
        {
            EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnInventoryUnequipItem", OnUnequipItem);

            for (var i = 0; i < m_Renderers.Count; ++i)
                m_Renderers[i].materials = !m_CharacterLocomotion.FirstPersonPerspective ||
                                           m_RendererThirdPersonObjects[i] == null
                    ? m_OriginalMaterials[i]
                    : m_InvisibleMaterials[i];
        }
    }
}