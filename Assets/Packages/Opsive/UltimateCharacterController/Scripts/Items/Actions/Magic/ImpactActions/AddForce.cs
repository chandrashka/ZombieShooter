/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    /// <summary>
    ///     Adds a force to the impacted object.
    /// </summary>
    public class AddForce : ImpactAction
    {
        [Tooltip("Should the force be applied at the impact position?")] [SerializeField]
        protected bool m_AddForceAtPosition;

        [Tooltip("The amount of force that should be added to the impact object.")] [SerializeField]
        protected Vector3 m_Amount;

        [Tooltip("Specifies how to apply the force.")] [SerializeField]
        protected ForceMode m_Mode;

        public Vector3 Amount
        {
            get => m_Amount;
            set => m_Amount = value;
        }

        public ForceMode Mode
        {
            get => m_Mode;
            set => m_Mode = value;
        }

        public bool AddForceAtPosition
        {
            get => m_AddForceAtPosition;
            set => m_AddForceAtPosition = value;
        }

        /// <summary>
        ///     Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected override void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            var rigidbody = target.GetComponent<Rigidbody>();
            if (rigidbody == null) return;

            var amount = source.transform.TransformDirection(m_Amount);
            if (m_AddForceAtPosition)
                rigidbody.AddForceAtPosition(amount, hit.point, m_Mode);
            else
                rigidbody.AddForce(amount, m_Mode);
        }
    }
}