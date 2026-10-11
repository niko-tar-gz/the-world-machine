using HarmonyLib;
using PlayFab.ClientModels;
using UnityEngine;

namespace TheWorldMachine.impl.Patches
{
    [HarmonyPatch(typeof(GorillaComputer), "OnReturnCurrentVersion")]
    public class GorillaComputerPatch
    {
        public static bool Prefix(ExecuteCloudScriptResult result, GorillaComputer __instance)
        {
            Debug.Log("intercepted OnReturnCurrentVersion");

            try
            {
                Traverse.Create(__instance).Field("allowedToPlay").SetValue(true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"error setting shit: {ex}");
            }

            return false;
        }
    }
}
