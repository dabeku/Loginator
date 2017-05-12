using Backend.Dao;
using Backend.Model;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Manager {
    public class ConfigurationDaoSettings : IConfigurationDao {

        public event EventHandler<EventArgs> OnConfigurationChanged;

        public Configuration Read() {
            Configuration configuration = new Configuration();

            string type = Settings.Default["Type"] as string;
            if (String.IsNullOrEmpty(type)) {
                type = "chainsaw";
                Settings.Default["Type"] = type;
            }
            if (type == "chainsaw") {
                configuration.LogType = LogType.CHAINSAW;
            } else if (type == "logcat") {
                configuration.LogType = LogType.LOGCAT;
            }

            string portChainsaw = Settings.Default["PortChainsaw"] as string;
            if (String.IsNullOrEmpty(portChainsaw)) {
                portChainsaw = "7071";
                Settings.Default["PortChainsaw"] = portChainsaw;
            }
            configuration.PortChainsaw = Convert.ToInt32(portChainsaw);

            string portLogcat = Settings.Default["portLogcat"] as string;
            if (String.IsNullOrEmpty(portLogcat)) {
                portLogcat = "7081";
                Settings.Default["PortLogcat"] = portLogcat;
            }
            configuration.PortLogcat = Convert.ToInt32(portLogcat);

            string logTimeFormat = Settings.Default["LogTimeFormat"] as string;
            if (String.IsNullOrEmpty(logTimeFormat)) {
                logTimeFormat = "convertToLocalTime";
                Settings.Default["LogTimeFormat"] = logTimeFormat;
            }
            if (logTimeFormat == "convertToLocalTime") {
                configuration.LogTimeFormat = LogTimeFormat.CONVERT_TO_LOCAL_TIME;
            } else if (logTimeFormat == "doNotChange") {
                configuration.LogTimeFormat = LogTimeFormat.DO_NOT_CHANGE;
            }

            Settings.Default.Save();

            return configuration;
        }

        public void Write(Configuration configuration) {

            if (configuration.LogType == LogType.CHAINSAW) {
                Settings.Default["Type"] = "chainsaw";
            } else if (configuration.LogType == LogType.LOGCAT) {
                Settings.Default["Type"] = "logcat";
            }

            Settings.Default["PortChainsaw"] = configuration.PortChainsaw.ToString();

            Settings.Default["PortLogcat"] = configuration.PortLogcat.ToString();

            if (configuration.LogTimeFormat == LogTimeFormat.CONVERT_TO_LOCAL_TIME) {
                Settings.Default["LogTimeFormat"] = "convertToLocalTime";
            } else if (configuration.LogTimeFormat == LogTimeFormat.DO_NOT_CHANGE) {
                Settings.Default["LogTimeFormat"] = "doNotChange";
            }

            if (OnConfigurationChanged != null) {
                OnConfigurationChanged(this, new EventArgs());
            }

            Settings.Default.Save();
        }
    }
}
