using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using Posh_sharp.POSHBot.util;
using POSH.sys.strict;

namespace Posh_sharp.POSHBot {
    public class Movement : UTBehaviour {
        internal PositionsInfo info;
        string pathHomeId;
        string reachPathHomeId;
        string pathToEnemyBaseId;
        string reachPathToEnemyBaseID;

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

        public Movement(AgentBase agent) : base(agent, actions, senses) {
            this.info = new PositionsInfo();
            pathHomeId = "PathHome";
            reachPathHomeId = "ReachPathHome";
            pathToEnemyBaseId = "PathThere";
            reachPathToEnemyBaseID = "ReachPathThere";
        }


        /*
         * 
         * internal methods
         * 
         */

        /// <summary>
        /// updates the flag positions in PositionsInfo
        /// also updates details of bases, if relevant info sent
        /// the position of a flag is how we determine where the bases are
        /// </summary>
        /// <param name="values">Dictionary containing the Flag details</param>
        override internal void ReceiveFlagDetails(Dictionary<string, string> values) {
            // TODO: fix the mix of information in this method it should just contain relevant info


            if (GetBot().info == null || GetBot().info.Count < 1)
                return;
            // set flag stuff
            if (values["Team"] == GetBot().info["Team"]) {
                if (info.ourFlagInfo != null && info.ourFlagInfo.ContainsKey("Location"))
                    return;
                info.ourFlagInfo = values;
            } else {
                if (info.enemyFlagInfo != null && info.enemyFlagInfo.ContainsKey("Location"))
                    return;
                info.enemyFlagInfo = values;
            }

            if (values["State"] == "home")
                if (values["Team"] == GetBot().info["Team"])
                    info.ownBasePos = NavPoint.ConvertToNavPoint(values);
                else
                    info.enemyBasePos = NavPoint.ConvertToNavPoint(values);
        }

        /// <summary>
        /// if the 'ID' key is PathHome then it tells the bot how to get home.
        /// we need to turn the dictionary into a list, ordered by key ('0' ... 'n')
        /// at present other IDs are ignored
        /// </summary>
        /// <param name="valuesDict"></param>
        override internal void ReceivePathDetails(Dictionary<string, string> valuesDict) {
            if (!valuesDict.ContainsKey("Id"))
                return;
            if (valuesDict["Id"] == pathHomeId)
                info.pathHome = NavPoint.ConvertToPath(valuesDict);
            else if (valuesDict["Id"] == pathToEnemyBaseId)
                info.pathToEnemyBase = NavPoint.ConvertToPath(valuesDict);

            // if there's no 0 key we're being given an empty path, so set TooCloseForPath to the current time
            // so that we can check how old the information is later on
            if (!valuesDict.ContainsKey("0"))
                info.TooCloseForPath = TimerBase.CurrentTimeStamp();
            else
                info.TooCloseForPath = 0L;
        }

        /// <summary>
        /// used in validating the bot's path home or to the enemy flag
        /// if the thing has the right ID, then clear the relevant path if it's not reachable
        /// </summary>
        /// <param name="valuesDict"></param>
        override internal void ReceiveCheckReachDetails(Dictionary<string, string> valuesDict) {
            Console.Out.WriteLine("[INFO ] Recieved check reach details");

            if (!valuesDict.ContainsKey("Id"))
                return;

            if (valuesDict["Id"] == reachPathHomeId && valuesDict["Reachable"] == "False") {
                info.pathHome.Clear();
                Console.Out.WriteLine("Cleared PathHome");
            } else if (valuesDict["Id"] == reachPathToEnemyBaseID && valuesDict["Reachable"] == "False") {
                info.pathToEnemyBase.Clear();
                Console.Out.WriteLine("Cleared PathToEnemyBase");
            }
        }

