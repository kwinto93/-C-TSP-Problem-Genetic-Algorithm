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
using Microsoft.Win32;
using Tsp.Controllers;
using Tsp.ViewModels;

namespace Tsp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OptionsViewModel _viewModel;
        private GeneticController _geneticController;
        private ProgressWindow _progressWindow;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new OptionsViewModel();
            this.DataContext = _viewModel;

            _geneticController = new GeneticController();
        }

        private void ButtonLoadData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            Nullable<bool> result = openFileDialog.ShowDialog(this);

            if (result == true)
            {
                LoadTspFileController loadTspFile = new LoadTspFileController(openFileDialog.FileName);
                loadTspFile.OnLoadingFinishedEvent += loadTspFile_OnLoadingFinishedEvent;

                _progressWindow = new ProgressWindow();
                _progressWindow.Owner = this;
                _progressWindow.ListenChangesFrom(loadTspFile);
                _progressWindow.Show();

                Task.Run(() => loadTspFile.GetCities());
            }
        }

        void loadTspFile_OnLoadingFinishedEvent(List<Models.CityModel> cities)
        {
            _geneticController.CityModels = cities;

            _progressWindow.Dispatcher.Invoke(() => _progressWindow.Close());
        }
    }
}
