/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using System.Collections.Generic;
using Opsive.Shared.Inventory;
using Opsive.Shared.Utility;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    ///     A Category contains a grouping of ItemTypes.
    /// </summary>
    [Serializable]
    public class Category : ScriptableObject, IItemCategoryIdentifier
    {
        [Tooltip("The ID of the category.")] [SerializeField]
        protected uint m_ID;

        private Category[] m_Parents;

        public Category[] Parents
        {
            set => m_Parents = value;
        }

        public uint ID
        {
            get
            {
                if (RandomID.IsIDEmpty(m_ID)) m_ID = GenerateID();
                return m_ID;
            }
            set => m_ID = value;
        }

        /// <summary>
        ///     Returns a read only array of the direct parents of the current category.
        /// </summary>
        /// <returns>The direct parents of the current category.</returns>
        public IReadOnlyList<IItemCategoryIdentifier> GetDirectParents()
        {
            return m_Parents;
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return name;
        }

        /// <summary>
        ///     Returns a new ID for the category.
        /// </summary>
        /// <returns>The new cateogry ID.</returns>
        public static uint GenerateID()
        {
            uint id;
            // The category ID is stored as a uint. Inspector fields aren't able to cast to a uint so keep generating a new ID for as long as 
            // the value is greater than the max int value.
            do
            {
                id = RandomID.Generate();
            } while (id > int.MaxValue);

            return id;
        }
    }
}