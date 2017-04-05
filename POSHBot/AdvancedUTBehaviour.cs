using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using System.Reflection;
using System.IO;

namespace Posh_sharp.POSHBot.util {
    public class AdvancedUTBehaviour : UTBehaviour {

        private readonly FieldInfo writerField;

        public AdvancedUTBehaviour(AgentBase agent, string[] actions, string[] senses) : base(agent, actions, senses) {
            Type fieldsType = typeof(POSHBot);
            writerField = fieldsType.GetField("writer", BindingFlags.NonPublic | BindingFlags.Instance);
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

        public bool SendMessageAsync(string command, Dictionary<string, string> dictionary) {
            string output = command;

            foreach (KeyValuePair<string, string> item in dictionary) {
                output += " {" + item.Key + " " + item.Value + "}";
            }
            // print "About to send " + string
            output += "\r\n";

            try {
                POSHBot poshBot = GetBot();
                StreamWriter writer = (StreamWriter) writerField.GetValue(poshBot);

                writer.Write(output);
                writer.Flush();
            } catch (Exception) {
                log.Error(string.Format("Message : {0} unable to send", output));
                return false;
            }

            return true;
        }
    }
}
