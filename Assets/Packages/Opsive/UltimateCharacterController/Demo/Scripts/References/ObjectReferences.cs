﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.References
{
    /// <summary>
    ///     Helper class which references the objects.
    /// </summary>
    public class ObjectReferences : MonoBehaviour
    {
        [Tooltip("A reference to the first person objects.")] [SerializeField]
        protected Object[] m_FirstPersonObjects;

        [Tooltip("A reference to the third person objects.")] [SerializeField]
        protected Object[] m_ThirdPersonObjects;

        [Tooltip("A reference to the shooter objects.")] [SerializeField]
        protected Object[] m_ShooterObjects;

        [Tooltip("A reference to the melee objects.")] [SerializeField]
        protected Object[] m_MeleeObjects;

        [Tooltip("Any object that should always be removed.")] [SerializeField]
        protected Object[] m_RemoveObjects;

        [Tooltip("Objects that should use the shadow caster while in a first person only perspective.")]
        [SerializeField]
        protected GameObject[] m_ShadowCasterObjects;

        [Tooltip("A reference to other Object References that should be checked.")] [SerializeField]
        protected ObjectReferences[] m_NestedReferences;

        [Tooltip("A reference to the first person door objects.")] [SerializeField]
        protected GameObject[] m_FirstPersonDoors;

        [Tooltip("A reference to the third person door objects.")] [SerializeField]
        protected GameObject[] m_ThirdPersonDoors;

        public Object[] FirstPersonObjects
        {
            get => m_FirstPersonObjects;
            set => m_FirstPersonObjects = value;
        }

        public Object[] ThirdPersonObjects
        {
            get => m_ThirdPersonObjects;
            set => m_ThirdPersonObjects = value;
        }

        public Object[] ShooterObjects
        {
            get => m_ShooterObjects;
            set => m_ShooterObjects = value;
        }

        public Object[] MeleeObjects
        {
            get => m_MeleeObjects;
            set => m_MeleeObjects = value;
        }

        public Object[] RemoveObjects
        {
            get => m_RemoveObjects;
            set => m_RemoveObjects = value;
        }

        public GameObject[] ShadowCasterObjects
        {
            get => m_ShadowCasterObjects;
            set => m_ShadowCasterObjects = value;
        }

        public ObjectReferences[] NestedReferences
        {
            get => m_NestedReferences;
            set => m_NestedReferences = value;
        }

        public GameObject[] FirstPersonDoors
        {
            get => m_FirstPersonDoors;
            set => m_FirstPersonDoors = value;
        }

        public GameObject[] ThirdPersonDoors
        {
            get => m_ThirdPersonDoors;
            set => m_ThirdPersonDoors = value;
        }
    }
}