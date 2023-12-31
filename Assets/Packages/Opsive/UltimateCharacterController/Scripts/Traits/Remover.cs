﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Game;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    ///     Places the object back in the ObjectPool after the specified number of seconds.
    /// </summary>
    public class Remover : MonoBehaviour
    {
        [Tooltip("The number of seconds until the object should be placed back in the pool.")] [SerializeField]
        protected float m_Lifetime = 5;

        private GameObject m_GameObject;
        private ScheduledEventBase m_RemoveEvent;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
        }

        /// <summary>
        ///     Schedule the object for removal.
        /// </summary>
        private void OnEnable()
        {
            m_RemoveEvent = SchedulerBase.Schedule(m_Lifetime, Remove);
        }

        /// <summary>
        ///     The object has been destroyed - no need for removal if it hasn't already been removed.
        /// </summary>
        private void OnDisable()
        {
            CancelRemoveEvent();
        }

        /// <summary>
        ///     Cancels the remove event.
        /// </summary>
        public void CancelRemoveEvent()
        {
            if (m_RemoveEvent != null)
            {
                SchedulerBase.Cancel(m_RemoveEvent);
                m_RemoveEvent = null;
            }
        }

        /// <summary>
        ///     Remove the object.
        /// </summary>
        private void Remove()
        {
            ObjectPoolBase.Destroy(m_GameObject);
            m_RemoveEvent = null;
        }
    }
}