using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.Internal;
using TheWorldMachine.impl;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorldMachine.Patches
{
    [HarmonyPatch(typeof(PlayFabAuthenticator))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class AuthPatch
    {
        public static bool Prefix(PlayFabAuthenticator __instance)
        {
            Debug.Log("shit started");
            MothershipAuthenticator silly = new GameObject(
                "mothership auth"
            ).AddComponent<MothershipAuthenticator>();
            __instance.StartCoroutine(silly.DoAuth(__instance));

            return false;
        }
    }
}
