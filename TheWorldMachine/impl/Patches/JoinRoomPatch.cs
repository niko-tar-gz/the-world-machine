using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace TheWorldMachine.impl.Patches
{
    [HarmonyPatch(typeof(PhotonNetwork))]
    [HarmonyPatch(
        "JoinRandomRoom",
        typeof(Hashtable),
        typeof(byte),
        typeof(MatchmakingMode),
        typeof(TypedLobby),
        typeof(string),
        typeof(string[])
    )]
    static class JoinRoomPatch
    {
        static void Prefix(ref Hashtable expectedCustomRoomProperties)
        {
            expectedCustomRoomProperties = new Hashtable
            {
                { "platform", "OTHER" },
                { "queueName", "DEFAULT" },
                { "language", "English" },
                { "gameMode", "forest|DEFAULT|Casual" },
            };
        }
    }
}
