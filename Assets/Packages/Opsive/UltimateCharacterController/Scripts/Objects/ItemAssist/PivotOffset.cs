/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    /// <summary>
    ///     Specifies an offset for the pivot position.
    /// </summary>
    public class PivotOffset : MonoBehaviour
    {
        [Tooltip("The pivot offset.")] [SerializeField]
        protected Vector3 m_Offset;

        public Vector3 Offset
        {
            get => m_Offset;
            set => m_Offset = value;
        }
    }
}