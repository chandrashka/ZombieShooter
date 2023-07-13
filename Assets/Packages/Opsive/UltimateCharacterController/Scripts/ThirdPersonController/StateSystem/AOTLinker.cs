/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using Opsive.Shared.StateSystem;
using Opsive.UltimateCharacterController.ThirdPersonController.Character;
using UnityEngine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.StateSystem
{
    // See Opsive.UltimateCharacterController.StateSystem.AOTLinker for an explanation of this class.
    public class AOTLinker : MonoBehaviour
    {
        public void Linker()
        {
#pragma warning disable 0219
#if THIRD_PERSON_CONTROLLER
            var objectDeathVisiblityGenericDelegate =
                new Preset.GenericDelegate<PerspectiveMonitor.ObjectDeathVisiblity>();
            var objectDeathVisiblityFuncDelegate =
                new Func<PerspectiveMonitor.ObjectDeathVisiblity>(() => { return 0; });
            var objectDeathVisiblityActionDelegate = new Action<PerspectiveMonitor.ObjectDeathVisiblity>(value => { });
#endif
#pragma warning restore 0219
        }
    }
}