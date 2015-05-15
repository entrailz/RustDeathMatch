using System.Collections.Generic;
using System;
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

        void OnPlayerInit(BasePlayer player)
        {
            player.inventory.Strip();
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
        }
        
        void OnPlayerRespawned(BasePlayer player)
        {
            player.inventory.Strip();
            ItemContainer belt = player.inventory.containerBelt;
            ItemContainer main = player.inventory.containerMain;
            ItemContainer wear = player.inventory.containerWear;
        }

        void OnEntityDeath(BaseCombatEntity entity, HitInfo hitinfo)
        {
            //Handle when a player dies, for example check who killed and increase their score.
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
    }
}
