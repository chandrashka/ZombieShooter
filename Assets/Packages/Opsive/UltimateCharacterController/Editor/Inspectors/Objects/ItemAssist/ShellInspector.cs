/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Objects.ItemAssist;
using UnityEditor;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.ItemAssist
{
    /// <summary>
    ///     Custom inspector for the Shell component.
    /// </summary>
    [CustomEditor(typeof(Shell))]
    public class ShellInspector : TrajectoryObjectInspector
    {
        /// <summary>
        ///     Draws the inspector fields for the object.
        /// </summary>
        protected override void DrawObjectFields()
        {
            base.DrawObjectFields();

            if (Foldout("Shell"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_Lifespan"));
                EditorGUILayout.PropertyField(PropertyFromName("m_Persistence"));
                EditorGUI.indentLevel--;
            }
        }
    }
}