using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using Posh_sharp.POSHBot.util;
using POSH.sys.strict;
//import utilityfns

namespace Posh_sharp.POSHBot {
    /// <summary>
    /// This class presents a template for creating a new Behaviour for UT2004 based agents. It is not intended to be used as is but modified and renamed.
    /// Once your own UTBehaviour is created by renaming this class you will receive ann information about the virtual environment at runtime and can execute actions. 
    /// </summary>
    public class Runner : UTBehaviour {
        internal CombatInfo info;

        // You must list all actions here
        private readonly static string[] actions = new string[] {
            "jump-obstacle"
        };

        // You must list all senses here
        private readonly static string[] senses = new string[] {
        };


        public Runner(AgentBase agent) : base(agent, actions, senses) {
            info = new CombatInfo();
        }

        override internal void ReceiveProjectileDetails(Dictionary<string, string> values) {
        }

        override internal void ReceiveDamageDetails(Dictionary<string, string> values) {
        }

        /// <summary>
        /// handle details about a player (not itself) dying
        /// remove any info about that player from CombatInfo
        /// </summary>
        /// <param name="values"></param>
        override internal void ReceiveKillDetails(Dictionary<string, string> values) {
        }

        override internal void ReceiveDeathDetails(Dictionary<string, string> value) {
        }

        /*
        * ACTIONS 
        */

        [ExecutableAction("jump-obstacle")]
        public bool jump() {
            GetBot().SendMessage("JUMP", new Dictionary<string, string>());
            if (_debug_) {
                Console.Out.WriteLine("Debug: Attempt to jump");
            }
            return true;
        }

        /*
         * SENSES
         */

    }
}


