using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tsp.ViewModels;

namespace Tsp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OptionsViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new OptionsViewModel();
            this.DataContext = _viewModel;
        }

        private void ButtonLoadData_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
