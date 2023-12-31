﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Traits;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    /// <summary>
    ///     Instantly kills the character if the character moves beneath the moving platform as it is moving down.
    /// </summary>
    public class MovingPlatformDeathZone : MonoBehaviour
    {
        private bool m_DownwardMovement;
        private Vector3 m_PrevPosition;
        private Transform m_Transform;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_PrevPosition = m_Transform.position;
        }

        /// <summary>
        ///     Detect if the platform is moving downward.
        /// </summary>
        private void FixedUpdate()
        {
            var position = m_Transform.position;
            m_DownwardMovement = m_Transform.InverseTransformDirection(position - m_PrevPosition).y < 0;
            m_PrevPosition = position;
        }

        /// <summary>
        ///     An
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            // The platform has to be moving downward in order to kill the player.
            if (!m_DownwardMovement) return;

            // Kill the character.
            var health = other.GetComponentInParent<CharacterHealth>();
            if (health == null) return;

            var position = m_Transform.position;
            health.ImmediateDeath(position, Vector3.down, (position - m_PrevPosition).magnitude);
        }
    }
}