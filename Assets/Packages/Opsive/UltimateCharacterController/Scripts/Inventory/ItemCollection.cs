/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    ///     The ItemCollection ScriptableObject is a container for the static item data.
    /// </summary>
    public class ItemCollection : ScriptableObject
    {
        [Tooltip("An array of all of the possible Categories.")] [SerializeField]
        protected Category[] m_Categories;

        [Tooltip("An array of all of the possible ItemTypes.")] [SerializeField]
        protected ItemType[] m_ItemTypes;

        public Category[] Categories
        {
            get => m_Categories;
            set => m_Categories = value;
        }

        public ItemType[] ItemTypes
        {
            get => m_ItemTypes;
            set => m_ItemTypes = value;
        }

        /// <summary>
        ///     Returns the category that has the specified ID.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <returns>The category that has the specified ID. Returns null if no categories are found.</returns>
        public Category GetCategory(uint id)
        {
            if (m_Categories == null) return null;

            for (var i = 0; i < m_Categories.Length; ++i)
            {
                if (m_Categories[i] == null)
                {
                    Debug.LogError(
                        $"The category at index {i} doesn't exist. Ensure the new categories have been created within the Item Type Manager.");
                    continue;
                }

                if (m_Categories[i].ID == id) return m_Categories[i];
            }

            return null;
        }
    }
}