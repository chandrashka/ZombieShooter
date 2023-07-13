﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Editor.Inspectors;
using Opsive.Shared.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Camera
{
    /// <summary>
    ///     Draws a custom inspector for the base Ability type.
    /// </summary>
    [InspectorDrawer(typeof(ViewType))]
    public class ViewTypeInspectorDrawer : InspectorDrawer
    {
        /// <summary>
        ///     Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            ObjectInspector.DrawFields(target, true);
        }

        /// <summary>
        ///     The ability has been added to the camera. Perform any initialization.
        /// </summary>
        /// <param name="viewType">The view type that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public virtual void ViewTypeAdded(ViewType viewType, Object parent)
        {
        }

        /// <summary>
        ///     The view type has been removed from the camera. Perform any destruction.
        /// </summary>
        /// <param name="viewType">The view type that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public virtual void ViewTypeRemoved(ViewType viewType, Object parent)
        {
        }
    }
}