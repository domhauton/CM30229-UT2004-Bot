using System;
using Posh_sharp.POSHBot.util;
using POSH.sys;
using System.Collections.Generic;
using POSH.sys.annotations;
using System.Reflection;
using static Posh_sharp.POSHBot.util.NavPoint;

namespace Posh_sharp.POSHBot {
    public class AdvancedNavigator : AdvancedUTBehaviour {

        private readonly FieldInfo navPointsField;
        private readonly FieldInfo closestNavPointField;

        private NavPoint ourRoughFlagPosition;
        private NavPoint theirRoughFlagPosition;

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
            closestNavPointField = fieldsType.GetField("__closestNavpointID__", BindingFlags.NonPublic | BindingFlags.Instance);
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
            if(ourRoughFlagPosition == null || theirRoughFlagPosition == null) {
                NavPoint ourNavPoint = GetClosestNavPoint();
                Tuple<NavPoint, NavPoint> result = CalcCloseAndFurthestNav(ourNavPoint, 3);
                ourRoughFlagPosition = result.First;
                theirRoughFlagPosition = CalcCloseAndFurthestNav(result.Second, 3).First;
            }
            
             NavPoint nextBestNavPoint = AStarPathFinder(theirRoughFlagPosition.Id);
             GetNavigator().select_navpoint(nextBestNavPoint.Id);
            return true;
        }

        [ExecutableAction("nav_select_our_base")]
        public bool SetOurBaseAsNavPoint() {
            if (ourRoughFlagPosition == null || theirRoughFlagPosition == null) {
                NavPoint ourNavPoint = GetClosestNavPoint();
                Tuple<NavPoint, NavPoint> result = CalcCloseAndFurthestNav(ourNavPoint, 3);
                ourRoughFlagPosition = result.First;
                theirRoughFlagPosition = CalcCloseAndFurthestNav(result.Second, 3).First;
            }

            NavPoint nextBestNavPoint = AStarPathFinder(ourRoughFlagPosition.Id);
            GetNavigator().select_navpoint(nextBestNavPoint.Id);
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

        /// <summary>
        /// Calculates rough flag positions based on spawn point.
        /// </summary>
        private Tuple<NavPoint, NavPoint> CalcCloseAndFurthestNav(NavPoint startNavPoint, int closeDistance) {
            Navigator navigator = GetNavigator();
            Dictionary<string, NavPoint> navPoints = (Dictionary<string, NavPoint>)navPointsField.GetValue(navigator);

            String closestNavPointKey = startNavPoint.Id;
            NavPoint closestNavPoint = startNavPoint;

            HashSet<String> allNavPointIds = new HashSet<String>(navPoints.Keys);

            HashSet<String> usedNavPointIds = new HashSet<String>();
            usedNavPointIds.Add(closestNavPoint.Id);

            List<Tuple<NavPoint, NavPoint, int>> currentNavPointPairList = new List<Tuple<NavPoint, NavPoint, int>>();

            foreach (Neighbor navPoint in closestNavPoint.NGP) {
                String id = navPoint.Id;
                NavPoint neighbourNavPoint = navPoints[id];
                usedNavPointIds.Add(id);
                currentNavPointPairList.Add(new Tuple<NavPoint, NavPoint, int>(neighbourNavPoint, neighbourNavPoint, 0));
            }

            Tuple<NavPoint, NavPoint> returnVal = new Tuple<NavPoint, NavPoint>(closestNavPoint, closestNavPoint);

            NavPoint closeNavPoint = closestNavPoint;
            NavPoint furthestNavPoint = closestNavPoint;

            while (!usedNavPointIds.SetEquals(allNavPointIds) || currentNavPointPairList.Count == 0) {
                List<Tuple<NavPoint, NavPoint, int>> newNavPointSet = new List<Tuple<NavPoint, NavPoint, int>>();
                foreach (Tuple<NavPoint, NavPoint, int> navPointTuple in currentNavPointPairList) {
                    foreach (Neighbor navPoint in navPointTuple.Second.NGP) {
                        String id = navPoint.Id;
                        NavPoint neighbourNavPoint = navPoints[id];
                        if (navPointTuple.Third == closeDistance) {
                            closeNavPoint = navPointTuple.Second;
                        }
                        if (!usedNavPointIds.Contains(id)) {
                            usedNavPointIds.Add(id);
                            furthestNavPoint = neighbourNavPoint;
                            newNavPointSet.Add(new Tuple<NavPoint, NavPoint, int>(navPointTuple.First, neighbourNavPoint, navPointTuple.Third+1));
                        }
                    }
                }
                currentNavPointPairList = newNavPointSet;
            }

            return new Tuple<NavPoint, NavPoint>(closeNavPoint, furthestNavPoint);
        }

        private NavPoint GetClosestNavPoint() {
            Navigator navigator = GetNavigator();
            Dictionary<string, NavPoint> navPoints = (Dictionary<string, NavPoint>)navPointsField.GetValue(navigator);
            navigator.close_navpoint();
            String closestNavPointKey = (String)closestNavPointField.GetValue(navigator);
            return navPoints[closestNavPointKey];
        }

        private NavPoint AStarPathFinder(String searchForNavpoint) {
            Navigator navigator = GetNavigator();
            Dictionary<string, NavPoint> navPoints = (Dictionary<string, NavPoint>) navPointsField.GetValue(navigator);
            navigator.close_navpoint();
            String closestNavPointKey = (String)closestNavPointField.GetValue(navigator);
            NavPoint closestNavPoint = navPoints[closestNavPointKey];

            if (searchForNavpoint.Equals(closestNavPointKey)) {
                return closestNavPoint;
            }

            HashSet<String> allNavPointIds = new HashSet<String>(navPoints.Keys); 

            HashSet<String> usedNavPointIds = new HashSet<String>();
            usedNavPointIds.Add(closestNavPoint.Id);

            List<Tuple<NavPoint, NavPoint>> currentNavPointPairList = new List<Tuple<NavPoint, NavPoint>>();

            foreach(Neighbor navPoint in closestNavPoint.NGP) {
                String id = navPoint.Id;
                NavPoint neighbourNavPoint = navPoints[id];
                if (id.Equals(searchForNavpoint)) {
                    return neighbourNavPoint;
                }
                usedNavPointIds.Add(id);
                currentNavPointPairList.Add(new Tuple<NavPoint, NavPoint>(neighbourNavPoint, neighbourNavPoint));
            }

            while(!usedNavPointIds.SetEquals(allNavPointIds) || currentNavPointPairList.Count == 0) {
                List<Tuple<NavPoint, NavPoint>> newNavPointSet = new List<Tuple<NavPoint, NavPoint>>();
                foreach (Tuple<NavPoint, NavPoint> navPointTuple in currentNavPointPairList) {
                    foreach (Neighbor navPoint in navPointTuple.Second.NGP) {
                        String id = navPoint.Id;
                        NavPoint neighbourNavPoint = navPoints[id];
                        if (id.Equals(searchForNavpoint)) {
                            return navPointTuple.First;
                        } else if (!usedNavPointIds.Contains(id)) {
                            usedNavPointIds.Add(id);
                            newNavPointSet.Add(new Tuple<NavPoint, NavPoint>(navPointTuple.First, neighbourNavPoint));
                        }
                    }
                }
                currentNavPointPairList = newNavPointSet;
            }
          
            return closestNavPoint;
        }

    }
}

