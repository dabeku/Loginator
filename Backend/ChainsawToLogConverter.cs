using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Backend.Model;

namespace Backend {

    public class ChainsawToLogConverter {

        private string log4j = "http://jakarta.apache.org/log4j";

        public IEnumerable<Log> Convert(string text) {

            Log log = new Log();

            NameTable nt = new NameTable();
            XmlNamespaceManager mgr = new XmlNamespaceManager(nt);
            mgr.AddNamespace("log4j", log4j);

            XmlParserContext pc = new XmlParserContext(nt, mgr, "", XmlSpace.Default);

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
                            log.Level = (string) eventEl.Attribute("level");
                            log.Message = (string) eventEl.Element(log4jNs + "message");
                            log.Exception = (string) eventEl.Element(log4jNs + "throwable");
                            log.Namespace = (string) eventEl.Attribute("logger");
                            log.Thread = (string) eventEl.Attribute("thread");
                            
                            var props = eventEl.Element(log4jNs + "properties").Elements(log4jNs + "data");
                            log.Properties = props.Select(m => new Property(m.Attribute("name").Value, m.Attribute("value").Value));

                            eventReader.Close();
                        }
                    }
                }
                xr.Close();
            }

            return new Log[] { log };
        }

        private double ConvertToUnixTimestamp(DateTime date) {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan sinceEpoch = date.ToUniversalTime() - epoch;
            return Math.Floor(sinceEpoch.TotalMilliseconds);
        }
    }
}
