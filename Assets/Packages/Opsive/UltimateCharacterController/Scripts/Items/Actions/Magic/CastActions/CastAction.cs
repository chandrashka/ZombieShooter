﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;
using UnityEngine.Scripting;

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions
{
    /// <summary>
    ///     The Cast Action class performs the magic cast.
    /// </summary>
    [AllowDuplicateTypes]
    [Preserve]
    [Serializable]
    public abstract class CastAction : StateObject
    {
        [Tooltip("The delay to start the cast after the item has been used.")] [HideInInspector] [SerializeField]
        protected float m_Delay;

        protected uint m_CastID;

        protected GameObject m_GameObject;
        protected int m_Index;
        protected MagicItem m_MagicItem;

        public float Delay
        {
            get => m_Delay;
            set => m_Delay = value;
        }

        public uint CastID
        {
            get => m_CastID;
            set => m_CastID = value;
        }

        /// <summary>
        ///     Initializes the CastAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the CastAction belongs to.</param>
        /// <param name="index">The index of the CastAction.</param>
        public virtual void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character);

            m_GameObject = character;
            m_MagicItem = magicItem;
            m_Index = index;
        }

        /// <summary>
        ///     Awake is called after all of the actions have been initialized.
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        ///     Is the specified position a valid target position?
        /// </summary>
        /// <param name="position">The position that may be a valid target position.</param>
        /// <param name="normal">The normal of the position.</param>
        /// <returns>True if the specified position is a valid target position.</returns>
        public virtual bool IsValidTargetPosition(Vector3 position, Vector3 normal)
        {
            return true;
        }

        /// <summary>
        ///     Performs the cast.
        /// </summary>
        /// <param name="origin">The location that the cast should spawn from.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        public abstract void Cast(Transform origin, Vector3 direction, Vector3 targetPosition);

        /// <summary>
        ///     The cast will be stopped. Start any cleanup.
        /// </summary>
        public virtual void WillStop()
        {
        }

        /// <summary>
        ///     Stops the cast.
        /// </summary>
        /// <param name="castID">The ID of the cast that should be stopped.</param>
        public void Stop(uint castID)
        {
            m_CastID = castID;
            Stop();
        }

        /// <summary>
        ///     Stops the cast.
        /// </summary>
        public virtual void Stop()
        {
            m_CastID = 0;
        }

        /// <summary>
        ///     The character has changed perspectives.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public virtual void OnChangePerspectives(Transform origin)
        {
        }

        /// <summary>
        ///     The action has been destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
        }
    }
}