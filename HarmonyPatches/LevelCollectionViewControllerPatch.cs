using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnhancedStreamChat.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController), "DidActivate")]
    internal class LevelCollectionViewControllerPatch
    {
        public static void Prefix(LevelCollectionViewController __instance, bool firstActivation)
        {
            if (firstActivation) {
                foreach (var canvas in __instance.GetComponentsInChildren<Canvas>()) {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 3;
                }
            }
        }
    }

    [HarmonyPatch(typeof(StandardLevelDetailViewController), "DidActivate")]
    internal class StandardLevelDetailViewControllerPatch
    {
        public static void Prefix(StandardLevelDetailViewController __instance, bool firstActivation)
        {
            if (firstActivation) {
                foreach (var canvas in __instance.GetComponentsInChildren<Canvas>()) {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 3;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LevelPackDetailViewController), "DidActivate")]
    internal class LevelPackDetailViewControllerPatch
    {
        public static void Prefix(LevelPackDetailViewController __instance, bool firstActivation)
        {
            if (firstActivation) {
                foreach (var canvas in __instance.GetComponentsInChildren<Canvas>()) {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 3;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SelectLevelCategoryViewController), "DidActivate")]
    internal class SelectLevelCategoryViewControllerPatch
    {
        public static void Prefix(SelectLevelCategoryViewController __instance, bool firstActivation)
        {
            if (firstActivation) {
                foreach (var canvas in __instance.GetComponentsInChildren<Canvas>()) {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 3;
                }
            }
        }
    }
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), "DidActivate")]
    internal class AnnotatedBeatmapLevelCollectionsViewControllerPatch
    {
        public static void Prefix(AnnotatedBeatmapLevelCollectionsViewController __instance, bool firstActivation)
        {
            if (firstActivation) {
                foreach (var canvas in __instance.GetComponentsInChildren<Canvas>()) {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 3;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LevelSearchViewController), "DidActivate")]
    internal class LevelSearchViewControllerPatch
    {
        public static void Prefix(LevelSearchViewController __instance, bool firstActivation)
        {
            if (firstActivation) {
                foreach (var canvas in __instance.GetComponentsInChildren<Canvas>()) {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 3;
                }
            }
        }
    }
}
