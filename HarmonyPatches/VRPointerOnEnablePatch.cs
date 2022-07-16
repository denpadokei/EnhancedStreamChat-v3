using HarmonyLib;
using System;
using VRUIControls;

namespace EnhancedStreamChat.HarmonyPatches
{
    [HarmonyPatch(typeof(VRPointer), nameof(VRPointer.OnEnable))]
    public class VRPointerOnEnablePatch
    {
        public static void Postfix(VRPointer __instance)
        {
            OnEnabled?.Invoke(__instance);
        }

        public static event Action<VRPointer> OnEnabled;
    }
}
