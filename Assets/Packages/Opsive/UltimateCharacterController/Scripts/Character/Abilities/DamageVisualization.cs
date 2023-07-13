/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    ///     Plays an animation when the character takes damage.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultStopType(AbilityStopType.Manual)]
    [DefaultAbilityIndex(10)]
    public class DamageVisualization : Ability
    {
        private ScheduledEventBase m_CompleteEvent;

        [Tooltip(
            "Specifies if the ability should wait for the OnAnimatorDamageVisualizationComplete animation event or wait for the specified duration before interacting with the item.")]
        [SerializeField]
        protected AnimationEventTrigger m_DamageVisualizationCompleteEvent = new(false, 0.2f);

        [Tooltip("The minimum amount of damage required for the ability to start.")] [SerializeField]
        protected float m_MinDamageAmount;

        private int m_TakeDamageIndex;

        private float MinDamageAmount
        {
            get => m_MinDamageAmount;
            set => m_MinDamageAmount = value;
        }

        private AnimationEventTrigger DamageVisualizationCompleteEvent
        {
            get => m_DamageVisualizationCompleteEvent;
            set => m_DamageVisualizationCompleteEvent = value;
        }

        public override int AbilityIntData => m_TakeDamageIndex;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_GameObject, "OnHealthDamage",
                OnDamage);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorDamageVisualizationComplete",
                OnDamageVisualizationComplete);
        }

        /// <summary>
        ///     The character has taken damage.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        private void OnDamage(float amount, Vector3 position, Vector3 force, GameObject attacker, Collider hitCollider)
        {
            // The ability shouldn't start if the damage amount doesn't meet the minimum amount required.
            if (amount < m_MinDamageAmount) return;

            // The ability shouldn't start if the damage is internal (such as a fall damage).
            if (attacker == null) return;

            m_TakeDamageIndex = GetDamageTypeIndex(amount, position, force, attacker);
            if (m_TakeDamageIndex != -1) StartAbility();
        }

        /// <summary>
        ///     Returns the value that the AbilityIntData parameter should be set to.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the character.</param>
        /// <param name="attacker">The GameObject that damaged the character.</param>
        /// <returns>
        ///     The value that the AbilityIntData parameter should be set to. A value of -1 will prevent the ability from
        ///     starting.
        /// </returns>
        protected virtual int GetDamageTypeIndex(float amount, Vector3 position, Vector3 force, GameObject attacker)
        {
            var direction = m_Transform.InverseTransformPoint(position);
            if (direction.z > 0)
            {
                if (direction.x > 0) return (int)TakeDamageIndex.FrontRight;
                return (int)TakeDamageIndex.FrontLeft;
            }

            if (direction.z < 0)
            {
                if (direction.x > 0) return (int)TakeDamageIndex.BackRight;
                return (int)TakeDamageIndex.BackLeft;
            }

            return -1;
        }

        /// <summary>
        ///     The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            if (!m_DamageVisualizationCompleteEvent.WaitForAnimationEvent)
                m_CompleteEvent = SchedulerBase.Schedule(m_DamageVisualizationCompleteEvent.Duration,
                    OnDamageVisualizationComplete);
        }

        /// <summary>
        ///     Animation event callback when the damage visualization animation has completed.
        /// </summary>
        private void OnDamageVisualizationComplete()
        {
            StopAbility();
        }

        /// <summary>
        ///     The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            SchedulerBase.Cancel(m_CompleteEvent);
        }

        /// <summary>
        ///     Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_GameObject, "OnHealthDamage",
                OnDamage);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorDamageVisualizationComplete",
                OnDamageVisualizationComplete);
        }

        /// <summary>
        ///     The type of animation that the ability should play.
        /// </summary>
        private enum TakeDamageIndex
        {
            FrontLeft, // Play an animation based upon a damage position on the front left.
            FrontRight, // Play an animation based upon a damage position on the front right.
            BackLeft, // Play an animation based upon a damage position on the back left.
            BackRight // Play an animation based upon a damage position on the back right.
        }
    }
}