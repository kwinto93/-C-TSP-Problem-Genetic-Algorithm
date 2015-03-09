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
            if (x.OverallDistance > y.OverallDistance)
                return 1;
            else if (x.OverallDistance == y.OverallDistance)
                return 0;
            else
                return -1;
        }
    }
}
