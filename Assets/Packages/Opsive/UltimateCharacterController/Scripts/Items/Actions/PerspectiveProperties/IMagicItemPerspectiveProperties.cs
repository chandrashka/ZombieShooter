/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties
{
    /// <summary>
    ///     Interface for a magic item.
    /// </summary>
    public interface IMagicItemPerspectiveProperties
    {
        Transform OriginLocation { get; set; }
    }
}