using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using VRUIControls;

namespace EnhancedStreamChat.Models
{
    [HarmonyPatch(typeof(VRPointer), "Awake")]
    public class VRPointerHelper : MonoBehaviour
    {
        public static event Action<VRPointer, EventArgs> OnPointerEnable;

        protected void OnEnable()
        {
            _ = this.StartCoroutine(this.InvokeEvent());
        }

        private IEnumerator InvokeEvent()
        {
            var waitInit = new WaitWhile(() => !this);
            yield return waitInit;
            try {
                var vrpointer = this.GetComponent<VRPointer>();
                if (vrpointer != null) {
                    OnPointerEnable?.Invoke(vrpointer, EventArgs.Empty);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }

        public static void Postfix(VRPointer __instance)
        {
            _ = __instance.gameObject.AddComponent<VRPointerHelper>();
        }
    }
}
