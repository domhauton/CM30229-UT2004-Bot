using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using Posh_sharp.POSHBot.util;
using POSH.sys.strict;
using System.Net.Sockets;
//import utilityfns

namespace Posh_sharp.POSHBot
{
    public class AdvancedCombat : AdvancedUTBehaviour
    {

        // You must list all actions here
        private readonly static string[] actions = new string[] {
            //"com_shoot_enemy_carrying_our_flag",
            //"com_run_to_enemy_carrying_our_flag",
            //"com_face_attacker",
            //"com_set_attacker",
            //"com_shoot_attacker"
        };

        // You must list all senses here
        private readonly static string[] senses = new string[] {
            //"com_can_see_enemy_with_flag",
            //"com_is_our_flag_on_ground",
            //"com_is_enemy_flag_on_ground",
            //"com_is_projectile_incoming",
            //"com_has_taken_damage_from_specific_player",
            //"com_has_taken_damage"
        };

        public AdvancedCombat(AgentBase agent) : base(agent, actions, senses)
        {
            // Empty constructor
        }

        /*
         * SENSES
         */

        [ExecutableSense("com_can_see_enemy_with_flag")]
        public bool com_can_see_enemy_with_flag()
        {
            return GetCombat().SeeEnemyWithOurFlag();
        }

        [ExecutableSense("com_is_our_flag_on_ground")]
        public bool com_is_our_flag_on_ground()
        {
            return GetCombat().OurFlagOnGround();
        }

        [ExecutableSense("com_is_enemy_flag_on_ground")]
        public bool com_is_enemy_flag_on_ground()
        {
            return GetCombat().EnemyFlagOnGround();
        }

        [ExecutableSense("com_is_projectile_incoming")]
        public bool com_is_projectile_incoming()
        {
            return GetCombat().IncomingProjectile();
        }

        [ExecutableSense("com_has_taken_damage_from_specific_player")]
        public bool com_has_taken_damage_from_specific_player()
        {
            return GetCombat().TakenDamageFromSpecificPlayer();
        }
        /// <summary>
        /// expire damage info if necassary FA
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("com_has_taken_damage")]
        public bool com_has_taken_damage()
        {
            return GetCombat().TakenDamage();
        }

        /*
         * ACTIONS 
         */

        [ExecutableAction("com_shoot_enemy_carrying_our_flag")]
        public bool com_shoot_enemy_carrying_our_flag()
        {
            return GetCombat().ShootEnemyCarryingOurFlag();
        }

        [ExecutableAction("com_run_to_enemy_carrying_our_flag")]
        public bool com_run_to_enemy_carrying_our_flag()
        {
            return GetCombat().RunToEnemyCarryingOurFlag();
        }

        /// <summary>
        /// we can see the player currently, store his ID so e.g. runtos will be replaced 
        /// by strafes to keep him in focus and issue a turnto command
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("com_face_attacker")]
        public bool com_face_attacker()
        {
            return GetCombat().FaceAttacker();
        }

        /// <summary>
        /// sets the attacker (i.e. the keepfocuson one) to be the first enemy player we have seen
        /// or the instigator of the most recent damage, if we know who that is
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("com_set_attacker")]
        public bool com_set_attacker()
        {
            return GetCombat().SetAttacker();
        }

        [ExecutableAction("com_shoot_attacker")]
        public bool com_shoot_attacker()
        {
            return GetCombat().ShootAttacker();
        }

    }
}     

    
