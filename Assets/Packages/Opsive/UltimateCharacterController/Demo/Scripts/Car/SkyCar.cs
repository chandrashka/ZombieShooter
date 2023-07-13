/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using UnityEngine;
using UnityEngine.Serialization;

namespace Opsive.UltimateCharacterController.Demo.Car
{
    /// <summary>
    ///     Provides a sample implementation of the IDriveSource.
    /// </summary>
    public class SkyCar : MonoBehaviour, IDriveSource
    {
        private static readonly int s_OpenCloseDoorParameter = Animator.StringToHash("OpenCloseDoor");

        [Tooltip("A reference to the headlights that should turn on when the character enters the car.")]
        [SerializeField]
        protected GameObject[] m_Headlights;

        [Tooltip("A reference to the colliders that should be disabled when the character enters the car.")]
        [FormerlySerializedAs("m_Colliders")]
        [SerializeField]
        protected GameObject[] m_DisableColliders;

        [Tooltip("The location that the character drives from.")] [SerializeField]
        protected Transform m_DriverLocation;

        private Animator m_Animator;
        private CarAudio m_Audio;
        private AnimatorMonitor m_CharacterAnimatorMonitor;

        private int m_HorizontalInputID;
        private Collider[] m_IgnoreColliders;
        private bool m_OpenedDoor;
        private Rigidbody m_Rigidbody;
        private CarUserControl m_UserControl;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            GameObject = gameObject;
            Transform = transform;
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_IgnoreColliders = GameObject.GetComponentsInChildren<Collider>();
            m_UserControl = GetComponent<CarUserControl>();
            m_Audio = GetComponent<CarAudio>();
            m_HorizontalInputID = Animator.StringToHash("HorizontalInput");
            EnableDisableCar(false);
        }

        /// <summary>
        ///     Updates the animator.
        /// </summary>
        public void Update()
        {
            m_Animator.SetFloat(m_HorizontalInputID, m_CharacterAnimatorMonitor.AbilityFloatData, 0, 0);
        }

        public GameObject GameObject { get; private set; }

        public Transform Transform { get; private set; }

        public Transform DriverLocation => m_DriverLocation;
        public int AnimatorID => 0;

        /// <summary>
        ///     The character has started to enter the vehicle.
        /// </summary>
        /// <param name="character">The character that is entering the vehicle.</param>
        public void EnterVehicle(GameObject character)
        {
            EventHandler.RegisterEvent(character, "OnAnimatorOpenCloseDoor", OpenCloseDoor);

            var characterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
            for (var i = 0; i < m_IgnoreColliders.Length; ++i)
            for (var j = 0; j < characterLocomotion.ColliderCount; ++j)
                Physics.IgnoreCollision(m_IgnoreColliders[i], characterLocomotion.Colliders[j], true);
            characterLocomotion.AddIgnoredColliders(m_IgnoreColliders);
        }

        /// <summary>
        ///     The character has entered the vehicle.
        /// </summary>
        /// <param name="character">The character that entered the vehicle.</param>
        public void EnteredVehicle(GameObject character)
        {
            m_CharacterAnimatorMonitor = character.GetCachedComponent<AnimatorMonitor>();
            EnableDisableCar(true);
        }

        /// <summary>
        ///     The character has started to exit the vehicle.
        /// </summary>
        /// <param name="character">The character that is exiting the vehicle.</param>
        public void ExitVehicle(GameObject character)
        {
            EnableDisableCar(false);
        }

        /// <summary>
        ///     The character has exited the vehicle.
        /// </summary>
        /// <param name="character">The character that exited the vehicle.</param>
        public void ExitedVehicle(GameObject character)
        {
            EventHandler.UnregisterEvent(character, "OnAnimatorOpenCloseDoor", OpenCloseDoor);

            var characterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
            characterLocomotion.RemoveIgnoredColliders(m_IgnoreColliders);
            for (var i = 0; i < m_IgnoreColliders.Length; ++i)
            for (var j = 0; j < characterLocomotion.ColliderCount; ++j)
                Physics.IgnoreCollision(m_IgnoreColliders[i], characterLocomotion.Colliders[j], false);

            if (m_OpenedDoor) OpenCloseDoor();
        }

        /// <summary>
        ///     Enables or disables the car components.
        /// </summary>
        /// <param name="enable">Should the car be enabled?</param>
        private void EnableDisableCar(bool enable)
        {
            enabled = m_UserControl.enabled = m_Audio.enabled = enable;
            m_Rigidbody.isKinematic = !enable;
            for (var i = 0; i < m_Headlights.Length; ++i) m_Headlights[i].SetActive(enable);
            for (var i = 0; i < m_DisableColliders.Length; ++i) m_DisableColliders[i].SetActive(!enable);
        }

        /// <summary>
        ///     Triggers the OpenCloseDoor parameter.
        /// </summary>
        private void OpenCloseDoor()
        {
            m_OpenedDoor = !m_OpenedDoor;
            m_Animator.SetTrigger(s_OpenCloseDoorParameter);
        }
    }
}