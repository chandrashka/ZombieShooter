/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    ///     Attribute which specifies the default input name for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DefaultInputName : Attribute
    {
        public DefaultInputName(string inputName)
        {
            InputName = inputName;
        }

        public DefaultInputName(string inputName, int index)
        {
            InputName = inputName;
            Index = index;
        }

        public string InputName { get; }

        public int Index { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default start type for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultStartType : Attribute
    {
        public DefaultStartType(Ability.AbilityStartType startType)
        {
            StartType = startType;
        }

        public Ability.AbilityStartType StartType { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default stop type for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultStopType : Attribute
    {
        public DefaultStopType(Ability.AbilityStopType stopType)
        {
            StopType = stopType;
        }

        public Ability.AbilityStopType StopType { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Ability Index for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultAbilityIndex : Attribute
    {
        public DefaultAbilityIndex(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Ability Int Data for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultAbilityIntData : Attribute
    {
        public DefaultAbilityIntData(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default item state index for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultItemStateIndex : Attribute
    {
        public DefaultItemStateIndex(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default State value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultState : Attribute
    {
        public DefaultState(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Allow Positional Input for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultAllowPositionalInput : Attribute
    {
        public DefaultAllowPositionalInput(bool value)
        {
            Value = value;
        }

        public bool Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Allow Rotational Input for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultAllowRotationalInput : Attribute
    {
        public DefaultAllowRotationalInput(bool value)
        {
            Value = value;
        }

        public bool Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Use Gravity value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultUseGravity : Attribute
    {
        public DefaultUseGravity(Ability.AbilityBoolOverride value)
        {
            Value = value;
        }

        public Ability.AbilityBoolOverride Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Use Root Motion Position value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultUseRootMotionPosition : Attribute
    {
        public DefaultUseRootMotionPosition(Ability.AbilityBoolOverride value)
        {
            Value = value;
        }

        public Ability.AbilityBoolOverride Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Use Root Motion Rotation value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultUseRootMotionRotation : Attribute
    {
        public DefaultUseRootMotionRotation(Ability.AbilityBoolOverride value)
        {
            Value = value;
        }

        public Ability.AbilityBoolOverride Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Detect Horizontal Collisions for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultDetectHorizontalCollisions : Attribute
    {
        public DefaultDetectHorizontalCollisions(Ability.AbilityBoolOverride value)
        {
            Value = value;
        }

        public Ability.AbilityBoolOverride Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Detect Vertical Collisions for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultDetectVerticalCollisions : Attribute
    {
        public DefaultDetectVerticalCollisions(Ability.AbilityBoolOverride value)
        {
            Value = value;
        }

        public Ability.AbilityBoolOverride Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Object Detection Mode for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultObjectDetection : Attribute
    {
        public DefaultObjectDetection(DetectObjectAbilityBase.ObjectDetectionMode value)
        {
            Value = value;
        }

        public DetectObjectAbilityBase.ObjectDetectionMode Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Use Look Direction for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultUseLookDirection : Attribute
    {
        public DefaultUseLookDirection(bool value)
        {
            Value = value;
        }

        public bool Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Cast Offset for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultCastOffset : Attribute
    {
        public DefaultCastOffset(float x, float y, float z)
        {
            Value = new Vector3(x, y, z);
        }

        public Vector3 Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Equipped Slots for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultEquippedSlots : Attribute
    {
        public DefaultEquippedSlots(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    /// <summary>
    ///     Attribute which specifies the default Reequip Slots for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultReequipSlots : Attribute
    {
        public DefaultReequipSlots(bool value)
        {
            Value = value;
        }

        public bool Value { get; }
    }
}