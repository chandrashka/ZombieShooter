/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using System.Collections.Generic;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Traits;
using UnityEngine;
using Attribute = System.Attribute;
using Random = UnityEngine.Random;

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    ///     Contains a set of utility functions useful for interacting with the Unity Engine.
    /// </summary>
    public class UnityEngineUtility
    {
        public static HashSet<object> s_ObjectUpdated = new();
        public static ScheduledEventBase s_ObjectClearEvent;

        /// <summary>
        ///     Returns a display name for the specified type.
        /// </summary>
        /// <param name="type">The type to retieve the name of.</param>
        /// <returns>A display name for the specified type.</returns>
        public static string GetDisplayName(Type type)
        {
            return GetDisplayName(type.FullName, type.Name);
        }

        /// <summary>
        ///     Returns a display name for the specified type.
        /// </summary>
        /// <param name="fullName">The full name of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>A display name for the specified type.</returns>
        public static string GetDisplayName(string fullName, string name)
        {
            if (fullName.Contains("FirstPersonController"))
                return "First Person " + name;
            if (fullName.Contains("ThirdPersonController")) return "Third Person " + name;
            return name;
        }

        /// <summary>
        ///     Returns true if the specified object has been updated.
        /// </summary>
        /// <param name="obj">The object to check if it has been updated.</param>
        /// <returns>True if the specified object has been updated.</returns>
        public static bool HasUpdatedObject(object obj)
        {
            return s_ObjectUpdated.Contains(obj);
        }

        /// <summary>
        ///     Adds the specified object to the set.
        /// </summary>
        /// <param name="obj">The object that has been updated.</param>
        public static void AddUpdatedObject(object obj)
        {
            AddUpdatedObject(obj, false);
        }

        /// <summary>
        ///     Adds the specified object to the set.
        /// </summary>
        /// <param name="obj">The object that has been updated.</param>
        /// <param name="autoClear">Should the object updated map be automatically cleared on the next tick?</param>
        public static void AddUpdatedObject(object obj, bool autoClear)
        {
            s_ObjectUpdated.Add(obj);

            if (autoClear && s_ObjectClearEvent == null)
                s_ObjectClearEvent = SchedulerBase.Schedule(0.0001f, ClearUpdatedObjectsEvent);
        }

        /// <summary>
        ///     Removes all of the objects from the set.
        /// </summary>
        public static void ClearUpdatedObjects()
        {
            s_ObjectUpdated.Clear();
        }

        /// <summary>
        ///     Removes all of the objects from the set and sets the event to null.
        /// </summary>
        private static void ClearUpdatedObjectsEvent()
        {
            ClearUpdatedObjects();
            s_ObjectClearEvent = null;
        }

        /// <summary>
        ///     Change the size of the RectTransform according to the size of the sprite.
        /// </summary>
        /// <param name="sprite">The sprite that the RectTransform should change its size to.</param>
        /// <param name="spriteRectTransform">A reference to the sprite's RectTransform.</param>
        public static void SizeSprite(Sprite sprite, RectTransform spriteRectTransform)
        {
            if (sprite != null)
            {
                var sizeDelta = spriteRectTransform.sizeDelta;
                sizeDelta.x = sprite.textureRect.width;
                sizeDelta.y = sprite.textureRect.height;
                spriteRectTransform.sizeDelta = sizeDelta;
            }
        }

        /// <summary>
        ///     Clears the Unity Engine Utility cache.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearCache()
        {
            if (s_ObjectUpdated != null) s_ObjectUpdated.Clear();
        }

        /// <summary>
        ///     Allows for comparison between RaycastHit objects.
        /// </summary>
        public class RaycastHitComparer : IComparer<RaycastHit>
        {
            /// <summary>
            ///     Compare RaycastHit x to RaycastHit y. If x has a smaller distance value compared to y then a negative value will be
            ///     returned.
            ///     If the distance values are equal then 0 will be returned, and if y has a smaller distance value compared to x then
            ///     a positive value will be returned.
            /// </summary>
            /// <param name="x">The first RaycastHit to compare.</param>
            /// <param name="y">The second RaycastHit to compare.</param>
            /// <returns>The resulting difference between RaycastHit x and y.</returns>
            public int Compare(RaycastHit x, RaycastHit y)
            {
                if (x.transform == null) return int.MaxValue;
                if (y.transform == null) return int.MinValue;
                return x.distance.CompareTo(y.distance);
            }
        }

        /// <summary>
        ///     Allows for equity comparison checks between RaycastHit objects.
        /// </summary>
        public struct RaycastHitEqualityComparer : IEqualityComparer<RaycastHit>
        {
            /// <summary>
            ///     Determines if RaycastHit x is equal to RaycastHit y.
            /// </summary>
            /// <param name="x">The first RaycastHit to compare.</param>
            /// <param name="y">The second RaycastHit to compare.</param>
            /// <returns>True if the raycasts are equal.</returns>
            public bool Equals(RaycastHit x, RaycastHit y)
            {
                if (x.distance != y.distance) return false;
                if (x.point != y.point) return false;
                if (x.normal != y.normal) return false;
                if (x.transform != y.transform) return false;
                return true;
            }

            /// <summary>
            ///     Returns a hash code for the RaycastHit.
            /// </summary>
            /// <param name="hit">The RaycastHit to get the hash code of.</param>
            /// <returns>The hash code for the RaycastHit.</returns>
            public int GetHashCode(RaycastHit hit)
            {
                // Don't use hit.GetHashCode because that has boxing. This hash function won't always prevent duplicates but it's fine for what it's used for.
                return (int)(hit.distance * 10000) ^ (int)(hit.point.x * 10000) ^ (int)(hit.point.y * 10000) ^
                       (int)(hit.point.z * 10000) ^
                       (int)(hit.normal.x * 10000) ^ (int)(hit.normal.y * 10000) ^ (int)(hit.normal.z * 10000);
            }
        }
    }

    /// <summary>
    ///     A container for a min and max float value.
    /// </summary>
    [Serializable]
    public struct MinMaxFloat
    {
        [Tooltip("The minimum Vector3 value.")] [SerializeField]
        private float m_MinValue;

        [Tooltip("The maximum Vector3 value.")] [SerializeField]
        private float m_MaxValue;

        /// <summary>
        ///     MinMaxFloat constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum float value.</param>
        /// <param name="maxValue">The maximum float value.</param>
        public MinMaxFloat(float minValue, float maxValue)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
        }

        public float MinValue
        {
            get => m_MinValue;
            set => m_MinValue = value;
        }

        public float MaxValue
        {
            get => m_MaxValue;
            set => m_MaxValue = value;
        }

        public float RandomValue => Random.Range(m_MinValue, m_MaxValue);
    }

    /// <summary>
    ///     A container for a min and max Vector3 value.
    /// </summary>
    [Serializable]
    public struct MinMaxVector3
    {
        [Tooltip("The minimum Vector3 value.")] [SerializeField]
        private Vector3 m_MinValue;

        [Tooltip("The maximum Vector3 value.")] [SerializeField]
        private Vector3 m_MaxValue;

        [Tooltip("The minimum magnitude value when determining a random value.")] [SerializeField]
        private Vector3 m_MinMagnitude;

        /// <summary>
        ///     MinMaxVector3 constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum Vector3 value.</param>
        /// <param name="maxValue">The maximum Vector3 value.</param>
        public MinMaxVector3(Vector3 minValue, Vector3 maxValue)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_MinMagnitude = Vector3.zero;
        }

        /// <summary>
        ///     MinMaxVector3 constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum Vector3 value.</param>
        /// <param name="maxValue">The maximum Vector3 value.</param>
        /// <param name="minMagnitude">The minimum magnitude of the random value.</param>
        public MinMaxVector3(Vector3 minValue, Vector3 maxValue, Vector3 minMagnitude)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_MinMagnitude = minMagnitude;
        }

        public Vector3 MinValue
        {
            get => m_MinValue;
            set => m_MinValue = value;
        }

        public Vector3 MaxValue
        {
            get => m_MaxValue;
            set => m_MaxValue = value;
        }

        public Vector3 MinMagnitude
        {
            get => m_MinMagnitude;
            set => m_MinMagnitude = value;
        }

        public Vector3 RandomValue
        {
            get
            {
                var value = Vector3.zero;
                value.x = GetRandomFloat(m_MinValue.x, m_MaxValue.x, m_MinMagnitude.x);
                value.y = GetRandomFloat(m_MinValue.y, m_MaxValue.y, m_MinMagnitude.y);
                value.z = GetRandomFloat(m_MinValue.z, m_MaxValue.z, m_MinMagnitude.z);
                return value;
            }
        }

        /// <summary>
        ///     Returns a random float between the min and max value with the specified minimum magnitude.
        /// </summary>
        /// <param name="minValue">The minimum float value.</param>
        /// <param name="maxValue">The maximum float value.</param>
        /// <param name="minMagnitude">The minimum magnitude of the random value.</param>
        /// <returns>A random float between the min and max value.</returns>
        private float GetRandomFloat(float minValue, float maxValue, float minMagnitude)
        {
            if (minMagnitude != 0 && Mathf.Sign(m_MinValue.x) != Mathf.Sign(m_MaxValue.x))
            {
                if (Mathf.Sign(Random.Range(m_MinValue.x, m_MaxValue.x)) > 0)
                    return Random.Range(minMagnitude, Mathf.Max(minMagnitude, maxValue));
                return Random.Range(-minMagnitude, Mathf.Min(-minMagnitude, minValue));
            }

            return Random.Range(minValue, maxValue);
        }
    }

    /// <summary>
    ///     Represents the object which can be spawned.
    /// </summary>
    [Serializable]
    public class ObjectSpawnInfo
    {
        public GameObject Object => m_Object;
        public float Probability => m_Probability;
        public bool RandomSpin => m_RandomSpin;

        /// <summary>
        ///     Instantiate the object.
        /// </summary>
        /// <param name="position">The position to instantiate the object at.</param>
        /// <param name="normal">The normal of the instantiated object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <returns>The instantiated object (can be null). </returns>
        public GameObject Instantiate(Vector3 position, Vector3 normal, Vector3 gravityDirection)
        {
            if (m_Object == null) return null;

            // There is a random chance that the object cannot be spawned.
            if (Random.value < m_Probability)
            {
                var rotation = Quaternion.LookRotation(normal);
                // A random spin can be applied so the rotation isn't the same every hit.
                if (m_RandomSpin) rotation *= Quaternion.AngleAxis(Random.Range(0, 360), normal);
                var instantiatedObject = ObjectPoolBase.Instantiate(m_Object, position, rotation);
                // If the DirectionalConstantForce component exists then the gravity direction should be set so the object will move in the correct direction.
                var directionalConstantForce = instantiatedObject.GetCachedComponent<DirectionalConstantForce>();
                if (directionalConstantForce != null) directionalConstantForce.Direction = gravityDirection;
                return instantiatedObject;
            }

            return null;
        }
#pragma warning disable 0649
        [Tooltip("The object that can be spawned.")] [SerializeField]
        private GameObject m_Object;

        [Tooltip("The probability that the object can be spawned.")] [Range(0, 1)] [SerializeField]
        private float m_Probability = 1;

        [Tooltip("Should a random spin be applied to the object after it has been spawned?")] [SerializeField]
        private bool m_RandomSpin;
#pragma warning restore 0649
    }

    /// <summary>
    ///     Struct which stores the material values to revert back to after the material has been faded.
    /// </summary>
    public struct OriginalMaterialValue
    {
        [field: Tooltip("The color of the material.")]
        public Color Color { get; set; }

        [field: Tooltip("Does the material have a mode property?")]
        public bool ContainsMode { get; set; }

        [field: Tooltip("The render mode of the material.")]
        public float Mode { get; set; }

        [field: Tooltip("The SourceBlend BlendMode of the material.")]
        public int SrcBlend { get; set; }

        [field: Tooltip("The DestinationBlend BlendMode of the material.")]
        public int DstBlend { get; set; }

        [field: Tooltip("Is alpha blend enabled?")]
        public bool AlphaBlend { get; set; }

        [field: Tooltip("The render queue of the material.")]
        public int RenderQueue { get; set; }

        public static int ModeID { get; private set; }

        public static int SrcBlendID { get; private set; }

        public static int DstBlendID { get; private set; }

        public static string AlphaBlendString { get; } = "_ALPHABLEND_ON";

        /// <summary>
        ///     Initializes the OriginalMaterialValue.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            ModeID = Shader.PropertyToID("_Mode");
            SrcBlendID = Shader.PropertyToID("_SrcBlend");
            DstBlendID = Shader.PropertyToID("_DstBlend");
        }

        /// <summary>
        ///     Initializes the OriginalMaterialValue to the material values.
        /// </summary>
        /// <param name="material">The material to initialize.</param>
        /// <param name="colorID">The id of the color property.</param>
        /// <param name="containsMode">Does the material have a Mode property?</param>
        public void Initialize(Material material, int colorID, bool containsMode)
        {
            Color = material.GetColor(colorID);
            AlphaBlend = material.IsKeywordEnabled(AlphaBlendString);
            RenderQueue = material.renderQueue;
            ContainsMode = containsMode;
            if (containsMode)
            {
                Mode = material.GetFloat(ModeID);
                SrcBlend = material.GetInt(SrcBlendID);
                DstBlend = material.GetInt(DstBlendID);
            }
        }
    }

    /// <summary>
    ///     Storage class for determining if an event is triggered based on an animation event or time.
    /// </summary>
    [Serializable]
    public class AnimationEventTrigger
    {
        [Tooltip("Is the event triggered with a Unity animation event?")] [SerializeField]
        private bool m_WaitForAnimationEvent;

        [Tooltip("The amount of time it takes to trigger the event if not using an animation event.")] [SerializeField]
        private float m_Duration;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public AnimationEventTrigger()
        {
        }

        /// <summary>
        ///     Two parameter constructor for AnimationEventTrigger.
        /// </summary>
        /// <param name="waitForAnimationEvent">Is the event triggered with a Unity animation event?</param>
        /// <param name="duration">The amount of time it takes to trigger the event if not using an animation event.</param>
        public AnimationEventTrigger(bool waitForAnimationEvent, float duration)
        {
            m_WaitForAnimationEvent = waitForAnimationEvent;
            m_Duration = duration;
        }

        public bool WaitForAnimationEvent
        {
            get => m_WaitForAnimationEvent;
            set => m_WaitForAnimationEvent = value;
        }

        public float Duration
        {
            get => m_Duration;
            set => m_Duration = value;
        }
    }

    /// <summary>
    ///     Determines if an animation event should be triggered for a specified slot.
    /// </summary>
    [Serializable]
    public class AnimationSlotEventTrigger : AnimationEventTrigger
    {
        [Tooltip("Specifies if the item should wait for the specific slot animation event.")] [SerializeField]
        private bool m_WaitForSlotEvent;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public AnimationSlotEventTrigger()
        {
        }

        /// <summary>
        ///     Two parameter constructor for AnimationSlotEventTrigger.
        /// </summary>
        /// <param name="waitForAnimationEvent">Is the event triggered with a Unity animation event?</param>
        /// <param name="duration">The amount of time it takes to trigger the event if not using an animation event.</param>
        public AnimationSlotEventTrigger(bool waitForAnimationEvent, float duration) : base(waitForAnimationEvent,
            duration)
        {
        }

        public bool WaitForSlotEvent
        {
            get => m_WaitForSlotEvent;
            set => m_WaitForSlotEvent = value;
        }
    }

    /// <summary>
    ///     Attribute which allows the same type to be added multiple times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowDuplicateTypes : Attribute
    {
        // Intentionally left blank.
    }
}