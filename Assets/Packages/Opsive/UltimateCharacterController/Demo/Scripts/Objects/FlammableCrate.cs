﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    /// <summary>
    ///     A flammable crate will play a flame particle if it gets hit by a fireball.
    /// </summary>
    public class FlammableCrate : MonoBehaviour
    {
        [Tooltip("The SurfaceImpact that causes the flame to start.")] [SerializeField]
        protected SurfaceImpact m_FlameImpact;

        [Tooltip("A reference to the flame particle that should start when the fireball collides with the crate.")]
        [SerializeField]
        protected GameObject m_FlamePrefab;

        [Tooltip("The crate that is spawned with the wood shreds.")] [SerializeField]
        protected GameObject m_DestroyedCrate;

        [Tooltip("The interval that the object should have its health reduced.")] [SerializeField]
        protected MinMaxFloat m_HealthReductionInterval = new(0.2f, 0.8f);

        [Tooltip("The amount that the object should be damaged on each interval.")] [SerializeField]
        protected MinMaxFloat m_DamageAmount = new(4, 10);

        [Tooltip("The amount of time it takes for the wood shreds to be removed.")] [SerializeField]
        protected MinMaxFloat m_WoodShreadRemovalTime = new(5, 7);

        [Tooltip("The amount to fade out the AudioSource.")] [SerializeField]
        protected float m_AudioSourceFadeAmount = 0.05f;

        private BoxCollider m_DamageTrigger;
        private ParticleSystem m_FlameParticle;
        private AudioSource m_FlameParticleAudioSource;
        private Health m_Health;
        private GameObject m_SpawnedCrate;

        private float m_StartHealth;
        private ScheduledEventBase m_StopEvent;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Health = GetComponent<Health>();
            m_StartHealth = m_Health.HealthValue;

            // A box collider will be a trigger which damages the character if they stop within the flames.
            var colliders = GetComponents<BoxCollider>();
            for (var i = 0; i < colliders.Length; ++i)
            {
                if (!colliders[i].isTrigger) continue;

                m_DamageTrigger = colliders[i];
                m_DamageTrigger.enabled = false;
                break;
            }

            EventHandler.RegisterEvent<RaycastHit, SurfaceImpact>(gameObject, "OnMagicItemImpact", MagicCastCollision);
        }

        /// <summary>
        ///     The crate has been enabled.
        /// </summary>
        private void OnEnable()
        {
            StopParticles();
            if (m_Health.HealthValue != m_StartHealth) m_Health.Heal(m_StartHealth - m_Health.HealthValue);
        }

        /// <summary>
        ///     The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<RaycastHit, SurfaceImpact>(gameObject, "OnMagicItemImpact",
                MagicCastCollision);
        }

        /// <summary>
        ///     The magic cast has collided with another object.
        /// </summary>
        /// <param name="hit">The raycast that caused the impact.</param>
        /// <param name="surfaceImpact">The type of particle that collided with the object.</param>
        private void MagicCastCollision(RaycastHit hit, SurfaceImpact surfaceImpact)
        {
            if (m_FlameParticle != null || (m_FlameImpact != null && m_FlameImpact != surfaceImpact)) return;

            // A fireball has collided with the crate. Start the flame.
            var crateTransform = transform;
            var flamePrefab =
                ObjectPoolBase.Instantiate(m_FlamePrefab, crateTransform.position, crateTransform.rotation);
            m_FlameParticle = flamePrefab.GetComponent<ParticleSystem>();
            m_FlameParticleAudioSource = flamePrefab.GetCachedComponent<AudioSource>();
            m_FlameParticleAudioSource.volume = 1;
            m_DamageTrigger.enabled = true;

            // The crate should be destroyed by the flame.
            ReduceHealth();
        }

        /// <summary>
        ///     Reduces the health by the damage amount.
        /// </summary>
        private void ReduceHealth()
        {
            m_Health.Damage(m_DamageAmount.RandomValue);
            if (m_Health.IsAlive())
            {
                // Keep reducing the object's health until is is no longer alive.
                SchedulerBase.Schedule(m_HealthReductionInterval.RandomValue, ReduceHealth);
            }
            else
            {
                // After the object is no longer alive spawn some wood shreds. These shreds should be cleaned up after a random
                // amount of time.
                var crateTransform = transform;
                m_SpawnedCrate =
                    ObjectPoolBase.Instantiate(m_DestroyedCrate, crateTransform.position, crateTransform.rotation);
                var maxDestroyTime = 0f;
                for (var i = 0; i < m_SpawnedCrate.transform.childCount; ++i)
                {
                    var destroyTime = m_WoodShreadRemovalTime.RandomValue;
                    if (destroyTime > maxDestroyTime) maxDestroyTime = destroyTime;
                    Destroy(m_SpawnedCrate.transform.GetChild(i).gameObject, destroyTime);
                }

                m_StopEvent = SchedulerBase.Schedule(maxDestroyTime, StopParticles);
            }
        }

        /// <summary>
        ///     The crate has been destroyed. Stop the particles.
        /// </summary>
        private void StopParticles()
        {
            if (m_StopEvent == null) return;

            SchedulerBase.Cancel(m_StopEvent);
            m_StopEvent = null;
            m_DamageTrigger.enabled = false;
            m_FlameParticle.Stop(true);
            m_FlameParticle = null;
            SchedulerBase.Schedule(0.2f, FadeAudioSource);
        }

        /// <summary>
        ///     Fades the flame audio source.
        /// </summary>
        private void FadeAudioSource()
        {
            m_FlameParticleAudioSource.volume -= m_AudioSourceFadeAmount;
            if (m_FlameParticleAudioSource.volume > 0) SchedulerBase.Schedule(0.2f, FadeAudioSource);
        }
    }
}