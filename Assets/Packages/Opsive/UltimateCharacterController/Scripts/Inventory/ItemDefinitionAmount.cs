/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    ///     Specifies the amount of each ItemDefinitionBase that the character can pickup or is loaded with the default
    ///     inventory.
    /// </summary>
    [Serializable]
    public struct ItemDefinitionAmount
    {
        [Tooltip("The type of item.")]
        [FormerlySerializedAs("m_ItemType")]
        [FormerlySerializedAs("m_ItemDefinition")]
        [SerializeField]
        public ItemDefinitionBase ItemDefinition;

        [Tooltip("The number of ItemIdentifier units to pickup.")]
        [FormerlySerializedAs("m_Count")]
        [FormerlySerializedAs("m_Amount")]
        [SerializeField]
        public int Amount;

        private IItemIdentifier m_ItemIdentifier;

        /// <summary>
        ///     ItemDefinitionAmount constructor with two parameters.
        /// </summary>
        /// <param name="itemDefinition">The definition of item.</param>
        /// <param name="amount">The amount of ItemDefinitionBase.</param>
        public ItemDefinitionAmount(ItemDefinitionBase itemDefinition, int amount)
        {
            ItemDefinition = itemDefinition;
            Amount = amount;
            m_ItemIdentifier = null;
        }

        public IItemIdentifier ItemIdentifier
        {
            get
            {
                if (Application.isPlaying && m_ItemIdentifier == null)
                    m_ItemIdentifier = ItemDefinition.CreateItemIdentifier();
                return m_ItemIdentifier;
            }
        }
    }
}