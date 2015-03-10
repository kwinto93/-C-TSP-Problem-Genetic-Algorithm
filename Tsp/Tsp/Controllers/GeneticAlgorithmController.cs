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

        public GeneticAlgorithmController(List<CityModel> cityModels, OptionsViewModel optionsViewModel)
            : this(optionsViewModel)
        {
            _cityModels = cityModels;
        }
        private OptionsViewModel _optionsViewModel;

        private List<CityModel> _cityModels;

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

        private void ShuffleCities(CityModel[] cities, int source, int dest)
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
                ShuffleCities(cities, rand.Next(_cityModels.Count - 1), rand.Next(_cityModels.Count - 1));
            }

            return ind;
        }

        private CityModel[] ReplaceDuplicatedCities(int slicePos, CityModel[] firstBlock, CityModel[] secondBlock, CityModel[] parent2Cities)
        {
            Stack<CityModel> missingCities = new Stack<CityModel>();
            for (int i = slicePos; i < parent2Cities.Length; i++)
            {
                bool exist = false;
                for (int j = 0; j < firstBlock.Length; j++)
                {
                    if (parent2Cities[i].CityNumber == firstBlock[j].CityNumber)
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                    missingCities.Push(parent2Cities[i]);
            }

            for (int i = 0; i < secondBlock.Length; i++)
            {
                for (int j = 0; j < firstBlock.Length; j++)
                {
                    if (secondBlock[i].CityNumber == firstBlock[j].CityNumber)
                        secondBlock[i] = missingCities.Pop();
                }
            }

            return secondBlock;
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
            secondCityBlock = ReplaceDuplicatedCities(slicePos, firstCityBlock, secondCityBlock, parent2.CityModels);
            PutSliceOnThePosition(cities, secondCityBlock, 0);

            ind.CityModels = cities;
            return ind;
        }

        private void MutateIndividual(Individual ind, Random rand)
        {
            for (int i = 0; i < ind.CityModels.Length; i++)
            {
                if (rand.Next(100) < _optionsViewModel.MutationProbability)
                {
                    int pos1 = rand.Next(ind.CityModels.Length - 1);
                    int pos2;
                    while ((pos2 = rand.Next(ind.CityModels.Length - 1)) == pos1) ;

                    var cities = ind.CityModels;
                    ShuffleCities(cities, pos1, pos2);
                }
            }
        }

        private void PrintAndStoreEvaluateInfo(Tuple<ulong, Individual, double, ulong> info)
        {
            Console.WriteLine("Generation #{0}, current best fitness: {1}, average: {2}, worst: {3}", _currentGenerationNum, info.Item1, info.Item3, info.Item4);
            if (OnLogChangedEvent != null)
                OnLogChangedEvent(new Tuple<int, ulong, double, ulong>(_currentGenerationNum, info.Item1, info.Item3, info.Item4));
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

        public Tuple<ulong, Individual, double, ulong> EvaluatePopAndSetBest(Population pop)
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

            return best;
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
        }

        public void MutatePop(Population pop)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            foreach (var individual in pop.Individuals)
            {
                int random = rand.Next(100);
                if (random < _optionsViewModel.MutationProbability * 100d)
                    MutateIndividual(individual, rand);
            }
        }

        public void DoAlgorithm(CancellationToken token)
        {
            Population lastPop;
            InitPopulation(out lastPop);

            var evaluate = EvaluatePopAndSetBest(lastPop);
            PrintAndStoreEvaluateInfo(evaluate);

            _currentGenerationNum++;

            while (_optionsViewModel.MaxGenerationCount > _currentGenerationNum)
            {
                Population pop = SelectPop(lastPop);
                CrossoverPop(pop);
                MutatePop(pop);

                evaluate = EvaluatePopAndSetBest(pop);
                PrintAndStoreEvaluateInfo(evaluate);

                _currentGenerationNum++;
                lastPop = pop;

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

            foreach (var cityModel in _cityModels)
            {
                cityModel.CityX -= minX;
                cityModel.CityY -= minY;
            }
        }

        public delegate void OnLogChangedEventHandler(Tuple<int, ulong, double, ulong> info);

        public event OnLogChangedEventHandler OnLogChangedEvent;

        public delegate void OnAlgorithmFinishedEventHandler();

        public event OnAlgorithmFinishedEventHandler OnAlgorithmFinishedEvent;

        public delegate void OnAlgorithmStateHasChangedEventHandler(int progress, Individual best, int bestGenNum);

        public event OnAlgorithmStateHasChangedEventHandler OnAlgorithmStateHasChangedEvent;
    }
}
