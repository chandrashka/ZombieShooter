﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.UltimateCharacterController.Editor.Inspectors.Items;
using Opsive.UltimateCharacterController.ThirdPersonController.Items;
using UnityEditor;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Items
{
    /// <summary>
    ///     Shows a custom inspector for the ThirdPersonPerspectiveItem.
    /// </summary>
    [CustomEditor(typeof(ThirdPersonPerspectiveItem))]
    public class ThirdPersonPerspectiveItemInspector : PerspectiveItemInspector
    {
        /// <summary>
        ///     Draws the options for spawning based on a parent.
        /// </summary>
        protected override void DrawSpawnParentProperties()
        {
            CheckForObject();

            EditorGUILayout.PropertyField(PropertyFromName("m_LocalSpawnPosition"));
            EditorGUILayout.PropertyField(PropertyFromName("m_LocalSpawnRotation"));
            EditorGUILayout.PropertyField(PropertyFromName("m_LocalSpawnScale"));
        }

        /// <summary>
        ///     Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                if (Foldout("IK"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_NonDominantHandIKTarget"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_NonDominantHandIKTargetID"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_NonDominantHandIKTargetHint"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_NonDominantHandIKTargetHintID"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        ///     Draws the options for the render foldout.
        /// </summary>
        protected override void DrawRenderProperties()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_HolsterTarget"));
            EditorGUILayout.PropertyField(PropertyFromName("m_HolsterID"));
        }
    }
}