using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsp.Models
{
    public class CityVertex
    {
        private bool _isInChild = false;

        public bool IsInChild
        {
            get { return _isInChild; }
            set { _isInChild = value; }
        }
    }
}
