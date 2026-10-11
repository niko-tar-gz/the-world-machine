using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.PUN;

namespace TheWorldMachine.impl.Patches
{
    [HarmonyPatch(typeof(PhotonNetwork), "PunVersion")]
    static class PUNVersionPatch
    {
        static bool Prefix(ref string __result)
        {
            __result = "2.40";
            return false;
        }
    }
}
