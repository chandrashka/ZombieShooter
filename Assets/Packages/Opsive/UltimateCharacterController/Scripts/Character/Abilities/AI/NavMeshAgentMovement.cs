﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;
using UnityEngine.AI;

namespace Opsive.UltimateCharacterController.Character.Abilities.AI
{
    /// <summary>
    ///     Moves the character according to the NavMeshAgent desired velocity.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentMovement : PathfindingMovement
    {
        /// <summary>
        ///     Specifies if the rotation should be overridden.
        /// </summary>
        public enum RotationOverrideMode
        {
            NoOverride, // Does not override the rotation. Uses the NavMesh updateRotation property.
            NavMesh, // Forces the rotation according to the NavMesh path.
            Character // Forces the rotation according to the character's rotation.
        }

        [Tooltip(
            "The agent has arrived at the destination when the remaining distance is less than the arrived distance.")]
        [SerializeField]
        protected float m_ArrivedDistance = 0.2f;

        private Vector3 m_DeltaRotation;
        private Quaternion m_DestinationRotation = Quaternion.identity;
        private Fall m_FallAbility;
        private Vector2 m_InputVector;
        private Jump m_JumpAbility;

        [Tooltip("Should the jump ability be started on the manual offmesh link?")] [SerializeField]
        protected bool m_JumpAcrossManualOffMeshLink = true;

        private int m_ManualOffMeshLinkIndex;

        [Tooltip("The name of the manual offmesh link that the character can traverse across.")] [SerializeField]
        protected string m_ManualOffMeshLinkName = "Jump";

        private NavMeshAgent m_NavMeshAgent;

        private bool m_PrevEnabled = true;

        [Tooltip("Specifies if the rotation should be overridden.")] [SerializeField]
        protected RotationOverrideMode m_RotationOverride;

        private bool m_UpdateRotation;

        public RotationOverrideMode RotationOverride
        {
            get => m_RotationOverride;
            set => m_RotationOverride = value;
        }

        public float ArrivedDistance
        {
            get => m_ArrivedDistance;
            set => m_ArrivedDistance = value;
        }

        public string ManualOffMeshLinkName
        {
            get => m_ManualOffMeshLinkName;
            set => m_ManualOffMeshLinkName = value;
        }

        public bool JumpAcrossManualOffMeshLink
        {
            get => m_JumpAcrossManualOffMeshLink;
            set => m_JumpAcrossManualOffMeshLink = value;
        }

        public override Vector2 InputVector => m_InputVector;
        public override Vector3 DeltaRotation => m_DeltaRotation;
        public override bool HasArrived => m_NavMeshAgent.remainingDistance <= m_ArrivedDistance;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_NavMeshAgent.autoTraverseOffMeshLink = false;
            m_NavMeshAgent.updatePosition = false;
            m_ManualOffMeshLinkIndex = NavMesh.GetAreaFromName(m_ManualOffMeshLinkName);

            m_JumpAbility = m_CharacterLocomotion.GetAbility<Jump>();
            m_FallAbility = m_CharacterLocomotion.GetAbility<Fall>();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);

