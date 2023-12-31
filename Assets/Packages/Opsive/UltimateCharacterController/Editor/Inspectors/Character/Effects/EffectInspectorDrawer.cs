﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Editor.Inspectors;
using Opsive.Shared.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Character.Effects;
using UnityEngine;
using InspectorUtility = Opsive.UltimateCharacterController.Editor.Inspectors.Utility.InspectorUtility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    /// <summary>
    ///     Draws a custom inspector for the base Effect type.
    /// </summary>
    [InspectorDrawer(typeof(Effect))]
    public class EffectInspectorDrawer : InspectorDrawer
    {
        /// <summary>
        ///     Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            DrawInspectorDrawerFields(target, parent);

            InspectorUtility.DrawField(target, "m_InspectorDescription");
        }

        /// <summary>
        ///     Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected virtual void DrawInspectorDrawerFields(object target, Object parent)
        {
            ObjectInspector.DrawFields(target, false);
        }
    }
}