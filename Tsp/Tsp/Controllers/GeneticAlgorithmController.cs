using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tsp.Comparers;
using Tsp.Models;
using Tsp.ViewModels;

namespace Tsp.Controllers
{
    public class GeneticAlgorithmController
    {
        public GeneticAlgorithmController(OptionsViewModel optionsViewModel)
        {
            _optionsViewModel = optionsViewModel;
            _pastPopulations = new Population[_optionsViewModel.MaxGenerationCount];
        }

        private List<CityModel> _cityModels;
        private OptionsViewModel _optionsViewModel;

        public List<CityModel> CityModels
        {
            get { return _cityModels; }
            set
            {
                _cityModels = value;
            }
        }

        private Population[] _pastPopulations;
        private int _currentGenerationNum = 0;

        public Population[] PastPopulations
        {
            get { return _pastPopulations; }
            set { _pastPopulations = value; }
        }

        private Individual _bestIndividual;

        public Individual BestIndividual
        {
            get { return _bestIndividual; }
            private set { _bestIndividual = value; }
        }

        private int _bestGenerationNumber;

        public int BestGenerationNumber
        {
            get { return _bestGenerationNumber; }
            private set { _bestGenerationNumber = value; }
        }

        private void ShuffleCities(ref CityModel[] cities, int source, int dest)
        {
            CityModel tmp = cities[source];
            cities[source] = cities[dest];
            cities[dest] = tmp;
        }

        private Individual GenerateIndividualWithGenesRandomOrder()
        {
            Individual ind = new Individual();
            ind.CityModels = new CityModel[_cityModels.Count];

            int pos = 0;
            foreach (var cityModel in _cityModels)
            {
                ind.CityModels[pos] = cityModel;
                pos++;
            }

            // shuffle
            Random rand = new Random(DateTime.Now.Millisecond);
            int numberOfShuffles = rand.Next(_cityModels.Count);
            for (int i = 0; i < numberOfShuffles; i++)
            {
                var cities = ind.CityModels;
                ShuffleCities(ref cities, rand.Next(_cityModels.Count - 1), rand.Next(_cityModels.Count - 1));
            }

            return ind;
        }

        private CityModel[] ReplaceDuplicatedCities(int slicePos, CityModel[] citiesSource, CityModel[] citiesDestination, CityModel[] orignalCities)
        {
            for (int i = 0; i < citiesSource.Length; i++)
            {
                for (int j = 0; j < citiesDestination.Length; j++)
                {
                    if (citiesSource[i] == citiesDestination[j])
                    {
                        citiesSource[i] = orignalCities[i];
                        break;
                    }
                }
            }

            return citiesSource;
        }

        private CityModel[] SkipCities(CityModel[] source, int slicePos, int sliceLength)
        {
            CityModel[] tmp = new CityModel[sliceLength];
            for (int i = 0; i < sliceLength; i++)
            {
                tmp[i] = source[i + slicePos];
            }

            return tmp;
        }

        private void PutSliceOnThePosition(CityModel[] cities, CityModel[] sourceCities, int sliceStartPos)
        {
            for (int i = 0; i < sourceCities.Length; i++)
            {
                cities[i + sliceStartPos] = sourceCities[i];
            }
        }

        private Individual CrossIndividuals(Individual parent1, Individual parent2, Random rand)
        {
            int sliceMinPos = 1;
            int slicePos = rand.Next(parent1.CityModels.Length - sliceMinPos - 1) + sliceMinPos;

            Individual ind = new Individual();

            var cities = ind.CityModels;
            cities = new CityModel[parent1.CityModels.Length];

            var firstCityBlock = SkipCities(parent1.CityModels, slicePos, parent1.CityModels.Length - slicePos);
            PutSliceOnThePosition(cities, firstCityBlock, slicePos);

            var secondCityBlock = SkipCities(parent2.CityModels, 0, slicePos);
            secondCityBlock = ReplaceDuplicatedCities(slicePos, secondCityBlock, firstCityBlock, parent1.CityModels);
            PutSliceOnThePosition(cities, secondCityBlock, 0);

            ind.CityModels = cities;
            return ind;
        }

        private void MutateIndividual(Individual ind, Random rand)
        {
            int pos1 = rand.Next(ind.CityModels.Length - 1);
            int pos2;
            while ((pos2 = rand.Next(ind.CityModels.Length - 1)) == pos1) ;

            var cities = ind.CityModels;
            ShuffleCities(ref cities, pos1, pos2);
        }

