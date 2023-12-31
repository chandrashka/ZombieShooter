﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Editor.Inspectors;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities
{
    /// <summary>
    ///     Draws a custom inspector for the DetectObjectAbilityBase ability.
    /// </summary>
    [InspectorDrawer(typeof(DetectObjectAbilityBase))]
    public class DetectObjectAbilityBaseInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        ///     Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            // Draw ObjectDetectionMode manually so it'll use the MaskField.
            var objectDetection =
                (int)InspectorUtility.GetFieldValue<DetectObjectAbilityBase.ObjectDetectionMode>(target,
                    "m_ObjectDetection");
            var objectDetectionString = Enum.GetNames(typeof(DetectObjectAbilityBase.ObjectDetectionMode));
            var value = EditorGUILayout.MaskField(
                new GUIContent("Object Detection", InspectorUtility.GetFieldTooltip(target, "m_ObjectDetection")),
                objectDetection, objectDetectionString);
            if (value != objectDetection) InspectorUtility.SetFieldValue(target, "m_ObjectDetection", value);
            // The ability may not use any detection.
            if (value != 0)
            {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_DetectLayers");
                InspectorUtility.DrawField(target, "m_UseLookPosition");
                InspectorUtility.DrawField(target, "m_UseLookDirection");
                InspectorUtility.DrawField(target, "m_AngleThreshold");
                InspectorUtility.DrawField(target, "m_ObjectID");

                var objectDetectionEnumValue = (DetectObjectAbilityBase.ObjectDetectionMode)value;
                if (objectDetectionEnumValue != DetectObjectAbilityBase.ObjectDetectionMode.Trigger)
                {
                    InspectorUtility.DrawField(target, "m_CastDistance");
                    InspectorUtility.DrawField(target, "m_CastFrameInterval");
                    InspectorUtility.DrawField(target, "m_CastOffset");
                    InspectorUtility.DrawField(target, "m_TriggerInteraction");
                    if ((objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Spherecast) != 0)
                        InspectorUtility.DrawField(target, "m_SpherecastRadius");
                }

                if ((objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Trigger) != 0)
                    InspectorUtility.DrawField(target, "m_MaxTriggerObjectCount");

                if (EditorApplication.isPlaying)
                {
                    var detectObjectAbility = (DetectObjectAbilityBase)target;
                    EditorGUILayout.LabelField("Detected Object",
                        detectObjectAbility.DetectedObject != null
                            ? detectObjectAbility.DetectedObject.name
                            : "(none)");
                }

                EditorGUI.indentLevel--;
            }

            InspectorUtility.DrawField(target, "m_MoveWithObject");

            base.DrawInspectorDrawerFields(target, parent);
        }
    }
}