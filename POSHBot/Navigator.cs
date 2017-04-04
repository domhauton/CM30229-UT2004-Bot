using System;
using Posh_sharp.POSHBot.util;
using POSH.sys;
using System.Collections.Generic;
using POSH.sys.annotations;

namespace Posh_sharp.POSHBot {
    public class Navigator : AdvancedUTBehaviour {
        private NavPoint __selectedNavpoint__;
        private string __closestNavpointID__;
        private string __lastVisitedNavpoint__;
        private int __deviation__;

        private Dictionary<string, bool> closestNavPointReachable;
        private Dictionary<string, int> navPointHistory;
        private Dictionary<string, NavPoint> navPoints;

        private int directionWeight;


        // You must list all actions here
        private readonly static string[] actions = new string[] {
            //"nav_select_navpoint",
            //"nav_select_enemy_flag",
            //"nav_select_own_flag",
            //"nav_retrace_navpoint"
        };

        // You must list all senses here
        private readonly static string[] senses = new string[] {
            //"nav_is_target_selected",
            //"nav_has_reached_target",
            //"nav_is_close_to_navpoint",
            //"nav_is_selected_navpoint_reachable"
        };

        public Navigator(AgentBase agent) : base(agent, actions, senses) {
            closestNavPointReachable = new Dictionary<string, bool>();
            navPointHistory = new Dictionary<string, int>();
            navPoints = new Dictionary<string, NavPoint>();
            __closestNavpointID__ = "";
            directionWeight = 1;
            __deviation__ = 50;
        }

        /*
         * 
         * internal methods
         * 
         */


        override internal void ReceiveDeathDetails(Dictionary<string, string> value) {
            __selectedNavpoint__ = null;
            closestNavPointReachable = new Dictionary<string, bool>();
        }

        /// <summary>
        /// used in validating the bot's path home or to the enemy flag
        /// if the thing has the right ID, then clear the relevant path if it's not reachable
        /// </summary>
        /// <param name="valuesDict"></param>
        override internal void ReceiveCheckReachDetails(Dictionary<string, string> valuesDict) {
            if (_debug_)
                Console.Out.WriteLine("[INFO ] Navigator: Recieved Check Reach Details");

            if (!valuesDict.ContainsKey("Id"))
                return;

            if (valuesDict["Id"] == "Nav" + __closestNavpointID__) {
                if (valuesDict["Reachable"] == "True")
                    closestNavPointReachable[__closestNavpointID__] = true;
                else
                    closestNavPointReachable[__closestNavpointID__] = false;
            }
        }

        protected internal void SetNavPoints(Dictionary<string, NavPoint> dict) {
            this.navPoints = dict;
        }

        public void MovedToNavpoint(NavPoint nav) {

            Console.Out.WriteLine("in MovedToNavpoint");

            if (nav != null) {
                __lastVisitedNavpoint__ = __closestNavpointID__;
                closestNavPointReachable[nav.Id] = false;
            }
            if (navPointHistory.ContainsKey(nav.Id))
                navPointHistory[nav.Id] += directionWeight;
            else
                navPointHistory[nav.Id] = 1;
        }

        public NavPoint GetSelectedNavpoint() {
            return __selectedNavpoint__;
        }

        public bool select_navpoint(string navID) {
            string leastVisitedNav = "";

            directionWeight = 1;
            Console.Out.WriteLine("in select_navpoint");

            if (navID != "" && navPoints.ContainsKey(navID)) {
                __selectedNavpoint__ = navPoints[navID];
                return true;
            }

            if (__closestNavpointID__ == null)
                return false;

            int count = 0;

            foreach (NavPoint.Neighbor nei in navPoints[__closestNavpointID__].NGP)
                if (leastVisitedNav == "") {
                    leastVisitedNav = nei.Id;
                    if (navPointHistory.ContainsKey(leastVisitedNav))
                        count = navPointHistory[leastVisitedNav];
                    else
                        break;
                } else if (navPointHistory.ContainsKey(nei.Id)) {
                    if (navPointHistory[nei.Id] < count) {
                        leastVisitedNav = nei.Id;
                        count = navPointHistory[nei.Id];
                    }
                } else {
                    leastVisitedNav = nei.Id;
                    break;
                }
            if (leastVisitedNav == "") {
                __selectedNavpoint__ = null;
                return false;
            }

            __selectedNavpoint__ = navPoints[leastVisitedNav];

            if (_debug_)
                Console.Out.WriteLine("selected Navpoint: " + leastVisitedNav);

            return true;
        }

        bool select_flag(bool ourFlag) {
            if (GetMovement().info == null)
                return false;

            string flagID = "";

            if (ourFlag && GetMovement().info.ourFlagInfo.ContainsKey("Id"))
                flagID = GetMovement().info.ourFlagInfo["Id"];
            else if (!ourFlag && GetMovement().info.enemyFlagInfo.ContainsKey("Id"))
                flagID = GetMovement().info.enemyFlagInfo["Id"];
            else
                return false;

            if (navPoints.ContainsKey(__closestNavpointID__))
                foreach (string navid in navPoints.Keys) {
                    if (navid.Contains(flagID)) {
                        foreach (NavPoint.Neighbor neigh in navPoints[__closestNavpointID__].NGP)
                            if (neigh.Id == navid) {
                                __selectedNavpoint__ = navPoints[navid];
                                break;
                            }
                        if (__selectedNavpoint__ != navPoints[navid])
                            select_navpoint();
                        break;
                    }
                }
            return true;
        }

        /*
         * 
         * ACTIONS
         * 
         */

        [ExecutableAction("nav_select_navpoint")]
        public bool select_navpoint() {
            if (_debug_)
                Console.Out.WriteLine("in nav_select_navpoint");
            return select_navpoint("");
        }

