/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.Shared.Editor.Utility
{
    /// <summary>
    ///     Small ScriptableObject which shows the import window if it has not been shown.
    /// </summary>
    public class ImportStatus : ScriptableObject
    {
        [Tooltip("Has the Character Controller Update Project Settings window been shown?")] [SerializeField]
        protected bool m_CharacterProjectSettingsShown;

        public bool CharacterProjectSettingsShown
        {
            get => m_CharacterProjectSettingsShown;
            set => m_CharacterProjectSettingsShown = value;
        }
    }
}