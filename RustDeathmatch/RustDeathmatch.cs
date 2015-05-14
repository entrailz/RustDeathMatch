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
        void OnPlayerInit(BasePlayer player)
        {
            //Handle adding of items here, such as worn clothes, guns and ammo.
        }
        
        void OnPlayerRespawned(BasePlayer player)
        {
            //When a player has respawned, re-add items.
        }

        void OnEntityDeath(BaseCombatEntity entity, HitInfo hitinfo)
        {
            //Handle when a player dies, for example check who killed and increase their score.
        }
    }
}
