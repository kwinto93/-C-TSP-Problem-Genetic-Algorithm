using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Tsp.Models;

namespace Tsp.Controllers
{
    public class DrawingCitiesController
    {
        private CityModel[] _cities;

        public DrawingCitiesController(CityModel[] cities)
        {
            _cities = cities;
        }

        private Line CreatePoint(int x, int y)
        {
            Line point = new Line();
            point.StrokeThickness = 1;
            point.Stroke = Brushes.Black;

            point.X1 = x;
            point.X2 = x + 1;
            point.Y1 = y;
            point.Y2 = y + 1;

            return point;
        }

        private Line CreateLine(int x1, int x2, int y1, int y2)
        {
            Line line = new Line();
            line.StrokeThickness = 1;
            line.Stroke = Brushes.Black;

            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;

            return line;
        }

        public void DrawPoints(Grid destination)
        {
            ClearAll(destination);
            for (int i = 0; i < _cities.Length; i++)
            {
                Line point = CreatePoint((int)_cities[i].CityX, (int)_cities[i].CityY);

                destination.Children.Add(point);
            }
        }

        public void DrawRoutes(Grid destination)
        {
            ClearAll(destination);
            for (int i = 0; i < _cities.Length-1; i++)
            {
                destination.Children.Add(CreateLine((int) _cities[i].CityX, (int) _cities[i + 1].CityX,
                    (int) _cities[i].CityY, (int) _cities[i + 1].CityY));
            }

            // last connection
            destination.Children.Add(CreateLine((int) _cities[0].CityX, (int) _cities[_cities.Length-1].CityX,
                    (int) _cities[0].CityY, (int) _cities[_cities.Length-1].CityY));
        }

        public void ClearAll(Grid destination)
        {
            destination.Children.Clear();
        }
    }
}
