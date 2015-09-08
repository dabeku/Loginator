using Loginator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Loginator {
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window {
        public ConfigurationWindow() {
            InitializeComponent();
            DataContextChanged += Configuration_DataContextChanged;
            DataContext = new ConfigurationViewModel();
        }

        private void Configuration_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ConfigurationViewModel vm = e.NewValue as ConfigurationViewModel;
            if (vm != null) {
                vm.CloseAction = Close;
            }
        }
    }
}
