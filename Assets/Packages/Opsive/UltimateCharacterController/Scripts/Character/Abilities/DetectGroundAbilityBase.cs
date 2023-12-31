﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Objects;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    ///     Abstract class which determines if the ground object is a valid object.
    /// </summary>
    public abstract class DetectGroundAbilityBase : Ability
    {
        [Tooltip("The maximum angle that the character can be relative to the forward direction of the object.")]
        [Range(0, 180)]
        [SerializeField]
        protected float m_AngleThreshold = 180;

        protected Collider m_GroundCollider;

        [Tooltip(
            "The character is no longer over the ground if the dot product between the character's up direction and the ground normal is less than the sensitivity.")]
        [Range(0, 1)]
        [SerializeField]
        protected float m_GroundNormalSensitivity = 0.5f;

        protected Transform m_GroundTransform;

        [Tooltip("The layer mask of the ground object.")] [SerializeField]
        protected LayerMask m_LayerMask = -1;

        [Tooltip("Should the character move with the ground object?")] [HideInInspector] [SerializeField]
        protected bool m_MoveWithObject;

        [Tooltip(
            "The unique ID value of the Object Identifier component. A value of -1 indicates that this ID should not be used.")]
        [SerializeField]
        protected int m_ObjectID = -1;

        public int ObjectID
        {
            get => m_ObjectID;
            set => m_ObjectID = value;
        }

        public LayerMask LayerMask
        {
            get => m_LayerMask;
            set => m_LayerMask = value;
        }

        public float NormalSensitivity
        {
            get => m_GroundNormalSensitivity;
            set => m_GroundNormalSensitivity = value;
        }

        public float AngleThreshold
        {
            get => m_AngleThreshold;
            set => m_AngleThreshold = value;
        }

        public bool MoveWithObject
        {
            get => m_MoveWithObject;
            set
            {
                if (m_MoveWithObject == value) return;
                m_MoveWithObject = value;
                if (!IsActive || m_GroundTransform == null) return;
                if (m_MoveWithObject && m_CharacterLocomotion.Platform == null)
                    m_CharacterLocomotion.SetPlatform(m_GroundTransform);
                else if (!m_MoveWithObject && m_CharacterLocomotion.Platform == m_GroundTransform)
                    m_CharacterLocomotion.SetPlatform(null);
            }
        }

        /// <summary>
        ///     Can the ability be started?
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) return false;

            return IsOverValidObject();
        }

        /// <summary>
        ///     Is the character over a valid ground object?
        /// </summary>
        /// <returns>True if the character is over a valid ground object.</returns>
        protected bool IsOverValidObject()
        {
            if (!m_CharacterLocomotion.Grounded) return false;

            if (IsOverValidObject(m_CharacterLocomotion.GroundRaycastHit))
            {
                // The ground object is valid.
                m_GroundCollider = m_CharacterLocomotion.GroundRaycastHit.collider;
                m_GroundTransform = m_GroundCollider.transform;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Is the raycast hit over a valid ground object?
        /// </summary>
        /// <param name="raycastHit">The raycast hit to check against.</param>
        /// <returns>True if the raycast hit is over a valid ground object.</returns>
        protected bool IsOverValidObject(RaycastHit raycastHit)
        {
            // The transform may have been destroyed during a scene load.
            if (raycastHit.transform == null) return false;

            // The ability may require the character to be directly on top of the ground.
            if (Vector3.Dot(m_CharacterLocomotion.Up, raycastHit.normal) < m_GroundNormalSensitivity) return false;

            var angle = Quaternion.Angle(Quaternion.LookRotation(m_Transform.forward, m_CharacterLocomotion.Up),
                Quaternion.LookRotation(raycastHit.transform.forward, m_CharacterLocomotion.Up));
            var objectFaces = raycastHit.transform.gameObject.GetCachedParentComponent<ObjectForwardFaces>();
            // If an object has multiple faces then the ability can start from multiple directions.
            if (objectFaces != null)
            {
                var roundedAngle = 180 / objectFaces.ForwardFaceCount;
                angle = Mathf.Abs(
                    MathUtility.ClampInnerAngle(angle - roundedAngle * Mathf.RoundToInt(angle / roundedAngle)));
            }

            if (angle > m_AngleThreshold) return false;

            // Determine if the ground object is a valid ground object. This check only needs to be run when the grounded object changes.
            if (m_GroundCollider != raycastHit.collider)
            {
                // The ground object can be detected by using the ObjectIdentifier component.
                if (m_ObjectID != -1)
                {
                    var objIdentifiers = raycastHit.collider.gameObject.GetCachedComponents<ObjectIdentifier>();
                    if (objIdentifiers == null || objIdentifiers.Length == 0) return false;

                    var idMatch = false;
                    for (var i = 0; i < objIdentifiers.Length; ++i)
                        if (objIdentifiers[i].ID == m_ObjectID)
                        {
                            idMatch = true;
                            break;
                        }

                    if (!idMatch) return false;
                }

                // The ground object can be detected by using the layer mask.
                if (!MathUtility.InLayerMask(raycastHit.collider.gameObject.layer, m_LayerMask)) return false;
            }

            return true;
        }

        /// <summary>
        ///     The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // The character can move with the ground.
            if (m_MoveWithObject && m_CharacterLocomotion.Platform == null)
                m_CharacterLocomotion.SetPlatform(m_GroundTransform);
        }


        /// <summary>
        ///     Stops the ability if the character is no longer over a valid object.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (!IsOverValidObject()) StopAbility();
        }

        /// <summary>
        ///     The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            // The character is no longer moving with the object.
            if (m_MoveWithObject && m_CharacterLocomotion.Platform == m_GroundTransform)
                m_CharacterLocomotion.SetPlatform(null);

            m_GroundCollider = null;
            m_GroundTransform = null;
        }
    }
}