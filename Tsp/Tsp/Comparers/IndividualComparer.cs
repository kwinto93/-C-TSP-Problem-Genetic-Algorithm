using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsp.Models;

namespace Tsp.Comparers
{
    public class IndividualComparer : IComparer<Individual>
    {
        public int Compare(Individual x, Individual y)
        {
            return x.OverallDistance > y.OverallDistance ? 1 : -1;
        }
    }
}
