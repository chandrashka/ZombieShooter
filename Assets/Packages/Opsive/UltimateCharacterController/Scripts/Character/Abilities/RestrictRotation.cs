/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    ///     The RestrictPosition ability restricts the character to the specified rotation.
    /// </summary>
    [DefaultStartType(AbilityStartType.Automatic)]
    [DefaultStopType(AbilityStopType.Manual)]
    public class RestrictRotation : Ability
    {
        private ILookSource m_LookSource;

        [Tooltip("Any offset that should be applied to the look source y rotation.")] [SerializeField]
        protected float m_LookSourceOffset;

        [Tooltip("Any offset that should be applied to the local y rotation.")] [SerializeField]
        protected float m_Offset;

        [Tooltip("Should the local y rotation of the look source be applied to the rotation?")] [SerializeField]
        protected bool m_RelativeLookSourceRotation;

        [Tooltip("The number of degrees that the character can rotate between.")] [SerializeField]
        protected float m_Restriction = 45f;

        public float Restriction
        {
            get => m_Restriction;
            set => m_Restriction = value;
        }

        public float Offset
        {
            get => m_Offset;
            set => m_Offset = value;
        }

        public bool RelativeLookSourceRotation
        {
            get => m_RelativeLookSourceRotation;
            set => m_RelativeLookSourceRotation = value;
        }

        public float LookSourceOffset
        {
            get => m_LookSourceOffset;
            set => m_LookSourceOffset = value;
        }

        public override bool IsConcurrent => true;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // The look source may have already been assigned if the ability was added to the character after the look source was assigned.
            m_LookSource = m_CharacterLocomotion.LookSource;
            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
        }

        /// <summary>
        ///     A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
        }

        /// <summary>
        ///     Verify the rotation values. Called immediately before the rotation is applied.
        /// </summary>
        public override void ApplyRotation()
        {
            var targetRotation = m_Transform.rotation * Quaternion.Euler(m_CharacterLocomotion.DeltaRotation);
            var localTargetRotation =
                MathUtility.InverseTransformQuaternion(
                    Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up), targetRotation);

            // Find the closest angle to the degree restriction.
            var localEulerRotation = localTargetRotation.eulerAngles;
            var offset = m_Offset;

            // The rotation can be applied based on the look source angle.
            if (m_RelativeLookSourceRotation)
            {
                var localLookSourceRotation = MathUtility.InverseTransformQuaternion(
                    Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up),
                    m_LookSource.Transform.rotation);
                offset += localLookSourceRotation.eulerAngles.y + m_LookSourceOffset;
            }

            // Set the restricted rotation.
            localEulerRotation.y =
                MathUtility.ClampAngle(Mathf.Round((localEulerRotation.y - offset) / m_Restriction) * m_Restriction) +
                offset;

            // Rotate towards the restricted angle.
            var transformRotation = m_Transform.rotation;
            targetRotation = MathUtility.TransformQuaternion(
                Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up),
                Quaternion.Euler(localEulerRotation));
            targetRotation = Quaternion.Slerp(transformRotation, targetRotation,
                m_CharacterLocomotion.MotorRotationSpeed * Time.deltaTime);
            m_CharacterLocomotion.Torque = targetRotation * Quaternion.Inverse(transformRotation);
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
        }
    }
}