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
            int shuffleMin = (int)(_cityModels.Count * 0.5);
            int numberOfShuffles = rand.Next(shuffleMin, _cityModels.Count);
            for (int i = 0; i < numberOfShuffles; i++)
            {
                var cities = ind.CityModels;
                ShuffleCities(cities, rand.Next(_cityModels.Count - 1), rand.Next(_cityModels.Count - 1));
            }

            return ind;
        }

        private Individual CrossIndividuals(Individual parent1, Individual parent2, Random rand)
        {
            Individual ind = new Individual();
            var cities = new CityModel[parent1.CityModels.Length];
            var cities1 = parent1.CityModels;
            var cities2 = parent2.CityModels;

            int sliceOffset = 2;
            int slicePos = rand.Next(sliceOffset, cities.Length - 1 - sliceOffset);
            for (int i = slicePos; i < cities.Length; i++)
            {
                cities[i] = cities1[i];
            }

            int pos = 0;
            for (int i = 0; i < cities.Length && pos < slicePos; i++)
            {
                bool conflict = false;
                for (int j = slicePos; j < cities1.Length; j++)
                {
                    if (cities2[i].CityNumber == cities1[j].CityNumber)
                    {
                        conflict = true;
                        break;
                    }
                }

                if (!conflict)
                {
                    cities[pos] = cities2[i];
                    pos++;
                }
            }

            ind.CityModels = cities;
            return ind;
        }

        private void MutateIndividual(Individual ind, Random rand)
        {
            int pos1 = rand.Next(ind.CityModels.Length - 1);
            int pos2;
            while ((pos2 = rand.Next(ind.CityModels.Length - 1)) == pos1) ;

            ShuffleCities(ind.CityModels, pos1, pos2);
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

        public Individual SelectTournamentWinner(Individual[] individuals)
        {
            Tuple<int, ulong> best = new Tuple<int, ulong>(0, individuals[0].OverallDistance);

            Random rand = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < individuals.Length; i++)
            {
                if (rand.Next(100) <= _optionsViewModel.SelectionProbablityOfTournamentParticipation * 100d && individuals[i].OverallDistance < best.Item2)
                {
                    best = new Tuple<int, ulong>(i, individuals[i].OverallDistance);
                }
            }

            return individuals[best.Item1];
        }

        public void CrossoverPop(Population pop)
        {
            Individual[] individualsToCrossing = pop.Individuals.ToArray();
            pop.Individuals.Clear();

            Random rand = new Random(DateTime.Now.Millisecond);
            while (pop.Individuals.Count < individualsToCrossing.Length - 1)
            {
                Individual parent1, parent2;

                parent1 = SelectTournamentWinner(individualsToCrossing);
                parent2 = SelectTournamentWinner(individualsToCrossing);

                pop.Individuals.Add(CrossIndividuals(
                    parent1,
                    parent2,
                    rand
                    )
                    );

                pop.Individuals.Add(CrossIndividuals(
                    parent2,
                    parent1,
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
                {
                    MutateIndividual(individual, rand);
                }
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
                //Population pop = SelectTournamentWinner(lastPop);
                Population pop = lastPop;
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
