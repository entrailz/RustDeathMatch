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
    [Info("Compass Plugin", "Lederp", "1.0.0")]
    class Compass : RustPlugin
    {
        
        [ChatCommand("showcompass")]
        void chatCommandShowcompass(BasePlayer player, string command, string[] args)
        {
            var playerPos = player.eyes.position + player.eyes.Forward() * 2.1 + player.eyes.Up() * 0.025;
            player.SendConsoleCommand("ddraw.text", 0.7, UnityEngine.Color.red, playerPos, "TEST");
        }
    }
}