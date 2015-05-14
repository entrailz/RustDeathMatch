
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Data;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Waypoint System", "Lederp", "1.0.0")]
    class Waypoint : RustPlugin
    {
        static int beaconHeight = 500;
        static int arrowSize = 10;
        static float beaconRefresh = 2f;

        static Core.Configuration.DynamicConfigFile BeaconData;
        static Dictionary<string, bool> userBeacons = new Dictionary<string, bool>();
        static Dictionary<string, Oxide.Plugins.Timer> userBeaconTimers = new Dictionary<string, Oxide.Plugins.Timer>();

        void Loaded()
        {
            LoadBeaconData();
        }

        void Unload()
        {
            SaveBeaconData();
            //CleanUpBeacons();
        }

        void OnServerSave()
        {
            SaveBeaconData();
        }

        private void SaveBeaconData()
        {
            Interface.GetMod().DataFileSystem.SaveDatafile("PersonalBeacon_Data");
        }
        private void LoadBeaconData()
        {
            //Debug.Log("Loading data...");
            try
            {
                //BeaconData = Interface.GetMod().DataFileSystem.ReadObject<Oxide.Core.Configuration.DynamicConfigFile>("PersonalBeacon_Data");
                BeaconData = Interface.GetMod().DataFileSystem.GetDatafile("PersonalBeacon_Data");
            }
            catch
            {
                Debug.Log("Failed to load datafile.");
            }
            //Debug.Log("Data should be loaded.");
        }

        void DisplayBeacon(BasePlayer player)
        {
            var playerId = player.userID.ToString();
            // player has disconnected
            if (!player.IsConnected())
            {
                Debug.Log("Cleaning up disconnected player timer.");
                userBeacons[playerId] = false;
                userBeaconTimers[playerId].Destroy();
                return;
            }

            if (BeaconData == null)
            {
                Debug.Log("BeaconData wasn't loaded before use, forcing load.");
                LoadBeaconData();
            }
            if (BeaconData[playerId] == null)
            {
                Debug.Log(string.Format("Player [{0}] -- BeaconData is corrupt.", playerId));
                userBeacons[playerId] = false;
                userBeaconTimers[playerId].Destroy();
                return;
                /*
                foreach (var playerbeacons in BeaconData)
                {
                    Debug.Log(playerbeacons.ToString());
                }
                */
            }

            var table = BeaconData[playerId] as Dictionary<string, object>;
            //var beaconGround = new Vector3((float)table["x"], (float)table["y"], (float)table["z"]);
            var beaconGround = new Vector3();
            // Necessary evil here
            beaconGround.x = float.Parse(table["x"].ToString());
            beaconGround.y = float.Parse(table["y"].ToString());
            beaconGround.z = float.Parse(table["z"].ToString());

            var beaconSky = beaconGround;
            beaconSky.y = beaconSky.y + beaconHeight;

            player.SendConsoleCommand("ddraw.arrow", beaconRefresh, UnityEngine.Color.red, beaconGround, beaconSky, arrowSize);
            player.SendConsoleCommand("ddraw.text", beaconRefresh, UnityEngine.Color.white, beaconGround, "Test text....");
        }

        [ChatCommand("setwp")]
        void cmdSetBeacon(BasePlayer player, string command, string[] args)
        {
            Dictionary<string, object> coords = new Dictionary<string, object>();
            coords.Add("x", player.transform.position.x);
            coords.Add("y", player.transform.position.y);
            coords.Add("z", player.transform.position.z);

            if (BeaconData == null)
            {
                Debug.Log("BeaconData wasn't loaded before use, forcing load.");
                LoadBeaconData();
            }

            BeaconData[player.userID.ToString()] = coords;

            var newVals = BeaconData[player.userID.ToString()] as Dictionary<string, object>;

            SendReply(player, string.Format("Beacon set to: x: {0}, y: {1}, z: {2}", newVals["x"], newVals["y"], newVals["z"]));
        }

        [ChatCommand("wp")]
        void cmdBeacon(BasePlayer player, string command, string[] args)
        {

            if (BeaconData == null)
            {
                Debug.Log("BeaconData wasn't loaded before use, forcing load.");
                LoadBeaconData();
            }

            var playerId = player.userID.ToString();

            if (BeaconData[playerId] == null)
            {
                SendReply(player, "You have not set a waypoint yet.  Please run /setwp to create a waypoint.");
                Debug.Log(string.Format("Player [{0}] -- BeaconData is corrupt or non-existent.", playerId));
                return;
                /*
                foreach (var playerbeacons in BeaconData)
                {
                    Debug.Log(playerbeacons.ToString());
                }
                */
            }
            if (!userBeacons.ContainsKey(playerId)) userBeacons.Add(playerId, false);

            // maybe unecessary
            if (userBeacons[playerId] == null) userBeacons[playerId] = false;

            if (userBeacons[playerId] == false)
            {
                DisplayBeacon(player); // display immediately
                userBeaconTimers[playerId] = timer.Repeat(beaconRefresh, 0, delegate() { DisplayBeacon(player); });
                SendReply(player, "Beacon on.");
                userBeacons[playerId] = true;
            }
            else
            {
                userBeaconTimers[playerId].Destroy();
                SendReply(player, "Beacon off.");
                userBeacons[playerId] = false;
            }
        }

        // Admin commands:


        [HookMethod("SendHelpText")]
        private void SendHelpText(BasePlayer player)
        {
            var helpString = "<color=#11FF22>PersonalBeacon</color>:\n/setwp - Sets the beacon to the current location.\n/wp - Toggles beacon on or off.";
            player.ChatMessage(helpString.TrimEnd());
        }
    }
}