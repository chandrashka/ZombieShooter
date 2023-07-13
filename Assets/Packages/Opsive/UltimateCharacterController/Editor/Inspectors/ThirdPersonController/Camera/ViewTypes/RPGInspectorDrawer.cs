/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Editor.Inspectors;
using Opsive.Shared.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using UnityEditor;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    ///     Draws a custom inspector for the RPG View Type.
    /// </summary>
    [InspectorDrawer(typeof(RPG))]
    public class RPGInspectorDrawer : ThirdPersonInspectorDrawer
    {
        /// <summary>
        ///     Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            base.OnInspectorGUI(target, parent);

            if (InspectorUtility.Foldout(target, "RPG"))
            {
                EditorGUI.indentLevel++;
                Utility.InspectorUtility.DrawField(target, "m_YawSnapDamping");
                Utility.InspectorUtility.DrawField(target, "m_AllowFreeMovement");
                Utility.InspectorUtility.DrawField(target, "m_CameraFreeMovementInputName");
                EditorGUI.indentLevel--;
            }
        }
    }
}