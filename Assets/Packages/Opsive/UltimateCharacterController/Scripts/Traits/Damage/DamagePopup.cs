/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Opsive.UltimateCharacterController.Traits.Damage
{
    /// <summary>
    ///     Popup that appears when an object is damaged/healed.
    /// </summary>
    public class DamagePopup : DamagePopupBase
    {
        [Tooltip("The Unity Text.")] [SerializeField]
        protected Text m_Text;

        /// <summary>
        ///     Sets the popup text.
        /// </summary>
        /// <param name="text"></param>
        public override void SetText(string text)
        {
            m_Text.text = text;
        }
    }
}