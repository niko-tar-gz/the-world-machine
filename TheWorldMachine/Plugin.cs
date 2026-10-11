using System;
using BepInEx;
using Photon.Pun;
using TheWorldMachine.impl;
using UnityEngine;

namespace TheWorldMachine
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        void OnEnable()
        {
            Debug.Log("enabled");
            PhotonNetwork.GameVersion = "2026oldprependhorse.live1.1.1.128_2.40"; // what the fuck??????
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion =
                "2026oldprependhorse.live1.1.1.128";
            Debug.Log(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
            PhotonNetwork.AutomaticallySyncScene = false;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnGUI()
        {
            if (!Photon.Pun.PhotonNetwork.IsConnected)
            {
                GUI.Label(new Rect(20, 20, 420, 20), "Photon: NOT CONNECTED");
                return;
            }

            Rect windowRect = new Rect(20, 20, 580, 680);

            GUI.Window(
                1337,
                windowRect,
                _ =>
                {
                    Vector2 scroll = Vector2.zero;
                    scroll = GUILayout.BeginScrollView(scroll);

                    GUILayout.Label("=== CONNECTION ===");
                    GUILayout.Label("State: " + Photon.Pun.PhotonNetwork.NetworkClientState);
                    GUILayout.Label("Server: " + Photon.Pun.PhotonNetwork.ServerAddress);
                    GUILayout.Label("Region: " + Photon.Pun.PhotonNetwork.CloudRegion);
                    GUILayout.Label("Ping: " + Photon.Pun.PhotonNetwork.GetPing());

                    GUILayout.Space(8);
                    GUILayout.Label("=== MATCHMAKING / LOBBY ===");
                    GUILayout.Label("In Lobby: " + Photon.Pun.PhotonNetwork.InLobby);
                    GUILayout.Label(
                        "Lobby: " + (Photon.Pun.PhotonNetwork.CurrentLobby?.Name ?? "null")
                    );
                    GUILayout.Label(
                        "Lobby Type: " + Photon.Pun.PhotonNetwork.CurrentLobby?.Type.ToString()
                    );

                    GUILayout.Space(8);
                    GUILayout.Label("=== ROOM ===");
                    var room = Photon.Pun.PhotonNetwork.CurrentRoom;
                    if (room == null)
                    {
                        GUILayout.Label("Not in a room");
                    }
                    else
                    {
                        GUILayout.Label("Room Name: " + room.Name);
                        GUILayout.Label("Players: " + room.PlayerCount + " / " + room.MaxPlayers);
                        GUILayout.Label("Open: " + room.IsOpen);
                        GUILayout.Label("Visible: " + room.IsVisible);
                        GUILayout.Label("Master Client: " + room.MasterClientId);

                        GUILayout.Space(4);
                        GUILayout.Label("--- Room Properties ---");
                        foreach (var kv in room.CustomProperties)
                            GUILayout.Label($"{kv.Key}: {kv.Value}");

                        GUILayout.Space(4);
                        GUILayout.Label("--- Players ---");
                        foreach (var p in room.Players.Values)
                        {
                            GUILayout.Label(
                                $"[{p.ActorNumber}] {p.NickName} | "
                                    + $"IsMaster: {p.IsMasterClient} | "
                                    + $"Inactive: {p.IsInactive}"
                            );

                            foreach (var prop in p.CustomProperties)
                                GUILayout.Label("   " + prop.Key + ": " + prop.Value);
                        }
                    }

                    GUILayout.EndScrollView();
                    GUI.DragWindow();
                },
                "Photon Room / Matchmaking Debug"
            );
        }
    }
}
