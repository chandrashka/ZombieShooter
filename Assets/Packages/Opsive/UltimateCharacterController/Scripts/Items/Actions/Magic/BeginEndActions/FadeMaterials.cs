﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using System.Collections.Generic;
using Opsive.Shared.Game;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Character.Identifiers;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;
using UnityEngine.Rendering;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.BeginEndActions
{
    /// <summary>
    ///     Fades the materials on the character.
    /// </summary>
    [Serializable]
    public class FadeMaterials : BeginEndAction
    {
        [Tooltip("The name of the material property that should be faded.")] [SerializeField]
        protected string m_ColorPropertyName = "_Color";

        [Tooltip("The alpha color that the materials should fade to.")] [SerializeField]
        protected float m_TargetAlpha;

        [Tooltip("The speed of the fade.")] [SerializeField]
        protected float m_FadeSpeed = 0.02f;

        [Tooltip("Should the fade be reverted when the action stops?")] [SerializeField]
        protected bool m_RevertFadeOnStop;

        public string ColorPropertyName
        {
            get => m_ColorPropertyName;
            set => m_ColorPropertyName = value;
        }

        public float TargetAlpha
        {
            get => m_TargetAlpha;
            set => m_TargetAlpha = value;
        }

        public float FadeSpeed
        {
            get => m_FadeSpeed;
            set => m_FadeSpeed = value;
        }

        private int m_ColorID;
        private GameObject m_Character;
        private bool m_Active;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private ScheduledEventBase m_UpdateEvent;
#endif

        private FadeMaterials m_BeginFadeMaterials;
        public List<Material> Materials { get; private set; } = new();

        public HashSet<Material> ActiveMaterials { get; private set; } = new();

        public Dictionary<Material, OriginalMaterialValue> OriginalMaterialValuesMap { get; private set; } = new();

        /// <summary>
        ///     Initializes the BeginEndAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the BeginEndAction belongs to.</param>
        /// <param name="beginAction">True if the action is a begin action.</param>
        /// <param name="index">The index of the BeginEndAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, bool beginAction, int index)
        {
            base.Initialize(character, magicItem, beginAction, index);

            m_Character = character;

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        ///     The action has started.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void Start(Transform origin)
        {
            // Initialize any starting values after all of the actions have been deserialized.
            if (m_ColorID == 0)
            {
                m_ColorID = Shader.PropertyToID(m_ColorPropertyName);
                if (!m_BeginAction && m_MagicItem.BeginActions != null)
                    for (var i = 0; i < m_MagicItem.BeginActions.Length; ++i)
                        if (m_MagicItem.BeginActions[i] is FadeMaterials)
                        {
                            m_BeginFadeMaterials = m_MagicItem.BeginActions[i] as FadeMaterials;
                            break;
                        }
            }

            // The Object Fader should reset.
            EventHandler.ExecuteEvent(m_Character, "OnCharacterIndependentFade", true, true);
            if (m_BeginFadeMaterials == null)
            {
                // Return the previous objects.
                if (OriginalMaterialValuesMap.Count > 0)
                    for (var i = 0; i < Materials.Count; ++i)
                    {
                        GenericObjectPool.Return(OriginalMaterialValuesMap[Materials[i]]);
                        OriginalMaterialValuesMap.Remove(Materials[i]);
                    }

                Materials.Clear();
                ActiveMaterials.Clear();

                EnableRendererFade();
            }
            else
            {
                Materials = m_BeginFadeMaterials.Materials;
                ActiveMaterials = m_BeginFadeMaterials.ActiveMaterials;
                OriginalMaterialValuesMap = m_BeginFadeMaterials.OriginalMaterialValuesMap;
            }

            m_Active = true;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // Update isn't called automatically for the remote players.
            if (m_MagicItem.NetworkInfo != null && !m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                m_UpdateEvent = SchedulerBase.Schedule(0.001f, Update);
            }
#endif
        }

        /// <summary>
        ///     Enables fading on the renderers.
        /// </summary>
        private void EnableRendererFade()
        {
            // Fade all of the active renderers.
            var renderers = m_Character.GetComponentsInChildren<Renderer>(false);
            for (var i = 0; i < renderers.Length; ++i)
            {
                // The fade can be ignored.
                if (renderers[i].gameObject.GetCachedComponent<IgnoreFadeIdentifier>() != null) continue;

                var materials = renderers[i].materials;
                for (var j = 0; j < materials.Length; ++j)
                {
                    var material = materials[j];
                    if (ActiveMaterials.Contains(material) || !material.HasProperty(m_ColorID)) continue;

                    Materials.Add(material);
                    ActiveMaterials.Add(material);

                    // Cache the original values so they can be reverted.
                    var originalMaterialValues = GenericObjectPool.Get<OriginalMaterialValue>();
                    originalMaterialValues.Initialize(material, m_ColorID,
                        material.HasProperty(OriginalMaterialValue.ModeID));
                    OriginalMaterialValuesMap.Add(material, originalMaterialValues);

                    // The material should be able to fade.
                    material.SetFloat(OriginalMaterialValue.ModeID, 2);
                    material.SetInt(OriginalMaterialValue.SrcBlendID, (int)BlendMode.SrcAlpha);
                    material.SetInt(OriginalMaterialValue.DstBlendID, (int)BlendMode.OneMinusSrcAlpha);
                    material.EnableKeyword(OriginalMaterialValue.AlphaBlendString);
                    material.renderQueue = 3000;

                    // If the action is already active then the material is being faded when the perspective is switching. Set the alpha to the 
                    // same alpha value as the rest of the materials.
                    if (m_Active)
                    {
                        var color = material.GetColor(m_ColorID);
                        color.a = Materials[0].GetColor(m_ColorID).a;
                        material.SetColor(m_ColorID, color);
                    }
                }
            }
        }

        /// <summary>
        ///     Updates the action.
        /// </summary>
        public override void Update()
        {
            if (!m_Active) return;

            var active = false;
            for (var i = 0; i < Materials.Count; ++i)
            {
                var color = Materials[i].GetColor(m_ColorID);
                color.a = Mathf.MoveTowards(color.a, m_TargetAlpha, m_FadeSpeed);
                Materials[i].SetColor(m_ColorID, color);
                if (color.a != m_TargetAlpha) active = true;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // Update isn't called automatically for the remote players.
            if (active && m_MagicItem.NetworkInfo != null && !m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                m_UpdateEvent = SchedulerBase.Schedule(0.001f, Update);
            }
#endif

            m_Active = active;
        }

        /// <summary>
        ///     The action has stopped.
        /// </summary>
        public override void Stop()
        {
            if (!m_Active) return;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_MagicItem.NetworkInfo != null && !m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                SchedulerBase.Cancel(m_UpdateEvent);
                m_UpdateEvent = null;
            }
#endif

            EventHandler.ExecuteEvent(m_Character, "OnCharacterIndependentFade", false, false);
            m_Active = false;
            if (!m_RevertFadeOnStop) return;

            // Revert the values back to the original values.
            var fade = m_BeginFadeMaterials != null ? m_BeginFadeMaterials : this;
            var originalMaterialValues = fade.OriginalMaterialValuesMap;
            var materials = fade.Materials;
            for (var i = 0; i < materials.Count; ++i)
            {
                if (!originalMaterialValues.TryGetValue(materials[i], out var originalMaterialValue)) continue;

                // Revert the material back to the starting value.
                materials[i].SetColor(m_ColorID, originalMaterialValue.Color);
                if (originalMaterialValue.ContainsMode)
                {
                    materials[i].SetFloat(OriginalMaterialValue.ModeID, originalMaterialValue.Mode);
                    materials[i].SetInt(OriginalMaterialValue.SrcBlendID, originalMaterialValue.SrcBlend);
                    materials[i].SetInt(OriginalMaterialValue.DstBlendID, originalMaterialValue.DstBlend);
                }

                if (!originalMaterialValue.AlphaBlend)
                    materials[i].DisableKeyword(OriginalMaterialValue.AlphaBlendString);
                materials[i].renderQueue = originalMaterialValue.RenderQueue;
            }
        }

        /// <summary>
        ///     The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            if (firstPersonPerspective || !m_Active) return;

            EnableRendererFade();
        }

        /// <summary>
        ///     The action has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }
    }
}