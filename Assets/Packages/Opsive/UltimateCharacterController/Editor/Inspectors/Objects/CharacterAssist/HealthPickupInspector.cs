/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using UnityEditor;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.CharacterAssist
{
    /// <summary>
    ///     Custom inspector for the HealthPickup component.
    /// </summary>
    [CustomEditor(typeof(HealthPickup))]
    public class HealthPickupInspector : ObjectPickupInspector
    {
        /// <summary>
        ///     Draws the object pickup fields.
        /// </summary>
        protected override void DrawObjectPickupFields()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_HealthAmount"));
            EditorGUILayout.PropertyField(PropertyFromName("m_AlwaysPickup"));
        }
    }
}