        /// <summary>
        /// clean-up after dying
        /// </summary>
        override internal void ReceiveDeathDetails(Dictionary<string, string> value) {
            info.pathHome.Clear();
            info.pathToEnemyBase.Clear();
            info.visitedNavPoints.Clear();
            info.ourFlagInfo.Clear();
            info.enemyFlagInfo.Clear();
        }
        internal void SendMoveToLocation(Vector3 location) {
            SendMoveToLocation(location, true);
        }
        /// <summary>
        /// if the combatinfo class specifies that we need to remain focused on a player, send a relevant strafe command
        /// to move to the provided location.  Otherwise, a runto
        /// </summary>
        /// <param name="?"></param>
        /// <param name="performPrevCheck"></param>
        internal void SendMoveToLocation(Vector3 location, bool performPrevCheck) {
            // IMPORTANT-TODO: completely remodel this method as it uses stuff from a different behaviour it should not use.
            Tuple<string, Dictionary<string, string>> message;
            // expire focus id info if necessary FA

            if (GetCombat().info.GetFocusId() is Tuple<string, long>) {
                message = new Tuple<string, Dictionary<string, string>>("MOVE",
                    new Dictionary<string, string>()
                {
                    {"FirstLocation", location.ToString()},
                    {"FocusTarget", GetCombat().info.KeepFocusOnID.ToString()}
                });
            } else {
                GetBot().SendMessage("STOPSHOOT", new Dictionary<string, string>());
                message = new Tuple<string, Dictionary<string, string>>("MOVE",
                    new Dictionary<string, string>()
                {
                    {"FirstLocation", location.ToString()},
                });
            }
            if (performPrevCheck)
                GetBot().SendIfNotPreviousMessage(message.First, message.Second);
            else
                GetBot().SendMessage(message.First, message.Second);

        }

        private void SendGetPathHome() {
            if (_debug_)
                Console.Out.WriteLine("in SendGetPath");

            GetBot().SendIfNotPreviousMessage("GETPATH", new Dictionary<string, string>()
                {
                    // the ID allows us to match requests with answers
                    {"Location", info.ownBasePos.Location.ToString()}, {"Id", pathHomeId}
                });
        }
        /// <summary>
        /// returns 1 and sends a runto message for the provided location 
        /// if the DistanceTolerance check passes otherwise returns 0
        /// </summary>
        /// <param name="location"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        private bool ToKnownLocation(Dictionary<int, Vector3> location, int distanceTolerance) {
            if (location.Count == 0)
                // even though we failed, we return 1 so that it doesn't tail the list
                return true;
            if (location[0].Distance2DFrom(Vector3.ConvertToVector3(GetBot().info["Location"]), Vector3.Orientation.XY) > distanceTolerance) {
                Console.Out.WriteLine("DistanceTolerance check passed");
                Console.Out.WriteLine("About to send MOVE to");
                Console.Out.WriteLine(location[0].ToString());
                Console.Out.WriteLine("Current location");
                Console.Out.WriteLine(GetBot().info["Location"]);

                SendMoveToLocation(location[0], false);
                return true;
            } else
                return false;
        }

        ///
        /// SENSES
        /// 


        private bool atTargetLocation(NavPoint target, int distanceTolerance) {
            if (!GetBot().info.ContainsKey("Location"))
                return false;
            Vector3 location = Vector3.ConvertToVector3(GetBot().info["Location"]);

            if (target == null)
                return false;
            else {
                // this distance may need adjusting in future (we may also wish to consider the Z axis)
                if (location.DistanceFrom(target.Location) < distanceTolerance)
                    return true;
                else
                    return false;
            }
        }

        [ExecutableSense("mov_is_rotating")]
        public bool mov_is_rotating() {
            if (_debug_)
                Console.Out.WriteLine("in IsRotating");
            return GetBot().Turning();
        }

        [ExecutableSense("mov_is_walking")]
        public bool mov_is_walking() {
            if (_debug_)
                Console.Out.WriteLine("in is_walking");
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
            if (_debug_)
                Console.Out.WriteLine("in at_enemy_base");
            return atTargetLocation(info.enemyBasePos, 100);
        }

