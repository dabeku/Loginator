using Common.Exceptions;
using Microsoft.Practices.ServiceLocation;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Configuration {
    public class IoC : ServiceLocatorImplBase {

        private static IoC defaultIoC;

        public static Container Container = new Container();

        public static IoC Default {
            get { return defaultIoC ?? (defaultIoC = new IoC()); }
        }

        public static T Get<T>() {
            try {
                return Container.GetInstance<T>();
            } catch (Exception e) {
                throw new LoginatorException("Could not setup configuration", e);
            }
        }

        protected override object DoGetInstance(Type serviceType, string key) {
            try {
                return Container.GetInstance(serviceType);
            } catch (Exception e) {
                throw new LoginatorException("Could not setup configuration for single instance of type: " + serviceType, e);
            }
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType) {
            try {
                return (IEnumerable<object>)Container.GetAllInstances(serviceType);
            } catch (Exception e) {
                throw new LoginatorException("Could not setup configuration for all instances of type: " + serviceType, e);
            }
        }
    }
}
