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
using Common;

namespace Backend.Converter {

    public class ChainsawToLogConverter : ILogConverter {

        private ILogger Logger { get; set; }
        private string log4j = "http://jakarta.apache.org/log4j";
        private string nlog = "http://www.nlog-project.org/schemas/NLog.xsd";

        public ChainsawToLogConverter() {
            Logger = LogManager.GetCurrentClassLogger();
        }

        /*
            <log4j:event logger="WorldDirect.ChimneySweeper.Server.ChimneyService.BaseApplication" level="INFO" timestamp="1439817232886" thread="1">
	            <log4j:message>Starting Rauchfangkehrer</log4j:message>
	            <log4j:properties>
		            <log4j:data name="log4japp" value="Server.ChimneyService.exe(8428)" />
		            <log4j:data name="log4jmachinename" value="DKU" />
	            </log4j:properties>
            </log4j:event>
        */

        public IEnumerable<Log> Convert(string text) {
            try {
                Log log = new Log();

                NameTable nt = new NameTable();
                //XmlNamespaceManager mgr = new XmlNamespaceManager(nt);
                //mgr.AddNamespace("log4j", log4j);
                //mgr.AddNamespace("nlog", nlog);

                //XmlParserContext pc = new XmlParserContext(nt, mgr, String.Empty, XmlSpace.Default);

                XNamespace log4jNs = log4j;

                XmlReaderSettings settings = new XmlReaderSettings() {
                    ConformanceLevel = ConformanceLevel.Fragment
                };

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                var doc = new XmlDocument();

                using (var sr = new StringReader(text)) {
                    using (var xtr = new XmlTextReader(sr) { Namespaces = false }) {
                        doc.Load(xtr);
                    }
                }

                if (doc.ChildNodes.Count == 1) {
                    XmlNode root = doc.ChildNodes[0];
                    if (root.NodeType == XmlNodeType.Element) {
                        var attributes = root.Attributes;
                        XmlNode attribute = attributes.GetNamedItem("logger");
                        if (attribute != null) {
                            log.Namespace = attribute.Value;
                        }
                        attribute = attributes.GetNamedItem("level");
                        if (attribute != null) {
                            log.Level = LoggingLevel.FromName(attribute.Value);
                        }
                        attribute = attributes.GetNamedItem("timestamp");
                        if (attribute != null) {
                            var timespan = TimeSpan.FromMilliseconds(Int64.Parse(attribute.Value));
                            // TODO: Find out why we have to do this here (maybe UTC conversion?)
                            timespan.Add(TimeSpan.FromHours(2));
                            log.Timestamp = new DateTime(1970, 1, 1).Add(timespan);
                        }
                        attribute = attributes.GetNamedItem("thread");
                        if (attribute != null) {
                            log.Thread = attribute.Value;
                        }
                        foreach (XmlNode child in root.ChildNodes) {
                            if (child.NodeType != XmlNodeType.Element) {
                                continue;
                            }
                            if (child.Name.EndsWith("message")) {
                                log.Message = child.InnerText;
                            }
                            if (child.Name.EndsWith("throwable")) {
                                log.Exception = child.InnerText;
                            }
                            if (child.Name.EndsWith("properties")) {
                                IList<Property> properties = new List<Property>();
                                foreach (XmlNode property in child.ChildNodes) {
                                    properties.Add(new Property(property.Attributes.GetNamedItem("name").Value, property.Attributes.GetNamedItem("value").Value));
                                }
                                log.Properties = properties;

                                var application = log.Properties.FirstOrDefault(m => m.Name == "log4japp");
                                log.Application = application == null ? Constants.APPLICATION_GLOBAL : application.Value;

                                var context = log.Properties.Where(m => !m.Name.StartsWith("log4j")).Select(m => m.Name + ": " + m.Value);
                                log.Context = String.Join(", ", context);
                            }
                        }

                        //var timespan = TimeSpan.FromMilliseconds((long)eventEl.Attribute("timestamp"));
                        //        log.Timestamp = new DateTime(1970, 1, 1).Add(timespan);
                        //        log.Level = (string)eventEl.Attribute("level");
                        //        log.Message = (string)eventEl.Element(log4jNs + "message");
                        //        log.Exception = (string)eventEl.Element(log4jNs + "throwable");
                        //        log.Namespace = (string)eventEl.Attribute("logger");
                        //        log.Thread = (string)eventEl.Attribute("thread")
                    }
                }


                //using (XmlReader xr = XmlReader.Create(stream, settings, pc)) {
                //    while (xr.Read()) {
                //        if (xr.NodeType == XmlNodeType.Element && xr.LocalName == "event") {
                //            using (XmlReader eventReader = xr.ReadSubtree()) {
                //                eventReader.Read();
                //                XElement eventEl = XNode.ReadFrom(eventReader) as XElement;
                                

                //                var timespan = TimeSpan.FromMilliseconds((long)eventEl.Attribute("timestamp"));
                //                log.Timestamp = new DateTime(1970, 1, 1).Add(timespan);
                //                log.Level = (string)eventEl.Attribute("level");
                //                log.Message = (string)eventEl.Element(log4jNs + "message");
                //                log.Exception = (string)eventEl.Element(log4jNs + "throwable");
                //                log.Namespace = (string)eventEl.Attribute("logger");
                //                log.Thread = (string)eventEl.Attribute("thread");

                //                var props = eventEl.Element(log4jNs + "properties").Elements(log4jNs + "data");
                //                log.Properties = props.Select(m => new Property(m.Attribute("name").Value, m.Attribute("value").Value));

                //                var application = log.Properties.FirstOrDefault(m => m.Name == "log4japp");
                //                log.Application = application == null ? null : application.Value;

                //                var context = log.Properties.Where(m => !m.Name.StartsWith("log4j")).Select(m => m.Name + ": " + m.Value);
                //                log.Context = String.Join(", ", context);

                //                eventReader.Close();
                //            }
                //        }
                //    }
                //    xr.Close();
                //}
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
