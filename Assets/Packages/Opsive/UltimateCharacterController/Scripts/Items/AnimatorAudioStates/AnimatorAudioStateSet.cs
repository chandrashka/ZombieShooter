﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Audio;
using Opsive.Shared.StateSystem;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Character;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    /// <summary>
    ///     The AnimatorAudioStateSet represets a set of animation parameters and audio clips that should be played together.
    /// </summary>
    [Serializable]
    public class AnimatorAudioStateSet
    {
        [Tooltip("The serialization data for the AnimatorAudioStateState.")] [SerializeField]
        protected Serialization m_AnimatorAudioStateSelectorData;

        [Tooltip("An array of possible states for the animator parameter and audio clip.")] [SerializeField]
        protected AnimatorAudioState[] m_States;

        private AnimatorAudioStateSelector m_AnimatorAudioStateSelector;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public AnimatorAudioStateSet()
        {
            var animatorAudioOutputSelector = Activator.CreateInstance(typeof(Sequence)) as AnimatorAudioStateSelector;
            m_AnimatorAudioStateSelectorData = Serialization.Serialize(animatorAudioOutputSelector);
        }

        /// <summary>
        ///     Constructor with one parameter.
        /// </summary>
        /// <param name="itemSubstateParameter">The value of the animator's Item Substate Index parameter.</param>
        public AnimatorAudioStateSet(int itemSubstateParameter) : this()
        {
            if (m_States == null) m_States = new AnimatorAudioState[] { new(itemSubstateParameter) };
        }

        public Serialization AnimatorAudioStateSelectorData
        {
            get => m_AnimatorAudioStateSelectorData;
            set
            {
                m_AnimatorAudioStateSelectorData = value;
                if (!Application.isPlaying) DeserializeAnimatorAudioStateSelector(null, null);
            }
        }

        public AnimatorAudioState[] States
        {
            get => m_States;
            set => m_States = value;
        }

        public AnimatorAudioStateSelector AnimatorAudioStateSelector
        {
            get
            {
                if (!Application.isPlaying && m_AnimatorAudioStateSelector == null)
                    DeserializeAnimatorAudioStateSelector(null, null);
                return m_AnimatorAudioStateSelector;
            }
        }

        /// <summary>
        ///     Deserializes the AnimatorAudioStateSelector data.
        /// </summary>
        /// <param name="item">A reference to the item that the state belongs to.</param>
        /// <param name="characterLocomotion">A reference to the character that the state belongs to.</param>
        public void DeserializeAnimatorAudioStateSelector(Item item, UltimateCharacterLocomotion characterLocomotion)
        {
            if (m_AnimatorAudioStateSelectorData != null)
            {
                m_AnimatorAudioStateSelector =
                    m_AnimatorAudioStateSelectorData.DeserializeFields(MemberVisibility.Public) as
                        AnimatorAudioStateSelector;
                if (characterLocomotion != null && Application.isPlaying)
                {
                    if (m_AnimatorAudioStateSelector != null)
                        m_AnimatorAudioStateSelector.Initialize(item.gameObject, characterLocomotion, item, m_States);
                    else
                        Debug.LogError("Error: The AnimatorAudioState is null. Select the item " + item.name +
                                       " within the inspector to serialize.");
                }
            }
        }

        /// <summary>
        ///     Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject that the AnimatorAudioStateSet is attached to.</param>
        public void Awake(GameObject gameObject)
        {
            for (var i = 0; i < m_States.Length; ++i) m_States[i].Initialize(gameObject);
        }

        /// <summary>
        ///     Returns the current state index of the selector. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public int GetStateIndex()
        {
            if (m_AnimatorAudioStateSelector == null || m_States.Length == 0) return -1;
            return m_AnimatorAudioStateSelector.GetStateIndex();
        }

        /// <summary>
        ///     Starts or stops the state selection. Will activate or deactivate the state with the name specified within the
        ///     AnimatorAudioState.
        /// </summary>
        /// <param name="start">Is the object starting?</param>
        public void StartStopStateSelection(bool start)
        {
            m_AnimatorAudioStateSelector.StartStopStateSelection(start);
        }

        /// <summary>
        ///     Returns the Item Substate Index parameter value. -1 indicates this value is not set by the class.
        /// </summary>
        /// <returns>The current Item Substate Index parameter value.</returns>
        public int GetItemSubstateIndex()
        {
            var stateIndex = GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) return -1;
            return m_States[stateIndex].ItemSubstateIndex +
                   m_AnimatorAudioStateSelector.GetAdditionalItemSubstateIndex();
        }

        /// <summary>
        ///     Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        public void PlayAudioClip(GameObject gameObject)
        {
            if (m_AnimatorAudioStateSelector == null || m_States.Length == 0) return;
            var stateIndex = m_AnimatorAudioStateSelector.GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) return;
            m_States[stateIndex].PlayAudioClip(gameObject);
        }

        /// <summary>
        ///     Moves to the next state of the selector.
        /// </summary>
        public bool NextState()
        {
            if (m_AnimatorAudioStateSelector == null) return false;

            return m_AnimatorAudioStateSelector.NextState();
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        public void OnDestroy()
        {
            if (m_AnimatorAudioStateSelector == null) return;

            m_AnimatorAudioStateSelector.OnDestroy();
        }

        /// <summary>
        ///     Contains a single animator and audio clip state.
        /// </summary>
        [Serializable]
        public class AnimatorAudioState : StateObject
        {
            [Tooltip("Is the AnimatorAudioState enabled?")] [SerializeField]
            protected bool m_Enabled = true;

            [Tooltip("Can the state be selected when the character is moving?")] [SerializeField]
            protected bool m_AllowDuringMovement = true;

            [Tooltip("Does the state require the character to be grounded?")] [SerializeField]
            protected bool m_RequireGrounded;

            [Tooltip("The name of the state that should be active when the animation is playing.")] [SerializeField]
            protected string m_StateName;

            [Tooltip("The value of the animator's Item Substate Index parameter.")] [SerializeField]
            protected int m_ItemSubstateIndex;

            [Tooltip("Contains an array of AudioClips.")] [SerializeField]
            protected AudioClipSet m_AudioClipSet = new();

            /// <summary>
            ///     Default constructor.
            /// </summary>
            public AnimatorAudioState()
            {
            }

            /// <summary>
            ///     Constructor with one parameter.
            /// </summary>
            /// <param name="itemSubstateIndex">The value of the animator's Item Substate Index parameter.</param>
            public AnimatorAudioState(int itemSubstateIndex)
            {
                m_ItemSubstateIndex = itemSubstateIndex;
            }

            public bool Enabled
            {
                get => m_Enabled;
                set => m_Enabled = value;
            }

            public bool AllowDuringMovement
            {
                get => m_AllowDuringMovement;
                set => m_AllowDuringMovement = value;
            }

            public bool RequireGrounded
            {
                get => m_RequireGrounded;
                set => m_RequireGrounded = value;
            }

            public string StateName
            {
                get => m_StateName;
                set => m_StateName = value;
            }

            public int ItemSubstateIndex
            {
                get => m_ItemSubstateIndex;
                set => m_ItemSubstateIndex = value;
            }

            public AudioClipSet AudioClipSet
            {
                get => m_AudioClipSet;
                set => m_AudioClipSet = value;
            }

            /// <summary>
            ///     Plays the audio clip with a random set index.
            /// </summary>
            /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
            public void PlayAudioClip(GameObject gameObject)
            {
                m_AudioClipSet.PlayAudioClip(gameObject);
            }
        }
    }
}