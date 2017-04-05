using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;

namespace Posh_sharp.POSHBot.util {
    public class AdvancedUTBehaviour : UTBehaviour {
        public AdvancedUTBehaviour(AgentBase agent, string[] actions, string[] senses) : base(agent, actions, senses) {

        }

        protected AdvancedMovement GetAdvancedMovement() {
            return ((AdvancedMovement) agent.getBehaviour("AdvancedMovement"));
        }

        protected AdvancedCombat GetAdvancedCombat() {
            return ((AdvancedCombat) agent.getBehaviour("AdvancedCombat"));
        }

        protected AdvancedNavigator GetAdvancedNavigator() {
            return ((AdvancedNavigator) agent.getBehaviour("AdvancedNavigator"));
        }
    }
}
