using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsp.Models;

namespace Tsp.Controllers
{
    public class GeneticController
    {
        private List<CityModel> _cityModels; 

        public List<CityModel> CityModels
        {
            get { return _cityModels; }
            set
            {
                _cityModels = value;
            }
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
    }
}
