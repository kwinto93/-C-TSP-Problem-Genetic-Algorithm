using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Shapes;
using Tsp.Models;
using Brushes = System.Windows.Media.Brushes;

namespace Tsp.Controllers
{
    public class DrawingCitiesController
    {
        private readonly CityModel[] _cities;

        public DrawingCitiesController(CityModel[] cities)
        {
            _cities = cities;
        }

        private Line CreatePoint(int x, int y)
        {
            var point = new Line();
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
            var line = new Line();
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
            for (var i = 0; i < _cities.Length; i++)
            {
                var point = CreatePoint((int) _cities[i].CityX, (int) _cities[i].CityY);

                destination.Children.Add(point);
            }
        }

        public void DrawRoutes(Grid destination)
        {
            ClearAll(destination);
            for (var i = 0; i < _cities.Length - 1; i++)
            {
                destination.Children.Add(CreateLine((int) _cities[i].CityX, (int) _cities[i + 1].CityX,
                    (int) _cities[i].CityY, (int) _cities[i + 1].CityY));
            }

            // last connection
            destination.Children.Add(CreateLine((int) _cities[0].CityX, (int) _cities[_cities.Length - 1].CityX,
                (int) _cities[0].CityY, (int) _cities[_cities.Length - 1].CityY));
        }

        public Bitmap GenerateBitmap(int width, int height)
        {
            var bmp = new Bitmap(Convert.ToInt32(width + 1 + 0.01*width), Convert.ToInt32(height + 1 + 0.01*height), PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(bmp);

            g.Clear(Color.White);

            for (var i = 0; i < _cities.Length - 1; i++)
            {
                g.DrawLine(Pens.Black, (int) _cities[i].CityX, (int) _cities[i].CityY, (int) _cities[i + 1].CityX,
                    (int) _cities[i + 1].CityY);
            }

            // last connection
            g.DrawLine(Pens.Black, (int)_cities[0].CityX, (int)_cities[0].CityY,
                (int)_cities[_cities.Length - 1].CityX, (int)_cities[_cities.Length - 1].CityY);

            return bmp;
        }

        public void ClearAll(Grid destination)
        {
            destination.Children.Clear();
        }
    }
}