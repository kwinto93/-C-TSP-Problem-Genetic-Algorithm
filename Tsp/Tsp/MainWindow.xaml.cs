using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using Tsp.Models;
using Tsp.ViewModels;

namespace Tsp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OptionsViewModel _viewModel;
        private GeneticAlgorithmController _geneticAlgorithmController;
        private ProgressWindow _progressWindow;
        private CancellationTokenSource _algorithmCancellationToken;
        private int _lastBest;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new OptionsViewModel();
            this.DataContext = _viewModel;

            _geneticAlgorithmController = new GeneticAlgorithmController(_viewModel);
        }

        private void DisableAllControls()
        {
            bool[] controlsBools = new bool[_viewModel.ControlsEnableBools.Length];
            for (int i = 0; i < _viewModel.ControlsEnableBools.Length; i++)
            {
                controlsBools[i] = false;
            }
            _viewModel.ControlsEnableBools = controlsBools;
        }

        private void EnableAllControls()
        {
            bool[] controlsBools = new bool[_viewModel.ControlsEnableBools.Length];
            for (int i = 0; i < _viewModel.ControlsEnableBools.Length; i++)
            {
                controlsBools[i] = true;
            }
            _viewModel.ControlsEnableBools = controlsBools;
        }

        private void ClearAll()
        {
            _geneticAlgorithmController = new GeneticAlgorithmController(_geneticAlgorithmController.CityModels, _viewModel);
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
            _geneticAlgorithmController.CityModels = cities;
            _geneticAlgorithmController.MinimalizeCoords();

            DrawingCitiesController drawingCities = new DrawingCitiesController(_geneticAlgorithmController.CityModels.ToArray());
            _progressWindow.Dispatcher.Invoke(() =>
            {
                _progressWindow.Close();
                drawingCities.DrawPoints(MapGrid);
            });
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            DisableAllControls();

            _geneticAlgorithmController.OnAlgorithmStateHasChangedEvent += _geneticAlgorithmController_OnAlgorithmStateHasChangedEvent;
            _geneticAlgorithmController.OnAlgorithmFinishedEvent += _geneticAlgorithmController_OnAlgorithmFinishedEvent;

            _algorithmCancellationToken = new CancellationTokenSource();
            Task.Factory.StartNew(() => _geneticAlgorithmController.DoAlgorithm(_algorithmCancellationToken.Token), _algorithmCancellationToken.Token);
        }

        void _geneticAlgorithmController_OnAlgorithmFinishedEvent()
        {
            EnableAllControls();
            ClearAll();
        }

        void _geneticAlgorithmController_OnAlgorithmStateHasChangedEvent(int progress, Individual bestInd, int bestGenNum)
        {
            DrawingCitiesController drawingRoutes = new DrawingCitiesController(bestInd.CityModels);
            this.Dispatcher.Invoke(() =>
            {
                if (!_algorithmCancellationToken.IsCancellationRequested)
                    _viewModel.ProgressBarValue = progress;

                if (bestGenNum != _lastBest)
                {
                    drawingRoutes.DrawRoutes(MapGrid);
                    _viewModel.BestInfo = new[] { bestInd.OverallDistance.ToString(), bestGenNum.ToString() };
                }
            });
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            EnableAllControls();
            if (_algorithmCancellationToken != null)
                _algorithmCancellationToken.Cancel();
            _viewModel.ProgressBarValue = 0;

            ClearAll();
        }
    }
}
