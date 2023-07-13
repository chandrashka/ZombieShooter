/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    ///     Acts as a LookSource for any character that does not have a camera (or network) attached.
    /// </summary>
    public class LocalLookSource : MonoBehaviour, ILookSource
    {
        [Tooltip("The distance that the character should look ahead.")] [SerializeField]
        protected float m_LookDirectionDistance = 100;

        [Tooltip("The location of the look source. The character's head is a good value.")] [SerializeField]
        protected Transform m_LookTransform;

        [Tooltip("The object that the character should look at.")] [SerializeField]
        protected Transform m_Target;

        private bool m_Started;

        public Transform Target
        {
            get => m_Target;
            set => m_Target = value;
        }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            GameObject = gameObject;
            Transform = transform;

            if (m_LookTransform == null)
            {
                var animator = GetComponent<Animator>();
                if (animator != null) m_LookTransform = animator.GetBoneTransform(HumanBodyBones.Head);

                if (m_LookTransform == null) m_LookTransform = Transform;
            }
        }

        /// <summary>
        ///     The component has been started.
        /// </summary>
        private void Start()
        {
            m_Started = true;
            EventHandler.ExecuteEvent<ILookSource>(GameObject, "OnCharacterAttachLookSource", this);
        }

        /// <summary>
        ///     The component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // Only attach the look source again if the component has already been started. By attaching the look source within enable it allows for the character to switch
            // look sources between a player-controlled character and an AI character.
            if (m_Started) EventHandler.ExecuteEvent<ILookSource>(GameObject, "OnCharacterAttachLookSource", this);
        }

        /// <summary>
        ///     The component has been disabled.
        /// </summary>
        private void OnDisable()
        {
            EventHandler.ExecuteEvent<ILookSource>(GameObject, "OnCharacterAttachLookSource", null);
        }

        public GameObject GameObject { get; private set; }

        public Transform Transform { get; private set; }

        public float LookDirectionDistance => m_LookDirectionDistance;
        public float Pitch => 0;

        /// <summary>
        ///     Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public Vector3 LookDirection(bool characterLookDirection)
        {
            if (m_Target != null) return (m_Target.position - m_LookTransform.position).normalized;
            return Transform.forward;
        }

        /// <summary>
        ///     Returns the direction that the character is looking.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="includeRecoil">Should recoil be included in the look direction?</param>
        /// <param name="includeMovementSpread">Should the movement spread be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        public Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask,
            bool includeRecoil, bool includeMovementSpread)
        {
            if (m_Target != null) return (m_Target.position - lookPosition).normalized;
            return Transform.forward;
        }

        /// <summary>
        ///     Returns the position of the look source.
        /// </summary>
        /// <param name="characterLookPosition">Is the character look position being retrieved?</param>
        /// <returns>The position of the look source.</returns>
        public Vector3 LookPosition(bool characterLookPosition)
        {
            return m_LookTransform.position;
        }
    }
}