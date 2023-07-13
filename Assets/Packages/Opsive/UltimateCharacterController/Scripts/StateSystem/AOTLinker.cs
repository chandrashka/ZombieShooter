/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.Audio;
using Opsive.Shared.Game;
using Opsive.Shared.Input;
using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Character.Abilities.Starters;
using Opsive.UltimateCharacterController.Character.Effects;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Objects;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.UI;
using Opsive.UltimateCharacterController.Utility;
using UnityEngine;
using Attribute = Opsive.UltimateCharacterController.Traits.Attribute;
using Object = UnityEngine.Object;

namespace Opsive.UltimateCharacterController.StateSystem
{
    // This class is required in order for the preset system to work with AOT platforms. The preset system uses reflection to generate the delegates
    // and reflection doesn't play well with AOT because the classes need to be defined ahead of time. Define the classes here so the compiler will
    // add in the correct type. This code is not actually used anywhere, it is purely for the compiler.
    public class AOTLinker : MonoBehaviour
    {
        public void Linker()
        {
#pragma warning disable 0219
            var intGenericDelegate = new Preset.GenericDelegate<int>();
            var intFuncDelegate = new Func<int>(() => { return 0; });
            var intActionDelegate = new Action<int>(value => { });
            var floatGenericDelegate = new Preset.GenericDelegate<float>();
            var floatFuncDelegate = new Func<float>(() => { return 0; });
            var floatActionDelegate = new Action<float>(value => { });
            var uintGenericDelegate = new Preset.GenericDelegate<uint>();
            var uintFuncDelegate = new Func<uint>(() => { return 0; });
            var uintActionDelegate = new Action<uint>(value => { });
            var doubleGenericDelegate = new Preset.GenericDelegate<double>();
            var doubleFuncDelegate = new Func<double>(() => { return 0; });
            var doubleActionDelegate = new Action<double>(value => { });
            var longGenericDelegate = new Preset.GenericDelegate<long>();
            var longFuncDelegate = new Func<long>(() => { return 0; });
            var longActionDelegate = new Action<long>(value => { });
            var boolGenericDelegate = new Preset.GenericDelegate<bool>();
            var boolFuncDelegate = new Func<bool>(() => { return true; });
            var boolActionDelegate = new Action<bool>(value => { });
            var stringGenericDelegate = new Preset.GenericDelegate<string>();
            var stringFuncDelegate = new Func<string>(() => { return string.Empty; });
            var stringActionDelegate = new Action<string>(value => { });
            var byteGenericDelegate = new Preset.GenericDelegate<byte>();
            var byteFuncDelegate = new Func<byte>(() => { return new byte(); });
            var byteActionDelegate = new Action<byte>(value => { });
            var vector2GenericDelegate = new Preset.GenericDelegate<Vector2>();
            var vector2FuncDelegate = new Func<Vector2>(() => { return Vector2.zero; });
            var vector2ActionDelegate = new Action<Vector2>(value => { });
            var vector3GenericDelegate = new Preset.GenericDelegate<Vector3>();
            var vector3FuncDelegate = new Func<Vector3>(() => { return Vector3.zero; });
            var vector3ActionDelegate = new Action<Vector3>(value => { });
            var vector4GenericDelegate = new Preset.GenericDelegate<Vector4>();
            var vector4FuncDelegate = new Func<Vector4>(() => { return Vector4.zero; });
            var vector4ActionDelegate = new Action<Vector4>(value => { });
            var quaternionGenericDelegate = new Preset.GenericDelegate<Quaternion>();
            var quaternionFuncDelegate = new Func<Quaternion>(() => { return Quaternion.identity; });
            var quaternionActionDelegate = new Action<Quaternion>(value => { });
            var colorGenericDelegate = new Preset.GenericDelegate<Color>();
            var colorFuncDelegate = new Func<Color>(() => { return Color.white; });
            var colorActionDelegate = new Action<Color>(value => { });
            var rectGenericDelegate = new Preset.GenericDelegate<Rect>();
            var rectFuncDelegate = new Func<Rect>(() => { return Rect.zero; });
            var rectActionDelegate = new Action<Rect>(value => { });
            var matrix4x4GenericDelegate = new Preset.GenericDelegate<Matrix4x4>();
            var matrix4x4FuncDelegate = new Func<Matrix4x4>(() => { return Matrix4x4.zero; });
            var matrix4x4ActionDelegate = new Action<Matrix4x4>(value => { });
            var animationCurveGenericDelegate = new Preset.GenericDelegate<AnimationCurve>();
            var animationCurveFuncDelegate = new Func<AnimationCurve>(() => { return new AnimationCurve(); });
            var animationCurveActionDelegate = new Action<AnimationCurve>(value => { });
            var layerMaskGenericDelegate = new Preset.GenericDelegate<LayerMask>();
            var layerMaskFuncDelegate = new Func<LayerMask>(() => { return new LayerMask(); });
            var layerMaskActionDelegate = new Action<LayerMask>(value => { });
            var raycastHitGenericDelegate = new Preset.GenericDelegate<RaycastHit>();
            var raycastHitFuncDelegate = new Func<RaycastHit>(() => { return new RaycastHit(); });
            var raycastHitActionDelegate = new Action<RaycastHit>(value => { });
            var humanBodyBonesGenericDelegate = new Preset.GenericDelegate<HumanBodyBones>();
            var humanBodyBonesFuncDelegate = new Func<HumanBodyBones>(() => { return 0; });
            var humanBodyBonesActionDelegate = new Action<HumanBodyBones>(value => { });
            var queryTriggerInteractionGenericDelegate = new Preset.GenericDelegate<QueryTriggerInteraction>();
            var queryTriggerInteractionFuncDelegate = new Func<QueryTriggerInteraction>(() => { return 0; });
            var queryTriggerInteractionActionDelegate = new Action<QueryTriggerInteraction>(value => { });
            var forceModeGenericDelegate = new Preset.GenericDelegate<ForceMode>();
            var forceModeFuncDelegate = new Func<ForceMode>(() => { return 0; });
            var forceModeActionDelegate = new Action<ForceMode>(value => { });
            var unityObjectGenericDelegate = new Preset.GenericDelegate<Object>();
            var unityObjectFuncDelegate = new Func<Object>(() => { return new Object(); });
            var unityObjectActionDelegate = new Action<Object>(value => { });
            var gameObjectGenericDelegate = new Preset.GenericDelegate<GameObject>();
            var gameObjectFuncDelegate = new Func<GameObject>(() => { return null; });
            var gameObjectActionDelegate = new Action<GameObject>(value => { });
            var transformGenericDelegate = new Preset.GenericDelegate<Transform>();
            var transformFuncDelegate = new Func<Transform>(() => { return null; });
            var transformActionDelegate = new Action<Transform>(value => { });
            var minMaxFloatGenericDelegate = new Preset.GenericDelegate<MinMaxFloat>();
            var minMaxFloatFuncDelegate = new Func<MinMaxFloat>(() => { return new MinMaxFloat(); });
            var minMaxFloatActionDelegate = new Action<MinMaxFloat>(value => { });
            var minMaxVector3GenericDelegate = new Preset.GenericDelegate<MinMaxVector3>();
            var minMaxVector3FuncDelegate = new Func<MinMaxVector3>(() => { return new MinMaxVector3(); });
            var minMaxVector3ActionDelegate = new Action<MinMaxVector3>(value => { });
            var lookVectorModeGenericDelegate = new Preset.GenericDelegate<PlayerInput.LookVectorMode>();
            var lookVectorModeFuncDelegate = new Func<PlayerInput.LookVectorMode>(() => { return 0; });
            var lookVectorModeActionDelegate = new Action<PlayerInput.LookVectorMode>(value => { });
            var preloadedPrefabGenericDelegate = new Preset.GenericDelegate<ObjectPoolBase.PreloadedPrefab>();
            var preloadedPrefabFuncDelegate = new Func<ObjectPoolBase.PreloadedPrefab>(() =>
            {
                return new ObjectPoolBase.PreloadedPrefab();
            });
            var preloadedPrefabActionDelegate = new Action<ObjectPoolBase.PreloadedPrefab>(value => { });
            var abilityStartTypeGenericDelegate = new Preset.GenericDelegate<Ability.AbilityStartType>();
            var abilityStartTypeFuncDelegate = new Func<Ability.AbilityStartType>(() => { return 0; });
            var abilityStartTypeActionDelegate = new Action<Ability.AbilityStartType>(value => { });
            var abilityStopTypeGenericDelegate = new Preset.GenericDelegate<Ability.AbilityStopType>();
            var abilityStopTypeFuncDelegate = new Func<Ability.AbilityStopType>(() => { return 0; });
            var abilityStopTypeActionDelegate = new Action<Ability.AbilityStopType>(value => { });
            var abilityBoolOverrideGenericDelegate = new Preset.GenericDelegate<Ability.AbilityBoolOverride>();
            var abilityBoolOverrideFuncDelegate = new Func<Ability.AbilityBoolOverride>(() => { return 0; });
            var abilityBoolOverrideActionDelegate = new Action<Ability.AbilityBoolOverride>(value => { });
            var comboInputElementGenericDelegate = new Preset.GenericDelegate<ComboTimeout.ComboInputElement>();
            var comboInputElementFuncDelegate = new Func<ComboTimeout.ComboInputElement>(() =>
            {
                return new ComboTimeout.ComboInputElement();
            });
            var comboInputElementActionDelegate = new Action<ComboTimeout.ComboInputElement>(value => { });
            var restrictionTypeGenericDelegate = new Preset.GenericDelegate<RestrictPosition.RestrictionType>();
            var restrictionTypeFuncDelegate = new Func<RestrictPosition.RestrictionType>(() => { return 0; });
            var restrictionTypeActionDelegate = new Action<RestrictPosition.RestrictionType>(value => { });
            var objectDetectionModeGenericDelegate =
                new Preset.GenericDelegate<DetectObjectAbilityBase.ObjectDetectionMode>();
            var objectDetectionModeTypeFuncDelegate =
                new Func<DetectObjectAbilityBase.ObjectDetectionMode>(() => { return 0; });
            var objectDetectionModeTypeActionDelegate =
                new Action<DetectObjectAbilityBase.ObjectDetectionMode>(value => { });
            var autoEquipTypeGenericDelegate = new Preset.GenericDelegate<EquipUnequip.AutoEquipType>();
            var autoEquipTypeFuncDelegate = new Func<EquipUnequip.AutoEquipType>(() => { return 0; });
            var autoEquipTypeActionDelegate = new Action<EquipUnequip.AutoEquipType>(value => { });
            var shakeTargetGenericDelegate = new Preset.GenericDelegate<Shake.ShakeTarget>();
            var shakeTargetFuncDelegate = new Func<Shake.ShakeTarget>(() => { return 0; });
            var shakeTargetActionDelegate = new Action<Shake.ShakeTarget>(value => { });
            var attributeAutoUpdateValueTypeGenericDelegate = new Preset.GenericDelegate<Attribute.AutoUpdateValue>();
            var attributeAutoUpdateFuncDelegate = new Func<Attribute.AutoUpdateValue>(() => { return 0; });
            var attributeAutoUpdateActionDelegate = new Action<Attribute.AutoUpdateValue>(value => { });
            var surfaceImpactGenericDelegate = new Preset.GenericDelegate<SurfaceImpact>();
            var surfaceImpactFuncDelegate = new Func<SurfaceImpact>(() => { return null; });
            var surfaceImpactActionDelegate = new Action<SurfaceImpact>(value => { });
            var uvTextureGenericDelegate = new Preset.GenericDelegate<UVTexture>();
            var uvTextureFuncDelegate = new Func<UVTexture>(() => { return new UVTexture(); });
            var uvTextureActionDelegate = new Action<UVTexture>(value => { });
            var objectSurfaceGenericDelegate = new Preset.GenericDelegate<ObjectSurface>();
            var objectSurfaceFuncDelegate = new Func<ObjectSurface>(() => { return new ObjectSurface(); });
            var objectSurfaceActionDelegate = new Action<ObjectSurface>(value => { });
            var objectSpawnInfoGenericDelegate = new Preset.GenericDelegate<ObjectSpawnInfo>();
            var objectSpawnInfoFuncDelegate = new Func<ObjectSpawnInfo>(() => { return null; });
            var objectSpawnInfoActionDelegate = new Action<ObjectSpawnInfo>(value => { });
            var animationEventTriggerGenericDelegate = new Preset.GenericDelegate<AnimationEventTrigger>();
            var animationEventTriggerFuncDelegate = new Func<AnimationEventTrigger>(() => { return null; });
            var animationEventTriggerActionDelegate = new Action<AnimationEventTrigger>(value => { });
            var characterFootEffectsFootGenericDelegate = new Preset.GenericDelegate<CharacterFootEffects.Foot>();
            var characterFootEffectsFootFuncDelegate = new Func<CharacterFootEffects.Foot>(() =>
            {
                return new CharacterFootEffects.Foot();
            });
            var characterFootEffectsFootActionDelegate = new Action<CharacterFootEffects.Foot>(value => { });
            var characterFootEffectsFootstepPlacementModeGenericDelegate =
                new Preset.GenericDelegate<CharacterFootEffects.FootstepPlacementMode>();
            var characterFootEffectsFootstepPlacementModeFuncDelegate =
                new Func<CharacterFootEffects.FootstepPlacementMode>(() => { return 0; });
            var characterFootEffectsFootstepPlacementModeActionDelegate =
                new Action<CharacterFootEffects.FootstepPlacementMode>(value => { });
            var spawnPointSpawnShapeGenericDelegate = new Preset.GenericDelegate<SpawnPoint.SpawnShape>();
            var spawnShapeFuncDelegate = new Func<SpawnPoint.SpawnShape>(() => { return 0; });
            var spawnShapeActionDelegate = new Action<SpawnPoint.SpawnShape>(value => { });
            var respawnerSpawnPositioningModeGenericDelegate =
                new Preset.GenericDelegate<Respawner.SpawnPositioningMode>();
            var respawnerSpawnPositioningFuncDelegate = new Func<Respawner.SpawnPositioningMode>(() => { return 0; });
            var respawnerSpawnPositioningActionDelegate = new Action<Respawner.SpawnPositioningMode>(value => { });
            var movingPlatformWaypointGenericDelegate = new Preset.GenericDelegate<MovingPlatform.Waypoint>();
            var movingPlatformWaypointFuncDelegate =
                new Func<MovingPlatform.Waypoint>(() => { return new MovingPlatform.Waypoint(); });
            var movingPlatformWaypointActionDelegate = new Action<MovingPlatform.Waypoint>(value => { });
            var movingPlatformPathMovementTypeGenericDelegate =
                new Preset.GenericDelegate<MovingPlatform.PathMovementType>();
            var movingPlatformPathMovementTypeFuncDelegate =
                new Func<MovingPlatform.PathMovementType>(() => { return 0; });
            var movingPlatformPathMovementTypeActionDelegate =
                new Action<MovingPlatform.PathMovementType>(value => { });
            var movingPlatformPathDirectionGenericDelegate = new Preset.GenericDelegate<MovingPlatform.PathDirection>();
            var movingPlatformPathDirectionFuncDelegate = new Func<MovingPlatform.PathDirection>(() => { return 0; });
            var movingPlatformPathDirectionActionDelegate = new Action<MovingPlatform.PathDirection>(value => { });
            var movingPlatformMovementInterpolationModeGenericDelegate =
                new Preset.GenericDelegate<MovingPlatform.MovementInterpolationMode>();
            var movingPlatformMovementInterpolationModeFuncDelegate =
                new Func<MovingPlatform.MovementInterpolationMode>(() => { return 0; });
            var movingPlatformMovementInterpolationModeActionDelegate =
                new Action<MovingPlatform.MovementInterpolationMode>(value => { });
            var movingPlatformRotateInterpolationModeGenericDelegate =
                new Preset.GenericDelegate<MovingPlatform.RotateInterpolationMode>();
            var movingPlatformRotateInterpolationModeFuncDelegate =
                new Func<MovingPlatform.RotateInterpolationMode>(() => { return 0; });
            var movingPlatformRotateInterpolationModeActionDelegate =
                new Action<MovingPlatform.RotateInterpolationMode>(value => { });
            var audioClipSetGenericDelegate = new Preset.GenericDelegate<AudioClipSet>();
            var audioClipSetFuncDelegate = new Func<AudioClipSet>(() => { return null; });
            var audioClipSetActionDelegate = new Action<AudioClipSet>(value => { });
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            var autoReloadTypeGenericDelegate = new Preset.GenericDelegate<Reload.AutoReloadType>();
            var autoReloadTypeFuncDelegate = new Func<Reload.AutoReloadType>(() => { return 0; });
            var autoReloadTypeModeActionDelegate = new Action<Reload.AutoReloadType>(value => { });
            var shootableWeaponFireModeGenericDelegate = new Preset.GenericDelegate<ShootableWeapon.FireMode>();
            var shootableWeaponFireModeFuncDelegate = new Func<ShootableWeapon.FireMode>(() => { return 0; });
            var shootableWeaponFireModeActionDelegate = new Action<ShootableWeapon.FireMode>(value => { });
            var shootableWeaponFireTypeGenericDelegate = new Preset.GenericDelegate<ShootableWeapon.FireType>();
            var shootableWeaponFireTypeFuncDelegate = new Func<ShootableWeapon.FireType>(() => { return 0; });
            var shootableWeaponFireTypeActionDelegate = new Action<ShootableWeapon.FireType>(value => { });
            var shootableWeaponProjectileVisibilityGenericDelegate =
                new Preset.GenericDelegate<ShootableWeapon.ProjectileVisiblityType>();
            var shootableWeaponProjectileVisiblityTypeFuncDelegate =
                new Func<ShootableWeapon.ProjectileVisiblityType>(() => { return 0; });
            var shootableWeaponProjectileVisiblityTypeActionDelegate =
                new Action<ShootableWeapon.ProjectileVisiblityType>(value => { });
            var shootableWeaponReloadClipTypeGenericDelegate =
                new Preset.GenericDelegate<ShootableWeapon.ReloadClipType>();
            var shootableWeaponReloadClipTypeFuncDelegate =
                new Func<ShootableWeapon.ReloadClipType>(() => { return 0; });
            var shootableWeaponReloadClipTypeActionDelegate = new Action<ShootableWeapon.ReloadClipType>(value => { });
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
            var meleeWeaponTrailVisibilityGenericDelegate =
                new Preset.GenericDelegate<MeleeWeapon.TrailVisibilityType>();
            var meleeWeaponTrailVisibilityFuncDelegate = new Func<MeleeWeapon.TrailVisibilityType>(() => { return 0; });
            var meleeWeaponTrailVisibilityActionDelegate = new Action<MeleeWeapon.TrailVisibilityType>(value => { });
#endif
            var magicItemCastDirectionGenericDelegate = new Preset.GenericDelegate<MagicItem.CastDirection>();
            var magicItemCastDirectionFuncDelegate = new Func<MagicItem.CastDirection>(() => { return 0; });
            var magicItemCastDirectionActionDelegate = new Action<MagicItem.CastDirection>(value => { });
            var magicItemCastUseTypeGenericDelegate = new Preset.GenericDelegate<MagicItem.CastUseType>();
            var magicItemCastUseTypeFuncDelegate = new Func<MagicItem.CastUseType>(() => { return 0; });
            var magicItemCastUseTypeActionDelegate = new Action<MagicItem.CastUseType>(value => { });
            var magicItemCastInterruptSourceGenericDelegate =
                new Preset.GenericDelegate<MagicItem.CastInterruptSource>();
            var magicItemCastInterruptSourceFuncDelegate = new Func<MagicItem.CastInterruptSource>(() => { return 0; });
            var magicItemCastInterruptSourceActionDelegate = new Action<MagicItem.CastInterruptSource>(value => { });
            var healthFlashMonitorFlashGenericDelegate = new Preset.GenericDelegate<HealthFlashMonitor.Flash>();
            var healthFlashMonitorFlashFuncDelegate = new Func<HealthFlashMonitor.Flash>(() =>
            {
                return new HealthFlashMonitor.Flash();
            });
            var healthFlashMonitorFlashActionDelegate = new Action<HealthFlashMonitor.Flash>(value => { });
#pragma warning restore 0219
        }
    }
}