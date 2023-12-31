﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Character;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Camera
{
    /// <summary>
    ///     The AimAssist component allows for the camera and character to automatically to face the specified target.
    /// </summary>
    public class AimAssist : StateBehavior
    {
        [Tooltip("Should the component assist with the aiming?")] [SerializeField]
        protected bool m_AssistAim = true;

        [Tooltip("Does the Aim ability need to be active in order for the component to assist with aiming?")]
        [SerializeField]
        protected bool m_RequireActiveAim;

        [Tooltip("The maximum distance that the target can be away from the character in order to influence the aim.")]
        [SerializeField]
        protected float m_MaxDistance = 100;

        [Tooltip(
            "The amount of influence the aim assist has on the camera rotation. The x value represents the angle delta between the current camera rotation and the camera rotation. " +
            "The y value represents the amount of influence at that angle. A y value of 1 indicates complete influence while a value of 0 indicates no influence.")]
        [SerializeField]
        protected AnimationCurve m_Influence = AnimationCurve.EaseInOut(0, 1f, 5, 0.9f);

        [Tooltip("Specifies an offset to apply to the target.")] [SerializeField]
        protected Vector3 m_TargetOffset;

        [Tooltip("If the target is a humanoid should a bone from the humanoid be targeted?")] [SerializeField]
        protected bool m_TargetHumanoidBone;

        [Tooltip("Specifies which bone to target if targeting a humanoid bone.")] [SerializeField]
        protected HumanBodyBones m_HumanoidBoneTarget = HumanBodyBones.Chest;

        [Tooltip("The magnitude required in order to break the current target lock.")] [SerializeField]
        protected float m_BreakForce = 2;

        [Tooltip("If trying to switch targets, specifies the radius that the nearby targets should be in.")]
        [SerializeField]
        protected float m_SwitchTargetRadius = 5;

        [Tooltip("If switching targets, specifies the speed at which the camera rotates to face the new target.")]
        [SerializeField]
        protected float m_SwitchTargetRotationSpeed = 10;

        [Tooltip("The maximum number of colliders that should be considered within the target switch.")]
        [SerializeField]
        protected int m_MaxSwitchTargetColliders = 20;

        private bool m_Aiming;
        private GameObject m_Character;
        private CharacterLayerManager m_CharacterLayerManager;
        private Transform m_CharacterTransform;
        private Collider[] m_Colliders;
        private float m_MaxDistanceSquared;
        private bool m_SwitchingTargets;
        private AimAssistOffset m_TargetAimAssistOffset;

        private Transform m_Transform;

        public bool AssistAim
        {
            get => m_AssistAim;
            set
            {
                m_AssistAim = value;
                if (!m_AssistAim && Target != null) Target = null;
            }
        }

        public bool RequireActiveAim
        {
            get => m_RequireActiveAim;
            set => m_RequireActiveAim = value;
        }

        public float MaxDistance
        {
            get => m_MaxDistance;
            set
            {
                m_MaxDistance = value;
                if (Application.isPlaying) m_MaxDistanceSquared = m_MaxDistance * m_MaxDistance;
            }
        }

        public AnimationCurve Influence
        {
            get => m_Influence;
            set => m_Influence = value;
        }

        public Vector3 TargetOffset
        {
            get => m_TargetOffset;
            set => m_TargetOffset = value;
        }

        public HumanBodyBones HumanoidBoneTarget
        {
            get => m_HumanoidBoneTarget;
            set => m_HumanoidBoneTarget = value;
        }

        public float BreakForce
        {
            get => m_BreakForce;
            set => m_BreakForce = value;
        }

        public float SwitchTargetRadius
        {
            get => m_SwitchTargetRadius;
            set => m_SwitchTargetRadius = value;
        }

        public float SwitchTargetRotationSpeed
        {
            get => m_SwitchTargetRotationSpeed;
            set => m_SwitchTargetRotationSpeed = value;
        }

        public Transform Target { get; private set; }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_Transform = transform;
            m_Colliders = new Collider[m_MaxSwitchTargetColliders];
            m_MaxDistanceSquared = m_MaxDistance * m_MaxDistance;

            EventHandler.RegisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }

        /// <summary>
        ///     The camera has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }

        /// <summary>
        ///     Attaches the component to the specified character.
        /// </summary>
        /// <param name="character">The handler to attach the camera to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null)
            {
                EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
                m_Character = null;
                m_CharacterTransform = null;
                m_CharacterLayerManager = null;
            }

            if (character != null)
            {
                m_Character = character;
                m_CharacterTransform = m_Character.transform;
                m_CharacterLayerManager = character.GetCachedComponent<CharacterLayerManager>();
                EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
            }
        }

        /// <summary>
        ///     The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) return;
            m_Aiming = aim;

            if (!m_Aiming && m_RequireActiveAim) Target = null;
        }

        /// <summary>
        ///     Tries to set the target to the specified value.
        /// </summary>
        /// <param name="target">The value that the target should be set to.</param>
        public void SetTarget(Transform target)
        {
            if (target != null)
            {
                var distance = (target.position - m_CharacterTransform.position).sqrMagnitude;
                if (distance > m_MaxDistanceSquared) target = null;
            }

            if (!m_AssistAim || Target == target || m_SwitchingTargets || (m_RequireActiveAim && !m_Aiming)) return;

            Target = target;
            if (Target != null)
            {
                m_TargetAimAssistOffset = Target.gameObject.GetCachedComponent<AimAssistOffset>();

                // If the target is a humanoid then a specific bone can be targeted.
                if (m_TargetHumanoidBone)
                {
                    var animator = Target.gameObject.GetCachedComponent<Animator>();
                    if (animator != null && animator.isHuman) Target = animator.GetBoneTransform(m_HumanoidBoneTarget);
                }
            }
        }

        /// <summary>
        ///     The target can be reset if the specified force is greater than the break force.
        /// </summary>
        /// <param name="force">The amount of force applied.</param>
        public void UpdateBreakForce(float force)
        {
            if (force > m_BreakForce)
            {
                Target = null;
                m_SwitchingTargets = false;
            }
        }

        /// <summary>
        ///     Does the auto aimer have a target?
        /// </summary>
        /// <returns>True if the auto aimer has a target.</returns>
        public bool HasTarget()
        {
            return Target != null;
        }

        /// <summary>
        ///     Tries to switch to the next target. The target may not be able to be switched if there is only one collider
        ///     overlapping in the specified radius.
        /// </summary>
        /// <param name="rightTarget">Specifies if the next target should be to the right relative to the camera transform.</param>
        public void TrySwitchTargets(bool rightTarget)
        {
            // The targets can't be switched if there isn't a target to begin with.
            if (Target == null || !m_AssistAim) return;

            // Determine which collider should be switched to next based upon the radius of the current target's transform.
            var overlapCount = Physics.OverlapSphereNonAlloc(Target.position, m_SwitchTargetRadius, m_Colliders,
                m_CharacterLayerManager.EnemyLayers, QueryTriggerInteraction.Ignore);
            if (overlapCount > 1)
            {
                var nextTarget = DetermineNextTarget(rightTarget, true, overlapCount);

                // If no target was found then there is no overlapping colliders in the direction specified by rightTarget. The furtherst target in the opposite
                // direction of rightTarget should then be found. This will allow the targets to be cycled through linearly. 
                if (nextTarget == null) nextTarget = DetermineNextTarget(!rightTarget, false, overlapCount);

                if (nextTarget != null)
                {
                    // Allow the target to be switched multiple times while an existing switch is taking place.
                    m_SwitchingTargets = false;
                    SetTarget(nextTarget);
                    m_SwitchingTargets = true;
                }
            }
        }

        /// <summary>
        ///     Returns the next valid target within the colliders array.
        /// </summary>
        /// <param name="rightTarget">Specifies if the next target should be to the right relative to the camera transform.</param>
        /// <param name="closestTarget">
        ///     Should the closest target be found? If false the furthest away target will be found with
        ///     the specified direction.
        /// </param>
        /// <param name="overlapCount">The number of colliders that are overlapping in the colliders array.</param>
        /// <returns>The next valid  target within the colliders array.</returns>
        private Transform DetermineNextTarget(bool rightTarget, bool closestTarget, int overlapCount)
        {
            var interestedOffset = Vector3.zero;
            var interestedDistance = closestTarget ? float.MaxValue : 0;
            Transform nextTarget = null;
            var relativeTargetPosition = m_Transform.InverseTransformPoint(Target.position);

            for (var i = 0; i < overlapCount; ++i)
            {
                var overlapTransform = m_Colliders[i].transform;

                // The target can't switch to itself.
                if (overlapTransform.IsChildOf(Target)) continue;

                var distance = (Target.position - overlapTransform.position).sqrMagnitude;
                // If the closest target is being found then the distance needs to be less than the previously-least distance amount.
                // If the closest target is not being found then the furtherst target will be used and in that case the greatest distance will be found.
                if ((closestTarget && distance < interestedDistance) ||
                    (!closestTarget && distance > interestedDistance))
                {
                    // Use the relative direction so "right" and "left" is relative to the camera rather than to the world space position.
                    var relativePosition = m_Transform.InverseTransformPoint(overlapTransform.position);
                    var offset = relativePosition - relativeTargetPosition;
                    // If the closest target is being found then the offset should be the least value in the specified direction.
                    // If the closest target is not being found then the offset should be the greatest value in the specified direction.
                    if ((closestTarget && ((rightTarget && offset.x > 0 &&
                                            (nextTarget == null || offset.x < interestedOffset.x)) ||
                                           (!rightTarget && offset.x < 0 &&
                                            (nextTarget == null || offset.x > interestedOffset.x)))) ||
                        (!closestTarget && ((rightTarget && offset.x > 0 &&
                                             (nextTarget == null || offset.x > interestedOffset.x)) ||
                                            (!rightTarget && offset.x < 0 &&
                                             (nextTarget == null || offset.x < interestedOffset.x)))))
                    {
                        // The transform is at an extreme - save the values so they can be compared against for the next iteration.
                        interestedOffset = offset;
                        interestedDistance = distance;
                        nextTarget = overlapTransform;
                    }
                }
            }

            return nextTarget;
        }

        /// <summary>
        ///     Returns the rotation that the camera should use to face the target.
        /// </summary>
        /// <param name="cameraRotation">The target rotation of the camera.</param>
        /// <returns>The rotation that the camera should use to face the target.</returns>
        public Quaternion TargetRotation(Quaternion cameraRotation)
        {
            var direction =
                Target.TransformPoint(m_TargetOffset +
                                      (m_TargetAimAssistOffset != null
                                          ? m_TargetAimAssistOffset.Offset
                                          : Vector3.zero)) - m_Transform.position;
            var targetRotation = Quaternion.LookRotation(direction, cameraRotation * Vector3.up);
            var angle = Quaternion.Angle(cameraRotation, targetRotation);
            // If switching targets then don't evaluate the rotation amount based on a curve. The camera should always look at the target.
            if (m_SwitchingTargets)
            {
                if (angle < 0.1f) m_SwitchingTargets = false;
                return Quaternion.Slerp(cameraRotation, targetRotation,
                    m_SwitchTargetRotationSpeed * Time.fixedDeltaTime);
            }

            // The returned target rotation is based on the influence value determined by the angle between the current camera rotation and the target rotation.
            // This curve prevents the camera from always sticking to the target even if the target moved away quickly.
            return Quaternion.Slerp(cameraRotation, targetRotation, m_Influence.Evaluate(angle));
        }
    }
}