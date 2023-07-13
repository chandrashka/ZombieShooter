/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
using UnityEngine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    /// <summary>
    ///     Describes any third person perspective dependent properties for the ThrowableItem.
    /// </summary>
    public class ThirdPersonThrowableItemProperties : ThirdPersonWeaponProperties, IThrowableItemPerspectiveProperties
    {
        [Tooltip("The location to throw the object from.")] [SerializeField]
        protected Transform m_ThrowLocation;

        [Tooltip("The location of the trajectory curve.")] [SerializeField]
        protected Transform m_TrajectoryLocation;

        [NonSerialized]
        public Transform ThrowLocation
        {
            get => m_ThrowLocation;
            set => m_ThrowLocation = value;
        }

        [NonSerialized]
        public Transform TrajectoryLocation
        {
            get => m_TrajectoryLocation;
            set => m_TrajectoryLocation = value;
        }
    }
}