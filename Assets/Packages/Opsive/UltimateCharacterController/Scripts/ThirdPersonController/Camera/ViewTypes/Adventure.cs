﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    ///     The Adventure View Type will inherit the functionality from the Third Person View Type while allowing the camera
    ///     yaw to rotate freely.
    /// </summary>
    [RecommendedMovementType(typeof(Character.MovementTypes.Adventure))]
    [RecommendedMovementType(typeof(FourLegged))]
    [AddState("Zoom", "da67cc4518129ec40bc4e49daeff5c3a")]
    public class Adventure : ThirdPerson
    {
        [Tooltip("The maximum yaw angle (in degrees).")] [SerializeField]
        protected float m_MaxYawLimit = 180;

        [Tooltip("The minimum yaw angle (in degrees).")] [SerializeField]
        protected float m_MinYawLimit = -180;

        [Tooltip("The speed in which the camera should rotate towards the yaw limit when out of bounds.")]
        [Range(0, 1)]
        [SerializeField]
        protected float m_YawLimitLerpSpeed = 0.7f;

        public float MinYawLimit
        {
            get => m_MinYawLimit;
            set => m_MinYawLimit = value;
        }

        public float MaxYawLimit
        {
            get => m_MaxYawLimit;
            set => m_MaxYawLimit = value;
        }

        public float YawLimitLerpSpeed
        {
            get => m_YawLimitLerpSpeed;
            set => m_YawLimitLerpSpeed = value;
        }

        /// <summary>
        ///     Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediatePosition">Should the camera be positioned immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediatePosition)
        {
            // Update the rotation. The yaw may have a limit.
            if (Mathf.Abs(m_MinYawLimit - m_MaxYawLimit) < 360)
            {
                // Determine the new rotation with the updated yaw.
                var targetRotation =
                    MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0));
                var diff = MathUtility.InverseTransformQuaternion(
                    Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up),
                    targetRotation * Quaternion.Inverse(m_CharacterTransform.rotation));
                // The rotation shouldn't extend beyond the min and max yaw limit.
                var targetYaw = MathUtility.ClampAngle(diff.eulerAngles.y, horizontalMovement, m_MinYawLimit,
                    m_MaxYawLimit);
                m_Yaw += Mathf.Lerp(0, Mathf.DeltaAngle(diff.eulerAngles.y, targetYaw), m_YawLimitLerpSpeed);
            }
            else
            {
                m_Yaw += horizontalMovement;
            }

            // Return the rotation.
            return base.Rotate(horizontalMovement, verticalMovement, immediatePosition);
        }
    }
}