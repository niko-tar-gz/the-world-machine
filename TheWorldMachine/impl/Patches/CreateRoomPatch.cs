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
        "CreateRoom",
        typeof(string),
        typeof(RoomOptions),
        typeof(TypedLobby),
        typeof(string[])
    )]
    static class CreateRoomPatch
    {
        static void Prefix(ref RoomOptions roomOptions)
        {
            if (roomOptions == null)
                roomOptions = new RoomOptions();

            roomOptions.CustomRoomProperties = new Hashtable
            {
                { "platform", "OTHER" },
                { "queueName", "DEFAULT" },
                { "language", "English" },
                { "gameMode", "forest|DEFAULT|Casual" },
            };

            roomOptions.CustomRoomPropertiesForLobby = new[]
            {
                "platform",
                "queueName",
                "language",
                "gameMode",
            };

            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;
        }
    }
}
