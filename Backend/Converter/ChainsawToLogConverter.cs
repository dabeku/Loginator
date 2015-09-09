using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Backend.Model;
using NLog;

namespace Backend.Converter {

    public class ChainsawToLogConverter : ILogConverter {

        private ILogger Logger { get; set; }
        private string log4j = "http://jakarta.apache.org/log4j";
        private string nlog = "http://www.nlog-project.org/schemas/NLog.xsd";

        public ChainsawToLogConverter() {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public IEnumerable<Log> Convert(string text) {
            try {
                Log log = new Log();

                NameTable nt = new NameTable();
                XmlNamespaceManager mgr = new XmlNamespaceManager(nt);
                mgr.AddNamespace("log4j", log4j);
                mgr.AddNamespace("nlog", nlog);

                XmlParserContext pc = new XmlParserContext(nt, mgr, String.Empty, XmlSpace.Default);

                XNamespace log4jNs = log4j;

                XmlReaderSettings settings = new XmlReaderSettings() {
                    ConformanceLevel = ConformanceLevel.Fragment
                };

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                using (XmlReader xr = XmlReader.Create(stream, settings, pc)) {
                    while (xr.Read()) {
                        if (xr.NodeType == XmlNodeType.Element && xr.LocalName == "event") {
                            using (XmlReader eventReader = xr.ReadSubtree()) {
                                eventReader.Read();
                                XElement eventEl = XNode.ReadFrom(eventReader) as XElement;
                                /*
    <log4j:event logger="WorldDirect.ChimneySweeper.Server.ChimneyService.BaseApplication" level="INFO" timestamp="1439817232886" thread="1">
	    <log4j:message>Starting Rauchfangkehrer</log4j:message>
	    <log4j:properties>
		    <log4j:data name="log4japp" value="Server.ChimneyService.exe(8428)" />
		    <log4j:data name="log4jmachinename" value="DKU" />
	    </log4j:properties>
    </log4j:event>
                                */

                                var timespan = TimeSpan.FromMilliseconds((long)eventEl.Attribute("timestamp"));
                                log.Timestamp = new DateTime(1970, 1, 1).Add(timespan);
                                log.Level = (string)eventEl.Attribute("level");
                                log.Message = (string)eventEl.Element(log4jNs + "message");
                                log.Exception = (string)eventEl.Element(log4jNs + "throwable");
                                log.Namespace = (string)eventEl.Attribute("logger");
                                log.Thread = (string)eventEl.Attribute("thread");

                                var props = eventEl.Element(log4jNs + "properties").Elements(log4jNs + "data");
                                log.Properties = props.Select(m => new Property(m.Attribute("name").Value, m.Attribute("value").Value));

                                var application = log.Properties.FirstOrDefault(m => m.Name == "log4japp");
                                log.Application = application == null ? null : application.Value;

                                eventReader.Close();
                            }
                        }
                    }
                    xr.Close();
                }
                return new Log[] { log };
            } catch (Exception e) {
                Logger.Error(e, "Could not read chainsaw data");
            }
            return new Log[] { Log.DEFAULT };
        }

        private double ConvertToUnixTimestamp(DateTime date) {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan sinceEpoch = date.ToUniversalTime() - epoch;
            return Math.Floor(sinceEpoch.TotalMilliseconds);
        }
    }
}
