using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Data;
using UnityEngine;
using Oxide.Core;
using Rust;

namespace Oxide.Plugins
{
    [Info("Rust Deathmatch", "Lederp", "1.0.0")]
    class RustDeathmatch : RustPlugin
    {
        Dictionary<BasePlayer, int> playerKills = new Dictionary<BasePlayer, int>();
        Dictionary<BasePlayer, int> playerDeaths = new Dictionary<BasePlayer, int>();
        Dictionary<int, Vector3> spawnPoints = new Dictionary<int, Vector3>();
        List<BaseNPC> npcList = Component.FindObjectsOfType<BaseNPC>().ToList();
        List<BaseCorpse> corpseList = Component.FindObjectsOfType<BaseCorpse>().ToList();
       

        void OnServerInitialized()
        {
            loadSpawnfile("spawns");
            foreach (BaseNPC npc in npcList)
            {
                npc.Kill();
            }
            foreach (BaseCorpse corpse in corpseList)
            {
                corpse.RemoveCorpse();
            }
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (playerKills.ContainsKey(player) || playerDeaths.ContainsKey(player))
                {
                    return;
                }
                else
                {
                    playerKills.Add(player, 0);
                    playerDeaths.Add(player, 0);
                }
            }
        }

        bool OnEligibleForWounding(BasePlayer player, HitInfo info)
        {
            return false;
        }

        void OnPlayerInit(BasePlayer player)
        {
            //Setup player for use with mod.
            if (playerKills.ContainsKey(player) || playerDeaths.ContainsKey(player))
            {
                return;
            }
            else
            {
                playerKills.Add(player, 0);
                playerDeaths.Add(player, 0);
            }
            CreatePlayerInventory(player);
            setPlayerHealthandFood(player);
            increaseGunAmmo(player);
        }

        void OnPlayerRespawned(BasePlayer player)
        {
            CreatePlayerInventory(player);
            setPlayerHealthandFood(player);
            increaseGunAmmo(player);
        }

        void CreatePlayerInventory(BasePlayer player)
        {
            player.inventory.Strip();
            ItemContainer belt = player.inventory.containerBelt;
            ItemContainer main = player.inventory.containerMain;
            ItemContainer wear = player.inventory.containerWear;
            GiveItem(player, "coffeecan_helmet", 1, wear, false);
            GiveItem(player, "jacket_snow", 1, wear, false);
            GiveItem(player, "urban_pants", 1, wear, false);
            GiveItem(player, "urban_boots", 1, wear, false);
            GiveItem(player, "hazmat_gloves", 1, wear, false);
            GiveItem(player, "rifle_bolt", 1, belt, false);
            GiveItem(player, "rifle_ak", 1, belt, false);
            GiveItem(player, "smg_thompson", 1, belt, false);
            GiveItem(player, "ammo_rifle", 200, main, true);
            GiveItem(player, "ammo_pistol", 200, main, true);
            GiveItem(player, "ammo_rifle_hv", 64, main, true);
            GiveItem(player, "ammo_pistol_hv", 64, main, true);
            GiveItem(player, "bandage", 200, belt, true);
            GiveItem(player, "syringe_medical", 100, belt, true);
            GiveItem(player, "largemedkit", 5, belt, true);
        }

        void setPlayerHealthandFood(BasePlayer player)
        {
            player.health = 100f;
        }

        void increaseGunAmmo(BasePlayer player)
        {
            List<BaseProjectile> mags = Component.FindObjectsOfType<BaseProjectile>().ToList();
            foreach (BaseProjectile mag in mags)
            {
                if (mag.ownerPlayer == player)
                {
                    switch (StripWeapons(mag.LookupShortPrefabName()))
                    {
                        case "boltrifle":
                            mag.primaryMagazine.contents = 4;
                            break;
                        case "thompson":
                            mag.primaryMagazine.contents = 20;
                            break;
                        case "ak47u":
                            mag.primaryMagazine.contents = 30;
                            break;
                    }
                }
                //SendReply(player, mag.primaryMagazine.capacity.ToString());
            }
        }

