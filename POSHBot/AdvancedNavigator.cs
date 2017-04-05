using System;
using Posh_sharp.POSHBot.util;
using POSH.sys;
using System.Collections.Generic;
using POSH.sys.annotations;
using System.Reflection;

namespace Posh_sharp.POSHBot {
    public class AdvancedNavigator : AdvancedUTBehaviour {

        private readonly FieldInfo navPointsField;
    

        // You must list all actions here
        private readonly static string[] actions = new string[] {
            //"nav_select_navpoint",
            //"nav_select_enemy_flag",
            //"nav_select_own_flag",
            //"nav_retrace_navpoint"
            //"nav_select_enemy_base""nav_select_enemy_base"
        };

        // You must list all senses here
        private readonly static string[] senses = new string[] {
            //"nav_is_target_selected",
            //"nav_has_reached_target",
            //"nav_is_close_to_navpoint",
            //"nav_is_selected_navpoint_reachable"
        };

        public AdvancedNavigator(AgentBase agent) : base(agent, actions, senses) {
            Type fieldsType = typeof(Navigator);
            navPointsField = fieldsType.GetField("navPoints", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [ExecutableAction("nav_select_navpoint")]
        public bool select_navpoint() {
            return GetNavigator().select_navpoint();
        }

        [ExecutableAction("nav_select_enemy_flag")]
        public bool select_enemy_flag() {
            return GetNavigator().select_enemy_flag();
        }

        [ExecutableAction("nav_select_own_flag")]
        public bool select_own_flag() {
            return GetNavigator().select_own_flag();
        }

        [ExecutableAction("nav_retrace_navpoint")]
        public bool retrace_navpoint() {
            return GetNavigator().retrace_navpoint();
        }

        [ExecutableAction("nav_select_enemy_base")]
        public bool SetEnemyBaseAsNavPoint() {
            NavPoint enemyBasePos = GetMovement().info.enemyBasePos;
            if(enemyBasePos != null) {
                //GetNavigator().select_navpoint();
                GetNavigator().MovedToNavpoint(enemyBasePos);
                //GetNavigator().select_navpoint(enemyBasePos.Id);
            } else {
                GetNavigator().select_navpoint();
            }
            return true;
        }

        /*
         * 
         * SENSES
         * 
         */

        /// <summary>
        /// returns 1 if we're near enough to our own base
        /// </summary>
        [ExecutableSense("nav_is_at_own_base")]
        public bool nav_at_own_base() {
            return GetNavigator().at_own_base();
        }


        /// <summary>
        /// sense to check if a target is currently selected
        /// </summary>
        /// <returns><c>true</c>, if target was selecteded, <c>false</c> otherwise.</returns>
        [ExecutableSense("nav_is_target_selected")]
        public bool nav_is_target_selected() {
            return GetNavigator().selected_target();
        }

        [ExecutableSense("nav_has_reached_target")]
        public bool nav_has_reached_target() {
            return GetNavigator().reached_target();
        }

        /// <summary>
        /// sense to find the closest navpoint to current location or to specific location
        /// </summary>
        [ExecutableSense("nav_is_close_to_navpoint")]
        public bool nav_is_close_to_navpoint() {
            return GetNavigator().close_navpoint();
        }

        /// <summary>
        /// Checks if the the selected navpoint is a neighbor of the current/closeby navpoint. Neighbor checks are based on the navgrid which is static and contains all useable paths
        /// </summary>
        [ExecutableSense("nav_is_selected_navpoint_reachable")]
        public bool nav_is_selected_navpoint_reachable() {
            return GetNavigator().selected_navpoint_reachable();
        }

        private bool AStarPathFinder() {
            Navigator navigator = GetNavigator();
            Dictionary<string, NavPoint> navPoints = (Dictionary<string, NavPoint>) navPointsField.GetValue(navigator);



            return true;
        }

    }
}

