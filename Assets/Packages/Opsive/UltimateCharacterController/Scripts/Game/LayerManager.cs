/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    ///     Singleton object which manages the index values of the Unity layers.
    /// </summary>
    public class LayerManager : MonoBehaviour
    {
        // Built-in Unity layers.
        private const int DefaultLayer = 0;
        private const int TransparentFXLayer = 1;
        private const int IgnoreRaycastLayer = 2;
        private const int WaterLayer = 4;
        private const int UILayer = 5;

        // Custom layers.
        private const int EnemyLayer = 26;
        private const int MovingPlatformLayer = 27;
        private const int VisualEffectLayer = 28;
        private const int OverlayLayer = 29;
        private const int SubCharacterLayer = 30;
        private const int CharacterLayer = 31;
        private static LayerManager s_Instance;
        private static bool s_Initialized;

        private static Dictionary<Collider, List<Collider>> s_IgnoreCollisionMap;

        public static int Default => DefaultLayer;
        public static int TransparentFX => TransparentFXLayer;
        public static int IgnoreRaycast => IgnoreRaycastLayer;
        public static int Water => WaterLayer;
        public static int UI => UILayer;

        public static int Enemy => EnemyLayer;
        public static int MovingPlatform => MovingPlatformLayer;
        public static int VisualEffect => VisualEffectLayer;
        public static int Overlay => OverlayLayer;
        public static int SubCharacter => SubCharacterLayer;
        public static int Character => CharacterLayer;

        /// <summary>
        ///     Setup the layer collisions.
        /// </summary>
        private void Awake()
        {
            Physics.IgnoreLayerCollision(IgnoreRaycast, VisualEffect);
            Physics.IgnoreLayerCollision(SubCharacter, Default);
            Physics.IgnoreLayerCollision(SubCharacter, VisualEffect);
            Physics.IgnoreLayerCollision(VisualEffect, VisualEffect);
            Physics.IgnoreLayerCollision(Overlay, Default);
            Physics.IgnoreLayerCollision(Overlay, VisualEffect);
            Physics.IgnoreLayerCollision(Overlay, Enemy);
            Physics.IgnoreLayerCollision(Overlay, SubCharacter);
            Physics.IgnoreLayerCollision(Overlay, Character);
        }

        /// <summary>
        ///     The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null)
            {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        ///     The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        /// <summary>
        ///     The LayerManager may not have been added to the Game GameObject.
        /// </summary>
        public static void Initialize()
        {
            if (!s_Initialized)
            {
                s_Instance = new GameObject("Layer Manager").AddComponent<LayerManager>();
                s_Initialized = true;
            }
        }

        /// <summary>
        ///     Ignore the collision between the main collider and the other collider.
        /// </summary>
        /// <param name="mainCollider">The main collider collision to ignore.</param>
        /// <param name="otherCollider">The collider to ignore.</param>
        public static void IgnoreCollision(Collider mainCollider, Collider otherCollider)
        {
            // Keep a mapping of the colliders that mainCollider is ignorning so the collision can easily be reverted.
            if (s_IgnoreCollisionMap == null) s_IgnoreCollisionMap = new Dictionary<Collider, List<Collider>>();

            // Add the collider to the list so it can be reverted.
            List<Collider> colliderList;
            if (!s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList))
            {
                colliderList = new List<Collider>();
                s_IgnoreCollisionMap.Add(mainCollider, colliderList);
            }

            colliderList.Add(otherCollider);

            // The otherCollider must also keep track of the mainCollder. This allows otherCollider to be removed before mainCollider.
            if (!s_IgnoreCollisionMap.TryGetValue(otherCollider, out colliderList))
            {
                colliderList = new List<Collider>();
                s_IgnoreCollisionMap.Add(otherCollider, colliderList);
            }

            colliderList.Add(mainCollider);

            // Do the actual ignore.
            Physics.IgnoreCollision(mainCollider, otherCollider);
        }

        /// <summary>
        ///     The main collider should no longer ignore any collisions.
        /// </summary>
        /// <param name="mainCollider">The collider to revert the collisions on.</param>
        public static void RevertCollision(Collider mainCollider)
        {
            List<Collider> colliderList;
            List<Collider> otherColliderList;
            // Revert the IgnoreCollision setting on all of the colliders that the object is currently ignoring.
            if (s_IgnoreCollisionMap != null && s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList))
            {
                for (var i = 0; i < colliderList.Count; ++i)
                {
                    if (!mainCollider.enabled || !mainCollider.gameObject.activeInHierarchy ||
                        !colliderList[i].enabled || !colliderList[i].gameObject.activeInHierarchy) continue;

                    Physics.IgnoreCollision(mainCollider, colliderList[i], false);

                    // A two way map was added when the initial IgnoreCollision was added. Remove that second map because the IgnoreCollision has been removed.
                    if (s_IgnoreCollisionMap.TryGetValue(colliderList[i], out otherColliderList))
                        for (var j = 0; j < otherColliderList.Count; ++j)
                            if (otherColliderList[j].Equals(mainCollider))
                            {
                                otherColliderList.RemoveAt(j);
                                break;
                            }
                }

                colliderList.Clear();
            }
        }

        /// <summary>
        ///     Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        ///     Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }
    }
}