        void OnEntityDeath(BaseCombatEntity entity, HitInfo hitinfo)
        {
            if (entity is BasePlayer)
            {
                BasePlayer victim = entity.ToPlayer();
                //Handle when a player dies, for example check who killed and increase their score.
                if (hitinfo.damageTypes.Has(DamageType.Suicide))
                {

                }
                else
                { 
                    try
                    {
                        BasePlayer killer = hitinfo.Initiator.ToPlayer();
                        playerKills[killer] = playerKills[killer] + 1;
                        playerDeaths[victim] = playerDeaths[victim] + 1;
                        SendReply(victim, "You was killed by: " + killer.displayName + " using " + StripWeapons(hitinfo.Weapon.LookupShortPrefabName()));
                        SendReply(killer, "You killed: " + victim.displayName);
                    }
                    catch (Exception ex)
                    {
                        Puts(ex.Message);
                        //Error can occur on player suicide, this is to catch that.
                    }
                }
                timer.Once(0.5f, () =>
                {
                    spawnPlayerinPVPArea(victim);
                });
            }
        }

        void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity.GetComponent("BaseCorpse"))
            {
                BaseCorpse corpse = entity as BaseCorpse;
                corpse.RemoveCorpse();
            }
            if (entity.GetComponent("BaseNPC"))
            {
                BaseNPC npc = entity as BaseNPC;
                npc.Kill();
            }
        }
        void OnPlayerDisconnected(BasePlayer player)
        {
            //Remove player from list of players.
            playerKills.Remove(player);
            playerDeaths.Remove(player);
        }

        void spawnPlayerinPVPArea(BasePlayer player)
        {
            System.Random random = new System.Random();
            Vector3 location = new Vector3();
            int number = random.Next(1, spawnPoints.Count);
            location = spawnPoints[number];
            player.transform.position = location;
            player.Respawn(false);
            player.EndSleeping();
            ForcePlayerPosition(player, location);
        }

        void CreateItemName(string itemdata, out string itemname)
        {
            itemname = itemdata.Substring(0, itemdata.IndexOf(" "));
        }

        void GiveItem(BasePlayer player, string name, int amount, ItemContainer container, bool skipstack)
        {
            var itemdefinition = ItemManager.FindItemDefinition(name);
            if (itemdefinition != null)
            {
                if (skipstack)
                {
                    player.inventory.GiveItem(ItemManager.CreateByItemID(itemdefinition.itemid, amount), container);
                }
                else
                {
                    int stackable = 1;
                    if (itemdefinition.stackable == null || itemdefinition.stackable < 1) stackable = 1;
                    else stackable = itemdefinition.stackable;
                    for (var i = amount; i > 0; i = i - stackable)
                    {
                        var giveamount = 0;
                        if (i >= stackable)
                            giveamount = stackable;
                        else
                            giveamount = i;
                        if (giveamount > 0)
                        {
                            player.inventory.GiveItem(ItemManager.CreateByItemID(itemdefinition.itemid, giveamount), container);
                        }
                    }
                }
            }
        }

        [ChatCommand("top5")]
        void chatCmd_top5(BasePlayer player, string command, string[] args)
        {
            if (playerKills.Count == 0)
            {
                SendReply(player, "There is currently no one with any kills! Be the first to get a kill.");
                return;
            }
            int listed = 0;
            foreach (KeyValuePair<BasePlayer, int> kills in playerKills.OrderBy(key => key.Value))
            {
                if (listed == 5)
                {
                    break;
                }
                else
                {
                    SendReply(player, "Name: " + kills.Key.displayName + " - " + kills.Value.ToString());
                    listed++;
                }
            }
        }

        [ChatCommand("loc")]
        void chatCmd_Location(BasePlayer player, string command, string[] args)
        {
            float x = player.transform.position.x;
            float y = player.transform.position.y;
            float z = player.transform.position.z;
            SendReply(player, "X: " + x.ToString() + " Y: " + y.ToString() + " Z: " + z.ToString());
        }
        [ChatCommand("top10")]
        void chatCmd_top10(BasePlayer player, string command, string[] args)
        {
            if (playerKills.Count == 0)
            {
                SendReply(player, "There is currently no one with any kills! Be the first to get a kill.");
                return;
            }
            int listed = 0;
            foreach (KeyValuePair<BasePlayer, int> kills in playerKills.OrderBy(key => key.Value))
            {
                if (listed == 10)
                {
                    break;
                }
                else
                {
                    SendReply(player, "Name: " + kills.Key.displayName + " - " + kills.Value.ToString());
                    listed++;
                }
            }
        }

        [ChatCommand("spectate")]
        void chatCmd_spectate(BasePlayer player, string command, string[] args)
        {
            if (player.IsSpectating() == false)
            {
                player.StartSpectating();
            }
            else
            {
                player.StopSpectating();
            }
        }

        void ForcePlayerPosition(BasePlayer player, Vector3 destination)
        {
            player.StartSleeping();
            player.transform.position = destination;
            player.ClientRPCPlayer(null, player, "ForcePositionTo", new object[] { destination });
            player.TransformChanged();

            player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
            player.UpdateNetworkGroup();
            player.UpdatePlayerCollider(true, false);
            player.SendNetworkUpdateImmediate(false);
            player.ClientRPCPlayer(null, player, "StartLoading");
            player.SendFullSnapshot();
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, false);
            player.ClientRPCPlayer(null, player, "FinishLoading");

        }

        [ChatCommand("addspawn")]
        void chatCmd_addSpawn(BasePlayer player, string command, string[] args)
        {
            int nextnumber = spawnPoints.Count + 1;
            spawnPoints.Add(nextnumber, player.transform.position);
        }

        [ChatCommand("savespawns")]
        void chatCmd_saveSpawns(BasePlayer player, string command, string[] args)
        {
            var saveFile = Interface.GetMod().DataFileSystem.GetDatafile("spawns");
            saveFile.Clear();
            int i = 1;
            foreach (KeyValuePair<int, Vector3> spawn in spawnPoints)
            {
                var addSpawnPoint = new Dictionary<string, object>();
                addSpawnPoint.Add("x", Math.Round(spawn.Value.x * 100) / 100);
                addSpawnPoint.Add("y", Math.Round(spawn.Value.y * 100) / 100);
                addSpawnPoint.Add("z", Math.Round(spawn.Value.z * 100) / 100);
                saveFile[i.ToString()] = addSpawnPoint;
                i++;
            }
            Interface.GetMod().DataFileSystem.SaveDatafile("spawns");
        }

        [ChatCommand("reloadspawns")]
        void chatCmd_reloadSpawns(BasePlayer player, string command, string[] args)
        {
            loadSpawnfile("spawns");
        }

        string StripWeapons(string name)
        {
            string[] splitname = name.Split('.');
            return splitname[0];
        }

        void loadSpawnfile(string filename)
        {
            var loadFile = Interface.GetMod().DataFileSystem.GetDatafile(filename);
            if (loadFile["1"] == null)
            {
                Puts("Error loading spawns file.");
                return;
            }
            foreach (KeyValuePair<string, object> pair in loadFile)
            {
                int nextnumber = spawnPoints.Count + 1;
                var currentvalue = pair.Value as Dictionary<string, object>;
                spawnPoints.Add(nextnumber, new Vector3(Convert.ToInt32(currentvalue["x"]), Convert.ToInt32(currentvalue["y"]), Convert.ToInt32(currentvalue["z"])));
            }
            Puts("Spawns loaded!");
        }
    }
}
