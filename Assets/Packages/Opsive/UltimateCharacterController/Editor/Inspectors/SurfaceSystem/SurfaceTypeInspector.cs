﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Editor.Inspectors;
using Opsive.Shared.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.SurfaceSystem;
using UnityEditor;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.SurfaceSystem
{
    /// <summary>
    ///     Custom inspector for the SurfaceType component.
    /// </summary>
    [CustomEditor(typeof(SurfaceType))]
    public class SurfaceTypeInspector : InspectorBase
    {
        /// <summary>
        ///     Creates a new SurfaceType.
        /// </summary>
        [MenuItem("Assets/Create/Opsive/Ultimate Character Controller/Surface Type")]
        public static void CreateSurfaceType()
        {
            var path = EditorUtility.SaveFilePanel("Save Surface Type", InspectorUtility.GetSaveFilePath(),
                "SurfaceType.asset", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length)
            {
                var surfaceType = CreateInstance<SurfaceType>();

                // Save the surface type.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(surfaceType, path);
                AssetDatabase.ImportAsset(path);
            }
        }

        /// <summary>
        ///     Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropertyFromName("m_ImpactEffects"), true);
            if (EditorGUI.EndChangeCheck())
            {
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}