﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Audio;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.BeginEndActions
{
    /// <summary>
    ///     Plays an audio clip.
    /// </summary>
    [Serializable]
    public class PlayAudioClip : BeginEndAction
    {
        [Tooltip("The AudioClip that should be played. A random AudioClip will be selected.")] [SerializeField]
        protected AudioClip[] m_AudioClips;

        [Tooltip("Plays the AudioClip at the origin. If the value is false the character position will be used.")]
        [SerializeField]
        protected bool m_PlayAtOrigin = true;

        [Tooltip("Should the AudioClip loop?")] [SerializeField]
        protected bool m_Loop;

        private AudioSource m_AudioSource;

        private Transform m_CharacterTransform;

        public AudioClip[] AudioClips
        {
            get => m_AudioClips;
            set => m_AudioClips = value;
        }

        public bool PlayAtOrigin
        {
            get => m_PlayAtOrigin;
            set => m_PlayAtOrigin = value;
        }

        public bool Loop
        {
            get => m_Loop;
            set => m_Loop = value;
        }

        /// <summary>
        ///     Initializes the action.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the StartAction belongs to.</param>
        /// <param name="beginAction">True if the action is a starting action.</param>
        /// <param name="index">The index of the BeginEndAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, bool beginAction, int index)
        {
            base.Initialize(character, magicItem, beginAction, index);

            m_CharacterTransform = character.transform;
        }

        /// <summary>
        ///     The action has started.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void Start(Transform origin)
        {
            if (m_AudioSource != null) return;

            if (m_AudioClips == null || m_AudioClips.Length == 0)
            {
                Debug.LogError("Error: An Audio Clip must be specified", m_MagicItem);
                return;
            }

            var audioClip = m_AudioClips[Random.Range(0, m_AudioClips.Length)];
            if (audioClip == null)
            {
                Debug.Log("Error: The Audio Clip array has a null value.");
                return;
            }

            m_AudioSource = AudioManager
                .PlayAtPosition(audioClip, m_PlayAtOrigin ? origin.position : m_CharacterTransform.position)
                .AudioSource;
            if (m_AudioSource != null) m_AudioSource.loop = m_Loop;
        }

        /// <summary>
        ///     The action has stopped.
        /// </summary>
        public override void Stop()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.Stop();
                m_AudioSource = null;
            }
        }
    }
}