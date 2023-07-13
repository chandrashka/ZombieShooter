/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Items.Actions;
using UnityEditor;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    /// <summary>
    ///     Shows a custom inspector for the Flashlight component.
    /// </summary>
    [CustomEditor(typeof(Flashlight))]
    public class FlashlightInspector : UsableItemInspector
    {
        private Flashlight m_Flashlight;

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Flashlight = target as Flashlight;
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
                InspectorUtility.DrawAttributeModifier(m_AttributeManager, m_Flashlight.BatteryModifier,
                    "Battery Attribute");
            };

            return baseCallback;
        }
    }
}