        /// <summary>
        /// returns 1 if we're near enough to our own base
        /// </summary>
        /// <returns></returns>
		[ExecutableSense("mov_is_at_own_base")]
        public bool mov_is_at_own_base() {
            if (_debug_)
                Console.Out.WriteLine("in at_own_base");
            return atTargetLocation(info.ownBasePos, 10);
        }

        /// <summary>
        /// returns 1 if we have a location for the enemy base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("mov_know_enemy_base_pos")]
        public bool mov_know_enemy_base_pos() {
            return (info.enemyBasePos != null) ? true : false;
        }

        /// <summary>
        /// returns 1 if we have a location for our own base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("mov_know_own_base_pos")]
        public bool mov_know_own_base_pos() {
            return (info.ownBasePos != null) ? true : false;
        }

        /// <summary>
        /// returns 1 if there's a reachable nav point in the bot's list which we're not already at
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("mov_reachable_nav_point")]
        public bool mov_reachable_nav_point() {
            Vector3 location;
            NavPoint navPoint;
            int distanceTolerance = 30; // how near we must be to be thought of as at the nav point

            if (GetBot().info.ContainsKey("Location"))
                location = Vector3.ConvertToVector3(GetBot().info["Location"]);
            else
                // if we don't know where we are, treat it as (0,0,0) 
                // as that will just mean we go to the nav point even if we're close by
                location = Vector3.NullVector();

            //Console.Out.WriteLine(location.ToString());

            // is there already a navpoint we're aiming for?

            if (info.chosenNavPoint != null) {
                navPoint = info.chosenNavPoint;
                if (navPoint.DistanceFrom(location) > distanceTolerance)
                    return true;
                else {
                    info.visitedNavPoints.Add(navPoint.Id);
                    info.chosenNavPoint = null;
                }
            }
            // now look at the list of navpoints the bot can see
            if (GetBot().navPoints == null || GetBot().navPoints.Count() < 1) {
                Console.Out.WriteLine("  no nav points");
                return false;
            } else {
                // nav_points is a list of tuples.  Each tuple contains an ID and a dictionary of attributes as defined in the API
                // Search for reachable nav points
                List<NavPoint> possibleNPs = new List<NavPoint>(GetReachableNavPoints(GetBot().navPoints.Values.ToArray(), distanceTolerance, location));

                // now work through this list of NavPoints until we find once that we haven't been to
                // or the one we've been to least often
                if (possibleNPs.Count == 0) {
                    Console.Out.WriteLine("    No PossibleNPs");
                    return false;
                } else {
                    info.chosenNavPoint = GetLeastOftenVisitedNavPoint(possibleNPs);
                    Console.Out.WriteLine("    Possible NP, returning 1");
                    return true;
                }
            }
        }

        /// <summary>
        /// not well optimised because it also checks each duplicate
        /// </summary>
        /// <param name="navPoints"></param>
        /// <returns></returns>
        internal NavPoint GetLeastOftenVisitedNavPoint(List<NavPoint> navPoints) {

            // FIX: corrected mistake in the original implementation which should have faulted the code

            int currentMin = info.visitedNavPoints.Count(g => g == navPoints[0].Id);
            NavPoint currentMinNP = navPoints[0];

            foreach (NavPoint point in navPoints) {
                int currentCount = info.visitedNavPoints.Count(g => g == point.Id);
                if (currentCount < currentMin) {
                    currentMin = currentCount;
                    currentMinNP = point;
                }
            }

            return currentMinNP;
        }

        internal NavPoint[] GetReachableNavPoints(NavPoint[] navPoints, int distanceTolerance, Vector3 target) {
            List<NavPoint> result = new List<NavPoint>();

            foreach (NavPoint currentNP in navPoints)
                if (currentNP.Reachable && currentNP.Distance2DFrom(target, Vector3.Orientation.XY) > distanceTolerance)
                    result.Add(currentNP);

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if the enemy flag is specified as reachable</returns>
        [ExecutableSense("mov_is_enemy_flag_reachable")]
        public bool mov_is_enemy_flag_reachable() {
            if (this.info.HasEnemyFlagInfoExpired())
                info.ExpireEnemyFlagInfo();

            // debug
            if (_debug_) {
                Console.Out.WriteLine("in EnemyFlagReachable");
                if (info.enemyFlagInfo.Count() > 0)
                    Console.Out.WriteLine("enemyFlaginfo " + info.enemyFlagInfo.ToString());
            }

            // Made simpler FA
            if (info.enemyFlagInfo.Count() > 0 && bool.Parse(info.enemyFlagInfo["Reachable"]))
                return true;

            return false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if our flag is specified as reachable</returns>
		[ExecutableSense("mov_is_our_flag_reachable")]
        public bool mov_is_our_flag_reachable() {
            if (this.info.HasOurFlagInfoExpired())
                info.ExpireOurFlagInfo();

            // debug
            if (_debug_) {
                Console.Out.WriteLine("in OurFlagReachable");
                if (info.ourFlagInfo.Count() > 0)
                    Console.Out.WriteLine("ourFlaginfo " + info.ourFlagInfo.ToString());
            }

            // Made simpler FA
            if (info.ourFlagInfo.Count() > 0 && bool.Parse(info.ourFlagInfo["Reachable"]))
                return true;

            return false;

        }

        [ExecutableSense("mov_see_enemy")]
        public bool mov_see_enemy() {
            if (GetBot().viewPlayers.Count == 0 || GetBot().info.Count == 0)
                return false;

            // work through, looking for an enemy
            string ourTeam = GetBot().info["Team"];
            UTPlayer[] players = GetBot().viewPlayers.Values.ToArray();
            foreach (UTPlayer currentPlayer in players)
                if (currentPlayer.Team != ourTeam) {
                    if (_debug_)
                        Console.Out.WriteLine("SeeEnemy: we can see an enemy!");
                    return true;
                }

            return false;
        }



        ///
        /// ACTIONS
        /// 

        [ExecutableAction("mov_move_to_navpoint", 1f)]
        public bool mov_move_to_navpoint() {
            if (_debug_)
                Console.Out.Write("moveto_navpoint:");
            if (GetNavigator().nav_is_target_selected()) {
                SendMoveToLocation(GetNavigator().GetSelectedNavpoint().Location);
                GetNavigator().MovedToNavpoint(GetNavigator().GetSelectedNavpoint());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the Bot from doing stuff
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_stop_bot")]
        public bool mov_stop_bot() {
            // print "StGetBot Bot"
            GetBot().SendMessage("STOP", new Dictionary<string, string>());

            return true;
        }

        /// <summary>
        /// Idleing
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_idle")]
        public bool mov_idle() {
            // print "Idleing ..GetBot            
            return GetBot().Inch();
        }

        /// <summary>
        /// Rotating the bot around itself (left or right)
        /// </summary>
        /// <param name="angle">If angle is not given random 90 degree turn is made</param>
        /// <returns></returns>
        [ExecutableAction("mov_rotate")]
        public bool mov_otate() {
            return mov_rotate(0);
        }

        protected bool mov_rotate(int angle) {
            // print "Rotate ..."
            if (angle != 0)
                GetBot().Turn(angle);
            else if (new Random().Next(2) == 0)
                // tuGetBott
                GetBot().Turn(90);
            else
                // turGetBott
                GetBot().Turn(-90);


            return true;
        }

        /// <summary>
        /// Turns the bot 160 degrees
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_big_rotate")]
        public bool mov_big_rotate() {
            // print "big rotate ..."

            return mov_rotate(160);
        }

        /// <summary>
        /// Runs to the chosen Navpoint
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("mov_walk_to_nav_point")]
        public bool mov_walk_to_nav_point() {
            if (_debug_)
                Console.Out.WriteLine("in WalkToNavPoint");
            SendMoveToLocation(info.chosenNavPoint.Location);

            return true;
        }
    }

}