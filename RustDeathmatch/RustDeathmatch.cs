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
            //When a player has respawned, re-add items.
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
    }
}
