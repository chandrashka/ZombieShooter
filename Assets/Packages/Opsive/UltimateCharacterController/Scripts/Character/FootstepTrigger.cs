/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    ///     Notifies the CharacterFootEffects component when the foot has collided with the ground.
    /// </summary>
    public class FootstepTrigger : MonoBehaviour
    {
        [Tooltip("Should the footprint texture be flipped?")] [SerializeField]
        protected bool m_FlipFootprint;

        private CharacterLayerManager m_CharacterLayerManager;
        private CharacterFootEffects m_FootEffects;

        private Transform m_Transform;

        public bool FlipFootprint
        {
            get => m_FlipFootprint;
            set => m_FlipFootprint = value;
        }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_FootEffects = GetComponentInParent<CharacterFootEffects>();
            m_CharacterLayerManager = GetComponentInParent<CharacterLayerManager>();
        }

        /// <summary>
        ///     The trigger has collided with another object.
        /// </summary>
        /// <param name="other">The Collider that the trigger collided with.</param>
        private void OnTriggerEnter(Collider other)
        {
            // Notify the CharacterFootEffects component if the layer is valid.
            if (MathUtility.InLayerMask(other.gameObject.layer,
                    m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers))
                m_FootEffects.TriggerFootStep(m_Transform, m_FlipFootprint);
        }
    }
}