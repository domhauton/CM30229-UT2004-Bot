using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using Posh_sharp.POSHBot.util;

namespace Posh_sharp.POSHBot {
    /// <summary>
    /// The status behaviour has primitives for stuff to do with finding out
    /// the bot's state (e.g. amount of health).
    /// </summary>
    public class AdvancedStatus : AdvancedUTBehaviour {

        // You must list all actions here
        private readonly static string[] actions = new string[] {
        };

        // You must list all senses here
        private readonly static string[] senses = new string[] {
        };

        public AdvancedStatus(AgentBase agent) : base(agent, actions, senses) {
            // EMPTY Constructor
        }


        [ExecutableSense("status_fail")]
        public bool status_fail() {
            return false;
        }

        [ExecutableSense("status_succeed")]
        public bool status_succeed() {
            return true;
        }

        [ExecutableSense("status_game_ended")]
        public bool status_game_ended() {
            if (_debug_)
                Console.Out.WriteLine("in game_ended");
            if (GetBot().killConnection)
                return true;
            return false;
        }



        [ExecutableSense("status_focusing_task")]
        public bool status_focusing_task() {
            if (_debug_)
                Console.Out.WriteLine("in focusing_task");
            if (status_have_enemy_flag())
                return true;

            return false;
        }

        [ExecutableSense("status_have_enemy_flag")]
        public bool status_have_enemy_flag() {
            if (_debug_)
                Console.Out.WriteLine("in have_enemy_flag");

            if (GetCombat().info.HoldingEnemyFlag != null && GetCombat().info.HoldingEnemyFlag == GetBot().info["BotId"]) {
                return true;
            }

            return false;
        }

        [ExecutableSense("status_own_health_level")]
        public string status_own_health_level() {
            return GetBot().info["Health"];
        }

        [ExecutableSense("status_is_armed")]
        public bool status_is_armed() {
            if (GetBot().info.Count == 0)
                return false;

            if (GetBot().info["Weapon"] == "None") {
                Console.WriteLine("unarmed");
                return false;
            }
            Console.WriteLine("armed with " + GetBot().info["Weapon"]);

            return true;
        }

        [ExecutableSense("status_ammo_amount")]
        public int status_ammo_amount() {
            if (GetBot().info.Count == 0)
                return 0;
            return int.Parse(GetBot().info["PrimaryAmmo"]);
        }

        [ExecutableSense("status_is_armed_with_ammo")]
        public bool status_is_armed_with_ammo() {
            return status_is_armed() && status_ammo_amount() > 0;
        }
    }
}
