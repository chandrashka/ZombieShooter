﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Effects;
using Opsive.UltimateCharacterController.Traits.Damage;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    ///     Extends the health component by allowing the character to take fall damage. The amount of damage is specified by a
    ///     curve.
    /// </summary>
    public class CharacterHealth : Health
    {
        [Tooltip("Should fall damage be applied?")] [SerializeField]
        protected bool m_ApplyFallDamage;

        [Tooltip("The minimum height that the character has to fall in order for any damage to be applied.")]
        [SerializeField]
        protected float m_MinFallDamageHeight = 3;

        [Tooltip("The amount of damage to apply when the player falls by the minimum fall damage height.")]
        [SerializeField]
        protected float m_MinFallDamage = 1;

        [Tooltip("The amount of damage to apply when the player falls just less than the death height.")]
        [SerializeField]
        protected float m_MaxFallDamage = 50;

        [Tooltip("A fall greater than this value is an instant death.")] [SerializeField]
        protected float m_DeathHeight = 20;

        [Tooltip(
            "A curve specifying the amount of damage to apply if the character falls between the min and max fall damage values.")]
        [SerializeField]
        protected AnimationCurve m_DamageCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("The effect that should be started when the character takes damage.")]
        [HideInInspector]
        [SerializeField]
        protected string m_DamagedEffectName;

        [Tooltip("The index of the effect that should be started when the character takes damage.")]
        [HideInInspector]
        [SerializeField]
        protected int m_DamagedEffectIndex = -1;

        private int m_CharacterLayer;

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Effect m_DamagedEffect;

        public bool ApplyFallDamage
        {
            get => m_ApplyFallDamage;
            set => m_ApplyFallDamage = value;
        }

        public float MinFallDamageHeight
        {
            get => m_MinFallDamageHeight;
            set => m_MinFallDamageHeight = value;
        }

        public float MinFallDamage
        {
            get => m_MinFallDamage;
            set => m_MinFallDamage = value;
        }

        public float MaxFallDamage
        {
            get => m_MaxFallDamage;
            set => m_MaxFallDamage = value;
        }

        public float DeathHeight
        {
            get => m_DeathHeight;
            set => m_DeathHeight = value;
        }

        public AnimationCurve DamageCurve
        {
            get => m_DamageCurve;
            set => m_DamageCurve = value;
        }

        public string StartDamagedName
        {
            get => m_DamagedEffectName;
            set => m_DamagedEffectName = value;
        }

        public int DamagedEffectIndex
        {
            get => m_DamagedEffectIndex;
            set => m_DamagedEffectIndex = value;
        }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_CharacterLocomotion = m_GameObject.GetCachedComponent<UltimateCharacterLocomotion>();

            EventHandler.RegisterEvent<float>(m_GameObject, "OnCharacterLand", OnCharacterLand);
        }

        /// <summary>
        ///     Initialize the collider layers after the UltimateCharacterLocomotion has been initialized.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (!string.IsNullOrEmpty(m_DamagedEffectName))
                m_DamagedEffect =
                    m_CharacterLocomotion.GetEffect(TypeUtility.GetType(m_DamagedEffectName), m_DamagedEffectIndex);
        }

        /// <summary>
        ///     The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<float>(m_GameObject, "OnCharacterLand", OnCharacterLand);
        }

        /// <summary>
        ///     The object has taken been damaged.
        /// </summary>
        /// <param name="damageData">The data associated with the damage.</param>
        public override void OnDamage(DamageData damageData)
        {
            base.OnDamage(damageData);

            if (m_DamagedEffect != null) m_CharacterLocomotion.TryStartEffect(m_DamagedEffect);
        }

        /// <summary>
        ///     The object is no longer alive
        /// </summary>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        public override void Die(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_CharacterLayer = m_GameObject.layer;
            base.Die(position, force, attacker);

            if (m_DeathLayer != 0) m_CharacterLocomotion.SetCollisionLayer(m_DeathLayer);
        }

        /// <summary>
        ///     The object has spawned again. Set the health and shield back to their starting values.
        /// </summary>
        protected override void OnRespawn()
        {
            base.OnRespawn();

            if (m_DeathLayer != 0) m_CharacterLocomotion.SetCollisionLayer(m_CharacterLayer);
        }

        /// <summary>
        ///     The character has landed after falling a spcified amount. Determine if any damage should be taken.
        /// </summary>
        /// <param name="fallHeight"></param>
        private void OnCharacterLand(float fallHeight)
        {
            // Return immediately if the character isn't being damaged by a fall or the fall height is less than the minimum height
            // that the character has to fall in order to start taking damage.
            if (!m_ApplyFallDamage || fallHeight < m_MinFallDamageHeight) return;

            var damageAmount = 0f;
            // The fall was too great, specify an infinite amount of damage.
            if (fallHeight >= m_DeathHeight)
            {
                damageAmount = Mathf.Infinity;
            }
            else
            {
                // The fall was somewhere in between the min and max fall height. Use the damage curve to determine how much damage to apply.
                var normalizedHeight = (fallHeight - m_MinFallDamageHeight) / (m_DeathHeight - m_MinFallDamageHeight);
                var damageAmountMultiplier = m_DamageCurve.Evaluate(normalizedHeight);
                damageAmount = m_MinFallDamage + damageAmountMultiplier * (m_MaxFallDamage - m_MinFallDamage);
            }

            // Apply the damage.
            Damage(damageAmount, m_Transform.position, Vector3.zero, 0, m_GameObject);
        }
    }
}