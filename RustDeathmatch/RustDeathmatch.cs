using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Data;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Rust;

namespace Oxide.Plugins
{
    [Info("Rust Deathmatch", "Lederp", "1.0.0")]
    class RustDeathmatch : RustPlugin
    {

        Dictionary<BasePlayer, int> playerKills = new Dictionary<BasePlayer, int>();
        Dictionary<BasePlayer, int> playerDeaths = new Dictionary<BasePlayer, int>();

        void OnServerInitialized()
        {
            
        }

        void OnPlayerInit(BasePlayer player)
        {
            player.inventory.Strip();
            ItemContainer belt = player.inventory.containerBelt;
            ItemContainer main = player.inventory.containerMain;
            ItemContainer wear = player.inventory.containerWear;
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
        }
        
        void OnPlayerRespawned(BasePlayer player)
        {
            CreatePlayerInventory(player);
        }

        void CreatePlayerInventory(BasePlayer player)
        {
            player.inventory.Strip();
            ItemContainer belt = player.inventory.containerBelt;
            ItemContainer main = player.inventory.containerMain;
            ItemContainer wear = player.inventory.containerWear;
            GiveItem(player, "coffeecan_helmet", 1, wear);
            GiveItem(player, "jacket_snow", 1, wear);
            GiveItem(player, "urban_pants", 1, wear);
            GiveItem(player, "urban_boots", 1, wear);
            GiveItem(player, "hazmat_gloves", 1, wear);
            GiveItem(player, "rifle_bolt", 1, belt);
            GiveItem(player, "rifle_ak", 1, belt);
            GiveItem(player, "smg_thompson", 1, belt);
            GiveItem(player, "ammo_rifle", 200, main);
        }

        void OnEntityDeath(BaseCombatEntity entity, HitInfo hitinfo)
        {
            //Handle when a player dies, for example check who killed and increase their score.
            if (entity is BasePlayer)
            {
                BasePlayer killer = hitinfo.Initiator.ToPlayer();
                if (killer is BasePlayer)
                {
                    BasePlayer victim = entity.ToPlayer();
                    playerKills[killer] = playerKills[killer] + 1;
                    playerDeaths[victim] = playerDeaths[victim] + 1;
                    SendReply(victim, "You was killed by: " + killer.displayName + " using " + hitinfo.Weapon.LookupShortPrefabName());
                    SendReply(killer, "You killed: " + victim.displayName);
                }
            }
        }

        void OnPlayerDisconnected(BasePlayer player)
        {
            //Remove player from list of players.
            playerKills.Remove(player);
            playerDeaths.Remove(player);
        }

        void CreateItemName(string itemdata, out string itemname)
        {
            itemname = itemdata.Substring(0, itemdata.IndexOf(" "));
        }

        void GiveItem(BasePlayer player, string name, int amount, ItemContainer container)
        {
            var itemdefinition = ItemManager.FindItemDefinition(name);
            if (itemdefinition != null)
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

        [ChatCommand("top5")]
        void chatCmd_top5(BasePlayer player, string command, string[] args)
        {
            int listed = 0;
            foreach (KeyValuePair<BasePlayer, int> kills in playerKills.OrderBy(key=>key.Value))
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
        [ChatCommand("top10")]
        void chatCmd_top10(BasePlayer player, string command, string[] args)
        {
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
    }
}
