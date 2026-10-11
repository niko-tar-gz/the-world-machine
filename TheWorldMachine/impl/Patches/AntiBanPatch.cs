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
    [HarmonyPatch(typeof(GorillaComputer))]
    [HarmonyPatch("BanMe", MethodType.Normal)]
    internal class AntiBanPatch
    {
        public static bool Prefix()
        {
            Debug.LogError("oops");

            return false;
        }
    }
}
