/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Collections.Generic;
using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.StateSystem;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    ///     The AnimatorMonitor acts as a bridge for the parameters on the Animator component.
    ///     If an Animator component is not attached to the character (such as for first person view) then the updates will be
    ///     forwarded to the item's Animator.
    /// </summary>
    public class AnimatorMonitor : StateBehavior
    {
        private static readonly int s_HorizontalMovementHash = Animator.StringToHash("HorizontalMovement");
        private static readonly int s_ForwardMovementHash = Animator.StringToHash("ForwardMovement");
        private static readonly int s_PitchHash = Animator.StringToHash("Pitch");
        private static readonly int s_YawHash = Animator.StringToHash("Yaw");
        private static readonly int s_SpeedHash = Animator.StringToHash("Speed");
        private static readonly int s_HeightHash = Animator.StringToHash("Height");
        private static readonly int s_MovingHash = Animator.StringToHash("Moving");
        private static readonly int s_AimingHash = Animator.StringToHash("Aiming");
        private static readonly int s_MovementSetIDHash = Animator.StringToHash("MovementSetID");
        private static readonly int s_AbilityIndexHash = Animator.StringToHash("AbilityIndex");
        private static readonly int s_AbilityChangeHash = Animator.StringToHash("AbilityChange");
        private static readonly int s_AbilityIntDataHash = Animator.StringToHash("AbilityIntData");
        private static readonly int s_AbilityFloatDataHash = Animator.StringToHash("AbilityFloatData");
        private static int[] s_ItemSlotIDHash;
        private static int[] s_ItemSlotStateIndexHash;
        private static int[] s_ItemSlotStateIndexChangeHash;
        private static int[] s_ItemSlotSubstateIndexHash;

        [Tooltip(
            "The damping time for the Horizontal Movement parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField]
        protected float m_HorizontalMovementDampingTime = 0.1f;

        [Tooltip(
            "The damping time for the Forward Movement parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField]
        protected float m_ForwardMovementDampingTime = 0.1f;

        [Tooltip(
            "The damping time for the Pitch parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField]
        protected float m_PitchDampingTime = 0.1f;

        [Tooltip(
            "The damping time for the Yaw parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField]
        protected float m_YawDampingTime = 0.1f;

        [Tooltip("The runtime speed of the Animator.")] [SerializeField]
        protected float m_AnimatorSpeed = 1;

        protected Animator m_Animator;
        private bool m_EquippedItemsDirty;

        protected GameObject m_GameObject;

        private HashSet<int> m_ItemParameterExists;
        protected Transform m_Transform;

#if UNITY_EDITOR
        public bool LogEvents => m_LogEvents;
#endif
        public float HorizontalMovementDampingTime
        {
            get => m_HorizontalMovementDampingTime;
            set => m_HorizontalMovementDampingTime = value;
        }

        public float ForwardMovementDampingTime
        {
            get => m_ForwardMovementDampingTime;
            set => m_ForwardMovementDampingTime = value;
        }

        public float PitchDampingTime
        {
            get => m_PitchDampingTime;
            set => m_PitchDampingTime = value;
        }

        public float YawDampingTime
        {
            get => m_YawDampingTime;
            set => m_YawDampingTime = value;
        }

        public float AnimatorSpeed
        {
            get => m_AnimatorSpeed;
            set
            {
                m_AnimatorSpeed = value;
                if (m_Animator != null) m_Animator.speed = m_AnimatorSpeed;
            }
        }

        public bool AnimatorEnabled => m_Animator != null && m_Animator.enabled;
        public float HorizontalMovement { get; private set; }

        public float ForwardMovement { get; private set; }

        public float Pitch { get; private set; }

        public float Yaw { get; private set; }

        public float Speed { get; private set; }

        public int Height { get; private set; }

        public bool Moving { get; private set; }

        public bool Aiming { get; private set; }

        public int MovementSetID { get; private set; }

        public int AbilityIndex { get; private set; }

        public bool AbilityChange => m_Animator != null && m_Animator.GetBool(s_AbilityChangeHash);
        public int AbilityIntData { get; private set; }

        public float AbilityFloatData { get; private set; }

        public bool HasItemParameters { get; private set; }

        public int ParameterSlotCount => ItemSlotID.Length;
        public int[] ItemSlotID { get; private set; }

        public int[] ItemSlotStateIndex { get; private set; }

        public int[] ItemSlotSubstateIndex { get; private set; }

        [Snapshot] protected Item[] EquippedItems { get; set; }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Transform = transform;
            m_Animator = m_GameObject.GetCachedComponent<Animator>();

#if UNITY_EDITOR
            // If the animator doesn't have the required parameters then it's not a valid animator.
            if (m_Animator != null)
                if (!HasParameter(s_HorizontalMovementHash) || !HasParameter(s_ForwardMovementHash) ||
                    !HasParameter(s_AbilityChangeHash))
                {
                    Debug.LogError(
                        $"Error: The animator {m_Animator.name} is not designed to work with the Ultimate Character Controller. " +
                        "Ensure the animator has all of the required parameters.");
                    return;
                }
#endif
            InitializeItemParameters();

            EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnAbilityWillEquipItem", OnWillEquipItem);
            EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnInventoryRemoveItem", OnUnequipItem);
            if (m_Animator != null)
            {
                m_Animator.speed = m_AnimatorSpeed;
                EventHandler.RegisterEvent(m_GameObject, "OnCharacterSnapAnimator", SnapAnimator);
                EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterChangeUpdateLocation",
                    OnChangeUpdateLocation);
                EventHandler.RegisterEvent<float>(m_GameObject, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            }
        }

        /// <summary>
        ///     Prepares the Animator parameters for start.
        /// </summary>
        protected virtual void Start()
        {
            SnapAnimator();

            if (m_Animator != null)
            {
                var characterLocomotion = m_GameObject.GetCachedComponent<UltimateCharacterLocomotion>();
                OnChangeUpdateLocation(characterLocomotion.UpdateLocation ==
                                       KinematicObjectManager.UpdateLocation.FixedUpdate);
                OnChangeTimeScale(characterLocomotion.TimeScale);
            }
        }

        /// <summary>
        ///     The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnAbilityWillEquipItem", OnWillEquipItem);
            EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnInventoryRemoveItem", OnUnequipItem);
            if (m_Animator != null)
            {
                EventHandler.UnregisterEvent(m_GameObject, "OnCharacterSnapAnimator", SnapAnimator);
                EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterChangeUpdateLocation",
                    OnChangeUpdateLocation);
                EventHandler.UnregisterEvent<float>(m_GameObject, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            }
        }

        /// <summary>
        ///     Does the animator have the specified parameter?
        /// </summary>
        /// <param name="parameterHash">The hash of the parameter.</param>
        /// <returns>True if the animator has the specified parameter.</returns>
        private bool HasParameter(int parameterHash)
        {
            for (var i = 0; i < m_Animator.parameterCount; ++i)
                if (m_Animator.parameters[i].nameHash == parameterHash)
                    return true;
            return false;
        }

        /// <summary>
        ///     Initializes the item parameters.
        /// </summary>
        public void InitializeItemParameters()
        {
            if (HasItemParameters) return;
            // The Animator Controller may not have the item parameters if the character can never equip an item.
            HasItemParameters = m_GameObject.GetComponentInChildren<ItemPlacement>() != null;

            var inventory = m_GameObject.GetComponent<InventoryBase>();
            if (inventory == null) return;

            var slotCount = inventory.SlotCount;
            EquippedItems = new Item[slotCount];

            ItemSlotID = new int[slotCount];
            ItemSlotStateIndex = new int[slotCount];
            ItemSlotSubstateIndex = new int[slotCount];
            m_ItemParameterExists = new HashSet<int>();

            if (m_Animator == null) return;

            if (s_ItemSlotIDHash == null || s_ItemSlotIDHash.Length < slotCount)
            {
                s_ItemSlotIDHash = new int[slotCount];
                s_ItemSlotStateIndexHash = new int[slotCount];
                s_ItemSlotStateIndexChangeHash = new int[slotCount];
                s_ItemSlotSubstateIndexHash = new int[slotCount];
            }

            for (var i = 0; i < slotCount; ++i)
            {
                // Animators do not need to contain every slot index.
                var slotIDHash = Animator.StringToHash(string.Format("Slot{0}ItemID", i));
                if (!HasParameter(slotIDHash)) continue;
                m_ItemParameterExists.Add(i);

                if (s_ItemSlotIDHash[i] == 0)
                {
                    s_ItemSlotIDHash[i] = slotIDHash;
                    s_ItemSlotStateIndexHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemStateIndex", i));
                    s_ItemSlotStateIndexChangeHash[i] =
                        Animator.StringToHash(string.Format("Slot{0}ItemStateIndexChange", i));
                    s_ItemSlotSubstateIndexHash[i] =
                        Animator.StringToHash(string.Format("Slot{0}ItemSubstateIndex", i));
                }
            }
        }

        /// <summary>
        ///     Snaps the animator to the default values.
        /// </summary>
        protected virtual void SnapAnimator()
        {
            // A first person view may not use an Animator.
            if (m_Animator != null)
            {
                // The values should be reset enabled so the animator will snap to the correct animation.
                m_Animator.SetFloat(s_HorizontalMovementHash, HorizontalMovement, 0, 0);
                m_Animator.SetFloat(s_ForwardMovementHash, ForwardMovement, 0, 0);
                m_Animator.SetFloat(s_PitchHash, Pitch, 0, 0);
                m_Animator.SetFloat(s_YawHash, Yaw, 0, 0);
                m_Animator.SetFloat(s_SpeedHash, Speed, 0, 0);
                m_Animator.SetFloat(s_HeightHash, Height, 0, 0);
                m_Animator.SetBool(s_MovingHash, Moving);
                m_Animator.SetBool(s_AimingHash, Aiming);
                m_Animator.SetInteger(s_MovementSetIDHash, MovementSetID);
                m_Animator.SetInteger(s_AbilityIndexHash, AbilityIndex);
                m_Animator.SetTrigger(s_AbilityChangeHash);
                m_Animator.SetInteger(s_AbilityIntDataHash, AbilityIntData);
                m_Animator.SetFloat(s_AbilityFloatDataHash, AbilityFloatData, 0, 0);

                if (HasItemParameters)
                {
                    UpdateItemIDParameters();
                    for (var i = 0; i < EquippedItems.Length; ++i)
                    {
                        if (!m_ItemParameterExists.Contains(i)) continue;
                        m_Animator.SetInteger(s_ItemSlotIDHash[i], ItemSlotID[i]);
                        m_Animator.SetTrigger(s_ItemSlotStateIndexChangeHash[i]);
                        m_Animator.SetInteger(s_ItemSlotStateIndexHash[i], ItemSlotStateIndex[i]);
                        m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[i], ItemSlotSubstateIndex[i]);
                    }
                }

                EventHandler.ExecuteEvent(m_GameObject, "OnAnimatorWillSnap");

                // Root motion should not move the character when snapping.
                var position = m_Transform.position;
                var rotation = m_Transform.rotation;

                // Update 0 will force the changes.
                m_Animator.Update(0);
#if UNITY_EDITOR
                var count = 0;
#endif
                // Keep updating the Animator until it is no longer in a transition. This will snap the animator to the correct state immediately.
                while (IsInTrasition())
                {
#if UNITY_EDITOR
                    count++;
                    if (count > TimeUtility.TargetFramerate * 2)
                    {
                        Debug.LogError(
                            "Error: The animator is not leaving a transition. Ensure your Animator Controller does not have any infinite loops.");
                        return;
                    }
#endif
                    m_Animator.Update(Time.fixedDeltaTime);
                }

                m_Animator.Update(0);
                // The animator should be positioned at the start of each state.
                for (var i = 0; i < m_Animator.layerCount; ++i)
                    m_Animator.Play(m_Animator.GetCurrentAnimatorStateInfo(i).fullPathHash, i, 0);
                m_Animator.Update(Time.fixedDeltaTime);
                // Prevent the change parameters from staying triggered when the animator is on the idle state.
                SetAbilityChangeParameter(false);

                m_Transform.SetPositionAndRotation(position, rotation);
            }

            // The item animators should also snap.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                {
                    SetItemStateIndexChangeParameter(i, false);
                    if (EquippedItems[i] != null) EquippedItems[i].SnapAnimator();
                }

            EventHandler.ExecuteEvent(m_GameObject, "OnAnimatorSnapped");
        }

        /// <summary>
        ///     Is the Animator Controller currently in a transition?
        /// </summary>
        /// <returns>True if any layer within the Animator Controller is within a transition.</returns>
        private bool IsInTrasition()
        {
            for (var i = 0; i < m_Animator.layerCount; ++i)
                if (m_Animator.IsInTransition(i))
                    return true;
            return false;
        }

        /// <summary>
        ///     Returns true if the specified layer is in transition.
        /// </summary>
        /// <param name="layerIndex">The layer to determine if it is in transition.</param>
        /// <returns>True if the specified layer is in transition.</returns>
        public bool IsInTransition(int layerIndex)
        {
            if (m_Animator == null) return false;

            return m_Animator.IsInTransition(layerIndex);
        }

        /// <summary>
        ///     Sets the Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetHorizontalMovementParameter(float value, float timeScale)
        {
            SetHorizontalMovementParameter(value, timeScale, m_HorizontalMovementDampingTime);
        }

        /// <summary>
        ///     Sets the Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetHorizontalMovementParameter(float value, float timeScale, float dampingTime)
        {
            var change = HorizontalMovement != value;
            if (change)
            {
                if (m_Animator != null)
                {
                    m_Animator.SetFloat(s_HorizontalMovementHash, value, dampingTime,
                        TimeUtility.DeltaTimeScaled / timeScale);
                    HorizontalMovement = m_Animator.GetFloat(s_HorizontalMovementHash);
                    if (Mathf.Abs(HorizontalMovement) < 0.001f)
                    {
                        HorizontalMovement = 0;
                        m_Animator.SetFloat(s_HorizontalMovementHash, 0);
                    }
                }
                else
                {
                    HorizontalMovement = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetHorizontalMovementParameter(value, timeScale, dampingTime);

            return change;
        }

        /// <summary>
        ///     Sets the Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetForwardMovementParameter(float value, float timeScale)
        {
            SetForwardMovementParameter(value, timeScale, m_ForwardMovementDampingTime);
        }

        /// <summary>
        ///     Sets the Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetForwardMovementParameter(float value, float timeScale, float dampingTime)
        {
            var change = ForwardMovement != value;
            if (change)
            {
                if (m_Animator != null)
                {
                    m_Animator.SetFloat(s_ForwardMovementHash, value, dampingTime,
                        TimeUtility.DeltaTimeScaled / timeScale);
                    ForwardMovement = m_Animator.GetFloat(s_ForwardMovementHash);
                    if (Mathf.Abs(ForwardMovement) < 0.001f)
                    {
                        ForwardMovement = 0;
                        m_Animator.SetFloat(s_ForwardMovementHash, 0);
                    }
                }
                else
                {
                    ForwardMovement = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetForwardMovementParameter(value, timeScale, dampingTime);

            return change;
        }

        /// <summary>
        ///     Sets the Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetPitchParameter(float value, float timeScale)
        {
            SetPitchParameter(value, timeScale, m_PitchDampingTime);
        }

        /// <summary>
        ///     Sets the Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetPitchParameter(float value, float timeScale, float dampingTime)
        {
            var change = Pitch != value;
            if (change)
            {
                if (m_Animator != null)
                {
                    m_Animator.SetFloat(s_PitchHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    Pitch = m_Animator.GetFloat(s_PitchHash);
                    if (Mathf.Abs(Pitch) < 0.001f)
                    {
                        Pitch = 0;
                        m_Animator.SetFloat(s_PitchHash, 0);
                    }
                }
                else
                {
                    Pitch = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetPitchParameter(value, timeScale, dampingTime);

            return change;
        }

        /// <summary>
        ///     Sets the Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetYawParameter(float value, float timeScale)
        {
            SetYawParameter(value, timeScale, m_YawDampingTime);
        }

        /// <summary>
        ///     Sets the Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetYawParameter(float value, float timeScale, float dampingTime)
        {
            var change = Yaw != value;
            if (change)
            {
                if (m_Animator != null)
                {
                    m_Animator.SetFloat(s_YawHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    Yaw = m_Animator.GetFloat(s_YawHash);
                    if (Mathf.Abs(Yaw) < 0.001f)
                    {
                        Yaw = 0;
                        m_Animator.SetFloat(s_YawHash, 0);
                    }
                }
                else
                {
                    Yaw = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetYawParameter(value, timeScale, dampingTime);

            return change;
        }

        /// <summary>
        ///     Sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetSpeedParameter(float value, float timeScale)
        {
            SetSpeedParameter(value, timeScale, 0);
        }

        /// <summary>
        ///     Sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetSpeedParameter(float value, float timeScale, float dampingTime)
        {
            var change = Speed != value;
            if (change)
            {
                if (m_Animator != null)
                {
                    m_Animator.SetFloat(s_SpeedHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    Speed = m_Animator.GetFloat(s_SpeedHash);
                    if (Mathf.Abs(Speed) < 0.001f)
                    {
                        Speed = 0;
                        m_Animator.SetFloat(s_SpeedHash, 0);
                    }
                }
                else
                {
                    Speed = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetSpeedParameter(value, timeScale, dampingTime);

            return change;
        }

        /// <summary>
        ///     Sets the Height parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetHeightParameter(int value)
        {
            var change = Height != value;
            if (change)
            {
                if (m_Animator != null)
                {
                    m_Animator.SetFloat(s_HeightHash, value, 0, 0);
                    Height = (int)m_Animator.GetFloat(s_HeightHash);
                    if (Mathf.Abs(Height) < 0.001f)
                    {
                        Height = 0;
                        m_Animator.SetFloat(s_HeightHash, 0);
                    }
                }
                else
                {
                    Height = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetHeightParameter(value);

            return change;
        }

        /// <summary>
        ///     Sets the Moving parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetMovingParameter(bool value)
        {
            var change = Moving != value;
            if (change)
            {
                if (m_Animator != null) m_Animator.SetBool(s_MovingHash, value);
                Moving = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetMovingParameter(value);

            return change;
        }

        /// <summary>
        ///     Sets the Aiming parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAimingParameter(bool value)
        {
            var change = Aiming != value;
            if (change)
            {
                if (m_Animator != null) m_Animator.SetBool(s_AimingHash, value);
                Aiming = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetAimingParameter(value);
            return change;
        }

        /// <summary>
        ///     Sets the Movement Set ID parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetMovementSetIDParameter(int value)
        {
            var change = MovementSetID != value;
            if (change)
            {
                if (m_Animator != null) m_Animator.SetInteger(s_MovementSetIDHash, value);
                MovementSetID = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetMovementSetIDParameter(value);

            return change;
        }

        /// <summary>
        ///     Sets the Ability Index parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityIndexParameter(int value)
        {
            var change = AbilityIndex != value;
            if (change)
            {
#if UNITY_EDITOR
                if (m_LogAbilityParameterChanges)
                    Debug.Log($"{Time.frameCount} Changed AbilityIndex to {value} on GameObject {m_GameObject.name}.");
#endif
                if (m_Animator != null)
                {
                    m_Animator.SetInteger(s_AbilityIndexHash, value);
                    SetAbilityChangeParameter(true);
                }

                AbilityIndex = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetAbilityIndexParameter(value);

            return change;
        }

        /// <summary>
        ///     Sets the Ability Change parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityChangeParameter(bool value)
        {
            if (m_Animator != null && m_Animator.GetBool(s_AbilityChangeHash) != value)
            {
                if (value)
                    m_Animator.SetTrigger(s_AbilityChangeHash);
                else
                    m_Animator.ResetTrigger(s_AbilityChangeHash);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Sets the Int Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityIntDataParameter(int value)
        {
            var change = AbilityIntData != value;
            if (change)
            {
#if UNITY_EDITOR
                if (m_LogAbilityParameterChanges)
                    Debug.Log(
                        $"{Time.frameCount} Changed AbilityIntData to {value} on GameObject {m_GameObject.name}.");
#endif
                if (m_Animator != null) m_Animator.SetInteger(s_AbilityIntDataHash, value);
                AbilityIntData = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetAbilityIntDataParameter(value);

            return change;
        }

        /// <summary>
        ///     Sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetAbilityFloatDataParameter(float value, float timeScale)
        {
            SetAbilityFloatDataParameter(value, timeScale, 0);
        }

        /// <summary>
        ///     Sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityFloatDataParameter(float value, float timeScale, float dampingTime)
        {
            var change = AbilityFloatData != value;
            if (change)
            {
                if (m_Animator != null)
                {
                    m_Animator.SetFloat(s_AbilityFloatDataHash, value, dampingTime,
                        TimeUtility.DeltaTimeScaled / timeScale);
                    AbilityFloatData = m_Animator.GetFloat(s_AbilityFloatDataHash);
                }
                else
                {
                    AbilityFloatData = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetAbilityFloatDataParameter(value, timeScale, dampingTime);

            return change;
        }

        /// <summary>
        ///     Sets the Item ID parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public virtual bool SetItemIDParameter(int slotID, int value)
        {
            var change = ItemSlotID[slotID] != value;
            if (change)
            {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges)
                    Debug.Log(
                        $"{Time.frameCount} Changed Slot{slotID}ItemID to {value} on GameObject {m_GameObject.name}.");
#endif
                if (m_Animator != null && m_ItemParameterExists.Contains(slotID))
                {
                    m_Animator.SetInteger(s_ItemSlotIDHash[slotID], value);
                    // Even though no state index was changed the trigger should be set to true so the animator can transition to the new item id.
                    SetItemStateIndexChangeParameter(slotID, value != 0);
                }

                ItemSlotID[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetItemIDParameter(slotID, value);

            return change;
        }

        /// <summary>
        ///     Sets the Primary Item State Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemStateIndexParameter(int slotID, int value)
        {
            var change = ItemSlotStateIndex[slotID] != value;
            if (change)
            {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges)
                    Debug.Log(
                        $"{Time.frameCount} Changed Slot{slotID}ItemStateIndex to {value} on GameObject {m_GameObject.name}.");
#endif
                if (m_Animator != null && m_ItemParameterExists.Contains(slotID))
                {
                    m_Animator.SetInteger(s_ItemSlotStateIndexHash[slotID], value);
                    SetItemStateIndexChangeParameter(slotID, value != 0);
                }

                ItemSlotStateIndex[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetItemStateIndexParameter(slotID, value);

            return change;
        }

        /// <summary>
        ///     Sets the Item State Index Change parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot of that item that should be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemStateIndexChangeParameter(int slotID, bool value)
        {
            if (!m_ItemParameterExists.Contains(slotID)) return false;

            if (m_Animator != null && m_Animator.GetBool(s_ItemSlotStateIndexChangeHash[slotID]) != value)
            {
                if (value)
                    m_Animator.SetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                else
                    m_Animator.ResetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Sets the Item Substate Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemSubstateIndexParameter(int slotID, int value)
        {
            var change = ItemSlotSubstateIndex[slotID] != value;
            if (change)
            {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges)
                    Debug.Log(
                        $"{Time.frameCount} Changed Slot{slotID}ItemSubstateIndex to {value} on GameObject {m_GameObject.name}.");
#endif
                if (m_Animator != null && m_ItemParameterExists.Contains(slotID))
                    m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[slotID], value);
                ItemSlotSubstateIndex[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (EquippedItems != null)
                for (var i = 0; i < EquippedItems.Length; ++i)
                    if (EquippedItems[i] != null)
                        EquippedItems[i].SetItemSubstateIndexParameter(slotID, value);

            return change;
        }

        /// <summary>
        ///     Executes an event on the EventHandler.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public virtual void ExecuteEvent(string eventName)
        {
#if UNITY_EDITOR
            if (m_LogEvents) Debug.Log($"{Time.frameCount} Execute {eventName} on GameObject {m_GameObject.name}.");
#endif
            EventHandler.ExecuteEvent(m_GameObject, eventName);
        }

        /// <summary>
        ///     The specified item will be equipped.
        /// </summary>
        /// <param name="item">The item that will be equipped.</param>
        /// <param name="slotID">The slot that the item will occupy.</param>
        private void OnWillEquipItem(Item item, int slotID)
        {
            EquippedItems[slotID] = item;
            m_EquippedItemsDirty = true;
        }

        /// <summary>
        ///     An item has been unequipped.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (item != EquippedItems[slotID]) return;

            EquippedItems[slotID] = null;
            m_EquippedItemsDirty = true;
        }

        /// <summary>
        ///     Updates the ItemID and MovementSetID parameters to the equipped items.
        /// </summary>
        public void UpdateItemIDParameters()
        {
            if (m_EquippedItemsDirty)
            {
                var movementSetID = 0;
                for (var i = 0; i < EquippedItems.Length; ++i)
                {
                    var itemID = 0;
                    if (EquippedItems[i] != null)
                    {
                        if (EquippedItems[i].DominantItem) movementSetID = EquippedItems[i].AnimatorMovementSetID;
                        itemID = EquippedItems[i].AnimatorItemID;
                    }

                    SetItemIDParameter(i, itemID);
                }

                SetMovementSetIDParameter(movementSetID);
                m_EquippedItemsDirty = false;
            }
        }

        /// <summary>
        ///     The character has changed between Update and FixedUpdate location.
        /// </summary>
        /// <param name="fixedUpdate">Should the Animator update within the FixedUpdate loop?</param>
        private void OnChangeUpdateLocation(bool fixedUpdate)
        {
            m_Animator.updateMode = fixedUpdate ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;
        }

        /// <summary>
        ///     The character's local timescale has changed.
        /// </summary>
        /// <param name="timeScale">The new timescale.</param>
        private void OnChangeTimeScale(float timeScale)
        {
            m_Animator.speed = timeScale;
        }

        /// <summary>
        ///     Enables or disables the Animator.
        /// </summary>
        /// <param name="enable">Should the animator be enabled?</param>
        public void EnableAnimator(bool enable)
        {
            m_Animator.enabled = enable;
        }

        /// <summary>
        ///     Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_ItemSlotIDHash = null;
            s_ItemSlotStateIndexHash = null;
            s_ItemSlotStateIndexChangeHash = null;
            s_ItemSlotSubstateIndexHash = null;
        }
#if UNITY_EDITOR
        [Tooltip("Should the Animator log any changes to the item parameters?")] [SerializeField]
        protected bool m_LogAbilityParameterChanges;

        [Tooltip("Should the Animator log any changes to the item parameters?")] [SerializeField]
        protected bool m_LogItemParameterChanges;

        [Tooltip("Should the Animator log any events that it sends?")] [SerializeField]
        protected bool m_LogEvents;
#endif
    }
}