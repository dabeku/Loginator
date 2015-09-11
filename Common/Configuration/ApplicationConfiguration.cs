using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Configuration {
    public class ApplicationConfiguration : IApplicationConfiguration {

        public bool IsMessageTraceEnabled {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings[ConfigurationConstants.KEY_IS_MESSAGE_TRACE_ENABLED]); }
        }

        public bool IsTimingTraceEnabled {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings[ConfigurationConstants.KEY_IS_TIMING_TRACE_ENABLED]); }
        }
    }
}
