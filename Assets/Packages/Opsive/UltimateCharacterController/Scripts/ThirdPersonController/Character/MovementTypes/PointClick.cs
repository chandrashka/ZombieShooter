﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Game;
using Opsive.Shared.Input;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.Abilities.AI;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes
{
    /// <summary>
    ///     The PointClick MovementType will work with the MoveTowards ability to move the character to the clicked location.
    /// </summary>
    public class PointClick : MovementType
    {
        private UnityEngine.Camera m_Camera;

        [Tooltip("The name of the button which does the click.")] [SerializeField]
        protected string m_ClickButtonName = "Fire1";

        private CharacterLayerManager m_LayerManager;

        [Tooltip("The minimum distance required in order to move.")] [SerializeField]
        protected float m_MinPointClickDistance = 1f;

        private float m_MinPointClickDistanceSqr;

        [Tooltip(
            "The speed of the character as they move towards the target. The SpeedChange ability must be added to the character.")]
        [SerializeField]
        protected MinMaxFloat m_MoveSpeed = new(1f, 2f);

        private PathfindingMovement m_PathfindingMovement;

        private PlayerInput m_PlayerInput;

        [Tooltip(
            "The character will run towards the destination when the squared distance is greater than the specified value.")]
        [SerializeField]
        protected float m_RunMaxSquaredDistance = 140;

        private SpeedChange m_SpeedChange;

        public string ClickButtonName
        {
            get => m_ClickButtonName;
            set => m_ClickButtonName = value;
        }

        public float MinPointClickDistance
        {
            get => m_MinPointClickDistance;
            set
            {
                m_MinPointClickDistance = value;
                m_MinPointClickDistanceSqr = m_MinPointClickDistance * m_MinPointClickDistance;
            }
        }

        public MinMaxFloat MoveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }

        public float RunMaxSquaredDistance
        {
            get => m_RunMaxSquaredDistance;
            set => m_RunMaxSquaredDistance = value;
        }

        public override bool FirstPersonPerspective => false;

        /// <summary>
        ///     Initializes the MovementType.
        /// </summary>
        /// <param name="characterLocomotion">The reference to the character locomotion component.</param>
        public override void Initialize(UltimateCharacterLocomotion characterLocomotion)
        {
            base.Initialize(characterLocomotion);

            m_PlayerInput = characterLocomotion.gameObject.GetCachedComponent<PlayerInput>();
            m_LayerManager = characterLocomotion.gameObject.GetCachedComponent<CharacterLayerManager>();
            m_PathfindingMovement = characterLocomotion.GetAbility<PathfindingMovement>();
            if (m_PathfindingMovement == null)
            {
                Debug.LogError("Error: The Point Click Movement Type requires a PathfindingMovement ability.");
                return;
            }

            if (m_CharacterLocomotion.MoveTowardsAbility == null)
            {
                Debug.LogError("Error: The Point Click Movement Type requires the MoveTowards ability.");
                return;
            }

            m_SpeedChange = characterLocomotion.GetAbility<SpeedChange>();
            m_MinPointClickDistanceSqr = m_MinPointClickDistance * m_MinPointClickDistance;
        }

        /// <summary>
        ///     A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        protected override void OnAttachLookSource(ILookSource lookSource)
        {
            base.OnAttachLookSource(lookSource);

            if (lookSource != null)
                m_Camera = lookSource.GameObject.GetCachedComponent<UnityEngine.Camera>();
            else
                m_Camera = null;
        }

        /// <summary>
        ///     Returns the delta yaw rotation of the character.
        /// </summary>
        /// <param name="characterHorizontalMovement">The character's horizontal movement.</param>
        /// <param name="characterForwardMovement">The character's forward movement.</param>
        /// <param name="cameraHorizontalMovement">The camera's horizontal movement.</param>
        /// <param name="cameraVerticalMovement">The camera's vertical movement.</param>
        /// <returns>The delta yaw rotation of the character.</returns>
        public override float GetDeltaYawRotation(float characterHorizontalMovement, float characterForwardMovement,
            float cameraHorizontalMovement, float cameraVerticalMovement)
        {
            return 0;
        }

        /// <summary>
        ///     Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public override Vector2 GetInputVector(Vector2 inputVector)
        {
            // PointClick does not have any direct input, but it will tell the Move Towards ability to move towards the clicked location.
            if (m_PlayerInput.GetButton(m_ClickButtonName))
            {
                var ray = m_Camera.ScreenPointToRay(m_PlayerInput.GetMousePosition());
                if (Physics.Raycast(ray, out var hit, float.MaxValue, m_LayerManager.SolidObjectLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    // Do not allow movement if the location is close to the character.
                    var sqrDistance = Vector3.SqrMagnitude(m_Transform.position - hit.point);
                    if (sqrDistance >= m_MinPointClickDistanceSqr)
                        m_CharacterLocomotion.MoveTowardsAbility.MoveTowardsLocation(hit.point);
                }
            }

            // If a SpeedChange ability exists then the character can move at a faster speed further away from the target.
            if (m_CharacterLocomotion.MoveTowardsAbility.IsActive && m_SpeedChange != null &&
                m_MoveSpeed.MinValue != m_MoveSpeed.MaxValue)
            {
                var distance = (m_PathfindingMovement.GetDestination() - m_Transform.position).sqrMagnitude;
                m_SpeedChange.MaxSpeedChangeValue = m_SpeedChange.SpeedChangeMultiplier =
                    Mathf.Lerp(m_MoveSpeed.MinValue, m_MoveSpeed.MaxValue,
                        Mathf.Clamp01(distance / m_RunMaxSquaredDistance));
                m_SpeedChange.MinSpeedChangeValue = -m_SpeedChange.MaxSpeedChangeValue;
            }

            return Vector2.zero;
        }
    }
}