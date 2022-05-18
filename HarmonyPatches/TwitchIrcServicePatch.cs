using EnhancedStreamChat.Interfaces;
using HarmonyLib;

namespace EnhancedStreamChat.HarmonyPatches
{
    [HarmonyPatch]
    public class TwitchIrcServicePatch
    {
        private static readonly LazyCopyHashSet<IIrcServiceDisconnectReceiver> s_reciver = new LazyCopyHashSet<IIrcServiceDisconnectReceiver>();
        public static ILazyCopyHashSet<IIrcServiceDisconnectReceiver> Recevires => s_reciver;

        [HarmonyPatch("CatCore.Services.Twitch.TwitchIrcService, CatCore", "DisconnectHappenedHandler")]
        [HarmonyPostfix]
        public static void DisconnectHappenedHandlerPostfix(object __instance)
        {
            Logger.Info("TwitchIrcService disconnect.");
            foreach (var receiver in s_reciver.items) {
                receiver?.OnDisconnect(__instance);
            }
        }

        public static void RegistReceiver(IIrcServiceDisconnectReceiver receiver)
        {
            Recevires.Add(receiver);
        }

        public static void UnRegistReceiver(IIrcServiceDisconnectReceiver receiver)
        {
            Recevires.Remove(receiver);
        }
    }
}
