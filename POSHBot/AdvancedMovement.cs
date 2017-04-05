using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using Posh_sharp.POSHBot.util;
using POSH.sys.strict;

namespace Posh_sharp.POSHBot {
    public class AdvancedMovement : AdvancedUTBehaviour {

        // You must list all actions here
        private readonly static string[] actions = new string[] {
            //"mov_stop_bot",
            //"mov_move_to_navpoint",
            //"mov_idle",
            //"mov_rotate",
            //"mov_big_rotate",
            //"mov_walk_to_nav_point"
        };

        // You must list all senses here
        private readonly static string[] senses = new string[] {
            //"mov_is_rotating",
            //"mov_is_walking",
            //"mov_is_stuck",
            //"mov_is_at_enemy_base",
            //"mov_is_at_own_base",
            //"mov_know_enemy_base_pos",
            //"mov_know_own_base_pos",
            //"mov_reachable_nav_point",
            //"mov_is_enemy_flag_reachable",
            //"mov_is_our_flag_reachable",
            //"mov_see_enemy"
        };

        public AdvancedMovement(AgentBase agent) : base(agent, actions, senses) {
            // Empty Constructor
        }

        [ExecutableSense("mov_is_rotating")]
        public bool mov_is_rotating() {
            return GetBot().Turning();
        }

        [ExecutableSense("mov_is_walking")]
        public bool mov_is_walking() {
            return GetBot().Moving();
        }

        [ExecutableSense("mov_is_stuck")]
        public bool mov_is_stuck() {
            return GetBot().Stuck();
        }

        /// <summary>
        /// returns 1 if we're near enough to enemy base
        /// </summary>
        [ExecutableSense("mov_is_at_enemy_base")]
        public bool mov_is_at_enemy_base() {
            return GetMovement().at_enemy_base();
        }

        /// <summary>
        /// returns 1 if we're near enough to our own base
        /// </summary>
        /// <returns></returns>
		[ExecutableSense("mov_is_at_own_base")]
        public bool mov_is_at_own_base() {
            return GetMovement().at_own_base();

        }

        /// <summary>
        /// returns 1 if we have a location for the enemy base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("mov_know_enemy_base_pos")]
        public bool mov_know_enemy_base_pos() {
            return GetMovement().KnowEnemyBasePos();
        }

        /// <summary>
        /// returns 1 if we have a location for our own base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("mov_know_own_base_pos")]
        public bool mov_know_own_base_pos() {
            return GetMovement().KnowOwnBasePos();
        }

        /// <summary>
        /// returns 1 if there's a reachable nav point in the bot's list which we're not already at
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("mov_reachable_nav_point")]
        public bool mov_reachable_nav_point() {
            return GetMovement().ReachableNavPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if the enemy flag is specified as reachable</returns>
        [ExecutableSense("mov_is_enemy_flag_reachable")]
        public bool mov_is_enemy_flag_reachable() {
            return GetMovement().enemy_flag_reachable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if our flag is specified as reachable</returns>
		[ExecutableSense("mov_is_our_flag_reachable")]
        public bool mov_is_our_flag_reachable() {
            return GetMovement().our_flag_reachable();
        }

        [ExecutableSense("mov_see_enemy")]
        public bool mov_see_enemy() {
            return GetMovement().SeeEnemy();
        }



        ///
        /// ACTIONS
        /// 

        [ExecutableAction("mov_move_to_navpoint", 1f)]
        public bool mov_move_to_navpoint() {
            return GetMovement().moveto_navpoint();
        }

        /// <summary>
        /// Stops the Bot from doing stuff
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_stop_bot")]
        public bool mov_stop_bot() {
            GetBot().SendMessage("STOP", new Dictionary<string, string>());
            return true;
        }

        /// <summary>
        /// Idleing
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_idle")]
        public bool mov_idle() {        
            return GetBot().Inch();
        }

        /// <summary>
        /// Rotating the bot around itself (left or right)
        /// </summary>
        /// <param name="angle">If angle is not given random 90 degree turn is made</param>
        /// <returns></returns>
        [ExecutableAction("mov_rotate")]
        public bool mov_rotate() {
            return mov_rotate(0);
        }

        protected bool mov_rotate(int angle) {
            if (angle != 0)
                GetBot().Turn(angle);
            else if (new Random().Next(2) == 0)
                GetBot().Turn(90);
            else
                GetBot().Turn(-90);


            return true;
        }

        /// <summary>
        /// Turns the bot 160 degrees
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_big_rotate")]
        public bool mov_big_rotate() {
            return mov_rotate(160);
        }

        /// <summary>
        /// Runs to the chosen Navpoint
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_walk_to_nav_point")]
        public bool mov_walk_to_nav_point() {
            return GetMovement().WalkToNavPoint();
        }

        [ExecutableAction("mov_jump")]
        public bool mov_jump() {
            GetBot().SendMessage("JUMP", new Dictionary<string, string>());
            if (_debug_) {
                Console.Out.WriteLine("Debug: Attempt to jump");
            }
            return true;
        }
    }

}