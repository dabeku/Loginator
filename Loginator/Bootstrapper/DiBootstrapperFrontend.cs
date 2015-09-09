using Backend.Bootstrapper;
using LogApplication.ViewModels;
using Loginator.ViewModels;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loginator.Bootstrapper {
    public static class DiBootstrapperFrontend {
        public static void Initialize(IContainer container) {
            Console.WriteLine("Bootstrapping DI: Frontend");
            container.Configure(m => {
                m.For<LoginatorViewModel>().Singleton().Use<LoginatorViewModel>();
                m.For<ConfigurationViewModel>().Use<ConfigurationViewModel>();
            });
            DiBootstrapperBackend.Initialize(container);
        }
    }
}
