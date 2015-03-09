using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsp.Models
{
    public class Individual
    {
        /// <summary>
        /// Fitness value
        /// </summary>
        private ulong _overallDistance = 0;

        public ulong OverallDistance
        {
            get
            {
                if(_overallDistance == 0)
                    CalculateOverallDistance();

                return _overallDistance;
            }
            set { _overallDistance = value; }
        }

        private CityModel[] _cityModels;

        public CityModel[] CityModels
        {
            get { return _cityModels; }
            set { _cityModels = value; }
        }

        private double DistanceBetweenCities(CityModel city1, CityModel city2)
        {
            return Math.Sqrt(Math.Pow(city2.CityX - city1.CityX, 2) + Math.Pow(city2.CityY - city1.CityY, 2));
        }

        /// <summary>
        /// Fitness function
        /// </summary>
        public void CalculateOverallDistance()
        {
            if (_cityModels != null)
            {
                for (int i = 0; i < _cityModels.Length - 1; i++) // -1, because we bring one city ahead
                {
                    _overallDistance += (ulong)DistanceBetweenCities(_cityModels[i], _cityModels[i + 1]);
                }
            }
        }
    }
}
