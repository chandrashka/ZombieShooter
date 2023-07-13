/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    /// <summary>
    ///     A particle stream crate will reset its position when the crate reactivates.
    /// </summary>
    public class ParticleStreamCrate : MonoBehaviour
    {
        private Vector3 m_Position;
        private Rigidbody m_Rigibody;
        private Quaternion m_Rotation;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Rigibody = GetComponent<Rigidbody>();

            var crateTransform = transform;
            m_Position = crateTransform.position;
            m_Rotation = crateTransform.rotation;
        }

        /// <summary>
        ///     The crate has been enabled.
        /// </summary>
        private void OnEnable()
        {
            m_Rigibody.velocity = Vector3.zero;
            m_Rigibody.angularVelocity = Vector3.zero;

            transform.SetPositionAndRotation(m_Position, m_Rotation);
        }
    }
}