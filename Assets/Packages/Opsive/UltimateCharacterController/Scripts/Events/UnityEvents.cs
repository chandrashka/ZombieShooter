/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Inventory;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.SurfaceSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Opsive.UltimateCharacterController.Events
{
    /// <summary>
    ///     (float) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityFloatEvent : UnityEvent<float>
    {
    }

    /// <summary>
    ///     (Transform) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityTransformEvent : UnityEvent<Transform>
    {
    }

    /// <summary>
    ///     (MovementType, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityMovementTypeBoolEvent : UnityEvent<MovementType, bool>
    {
    }

    /// <summary>
    ///     (Ability, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityAbilityBoolEvent : UnityEvent<Ability, bool>
    {
    }

    /// <summary>
    ///     (ItemAbility, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemAbilityBoolEvent : UnityEvent<ItemAbility, bool>
    {
    }

    /// <summary>
    ///     (Item) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemEvent : UnityEvent<Item>
    {
    }

    /// <summary>
    ///     (Item, int) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemIntEvent : UnityEvent<Item, int>
    {
    }

    /// <summary>
    ///     (IItemIdentifier, float) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemIdentifierFloatEvent : UnityEvent<IItemIdentifier, float>
    {
    }

    /// <summary>
    ///     (Item, bool, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemBoolBoolEvent : UnityEvent<Item, bool, bool>
    {
    }

    /// <summary>
    ///     (Item, float, bool, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemFloatBoolBoolEvent : UnityEvent<Item, float, bool, bool>
    {
    }

    /// <summary>
    ///     (Item, RaycastHit, SurfaceImpact) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemRaycastHitSurfaceImpactEvent : UnityEvent<Item, RaycastHit, SurfaceImpact>
    {
    }

    /// <summary>
    ///     (IItemIdentifier, float, bool, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityItemIdentifierFloatBoolBoolEvent : UnityEvent<IItemIdentifier, float, bool, bool>
    {
    }

    /// <summary>
    ///     (Vector3, Vector3, GameObject) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityVector3Vector3GameObjectEvent : UnityEvent<Vector3, Vector3, GameObject>
    {
    }

    /// <summary>
    ///     (float, Vector3, Vector3, GameObject) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityFloatVector3Vector3GameObjectEvent : UnityEvent<float, Vector3, Vector3, GameObject>
    {
    }

    /// <summary>
    ///     (ViewType, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityViewTypeBoolEvent : UnityEvent<ViewType, bool>
    {
    }
}