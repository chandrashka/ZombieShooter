/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Game;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Effects;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions
{
    /// <summary>
    ///     Starts an effect on the character.
    /// </summary>
    [Serializable]
    public class StartEffect : CastAction
    {
        [Tooltip("The effect that should be started when the ability starts.")] [HideInInspector] [SerializeField]
        protected string m_EffectName;

        [Tooltip("The index of the effect that should be started when the ability starts.")]
        [HideInInspector]
        [SerializeField]
        protected int m_EffectIndex = -1;

        [Tooltip("Should the effect be stopped when the cast is stopped?")] [SerializeField]
        protected bool m_StopEffect;

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Effect m_Effect;

        public string EffectName
        {
            get => m_EffectName;
            set => m_EffectName = value;
        }

        public int EffectIndex
        {
            get => m_EffectIndex;
            set => m_EffectIndex = value;
        }

        public bool StopEffect
        {
            get => m_StopEffect;
            set => m_StopEffect = value;
        }

        /// <summary>
        ///     Initializes the CastAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the CastAction belongs to.</param>
        /// <param name="index">The index of the CastAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character, magicItem, index);

            m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
        }

        /// <summary>
        ///     Awake is called after all of the actions have been initialized.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            if (!string.IsNullOrEmpty(m_EffectName))
                m_Effect = m_CharacterLocomotion.GetEffect(TypeUtility.GetType(m_EffectName), m_EffectIndex);
            if (m_Effect == null) Debug.LogError($"Error: Unable to find effect {m_EffectName}.");
        }

        /// <summary>
        ///     Performs the cast.
        /// </summary>
        /// <param name="origin">The location that the cast should spawn from.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        public override void Cast(Transform origin, Vector3 direction, Vector3 targetPosition)
        {
            if (m_Effect == null) return;

            m_CharacterLocomotion.TryStartEffect(m_Effect);
        }

        /// <summary>
        ///     Stops the cast.
        /// </summary>
        public override void Stop()
        {
            if (m_StopEffect && m_Effect != null) m_CharacterLocomotion.TryStopEffect(m_Effect);

            base.Stop();
        }
    }
}