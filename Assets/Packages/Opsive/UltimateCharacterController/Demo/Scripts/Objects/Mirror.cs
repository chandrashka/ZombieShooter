﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.Objects
{
#if FIRST_PERSON_CONTROLLER
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.FirstPersonController.Camera;
#endif
    /// <summary>
    ///     Uses the MaterialSwapper to toggle between first and third person perspective materials. The Mirror component
    ///     should be attached to the camera used as the mirror.
    /// </summary>
    public class Mirror : MonoBehaviour
    {
#if FIRST_PERSON_CONTROLLER
        [Tooltip("An array of cameras that contain the MaterialSwapper component. The MaterialSwapper should be set to ManualSwap.")]
        [SerializeField] protected CameraController[] m_Cameras;

        private MaterialSwapper[] m_MaterialSwappers;

        /// <summary>
        /// Initailizes the default values.
        /// </summary>
        private void Awake()
        {
            var materialSwapperList = new System.Collections.Generic.List<MaterialSwapper>();
            for (int i = 0; i < m_Cameras.Length; ++i) {
                var materialSwappers = m_Cameras[i].GetComponentsInChildren<MaterialSwapper>(true);
                if (materialSwappers == null || materialSwappers.Length == 0) {
                    continue;
                }
                for (int j = 0; j < materialSwappers.Length; ++j) {
                    materialSwapperList.Add(materialSwappers[j]);
                }
            }
            m_MaterialSwappers = materialSwapperList.ToArray();
        }

        /// <summary>
        /// The mirror camera is starting to render. Enable the third person materials.
        /// </summary>
        private void OnPreRender()
        {
            for (int i = 0; i < m_MaterialSwappers.Length; ++i) {
                m_MaterialSwappers[i].EnableThirdPersonMaterials();
            }
        }

        /// <summary>
        /// The mirror camera has rendered. Enable the first person materials again.
        /// </summary>
        private void OnPostRender()
        {
            for (int i = 0; i < m_MaterialSwappers.Length; ++i) {
                m_MaterialSwappers[i].EnableFirstPersonMaterials();
            }
        }
#endif
    }
}