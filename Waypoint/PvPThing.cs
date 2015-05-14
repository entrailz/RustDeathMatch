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
    [Info("PvP Stuff", "Lederp", "1.0.0")]
    class PvPThing : RustPlugin
    {
        Dictionary<BasePlayer, bool> isPvPEnabled = new Dictionary<BasePlayer, bool>();
        void OnEntityTakeDamage(BaseCombatEntity entitiy, HitInfo hit)
        {
            if (hit.HitEntity is BasePlayer)
                OnAttackShared(entitiy.ToPlayer(), hit.Initiator.ToPlayer(), hit);
        }

        private object OnAttackShared(BasePlayer attacker, BasePlayer victim, HitInfo hit)
        {
            if (victim is BasePlayer)
            {
                BasePlayer hitPlayer = victim;
                BasePlayer hittingPlayer = attacker;
                if (isPvPEnabled.ContainsKey(hitPlayer))
                {
                    Puts(hit.damageTypes.Total().ToString());
                    hitPlayer.baseProtection.Scale(hit.damageTypes);
                    Puts(hit.damageTypes.Total().ToString());
                    hitPlayer.skeletonProperties.ScaleDamage(hit);
                    Puts(hit.damageTypes.Total().ToString());
                    string damageDone = hit.damageTypes.Total().ToString();
                    float currentHealth = hitPlayer.health;
                    SendReply(hitPlayer, "You took: " + damageDone + " damage");
                    SendReply(hittingPlayer, "You dealt: " + damageDone + " to: " + hitPlayer.displayName);
                    if (currentHealth - hit.damageTypes.Total() <= 10 || currentHealth - hit.damageTypes.Total() <= 15)
                    {
                        SendReply(hitPlayer, "You just died! " + hittingPlayer.displayName + " killed you with " + hit.Weapon.name + ", they dealt: " + damageDone + " damage");
                        hit.damageTypes = new DamageTypeList();
                        hit.DidHit = false;
                        hit.HitEntity = null;
                        hit.Initiator = null;
                        hit.DoHitEffects = false;
                        hitPlayer.WoundAssist();
                        hitPlayer.Heal(100f);
                        hitPlayer.metabolism.bleeding.value = 0;
                        hitPlayer.metabolism.bleeding.max = 0;
                        hitPlayer.metabolism.bleeding.value = 0;
                        hitPlayer.metabolism.calories.min = 1000;
                        hitPlayer.metabolism.calories.value = 1000;
                        hitPlayer.metabolism.dirtyness.max = 0;
                        hitPlayer.metabolism.dirtyness.value = 0;
                        hitPlayer.metabolism.heartrate.min = 0.5f;
                        hitPlayer.metabolism.heartrate.max = 0.5f;
                        hitPlayer.metabolism.heartrate.value = 0.5f;
                        hitPlayer.metabolism.hydration.min = 1000;
                        hitPlayer.metabolism.hydration.value = 1000;
                        hitPlayer.metabolism.oxygen.min = 1;
                        hitPlayer.metabolism.oxygen.value = 1;
                        hitPlayer.metabolism.poison.max = 0;
                        hitPlayer.metabolism.poison.value = 0;
                        hitPlayer.metabolism.radiation_level.max = 0;
                        hitPlayer.metabolism.radiation_level.value = 0;
                        hitPlayer.metabolism.radiation_poison.max = 0;
                        hitPlayer.metabolism.radiation_poison.value = 0;
                        hitPlayer.metabolism.temperature.min = 32;
                        hitPlayer.metabolism.temperature.max = 32;
                        hitPlayer.metabolism.temperature.value = 32;
                        hitPlayer.metabolism.wetness.max = 0;
                        hitPlayer.metabolism.wetness.value = 0;
                        hitPlayer.metabolism.SendChangesToClient();
                        hitPlayer.Update();
                    }
                }
            }
            return false;
        }

        void OnPlayerAttack(BasePlayer attacker, HitInfo hit)
        {
            try
            {
                if (hit.HitEntity is BasePlayer)
                    OnAttackShared(attacker, hit.HitEntity as BasePlayer, hit);
            }
            catch (Exception ex)
            {

            }
        }

        void NullifyDamage(ref HitInfo info)
        {
            info.damageTypes = new DamageTypeList();
            info.DidHit = false;
            info.HitEntity = null;
            info.Initiator = null;
            info.DoHitEffects = false;
        }
        [ChatCommand("pvpon")]
        void chatCmdPvPOn(BasePlayer player, string command, string[] args)
        {
            isPvPEnabled.Add(player, true);
            SendReply(player, "You have PvP mode enabled!");
        }
        [ChatCommand("pvpoff")]
        void chatCmdPvPOff(BasePlayer player, string command, string[] args)
        {
            isPvPEnabled.Remove(player);
            SendReply(player, "You have disabled PvP mode!");
        }
    }
}