            if (!Enabled) m_NavMeshAgent.enabled = false;
        }

        /// <summary>
        ///     Sets the destination of the pathfinding agent.
        /// </summary>
        /// <param name="target">The position to move towards.</param>
        /// <returns>True if the destination was set.</returns>
        public override bool SetDestination(Vector3 target)
        {
            m_DestinationRotation = Quaternion.identity;

            // Set the new destination if the ability is already active.
            if (m_NavMeshAgent.hasPath && IsActive) return m_NavMeshAgent.SetDestination(target);

            // The NavMeshAgent must be enabled in order to set the destination.
            m_PrevEnabled = Enabled;
            Enabled = true;
            // Move towards the destination.
            if (m_NavMeshAgent.isOnNavMesh && m_NavMeshAgent.SetDestination(target))
            {
                StartAbility();
                return true;
            }

            Enabled = m_PrevEnabled;
            return false;
        }

        /// <summary>
        ///     Returns the destination of the pathfinding agent.
        /// </summary>
        /// <returns>The destination of the pathfinding agent.</returns>
        public override Vector3 GetDestination()
        {
            return m_NavMeshAgent.destination;
        }

        /// <summary>
        ///     Sets the rotation of the agent after they have arrived at the destination.
        /// </summary>
        /// <param name="rotation">The destination rotation.</param>
        public override void SetDestinationRotation(Quaternion rotation)
        {
            m_DestinationRotation = rotation;
        }

        /// <summary>
        ///     Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility()) return false;

            return m_NavMeshAgent.isOnNavMesh;
        }

        /// <summary>
        ///     Updates the ability.
        /// </summary>
        public override void Update()
        {
            m_InputVector = Vector2.zero;
            var lookRotation = Quaternion.LookRotation(m_Transform.forward, m_CharacterLocomotion.Up);
            var updateInput = true;
            if (m_NavMeshAgent.isOnOffMeshLink && UpdateOffMeshLink()) updateInput = false;

            if (updateInput && m_NavMeshAgent.hasPath && !m_NavMeshAgent.isStopped)
            {
                var direction = m_NavMeshAgent.pathPending || m_NavMeshAgent.desiredVelocity.sqrMagnitude < 0.01f
                    ? m_NavMeshAgent.velocity
                    : m_NavMeshAgent.desiredVelocity;
                // Only move if a path exists.
                if (m_NavMeshAgent.remainingDistance > 0.01f)
                {
                    // A path can exist but the velocity returns 0 (??). Move in the direction of the destination.
                    if (direction.sqrMagnitude == 0) direction = m_NavMeshAgent.destination - m_Transform.position;
                    Vector3 velocity;
                    if ((m_NavMeshAgent.updateRotation && m_RotationOverride == RotationOverrideMode.NoOverride) ||
                        m_RotationOverride == RotationOverrideMode.NavMesh)
                    {
                        lookRotation = Quaternion.LookRotation(direction.normalized, m_CharacterLocomotion.Up);
                        // The normalized velocity should be relative to the target rotation.
                        velocity = Quaternion.Inverse(lookRotation) * direction.normalized;
                    }
                    else
                    {
                        velocity = m_Transform.InverseTransformDirection(direction);
                    }

                    // Only normalize if the magnitude is greater than 1. This will allow the character to walk.
                    if (velocity.sqrMagnitude > 1) velocity.Normalize();
                    m_InputVector.x = velocity.x;
                    m_InputVector.y = velocity.z;
                }
                else if (m_DestinationRotation != Quaternion.identity)
                {
                    lookRotation = m_DestinationRotation;
                }
            }

            var rotation = lookRotation * Quaternion.Inverse(m_Transform.rotation);
            m_DeltaRotation.y = MathUtility.ClampInnerAngle(rotation.eulerAngles.y);

            base.Update();
        }

        /// <summary>
        ///     Ensure the move direction is valid.
        /// </summary>
        public override void ApplyPosition()
        {
            if (m_NavMeshAgent.remainingDistance < m_NavMeshAgent.stoppingDistance)
            {
                // Prevent the character from jittering back and forth to land precisely on the target.
                var direction = m_Transform.InverseTransformPoint(m_NavMeshAgent.destination);
                var moveDirection = m_Transform.InverseTransformDirection(m_CharacterLocomotion.MoveDirection);
                if (Mathf.Abs(moveDirection.x) > Mathf.Abs(direction.x)) moveDirection.x = direction.x;
                if (Mathf.Abs(moveDirection.z) > Mathf.Abs(direction.z)) moveDirection.z = direction.z;
                m_CharacterLocomotion.MoveDirection = m_Transform.TransformDirection(moveDirection);
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            m_NavMeshAgent.nextPosition = m_Transform.position;
        }

        /// <summary>
        ///     Updates the velocity and look rotation using the off mesh link.
        /// </summary>
        /// <returns>True if the off mesh link was handled.</returns>
        protected virtual bool UpdateOffMeshLink()
        {
            if (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross ||
                (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeManual &&
                 m_NavMeshAgent.currentOffMeshLinkData.offMeshLink.area == m_ManualOffMeshLinkIndex))
            {
                // Ignore the y difference when determining a look direction and velocity.
                // This will give XZ distances a greater impact when normalized.
                var direction = m_NavMeshAgent.currentOffMeshLinkData.endPos - m_Transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.1f || m_CharacterLocomotion.Grounded)
                {
                    var nextPositionDirection =
                        m_Transform.InverseTransformPoint(m_NavMeshAgent.currentOffMeshLinkData.endPos);
                    nextPositionDirection.y = 0;
                    nextPositionDirection.Normalize();

                    m_InputVector.x = nextPositionDirection.x;
                    m_InputVector.y = nextPositionDirection.z;
                }

                // Jump if the agent hasn't jumped yet.
                if (m_JumpAbility != null &&
                    (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross ||
                     (m_JumpAcrossManualOffMeshLink && m_NavMeshAgent.currentOffMeshLinkData.linkType ==
                      OffMeshLinkType.LinkTypeManual &&
                      m_NavMeshAgent.currentOffMeshLinkData.offMeshLink.area == m_ManualOffMeshLinkIndex)))
                    if (!m_JumpAbility.IsActive && (m_FallAbility == null || !m_FallAbility.IsActive))
                        m_CharacterLocomotion.TryStartAbility(m_JumpAbility);
                return true;
            }

            if (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeDropDown &&
                m_CharacterLocomotion.Grounded)
            {
                m_NavMeshAgent.CompleteOffMeshLink();
            }

            return false;
        }

        /// <summary>
        ///     Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            if (!base.CanStopAbility()) return false;

            return m_NavMeshAgent.hasPath && !m_NavMeshAgent.pathPending &&
                   m_NavMeshAgent.remainingDistance <= m_ArrivedDistance;
        }

        /// <summary>
        ///     The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            if (!m_PrevEnabled) Enabled = false;
        }

        /// <summary>
        ///     The character has changed grounded state.
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        protected virtual void OnGrounded(bool grounded)
        {
            if (grounded && m_NavMeshAgent.enabled)
            {
                // The agent is no longer on an off mesh link if they just landed.
                if (m_NavMeshAgent.isOnOffMeshLink &&
                    (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeDropDown ||
                     m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross))
                    m_NavMeshAgent.CompleteOffMeshLink();
                // Warp the NavMeshAgent just in case the navmesh position doesn't match the transform position.
                var destination = m_NavMeshAgent.destination;
                m_NavMeshAgent.Warp(m_Transform.position);
                // Warp can change the destination so make sure that doesn't happen.
                if (m_NavMeshAgent.destination != destination) m_NavMeshAgent.SetDestination(destination);
            }
        }

        /// <summary>
        ///     The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_UpdateRotation = m_NavMeshAgent.updateRotation;
            m_NavMeshAgent.updateRotation = false;
        }

        /// <summary>
        ///     The character has respawned. Start moving again.
        /// </summary>
        private void OnRespawn()
        {
            // Reset the NavMeshAgent to the new position.
            m_NavMeshAgent.Warp(m_Transform.position);
            if (m_NavMeshAgent.isOnOffMeshLink) m_NavMeshAgent.ActivateCurrentOffMeshLink(false);
            m_NavMeshAgent.updateRotation = m_UpdateRotation;
        }

        /// <summary>
        ///     Called when the ability is enabled or disabled.
        /// </summary>
        /// <param name="enabled">Is the ability enabled?</param>
        protected override void SetEnabled(bool enabled)
        {
            if (m_NavMeshAgent != null) m_NavMeshAgent.enabled = enabled;
        }

        /// <summary>
        ///     The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}