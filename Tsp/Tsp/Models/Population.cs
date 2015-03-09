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
        public Tuple<ulong, Individual> GetBestFitness()
        {
            ulong minFitness = Individuals.Min(i => i.OverallDistance);

            Individual ind = Individuals.First(i => i.OverallDistance == minFitness);

            return new Tuple<ulong, Individual>(minFitness, ind);
        }
    }
}
