/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Character;
using UnityEngine;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    ///     The FullScreenItemUIMonitor will show the full screen item UI when the OnItemShowFullScreenUI event is triggered.
    /// </summary>
    public class FullScreenItemUIMonitor : CharacterMonitor
    {
        [Tooltip("Should the crosshairs be shown?")] [SerializeField]
        protected int m_ID;

        private UltimateCharacterLocomotion m_CharacterController;
        private bool m_FullScreenUIShown;
        private GameObject m_GameObject;

        public int ID
        {
            get => m_ID;
            set => m_ID = value;
        }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_GameObject.SetActive(false);
        }

        /// <summary>
        ///     Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null)
                EventHandler.UnregisterEvent<int, bool>(m_Character, "OnItemShowFullScreenUI", OnShowItemUI);

            base.OnAttachCharacter(character);

            if (m_Character == null) return;

            EventHandler.RegisterEvent<int, bool>(m_Character, "OnItemShowFullScreenUI", OnShowItemUI);
            m_CharacterController = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            gameObject.SetActive(CanShowUI());
        }

        /// <summary>
        ///     Shows or hides the full screen item UI.
        /// </summary>
        /// <param name="id">The ID of the UI that should be shown or hidden.</param>
        /// <param name="show">Should the UI be shown?</param>
        private void OnShowItemUI(int id, bool show)
        {
            if (id == -1 || id != m_ID) return;

            m_FullScreenUIShown = show;

            // Independent look movement types don't look in the direction of the camera so they shouldn't show the full screen UI.
            if (m_CharacterController.ActiveMovementType.UseIndependentLook(false)) show = false;
            m_GameObject.SetActive(m_ShowUI && show);
        }

        /// <summary>
        ///     Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() && m_FullScreenUIShown;
        }
    }
}