        public void InitPopulation(out Population pop)
        {
            pop = new Population();
            pop.Individuals = new List<Individual>(_optionsViewModel.PopulationSize);

            for (int i = 0; i < _optionsViewModel.PopulationSize; i++)
            {
                pop.Individuals.Add(GenerateIndividualWithGenesRandomOrder());
            }
        }

        public ulong EvaluatePopAndSetBest(Population pop)
        {
            var best = pop.GetBestFitness();

            if (_bestIndividual != null && best.Item2.OverallDistance < _bestIndividual.OverallDistance)
            {
                _bestGenerationNumber = _currentGenerationNum;
                _bestIndividual = best.Item2;
            }
            else if (_bestIndividual == null)
            {
                _bestGenerationNumber = _currentGenerationNum;
                _bestIndividual = best.Item2;
            }

            return best.Item1;
        }

        public Population SelectPop(Population pop)
        {
            pop.Individuals.Sort(new IndividualComparer());

            List<Individual> ind = new List<Individual>(pop.Individuals.Count);

            double nowBestProbability = 1d;
            double probabilityDownStep = 1d * _optionsViewModel.SelectionProbablityDownStepFactor / pop.Individuals.Count;

            Random rand = new Random(DateTime.Now.Millisecond);
            foreach (var individual in pop.Individuals)
            {
                if (rand.Next(100) <= nowBestProbability * 100)
                {
                    ind.Add(individual);
                }

                nowBestProbability -= probabilityDownStep;
                if (nowBestProbability < 0)
                    break;
            }

            Population newPop = new Population();
            newPop.Individuals = ind;

            return newPop;
        }

        public void CrossoverPop(Population popWithBestIndiv)
        {
            int crossOversNum = _optionsViewModel.PopulationSize - popWithBestIndiv.Individuals.Count;

            Individual[] individualsToCrossing = popWithBestIndiv.Individuals.ToArray();
            Random rand = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < crossOversNum; i++)
            {
                int parentIndex1 = rand.Next(individualsToCrossing.Length - 1);
                int parentIndex2;
                while ((parentIndex2 = rand.Next(individualsToCrossing.Length - 1)) == parentIndex1) ; // different next one

                popWithBestIndiv.Individuals.Add(CrossIndividuals(
                    individualsToCrossing[parentIndex1],
                    individualsToCrossing[parentIndex2],
                    rand
                    )
                    );
            }

            Debug.WriteLine("test");
        }

        public void MutatePop(Population pop)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            foreach (var individual in pop.Individuals)
            {
                if(rand.Next(100) < _optionsViewModel.MutationProbability * 100d)
                    MutateIndividual(individual, rand);
            }
        }

        public void DoAlgorithm(CancellationToken token)
        {
            Population lastPop;
            InitPopulation(out lastPop);
            Console.WriteLine("Generation #{0}, current fitness: {1}", _currentGenerationNum,  EvaluatePopAndSetBest(lastPop));
            _currentGenerationNum++;

            while (_optionsViewModel.MaxGenerationCount > _currentGenerationNum)
            {
                Population pop = SelectPop(lastPop);
                CrossoverPop(pop);
                MutatePop(pop);
                Console.WriteLine("Generation #{0}, current fitness: {1}", _currentGenerationNum,  EvaluatePopAndSetBest(pop));
                _currentGenerationNum++;

                if (OnAlgorithmStateHasChangedEvent != null)
                    OnAlgorithmStateHasChangedEvent(_currentGenerationNum, _bestIndividual, _bestGenerationNumber);

                if (token.IsCancellationRequested)
                    break;
            }

            if (OnAlgorithmFinishedEvent != null)
                OnAlgorithmFinishedEvent();
        }

        /// <summary>
        /// Minimalizing coords (ex. from 5210.11 4850.44 to 1210.11 850.44 - changing relative "0,0" point)
        /// </summary>
        public void MinimalizeCoords()
        {
            double minX = _cityModels[0].CityX;
            double minY = _cityModels[0].CityY;

            foreach (var cityModel in _cityModels)
            {
                if (cityModel.CityX < minX)
                    minX = cityModel.CityX;

                if (cityModel.CityY < minY)
                    minY = cityModel.CityY;
            }

            double offset = minX < minY ? minX : minY;

            foreach (var cityModel in _cityModels)
            {
                cityModel.CityX -= offset;
                cityModel.CityY -= offset;
            }
        }

        public delegate void OnAlgorithmFinishedEventHandler();

        public event OnAlgorithmFinishedEventHandler OnAlgorithmFinishedEvent;

        public delegate void OnAlgorithmStateHasChangedEventHandler(int progress, Individual best, int bestGenNum);

        public event OnAlgorithmStateHasChangedEventHandler OnAlgorithmStateHasChangedEvent;
    }
}
