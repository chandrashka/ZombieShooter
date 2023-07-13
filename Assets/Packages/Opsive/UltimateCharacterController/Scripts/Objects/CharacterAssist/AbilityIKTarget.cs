/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Character;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    ///     Specifies the location that the ability should use when determining where to move the limb to. This component
    ///     should be attached to the target limb location.
    /// </summary>
    public class AbilityIKTarget : MonoBehaviour
    {
        [Tooltip("The IK limb that should be positioned.")] [SerializeField]
        protected CharacterIKBase.IKGoal m_Goal;

        [Tooltip("The amount of time that the ability should wait before setting the IK goal.")] [SerializeField]
        protected float m_Delay;

        [Tooltip("The time it takes for the limb to reach the target. A positive value is required.")] [SerializeField]
        protected float m_InterpolationDuration = 0.2f;

        [Tooltip(
            "The amount of time after the IK goal is set that the limb should be in the IK location. This value should be greater than the interpolation duration.")]
        [SerializeField]
        protected float m_Duration = 1f;

        public CharacterIKBase.IKGoal Goal => m_Goal;
        public float Delay => m_Delay;
        public float InterpolationDuration => m_InterpolationDuration;
        public float Duration => m_Duration;
        public Transform Transform { get; private set; }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            Transform = transform;
        }
    }
}