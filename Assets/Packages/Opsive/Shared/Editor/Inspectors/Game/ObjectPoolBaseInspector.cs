/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Game;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using EditorUtility = Opsive.Shared.Editor.Utility.EditorUtility;

namespace Opsive.Shared.Editor.Inspectors.Game
{
    /// <summary>
    ///     Shows a custom inspector for the ObjectPool.
    /// </summary>
    [CustomEditor(typeof(ObjectPoolBase), true)]
    public class ObjectPoolBaseInspector : UnityEditor.Editor
    {
        private ReorderableList m_PreloadedPoolReorderableList;

        /// <summary>
        ///     Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            if (m_PreloadedPoolReorderableList == null)
            {
                var preloadedPrefabsProperty = serializedObject.FindProperty("m_PreloadedPrefabs");
                m_PreloadedPoolReorderableList = new ReorderableList(serializedObject, preloadedPrefabsProperty, true,
                    false, true, true);
                m_PreloadedPoolReorderableList.drawHeaderCallback = OnPreloadedPoolListDrawHeader;
                m_PreloadedPoolReorderableList.drawElementCallback = OnPreloadedPoolElementDraw;
            }

            m_PreloadedPoolReorderableList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        ///     Draws the header for the PreloadedPool list.
        /// </summary>
        private void OnPreloadedPoolListDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 12, rect.y, rect.width - 90, EditorGUIUtility.singleLineHeight),
                "Preloaded Prefab");
            EditorGUI.LabelField(new Rect(rect.x + (rect.width - 90), rect.y, 90, EditorGUIUtility.singleLineHeight),
                "Count");
        }

        /// <summary>
        ///     Draws the PreloadedPool ReordableList element.
        /// </summary>
        private void OnPreloadedPoolElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();

            var preloadedPrefab = m_PreloadedPoolReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var prefab = preloadedPrefab.FindPropertyRelative("m_Prefab");
            var count = preloadedPrefab.FindPropertyRelative("m_Count");
            EditorGUI.ObjectField(new Rect(rect.x, rect.y + 1, rect.width - 90, EditorGUIUtility.singleLineHeight),
                prefab, new GUIContent());
            count.intValue =
                EditorGUI.IntField(
                    new Rect(rect.x + (rect.width - 90), rect.y + 1, 90, EditorGUIUtility.singleLineHeight),
                    count.intValue);

            if (EditorGUI.EndChangeCheck())
            {
                var pooledSerializedObject = m_PreloadedPoolReorderableList.serializedProperty.serializedObject;
                EditorUtility.RecordUndoDirtyObject(pooledSerializedObject.targetObject, "Change Value");
                pooledSerializedObject.ApplyModifiedProperties();
            }
        }
    }
}