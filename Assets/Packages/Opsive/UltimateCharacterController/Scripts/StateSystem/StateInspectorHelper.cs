﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    ///     Inspector helper class for the UltimateCharacterControllerInspector to be able to display states within the
    ///     ReorderableList.
    /// </summary>
    public class StateInspectorHelper : MonoBehaviour
    {
        [SerializeField] private int[] m_StateIndexData;

        public int[] StateIndexData
        {
            get => m_StateIndexData;
            set => m_StateIndexData = value;
        }
    }
}
#endif