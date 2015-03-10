using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsp.Models
{
    public class Population
    {
        private List<Individual> _individuals;

        public List<Individual> Individuals
        {
            get { return _individuals; }
            set { _individuals = value; }
        }

        /// <summary>
        /// Returns best fitness value (minimal value of fitness function) and it owner
        /// </summary>
        /// <returns>Item1 - Best fitness value
        /// Item2 - Best fitness individual</returns>
        public Tuple<ulong, Individual, double, ulong> GetBestFitness()
        {
            Individual ind = Individuals[0];
            ulong minFitness = ind.OverallDistance;
            ulong averageFitness = 0;
            ulong maxFitness = 0;
            double count = 0;

            foreach (var individual in Individuals)
            {
                if (individual.OverallDistance < minFitness)
                {
                    minFitness = individual.OverallDistance;
                    ind = individual;
                }
                if (individual.OverallDistance > maxFitness)
                    maxFitness = individual.OverallDistance;

                averageFitness += individual.OverallDistance;
                count++;
            }

            return new Tuple<ulong, Individual, double, ulong>(minFitness, ind, (averageFitness / count), maxFitness);
        }
    }
}
