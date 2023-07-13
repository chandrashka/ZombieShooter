/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Editor.Inspectors;
using Opsive.Shared.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Editor.Managers;
using Opsive.UltimateCharacterController.Inventory;
using UnityEditor;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    /// <summary>
    ///     Custom inspector for the Category ScriptableObject.
    /// </summary>
    [CustomEditor(typeof(Category), true)]
    public class CategoryInspector : InspectorBase
    {
        /// <summary>
        ///     Draws the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUILayout.Label("The Category can be managed within the Item Type Manager.", InspectorStyles.WordWrapLabel);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Item Type Manager", GUILayout.MaxWidth(200)))
                MainManagerWindow.ShowItemTypeManagerWindow();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}