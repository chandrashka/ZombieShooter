/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using UnityEngine;

namespace Opsive.UltimateCharacterController.SurfaceSystem
{
    /// <summary>
    ///     A ImpactEffect pairs the SurfaceImpact with a SurfaceEffect.
    /// </summary>
    [Serializable]
    public struct ImpactEffect
    {
        public SurfaceImpact SurfaceImpact => m_SurfaceImpact;
        public SurfaceEffect SurfaceEffect => m_SurfaceEffect;
#pragma warning disable 0649
        [Tooltip("The SurfaceImpact which triggers the SurfaceEffect.")] [SerializeField]
        private SurfaceImpact m_SurfaceImpact;

        [Tooltip("The SurfaceEffect to spawn when triggered by the SurfaceImpact.")] [SerializeField]
        private SurfaceEffect m_SurfaceEffect;
#pragma warning restore 0649
    }

    /// <summary>
    ///     Maps a texture to a set of UV coordinates.
    /// </summary>
    [Serializable]
    public struct UVTexture
    {
        [Tooltip("The texture to map the UV coordinates to.")] [SerializeField]
        private Texture m_Texture;

        [Tooltip("The UV coordinates of the texture.")] [SerializeField]
        private Rect m_UV;

        public Texture Texture
        {
            get => m_Texture;
            set => m_Texture = value;
        }

        public Rect UV
        {
            get => m_UV;
            set => m_UV = value;
        }
    }

    /// <summary>
    ///     Represets a default surface listed within the SurfaceManager.
    /// </summary>
    [Serializable]
    public struct ObjectSurface
    {
        [Tooltip("The type of surface represented.")] [SerializeField]
        private SurfaceType m_SurfaceType;

        [Tooltip("The textures which go along with the specified SurfaceType.")] [SerializeField]
        private UVTexture[] m_UVTextures;

        public SurfaceType SurfaceType
        {
            get => m_SurfaceType;
            set => m_SurfaceType = value;
        }

        public UVTexture[] UVTextures
        {
            get => m_UVTextures;
            set => m_UVTextures = value;
        }
    }
}