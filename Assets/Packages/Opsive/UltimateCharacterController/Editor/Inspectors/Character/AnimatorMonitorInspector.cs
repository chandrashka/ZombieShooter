/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Editor.Inspectors.StateSystem;
using Opsive.UltimateCharacterController.Character;
using UnityEditor;

namespace Opsive.UltimateCharacterController.Editor.Inspectors
{
    /// <summary>
    ///     Shows a custom inspector for the AnimatorMonitor component.
    /// </summary>
    [CustomEditor(typeof(AnimatorMonitor), true)]
    public class AnimatorMonitorInspector : StateBehaviorInspector
    {
        /// <summary>
        ///     Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                EditorGUILayout.PropertyField(PropertyFromName("m_AnimatorSpeed"));
                if (Foldout("Time"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_HorizontalMovementDampingTime"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ForwardMovementDampingTime"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_PitchDampingTime"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_YawDampingTime"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Editor"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_LogAbilityParameterChanges"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LogItemParameterChanges"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LogEvents"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }
    }
}