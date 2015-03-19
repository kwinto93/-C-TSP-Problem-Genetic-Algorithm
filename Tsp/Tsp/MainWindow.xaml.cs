using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Tsp.Controllers;
using Tsp.Models;
using Tsp.ViewModels;

namespace Tsp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OptionsViewModel _viewModel;
        private CancellationTokenSource _algorithmCancellationToken;
        private GeneticAlgorithmController _geneticAlgorithmController;
        private List<Tuple<int, ulong, double, ulong>> _infoBacklog;
        private Individual _bestInd;
        private int _lastBest;
        private ProgressWindow _progressWindow;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new OptionsViewModel();
            DataContext = _viewModel;

            _geneticAlgorithmController = new GeneticAlgorithmController(_viewModel);
        }

        private void DisableAllControls()
        {
            var controlsBools = new bool[_viewModel.ControlsEnableBools.Length];
            for (var i = 0; i < _viewModel.ControlsEnableBools.Length; i++)
            {
                controlsBools[i] = false;
            }
            _viewModel.ControlsEnableBools = controlsBools;
        }

        private void EnableAllControls()
        {
            var controlsBools = new bool[_viewModel.ControlsEnableBools.Length];
            for (var i = 0; i < _viewModel.ControlsEnableBools.Length; i++)
            {
                controlsBools[i] = true;
            }
            _viewModel.ControlsEnableBools = controlsBools;
        }

        private void ClearAndSaveAll()
        {
            _geneticAlgorithmController = new GeneticAlgorithmController(_geneticAlgorithmController.CityModels,
                _viewModel);

            var logCsvFile = new LogCsvFileSavingController(_infoBacklog);
            logCsvFile.Save();
        }

        private void ButtonLoadData_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            var result = openFileDialog.ShowDialog(this);

            if (result == true)
            {
                var loadTspFile = new LoadTspFileController(openFileDialog.FileName);
                loadTspFile.OnLoadingFinishedEvent += loadTspFile_OnLoadingFinishedEvent;

                _progressWindow = new ProgressWindow();
                _progressWindow.Owner = this;
                _progressWindow.ListenChangesFrom(loadTspFile);
                _progressWindow.Show();

                Task.Run(() => loadTspFile.GetCities());
            }
        }

        private void loadTspFile_OnLoadingFinishedEvent(List<CityModel> cities)
        {
            _geneticAlgorithmController.CityModels = cities;
            _geneticAlgorithmController.MinimalizeCoords();

            var drawingCities = new DrawingCitiesController(_geneticAlgorithmController.CityModels.ToArray());
            _progressWindow.Dispatcher.Invoke(() =>
            {
                _progressWindow.Close();
                drawingCities.DrawPoints(MapGrid);
            });
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            DisableAllControls();

            _infoBacklog = new List<Tuple<int, ulong, double, ulong>>(_viewModel.MaxGenerationCount);

            _geneticAlgorithmController.OnAlgorithmStateHasChangedEvent += OnAlgorithmStateHasChangedEvent;
            _geneticAlgorithmController.OnAlgorithmFinishedEvent += OnAlgorithmFinishedEvent;
            _geneticAlgorithmController.OnLogChangedEvent += OnLogChangedEvent;

            _algorithmCancellationToken = new CancellationTokenSource();
            Task.Factory.StartNew(() => _geneticAlgorithmController.DoAlgorithm(_algorithmCancellationToken.Token),
                _algorithmCancellationToken.Token);
        }

        private void OnLogChangedEvent(Tuple<int, ulong, double, ulong> info)
        {
            _infoBacklog.Add(info);
        }

        private void OnAlgorithmFinishedEvent()
        {
            EnableAllControls();
            ClearAndSaveAll();
        }

        private void OnAlgorithmStateHasChangedEvent(int progress, Individual bestInd, int bestGenNum)
        {
            var drawingRoutes = new DrawingCitiesController(bestInd.CityModels);
            Dispatcher.Invoke(() =>
            {
                if (!_algorithmCancellationToken.IsCancellationRequested)
                    _viewModel.ProgressBarValue = progress;

                if (bestGenNum != _lastBest)
                {
                    drawingRoutes.DrawRoutes(MapGrid);
                    _viewModel.BestInfo = new[] {bestInd.OverallDistance.ToString(), bestGenNum.ToString()};
                    _bestInd = bestInd;
                }

                _viewModel.CurrentGeneration = progress;
            });
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            EnableAllControls();
            if (_algorithmCancellationToken != null)
                _algorithmCancellationToken.Cancel();
            _viewModel.ProgressBarValue = 0;

            ClearAndSaveAll();
        }

        private void ButtonHillStart_Click(object sender, RoutedEventArgs e)
        {
            DisableAllControls();

            _infoBacklog = new List<Tuple<int, ulong, double, ulong>>(_viewModel.MaxGenerationCount);

            var hill = new HillClimbing(_viewModel, _geneticAlgorithmController.CityModels);
            hill.OnAlgorithmStateHasChangedEvent += OnAlgorithmStateHasChangedEvent;
            hill.OnAlgorithmFinishedEvent += OnAlgorithmFinishedEvent;
            hill.OnLogChangedEvent += OnLogChangedEvent;

            _algorithmCancellationToken = new CancellationTokenSource();

            Task.Factory.StartNew(() => hill.DoAlgorithm(_algorithmCancellationToken.Token),
                _algorithmCancellationToken.Token);
        }

        private void ButtonSaveBMP_Click(object sender, RoutedEventArgs e)
        {
            if (_bestInd != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();

                var result = dialog.ShowDialog(this);

                if (result == true)
                {
                    DrawingCitiesController drawingCities =
                        new DrawingCitiesController(_bestInd.CityModels);

                    var bmp = drawingCities.GenerateBitmap((int)MapGrid.ActualWidth, (int)MapGrid.ActualHeight);

                    try
                    {
                        bmp.Save(dialog.FileName, ImageFormat.Png);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }
    }
}