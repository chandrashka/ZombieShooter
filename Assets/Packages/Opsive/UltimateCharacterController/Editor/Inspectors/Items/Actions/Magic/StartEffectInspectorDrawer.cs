﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Editor.Inspectors;
using Opsive.Shared.Editor.Utility;
using Opsive.UltimateCharacterController.Character.Effects;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions.Magic
{
    /// <summary>
    ///     Draws an inspector for the StartEffect CastAction.
    /// </summary>
    [InspectorDrawer(typeof(StartEffect))]
    public class StartEffectInspectorDrawer : InspectorDrawer
    {
        /// <summary>
        ///     Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            var effectValue = InspectorUtility.GetFieldValue<string>(target, "m_EffectName");
            var newEffectValue = InspectorUtility.DrawTypePopup(typeof(Effect), effectValue, "Effect", true);
            if (effectValue != newEffectValue)
            {
                InspectorUtility.SetFieldValue(target, "m_EffectName", newEffectValue);
                EditorUtility.SetDirty(parent);
            }

            InspectorUtility.DrawField(target, "m_EffectIndex");
            InspectorUtility.DrawField(target, "m_StopEffect");
        }
    }
}