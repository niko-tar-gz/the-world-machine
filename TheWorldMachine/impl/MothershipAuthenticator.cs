using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using TheWorldMachine.impl.Types;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorldMachine
{
    public class MothershipAuthenticator : MonoBehaviour
    {
        public static MothershipAuthenticator Instance;

        // cached shit
        public string CachedPlayFabTicket;
        public string CachedMothershipToken;
        public string CachedOculusId;

        // mothership shit
        private const string AUTH_API_URL = "https://auth-prod.gtag-cf.com";
        private const string MS_ENV_ID = "7f3a99dd-5598-4725-98cf-6538d28feb9f";
        private const string MS_DEPLOY_ID = "9f6a6a0d-aeda-4896-9c29-5cadfb213d6b";

        // ver shit
        private const string APP_VERSION = "2026oldprependhorse.live1.1.1.128_2.40";

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }

        public IEnumerator DoAuth(PlayFabAuthenticator __instance)
        {
            Debug.Log("====== The World Machine ======");
            Debug.Log("starting auth");

            string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = Path.Combine(dllDir, "creds.txt");
            if (!File.Exists(filepath))
            {
                Debug.LogError("creds.txt missing!!! read the fucking readme!!!!");
                yield break;
            }

            string credsRaw = File.ReadAllText(filepath);
            var creds = JsonConvert.DeserializeObject<CredShit>(credsRaw);

            // mothership nonce
            UnityWebRequest nonceReq1 = new UnityWebRequest(
                "https://graph.oculus.com/user_nonce_generate",
                "POST"
            );
            nonceReq1.downloadHandler = new DownloadHandlerBuffer();
            nonceReq1.SetRequestHeader("Authorization", "Bearer " + creds.AccessToken);
            yield return nonceReq1.SendWebRequest();

            if (nonceReq1.isNetworkError || nonceReq1.isHttpError)
            {
                Debug.LogError("Nonce 1 Failed");
                yield break;
            }
            string nonce1 = JsonConvert
                .DeserializeObject<NonceResponse>(nonceReq1.downloadHandler.text)
                .nonce;

            // mothership auth
            string authJson = JsonConvert.SerializeObject(
                new { Nonce = nonce1, UserId = creds.UserId }
            );

            UnityWebRequest msReq = new UnityWebRequest(
                "https://aa-mothership.com/v1/client/player/auth/RIFT",
                "POST"
            );
            msReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(authJson));
            msReq.downloadHandler = new DownloadHandlerBuffer();
            msReq.SetRequestHeader("Content-Type", "application/json");
            msReq.SetRequestHeader("x-mothership-sdk-version", "v2024.12.23-preview1");
            msReq.SetRequestHeader("x-mothership-title-id", "f3e9fb19");
            msReq.SetRequestHeader("x-mothership-env-id", MS_ENV_ID);
            msReq.SetRequestHeader("x-mothership-deployment-id", MS_DEPLOY_ID);

            yield return msReq.SendWebRequest();

            if (msReq.isNetworkError || msReq.isHttpError)
            {
                Debug.LogError("mothership auth failed: " + msReq.downloadHandler.text);
                yield break;
            }

            var msRes = JsonConvert.DeserializeObject<LoginResponse>(msReq.downloadHandler.text);
            string mothershipToken = msRes.Token;
            string mothershipId = msRes.PlayerId;

            // playfab nonce
            UnityWebRequest nonceReq2 = new UnityWebRequest(
                "https://graph.oculus.com/user_nonce_generate",
                "POST"
            );
            nonceReq2.downloadHandler = new DownloadHandlerBuffer();
            nonceReq2.SetRequestHeader("Authorization", "Bearer " + creds.AccessToken);
            yield return nonceReq2.SendWebRequest();

            if (nonceReq2.isNetworkError || nonceReq2.isHttpError)
            {
                Debug.LogError("Nonce 2 Failed");
                yield break;
            }
            string nonce2 = JsonConvert
                .DeserializeObject<NonceResponse>(nonceReq2.downloadHandler.text)
                .nonce;

            // nonce to ticket
            Debug.Log("doing mothership exchange fuckery");

            var exchangeData = new
            {
                AppId = "f3e9fb19",
                Nonce = nonce2,
                OculusId = creds.UserId,
                Platform = "PC",
                AgeCategory = "Te",
                MothershipEnvId = MS_ENV_ID,
                MothershipDeploymentId = MS_DEPLOY_ID,
                MothershipToken = mothershipToken,
                MothershipId = mothershipId,
            };

            string exchangeJson = JsonConvert.SerializeObject(exchangeData);
            UnityWebRequest pfReq = new UnityWebRequest(
                AUTH_API_URL + "/api/PlayFabAuthentication",
                "POST"
            );
            pfReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(exchangeJson));
            pfReq.downloadHandler = new DownloadHandlerBuffer();
            pfReq.SetRequestHeader("Content-Type", "application/json");

            yield return pfReq.SendWebRequest();

            if (pfReq.isNetworkError || pfReq.isHttpError)
            {
                Debug.LogError("exchange shit failed " + pfReq.downloadHandler.text);
                yield break;
            }

            var pfRes = JsonConvert.DeserializeObject<PlayfabExchangeResponse>(
                pfReq.downloadHandler.text
            );
            string playFabSessionTicket = pfRes.SessionTicket;
            string playFabId = pfRes.PlayFabId;

            Debug.Log($"got playfab ticket!!!!!!! id: {playFabId}");

            // reflection buhhhghhhhhhgghhhhh
            try
            {
                var context = PlayFabSettings.staticPlayer;

                var ticketProp = context.GetType().GetProperty("ClientSessionTicket");
                if (ticketProp != null)
                {
                    ticketProp.SetValue(context, playFabSessionTicket, null);
                }
                else
                {
                    var ticketField = context
                        .GetType()
                        .GetField(
                            "ClientSessionTicket",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                        );
                    ticketField?.SetValue(context, playFabSessionTicket);
                }

                var idProp = context.GetType().GetProperty("PlayFabId");
                if (idProp != null)
                {
                    idProp.SetValue(context, playFabId, null);
                }
                else
                {
                    var idField = context
                        .GetType()
                        .GetField(
                            "PlayFabId",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                        );
                    idField?.SetValue(context, playFabId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"failed to do playfab reflection fuckery: {ex}");
            }

            // photon nonce
            Debug.Log("generating photon nonce");
            UnityWebRequest nonceReq3 = new UnityWebRequest(
                "https://graph.oculus.com/user_nonce_generate",
                "POST"
            );
            nonceReq3.downloadHandler = new DownloadHandlerBuffer();
            nonceReq3.SetRequestHeader("Authorization", "Bearer " + creds.AccessToken);
            yield return nonceReq3.SendWebRequest();

            if (nonceReq3.isNetworkError || nonceReq3.isHttpError)
            {
                Debug.LogError("photon nonce failed");
                yield break;
            }
            string nonce3 = JsonConvert
                .DeserializeObject<NonceResponse>(nonceReq3.downloadHandler.text)
                .nonce;

            // penis

            CachedPlayFabTicket = playFabSessionTicket;
            CachedMothershipToken = mothershipToken;
            CachedOculusId = creds.UserId;

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            __instance
                .GetType()
                .GetField("userToken", flags)
                ?.SetValue(__instance, mothershipToken);
            __instance.GetType().GetField("userID", flags)?.SetValue(__instance, playFabId);

            AuthenticationValues auth = new AuthenticationValues();
            auth.AuthType = CustomAuthenticationType.Custom;
            auth.UserId = playFabId;

            var postParams = new System.Collections.Generic.Dictionary<string, object>
            {
                { "AppId", "63FDD" },
                { "AppVersion", APP_VERSION },
                { "Ticket", playFabSessionTicket },
                { "Nonce", nonce3 },
                { "MothershipEnvId", MS_ENV_ID },
                { "MothershipDeploymentId", MS_DEPLOY_ID },
                { "MothershipToken", mothershipToken },
                { "Platform", "PC" },
                { "OculusId", creds.UserId },
            };

            auth.SetAuthPostData(postParams);
            PhotonNetwork.AuthValues = auth;

            if (PhotonNetworkController.instance != null)
            {
                PhotonNetworkController.instance.InitiateConnection();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            Debug.Log("====== The World Machine ======");
        }

        public void RefreshNonce(Action<string> onComplete)
        {
            StartCoroutine(RefreshNonceRoutine(onComplete));
        }

        private IEnumerator RefreshNonceRoutine(Action<string> onComplete)
        {
            Debug.Log("refreshing nonce...");
            string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = Path.Combine(dllDir, "creds.txt");

            if (File.Exists(filepath))
            {
                string credsRaw = File.ReadAllText(filepath);
                var creds = JsonConvert.DeserializeObject<CredShit>(credsRaw);

                UnityWebRequest req = new UnityWebRequest(
                    "https://graph.oculus.com/user_nonce_generate",
                    "POST"
                );
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Authorization", "Bearer " + creds.AccessToken);

                yield return req.SendWebRequest();

                if (!req.isNetworkError && !req.isHttpError)
                {
                    var res = JsonConvert.DeserializeObject<NonceResponse>(
                        req.downloadHandler.text
                    );
                    Debug.Log("fresh nonce acquired " + res.nonce);
                    onComplete?.Invoke(res.nonce);
                }
                else
                {
                    Debug.LogError("failed to refresh nonce: " + req.error);
                    onComplete?.Invoke(null);
                }
            }
        }
    }
}
