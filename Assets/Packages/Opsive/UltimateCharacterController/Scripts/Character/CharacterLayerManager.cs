/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Game;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    ///     Sets up custom layers for the character.
    /// </summary>
    public class CharacterLayerManager : StateBehavior
    {
        [Tooltip("Layer Mask that specifies the layer that the enemies use.")] [SerializeField]
        protected LayerMask m_EnemyLayers = 1 << LayerManager.Enemy;

        [Tooltip(
            "Layer Mask that specifies any layers that are invisible to the character (such as water or invisible planes placed on top of stairs). ")]
        [SerializeField]
        protected LayerMask m_InvisibleLayers = (1 << LayerManager.TransparentFX) | (1 << LayerManager.IgnoreRaycast) |
                                                (1 << LayerManager.UI) | (1 << LayerManager.VisualEffect) |
                                                (1 << LayerManager.Overlay) | (1 << LayerManager.SubCharacter);

        [Tooltip(
            "Layer mask that specifies any layers that represent a solid object (such as the ground or a moving platform).")]
        [SerializeField]
        protected LayerMask m_SolidObjectLayers = ~((1 << LayerManager.IgnoreRaycast) | (1 << LayerManager.Water) |
                                                    (1 << LayerManager.UI) | (1 << LayerManager.VisualEffect) |
                                                    (1 << LayerManager.Overlay) | (1 << LayerManager.SubCharacter));

        public LayerMask EnemyLayers
        {
            get => m_EnemyLayers;
            set => m_EnemyLayers = value;
        }

        public LayerMask InvisibleLayers
        {
            get => m_InvisibleLayers;
            set => m_InvisibleLayers = value;
        }

        public LayerMask SolidObjectLayers
        {
            get => m_SolidObjectLayers;
            set => m_SolidObjectLayers = value;
        }

        // Represents the mask that ignores any invisible objects.
        public int IgnoreInvisibleLayers => ~m_InvisibleLayers;

        // Represents the mask that ignores any invisible objects and the character.
        public int IgnoreInvisibleCharacterLayers => ~(m_InvisibleLayers | CharacterLayer);

        // Represents the mask that ignores any invisible objects and the character/water.
        public int IgnoreInvisibleCharacterWaterLayers =>
            ~(m_InvisibleLayers | CharacterLayer | (1 << LayerManager.Water));

        public LayerMask CharacterLayer { get; set; } = 1 << LayerManager.Character;

        /// <summary>
        ///     Setups the character layer.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CharacterLayer = 1 << gameObject.layer;

            LayerManager.Initialize();
        }
    }
}