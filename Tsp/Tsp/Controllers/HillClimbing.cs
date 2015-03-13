using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tsp.Models;
using Tsp.ViewModels;

namespace Tsp.Controllers
{
    public class HillClimbing
    {
        private OptionsViewModel _optionsViewModel;
        private List<CityModel> _cityList;

        public List<CityModel> CityList
        {
            get { return _cityList; }
            set { _cityList = value; }
        }

        private Individual _currentBest;
        private int _currentGenerationNum;
        private int _bestGenerationNumber;

        public HillClimbing(OptionsViewModel optionsViewModel, List<CityModel> cityModels)
        {
            _optionsViewModel = optionsViewModel;
            _cityList = cityModels;
        }

        private void ShuffleCities(CityModel[] cities, int source, int dest)
        {
            CityModel tmp = cities[source];
            cities[source] = cities[dest];
            cities[dest] = tmp;
        }

        public void DoAlgorithm(CancellationToken token)
        {
            InitFirstIndividual();

            Random rand = new Random();
            while (_optionsViewModel.MaxGenerationCount > _currentGenerationNum)
            {
                Individual ind = new Individual();
                ind.CityModels = new CityModel[_currentBest.CityModels.Length];
                for (int i = 0; i < _currentBest.CityModels.Length; i++)
                {
                    ind.CityModels[i] = _currentBest.CityModels[i];
                }

                int source;
                int destination;

                source = rand.Next(ind.CityModels.Length - 1);
                while ((destination = rand.Next(ind.CityModels.Length - 1)) == source) ;

                ShuffleCities(ind.CityModels, source, destination);
                ind.CalculateOverallDistance();

                if (ind.OverallDistance < _currentBest.OverallDistance)
                {
                    _bestGenerationNumber = _currentGenerationNum;
                    _currentBest = ind;
                }

                if (OnAlgorithmStateHasChangedEvent != null)
                    OnAlgorithmStateHasChangedEvent(_currentGenerationNum, _currentBest, _bestGenerationNumber);

                if (token.IsCancellationRequested)
                    break;

                _currentGenerationNum++;
            }

            if (OnAlgorithmFinishedEvent != null)
                OnAlgorithmFinishedEvent();
        }

        private void InitFirstIndividual()
        {
            _currentBest = new Individual();
            _currentBest.CityModels = new CityModel[_cityList.Count];

            int pos = 0;
            foreach (var cityModel in _cityList)
            {
                _currentBest.CityModels[pos] = cityModel;
                pos++;
            }

            Random rand = new Random();
            for (int i = 0; i < _currentBest.CityModels.Length; i++)
            {
                ShuffleCities(_currentBest.CityModels, rand.Next(_currentBest.CityModels.Length), rand.Next(_currentBest.CityModels.Length));
            }

            _currentBest.CalculateOverallDistance();
        }

        public delegate void OnLogChangedEventHandler(Tuple<int, ulong, double, ulong> info);

        public event OnLogChangedEventHandler OnLogChangedEvent;

        public delegate void OnAlgorithmFinishedEventHandler();

        public event OnAlgorithmFinishedEventHandler OnAlgorithmFinishedEvent;

        public delegate void OnAlgorithmStateHasChangedEventHandler(int progress, Individual best, int bestGenNum);

        public event OnAlgorithmStateHasChangedEventHandler OnAlgorithmStateHasChangedEvent;
    }
}
