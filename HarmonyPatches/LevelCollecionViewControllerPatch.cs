using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnhancedStreamChat.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController), "DidActivate")]
    internal class LevelCollecionViewControllerPatch
    {
        public static void Prefix(LevelCollectionViewController __instance, bool firstActivation)
        {
            if (firstActivation) {
                var canvas = __instance.GetComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 3;
            }
        }
    }
}
