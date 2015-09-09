using Backend.Dao;
using Backend.Manager;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Bootstrapper {
    public static class DiBootstrapperBackend {
        public static void Initialize(IContainer container) {
            Console.WriteLine("Bootstrapping DI: Backend");
            container.Configure(m => {
                m.For<Receiver>().Singleton().Use<Receiver>();
                m.For<IConfigurationDao>().Singleton().Use<ConfigurationDaoSettings>();
            });
        }
    }
}
