﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Editor.Inspectors;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Objects;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using EditorUtility = Opsive.Shared.Editor.Utility.EditorUtility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    /// <summary>
    ///     Custom inspector for the TrajectoryObject component.
    /// </summary>
    [CustomEditor(typeof(TrajectoryObject), true)]
    public class TrajectoryObjectInspector : InspectorBase
    {
        private ReorderableList m_ReorderableActiveAudioClipsList;
        private TrajectoryObject m_TrajectoryObject;

        /// <summary>
        ///     The inspector has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_TrajectoryObject = target as TrajectoryObject;
        }

        /// <summary>
        ///     Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName("m_InitializeOnEnable"));

            if (Foldout("Physics"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_Mass"));
                EditorGUILayout.PropertyField(PropertyFromName("m_StartVelocityMultiplier"));
                EditorGUILayout.PropertyField(PropertyFromName("m_GravityMagnitude"));
                EditorGUILayout.PropertyField(PropertyFromName("m_Speed"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationSpeed"));
                EditorGUILayout.PropertyField(PropertyFromName("m_Damping"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationDamping"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RotateInMoveDirection"));
                EditorGUILayout.PropertyField(PropertyFromName("m_SettleThreshold"));
                EditorGUILayout.PropertyField(PropertyFromName("m_SidewaysSettleThreshold"));
                EditorGUILayout.PropertyField(PropertyFromName("m_StartSidewaysVelocityMagnitude"));
                EditorGUILayout.PropertyField(PropertyFromName("m_MaxCollisionCount"));
                EditorGUI.indentLevel--;
            }

            if (Foldout("Impact"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_ImpactLayers"));
                EditorGUILayout.PropertyField(PropertyFromName("m_SurfaceImpact"));
                EditorGUILayout.PropertyField(PropertyFromName("m_ForceMultiplier"));
                var collisionModeProperty = PropertyFromName("m_CollisionMode");
                EditorGUILayout.PropertyField(collisionModeProperty);
                if (collisionModeProperty.enumValueIndex != (int)TrajectoryObject.CollisionMode.Collide &&
                    collisionModeProperty.enumValueIndex != (int)TrajectoryObject.CollisionMode.Ignore)
                    EditorGUILayout.PropertyField(PropertyFromName("m_ReflectMultiplier"));
                else if (collisionModeProperty.enumValueIndex == (int)TrajectoryObject.CollisionMode.Collide)
                    if (target is Destructible)
                        EditorGUILayout.PropertyField(PropertyFromName("m_StickyLayers"), true);
                EditorGUI.indentLevel--;
            }

            if (Foldout("Audio"))
            {
                EditorGUI.indentLevel++;
                m_ReorderableActiveAudioClipsList = AudioClipSetInspector.DrawAudioClipSet(
                    m_TrajectoryObject.ActiveAudioClipSet, m_ReorderableActiveAudioClipsList, OnActiveAudioClipDraw,
                    OnActiveAudioClipListAdd, OnActiveAudioClipListRemove);
                EditorGUI.indentLevel--;
            }

            DrawObjectFields();

            if (Foldout("Curve"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_MaxPositionCount"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.RecordUndoDirtyObject(target, "Value Change");
                serializedObject.ApplyModifiedProperties();
            }
        }


        /// <summary>
        ///     Draws the AudioClip element.
        /// </summary>
        private void OnActiveAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableActiveAudioClipsList, rect, index,
                m_TrajectoryObject.ActiveAudioClipSet, null);
        }

        /// <summary>
        ///     Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnActiveAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_TrajectoryObject.ActiveAudioClipSet, null);
        }

        /// <summary>
        ///     Remove the AudioClip element at the list index.
        /// </summary>
        private void OnActiveAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_TrajectoryObject.ActiveAudioClipSet, null);
            m_TrajectoryObject.ActiveAudioClipSet.AudioClips = (AudioClip[])list.list;
        }

        /// <summary>
        ///     Draws the inspector fields for the child object.
        /// </summary>
        protected virtual void DrawObjectFields()
        {
        }
    }
}