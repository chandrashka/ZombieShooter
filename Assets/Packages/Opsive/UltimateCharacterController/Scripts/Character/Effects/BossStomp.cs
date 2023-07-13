/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Effects
{
    /// <summary>
    ///     Moves the camera downward similar to how a large boss would shake the camera as they are stomping on the ground.
    /// </summary>
    public class BossStomp : Effect
    {
        [Tooltip("The direction to apply the positional force.")] [SerializeField]
        protected Vector3 m_PositionalStompDirection = Vector3.down;

        [Tooltip("The strength of the positional boss stomp.")] [SerializeField]
        protected MinMaxFloat m_PositionalStrength = new(0.5f, 1);

        [Tooltip(
            "The number of times the stomp effect should play. Set to -1 to play the efffect until the effect is stopped or disabled.")]
        [SerializeField]
        protected int m_RepeatCount;

        [Tooltip("The delay until the stomp plays again.")] [SerializeField]
        protected float m_RepeatDelay = 1;

        [Tooltip("The direction to apply the rotational force.")] [SerializeField]
        protected Vector3 m_RotationalStompDirection = Vector3.forward;

        [Tooltip("The strength of the rotational boss stomp.")] [SerializeField]
        protected MinMaxFloat m_RotationalStrength = new(10, 15);

        private int m_StopCount;
        private ScheduledEventBase m_StopEvent;

        public Vector3 PositionalStompDirection
        {
            get => m_PositionalStompDirection;
            set => m_PositionalStompDirection = value;
        }

        public MinMaxFloat PositionalStrength
        {
            get => m_PositionalStrength;
            set => m_PositionalStrength = value;
        }

        public Vector3 RotationalStompDirection
        {
            get => m_RotationalStompDirection;
            set => m_RotationalStompDirection = value;
        }

        public MinMaxFloat RotationalStrength
        {
            get => m_RotationalStrength;
            set => m_RotationalStrength = value;
        }

        public int RepeatCount
        {
            get => m_RepeatCount;
            set => m_RepeatCount = value;
        }

        public float RepeatDelay
        {
            get => m_RepeatDelay;
            set => m_RepeatDelay = value;
        }

        /// <summary>
        ///     Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanStartEffect()
        {
            return m_CameraController != null;
        }

        /// <summary>
        ///     The effect has been started.
        /// </summary>
        protected override void EffectStarted()
        {
            base.EffectStarted();

            m_StopCount = 0;
            Stomp();
        }

        /// <summary>
        ///     Performs the stomp effect.
        /// </summary>
        private void Stomp()
        {
            m_CameraController.AddSecondaryPositionalForce(
                m_PositionalStompDirection * m_PositionalStrength.RandomValue, 0);
            m_CameraController.AddSecondaryRotationalForce(
                m_RotationalStrength.RandomValue * (Random.value > 0.5f ? 1 : -1) * m_RotationalStompDirection, 0);
            m_StopCount++;
            m_StopEvent = null;

            if (m_RepeatCount == -1 || m_StopCount < m_RepeatCount)
                m_StopEvent = SchedulerBase.ScheduleFixed(m_RepeatDelay, Stomp);
            else
                StopEffect();
        }

        /// <summary>
        ///     The effect has stopped running.
        /// </summary>
        protected override void EffectStopped()
        {
            base.EffectStopped();

            SchedulerBase.Cancel(m_StopEvent);
            m_StopEvent = null;
        }
    }
}