        [ExecutableAction("nav_select_enemy_flag")]
        public bool select_enemy_flag() {
            if (_debug_)
                Console.Out.WriteLine("in nav_select_enemy_flag");
            return select_flag(false);
        }

        [ExecutableAction("nav_select_own_flag")]
        public bool select_own_flag() {
            if (_debug_)
                Console.Out.WriteLine("in nav_select_own_flag");
            return select_flag(true);
        }

        [ExecutableAction("nav_retrace_navpoint")]
        public bool retrace_navpoint() {
            directionWeight = -1;

            if (_debug_)
                Console.Out.WriteLine("in nav_retrace_navpoint");

            if (__closestNavpointID__ == "")
                return false;

            string mostVisitedNav = "";
            int count = -10;

            foreach (NavPoint.Neighbor neigh in navPoints[__closestNavpointID__].NGP) {
                if (mostVisitedNav == "") {
                    mostVisitedNav = neigh.Id;
                    if (navPointHistory.ContainsKey(neigh.Id))
                        count = navPointHistory[neigh.Id];
                } else {
                    if (navPointHistory.ContainsKey(neigh.Id) && navPointHistory[neigh.Id] > count) {
                        mostVisitedNav = neigh.Id;
                        count = navPointHistory[neigh.Id];
                    }
                }
            }

            if (mostVisitedNav == "") {
                __selectedNavpoint__ = null;
                return false;
            }

            __selectedNavpoint__ = navPoints[mostVisitedNav];
            if (_debug_)
                Console.Out.WriteLine("retrace: select " + __selectedNavpoint__.Id);

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
        public bool at_own_base() {
            if (_debug_)
                Console.Out.WriteLine("in at_own_base");

            if (!GetBot().info.ContainsKey("Location"))
                return false;
            if (GetMovement().info.ourFlagInfo.ContainsKey("Location")
                && Vector3.ConvertToVector3(GetMovement().info.ourFlagInfo["Location"]).
                Distance2DFrom(Vector3.ConvertToVector3(GetBot().info["Location"]),
                               Vector3.Orientation.XY) < __deviation__)
                return true;

            return false;
        }


        /// <summary>
        /// sense to check if a target is currently selected
        /// </summary>
        /// <returns><c>true</c>, if target was selecteded, <c>false</c> otherwise.</returns>
        [ExecutableSense("nav_is_target_selected")]
        public bool nav_is_target_selected() {
            if (_debug_)
                Console.Out.WriteLine("in nav_is_target_selected");

            if (__selectedNavpoint__ != null && __selectedNavpoint__.Location != Vector3.NullVector())
                return true;

            return false;
        }

        [ExecutableSense("nav_has_reached_target")]
        public bool nav_has_reached_target() {
            if (_debug_)
                Console.Out.WriteLine("in nav_reached_target");
            if (__selectedNavpoint__ != null
                && __selectedNavpoint__.Location != Vector3.NullVector()
                && __selectedNavpoint__.Distance2DFrom(Vector3.ConvertToVector3(GetBot().info["Location"]), Vector3.Orientation.XY) < __deviation__)
                return true;
            return false;
        }

        /// <summary>
        /// sense to find the closest navpoint to current location or to specific location
        /// </summary>
        [ExecutableSense("nav_is_close_to_navpoint")]
        public bool nav_is_close_to_navpoint() {
            if (_debug_)
                Console.Out.WriteLine("in nav_close_navpoint");

            NavPoint target = null;
            float distance;

            if (!GetBot().info.ContainsKey("Location") || navPoints.Count < 1)
                return false;

            Vector3 location = Vector3.ConvertToVector3(GetBot().info["Location"]);

            if (navPoints.ContainsKey(__closestNavpointID__))
                if (location.Distance2DFrom(navPoints[__closestNavpointID__].Location, Vector3.Orientation.XY) < __deviation__
                    && closestNavPointReachable.ContainsKey(__closestNavpointID__)
                    && closestNavPointReachable[__closestNavpointID__])
                    return true;
            foreach (KeyValuePair<string, NavPoint> nav in navPoints)
                if (nav.Value != null && nav.Key != null) {
                    target = nav.Value;
                    break;
                }
            distance = location.Distance2DFrom(target.Location, Vector3.Orientation.XY);

            foreach (NavPoint nav in navPoints.Values)
                if (location.Distance2DFrom(nav.Location, Vector3.Orientation.XY) < distance) {
                    target = nav;
                    distance = location.Distance2DFrom(nav.Location, Vector3.Orientation.XY);
                }

            __closestNavpointID__ = target.Id;
            if (target == null)
                return false;
            else
                GetBot().SendIfNotPreviousMessage("CHECKREACH", new Dictionary<string, string> { { "Id", "Nav" + __closestNavpointID__ }, { "Target", __closestNavpointID__ } });

            return false;
        }

        /// <summary>
        /// Checks if the the selected navpoint is a neighbor of the current/closeby navpoint. Neighbor checks are based on the navgrid which is static and contains all useable paths
        /// </summary>
        [ExecutableSense("nav_is_selected_navpoint_reachable")]
        public bool nav_is_selected_navpoint_reachable() {
            if (_debug_)
                Console.Out.WriteLine("in nav_selected_navpoint_reachable");

            if (__selectedNavpoint__ == null || !navPoints.ContainsKey(__closestNavpointID__))
                return false;

            NavPoint selectedNav = __selectedNavpoint__;
            NavPoint currentNav = navPoints[__closestNavpointID__];

            if (currentNav.NGP.Count > 0)
                foreach (NavPoint.Neighbor neigh in currentNav.NGP)
                    if (neigh.Id == selectedNav.Id)
                        return true;
            return false;
        }

    }
}

