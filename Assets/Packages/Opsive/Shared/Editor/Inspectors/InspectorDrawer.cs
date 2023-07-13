/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Object = UnityEngine.Object;

namespace Opsive.Shared.Editor.Inspectors
{
    /// <summary>
    ///     InspectorDrawers allow non-Unity Objects to draw custom objects to the editor inspector.
    /// </summary>
    public abstract class InspectorDrawer
    {
        /// <summary>
        ///     Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public abstract void OnInspectorGUI(object target, Object parent);
    }

    /// <summary>
    ///     Specifies the type of object the Inspector Drawer should belong to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class InspectorDrawerAttribute : Attribute
    {
        public InspectorDrawerAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}