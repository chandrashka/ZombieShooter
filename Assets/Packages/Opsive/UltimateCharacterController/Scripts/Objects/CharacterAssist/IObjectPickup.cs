/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    ///     Interface for any object that can be picked up.
    /// </summary>
    public interface IObjectPickup
    {
        /// <summary>
        ///     Picks up the object.
        /// </summary>
        /// <param name="target">The object doing the pickup.</param>
        void DoPickup(GameObject target);
    }
}