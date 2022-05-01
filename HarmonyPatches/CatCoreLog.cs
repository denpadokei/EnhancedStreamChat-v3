using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnhancedStreamChat.HarmonyPatches
{
    [HarmonyPatch("CatCore.Services.Twitch.TwitchIrcService, CatCore", "MessageReceivedHandlerInternal")]
    internal class CatCoreLog
    {
        public static void Prefix(ref string rawMessage)
        {
#if DEBUG
            Logger.Debug(rawMessage);
#endif
        }
    }
}
