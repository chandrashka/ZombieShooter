﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    /// <summary>
    ///     The ShieldBubble will play an enlarging animation when the object spawns.
    /// </summary>
    public class ShieldBubble : MonoBehaviour
    {
        private Animator m_Animator;
        private int m_DefaultStateHash;

        private Vector3 m_Scale;
        private Transform m_Transform;

        /// <summary>
        ///     Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = GetComponent<Transform>();
            m_Animator = GetComponent<Animator>();

            m_Scale = m_Transform.localScale;
            m_DefaultStateHash = Animator.StringToHash("EnlargingBubble");
        }

        /// <summary>
        ///     Reset the changed values.
        /// </summary>
        private void OnEnable()
        {
            m_Animator.Play(m_DefaultStateHash, 0, 0);
            m_Transform.localScale = m_Scale;
        }
    }
}