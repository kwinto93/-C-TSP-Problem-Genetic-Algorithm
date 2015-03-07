using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tsp.ViewModels
{
    public class OptionsViewModel : INotifyPropertyChanged
    {
        private double _mutationProbability = 0.01d;

        public double MutationProbability
        {
            get { return _mutationProbability; }
            set { _mutationProbability = value; OnPropertyChanged(); }
        }

        private int _bestObjectsDownLimit = 10;

        public int BestObjectsDownLimit
        {
            get { return _bestObjectsDownLimit; }
            set { _bestObjectsDownLimit = value; OnPropertyChanged();}
        }

        private int _bestObjectUpLimit = 25;

        public int BestObjectUpLimit
        {
            get { return _bestObjectUpLimit; }
            set { _bestObjectUpLimit = value; OnPropertyChanged(); }
        }

        private int _populationSize = 1000;
        public int PopulationSize
        {

            get { return _populationSize; }
            set { _populationSize = value; OnPropertyChanged(); }
        }

        private int _maxGenerationCount = 1000;

        public int MaxGenerationCount
        {
            get { return _maxGenerationCount; }
            set { _maxGenerationCount = value; OnPropertyChanged();}
        }

        private int _currentGeneration = 0;

        public int CurrentGeneration
        {
            get { return _currentGeneration; }
            set { _currentGeneration = value; OnPropertyChanged(); }
        }

        private string[] _bestInfo = new string[]{"0","0"};

        public string[] BestInfo
        {
            get { return _bestInfo; }
            set { _bestInfo = value; OnPropertyChanged(); }
        }

        private bool[] _controlsEnableBools = { true, true, true, true, true, true, true };

        public bool[] ControlsEnableBools
        {
            get { return _controlsEnableBools; }
            set { _controlsEnableBools = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
