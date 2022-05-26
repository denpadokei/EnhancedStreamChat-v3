using EnhancedStreamChat.Interfaces;
using HarmonyLib;

namespace EnhancedStreamChat.HarmonyPatches
{
    [HarmonyPatch]
    public class TwitchIrcServicePatch
    {
        private static readonly LazyCopyHashSet<IIrcServiceDisconnectReceiver> s_ircreciver = new LazyCopyHashSet<IIrcServiceDisconnectReceiver>();
        private static readonly LazyCopyHashSet<IPubSubServiceDisconnectReceiver> s_pubsubreciver = new LazyCopyHashSet<IPubSubServiceDisconnectReceiver>();
        public static ILazyCopyHashSet<IIrcServiceDisconnectReceiver> IrcRecevires => s_ircreciver;
        public static ILazyCopyHashSet<IPubSubServiceDisconnectReceiver> PubSubRecevires => s_pubsubreciver;

        [HarmonyPatch("CatCore.Services.Twitch.TwitchIrcService, CatCore", "DisconnectHappenedHandler")]
        [HarmonyPostfix]
        public static void DisconnectHappenedHandlerPostfix(object __instance)
        {
            Logger.Info("TwitchIrcService disconnect.");
            //foreach (var receiver in s_ircreciver.items) {
            //    receiver?.OnIrcDisconnect(__instance);
            //}
        }

        [HarmonyPatch("CatCore.Services.Twitch.TwitchPubSubServiceExperimentalAgent, CatCore", "DisconnectHappenedHandler")]
        [HarmonyPostfix]
        public static void PubsubDisconnectHappenedHandlerPostfix(object __instance)
        {
            Logger.Info("TwitchPubsubService disconnect.");
            //foreach (var reciver in s_pubsubreciver.items) {
            //    reciver?.OnPubsubDisconnect(__instance);
            //}
        }

        public static void RegistIrcReceiver(IIrcServiceDisconnectReceiver receiver)
        {
            IrcRecevires.Add(receiver);
        }

        public static void UnRegistIrcReceiver(IIrcServiceDisconnectReceiver receiver)
        {
            IrcRecevires.Remove(receiver);
        }

        public static void RegistPubSubReceiver(IPubSubServiceDisconnectReceiver receiver)
        {
            PubSubRecevires.Add(receiver);
        }

        public static void UnRegistPubSubReceiver(IPubSubServiceDisconnectReceiver receiver)
        {
            PubSubRecevires.Remove(receiver);
        }
    }
}
