using Common.Configuration;
using LogApplication.ViewModels;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loginator.ViewModels {

    public class ViewModelLocator {

        static ViewModelLocator() {
            ServiceLocator.SetLocatorProvider(() => IoC.Default);
        }

        public LoginatorViewModel LoginatorViewModel {
            get { return ServiceLocator.Current.GetInstance<LoginatorViewModel>(); }
        }

        public ConfigurationViewModel ConfigurationViewModel {
            get { return ServiceLocator.Current.GetInstance<ConfigurationViewModel>(); }
        }
    }
}
