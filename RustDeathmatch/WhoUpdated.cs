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
    [Info("Who Updated Sign Last", "Lederp", "1.0.0")]
    class WhoUpdated : RustPlugin
    {
        Dictionary<ulong, string> signUpdated = new Dictionary<ulong, string>();
        float maxDistance = 2.0f;

        void OnServerInitialized()
        {
            loadAllSigns();
        }

        void loadAllSigns()
        {
            signUpdated.Clear();
            try
            {
                var loadFile = Interface.GetMod().DataFileSystem.GetDatafile("signs");
                foreach (KeyValuePair<string, object> pair in loadFile)
                {
                    signUpdated.Add(Convert.ToUInt64(pair.Key), pair.Value.ToString());
                }
                Puts("Loaded all sign files");
            }
            catch (Exception ex)
            {
                Puts("There was an issue loading the signs files, perhaps there are none?");
            }
        }

        void UpdateSign(Signage sign, BaseEntity.RPCMessage message)
        {
            try
            {
                signUpdated.Add(Convert.ToUInt64(sign.textureID), message.player.displayName + ", " + message.player.userID.ToString());
                var saveFile = Interface.GetMod().DataFileSystem.GetDatafile("signs");
                saveFile.Clear();
                int i = 1;
                foreach (KeyValuePair<ulong, string> signea in signUpdated)
                {
                    saveFile[signea.Key.ToString()] = signea.Value;
                    i++;
                }
                Interface.GetMod().DataFileSystem.SaveDatafile("signs");
            }
            catch (Exception ex)
            {
                Puts(ex.ToString());
            }
        }

        [ChatCommand("sign")]
        void chatCmd_sign(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin())
            {
                RaycastHit hit;
                if (Physics.Raycast(player.eyes.Ray(), out hit, maxDistance))
                {
                    Signage hitSign = hit.transform.GetComponentInParent<Signage>();
                    if (hitSign != null)
                    {
                        if (signUpdated.ContainsKey(Convert.ToUInt64(hitSign.textureID)))
                        {
                            SendReply(player, "The last person to edit this sign was: " + signUpdated[Convert.ToUInt64(hitSign.textureID)]);
                        }
                        else
                        {
                            SendReply(player, "This sign is not in the database.");
                        }
                    }
                }
            }
        }
    }
}
