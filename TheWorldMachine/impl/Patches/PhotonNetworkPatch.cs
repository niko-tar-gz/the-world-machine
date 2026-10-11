using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// i have zero idea how this patch works anymore but im out of options :p
// chatgpt to the rescue
namespace TheWorldMachine.impl.Patches
{
    [HarmonyPatch]
    public class PhotonNetworkPatch
    {
        private static bool isRefreshing = false;

        static IEnumerable<MethodBase> TargetMethods()
        {
            Debug.Log("doing loadbalancing shit");

            yield return AccessTools.Method(typeof(LoadBalancingClient), "ConnectUsingSettings");
            yield return AccessTools.Method(typeof(LoadBalancingClient), "ConnectToMasterServer");
            yield return AccessTools.Method(typeof(LoadBalancingClient), "ConnectToNameServer");
            yield return AccessTools.Method(typeof(LoadBalancingClient), "ConnectToRegionMaster");
        }

        public static bool Prefix(
            LoadBalancingClient __instance,
            MethodInfo __originalMethod,
            object[] __args,
            ref bool __result
        )
        {
            if (isRefreshing)
            {
                isRefreshing = false;
                return true;
            }

            if (
                __instance.AuthValues != null
                && __instance.AuthValues.AuthType == CustomAuthenticationType.Custom
            )
            {
                Debug.Log($"[Patch] Intercepting {__originalMethod.Name} to refresh Nonce...");

                __result = true;

                var msAuth = TheWorldMachine.MothershipAuthenticator.Instance;
                if (msAuth == null)
                {
                    Debug.LogError(
                        "[Patch] Authenticator instance is null! Allowing connection with old nonce."
                    );
                    return true;
                }

                msAuth.RefreshNonce(
                    (newNonce) =>
                    {
                        if (!string.IsNullOrEmpty(newNonce))
                        {
                            var authData =
                                __instance.AuthValues.AuthPostData as Dictionary<string, object>;

                            if (authData == null)
                                authData = new Dictionary<string, object>();

                            authData["Nonce"] = newNonce;

                            if (!string.IsNullOrEmpty(msAuth.CachedPlayFabTicket))
                                authData["Ticket"] = msAuth.CachedPlayFabTicket;

                            if (!string.IsNullOrEmpty(msAuth.CachedMothershipToken))
                                authData["MothershipToken"] = msAuth.CachedMothershipToken;

                            if (!string.IsNullOrEmpty(msAuth.CachedOculusId))
                                authData["OculusId"] = msAuth.CachedOculusId;

                            authData["AppId"] = "63FDD";
                            authData["Platform"] = "PC";
                            authData["AppVersion"] = "2026oldprependhorse.live1.1.1.128_2.40";
                            authData["MothershipEnvId"] = "7f3a99dd-5598-4725-98cf-6538d28feb9f";
                            authData["MothershipDeploymentId"] =
                                "9f6a6a0d-aeda-4896-9c29-5cadfb213d6b";

                            __instance.AuthValues.SetAuthPostData(authData);

                            Debug.Log(
                                $"[Patch] Re-calling {__originalMethod.Name} with Fresh Nonce..."
                            );

                            isRefreshing = true;
                            try
                            {
                                __originalMethod.Invoke(__instance, __args);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogError($"[Patch] Invoke Failed: {ex}");
                            }
                        }
                        else
                        {
                            Debug.LogError("[Patch] Failed to get nonce. Connection aborted.");
                        }
                    }
                );

                return false;
            }

            return true;
        }
    }
}
