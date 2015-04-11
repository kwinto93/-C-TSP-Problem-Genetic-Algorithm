using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsp.Models;

namespace Tsp.Controllers
{
    public class LoadTspFileController
    {
        private const string LoadingStartTrigger = "NODE_COORD_SECTION";
        private const string FileEndTrigger = "EOF";
        private const string DimensionProperty = "DIMENSION";
        private string[] _loadingStates = { "Wczytywanie pliku", "Przetwarzanie danych" };
        private string _filePath;

        public LoadTspFileController(string filePath)
        {
            _filePath = filePath;
        }

        private Tuple<string, int> NextNumber(string str)
        {
            string nextNumber = "";

            int pos = 0;
            while (pos < str.Length && !Char.IsDigit(str[pos])) // skipping
                pos++;

            // loading number
            if (pos <= str.Length - 1)
                do
                {
                    nextNumber += str[pos];
                    pos++;
                } while (pos < str.Length && (Char.IsDigit(str[pos]) || str[pos].Equals('.')));

            return new Tuple<string, int>(nextNumber, pos);
        }

        private string[] SliceString(string str)
        {
            string[] sliceStrings = new string[2];
            
            var cityNum = NextNumber(str);
            string tmp = str.Substring(cityNum.Item2);

            var firstNum = NextNumber(tmp);
            sliceStrings[0] = firstNum.Item1;
            sliceStrings[1] = NextNumber(tmp.Substring(firstNum.Item2)).Item1;

            Debug.WriteLine(sliceStrings[0] + " " + sliceStrings[1]);

            return sliceStrings;
        }

        private List<CityModel> ExportCities(List<string> fileLines)
        {
            if (OnLoadingStateChangedEvent != null)
                OnLoadingStateChangedEvent(_loadingStates[1]);

            bool load = false;
            int cityNumber = 0;
            int numberOfCities = 0;
            List<CityModel> cities = new List<CityModel>(fileLines.Count);
            foreach (var fileLine in fileLines)
            {
                if (load && !fileLine.Equals(FileEndTrigger))
                {
                    string[] str = SliceString(fileLine);

                    double x = 0d, y = 0d;
                    try
                    {
                        x = Double.Parse(str[0], CultureInfo.InvariantCulture);
                        y = Double.Parse(str[1], CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        x = Int32.Parse(str[0], CultureInfo.InvariantCulture);
                        y = Int32.Parse(str[1], CultureInfo.InvariantCulture);

                        Debug.WriteLine(e.Message);
                    }

                    cities.Add(new CityModel()
                    {
                        CityNumber = cityNumber,
                        CityX = x,
                        CityY = y
                    });

                    cityNumber++;
                    if (OnLoadingProgressChangedEvent != null)
                        OnLoadingProgressChangedEvent((int)(cityNumber * 100d / numberOfCities));
                }
                else if (fileLine.Equals(LoadingStartTrigger))
                {
                    load = true;
                }
                else if (fileLine.Contains(DimensionProperty))
                {
                    numberOfCities = Int32.Parse(NextNumber(fileLine).Item1);
                    Debug.WriteLine(numberOfCities);
                }
            }

            return cities;
        }

        private List<string> FileLines()
        {
            if (OnLoadingStateChangedEvent != null)
                OnLoadingStateChangedEvent(_loadingStates[0]);

            int count = 0;
            StreamReader countStreamReader = new StreamReader(_filePath);
            int numberOfCharacters = countStreamReader.ReadToEnd().Length;
            countStreamReader.Close();

            List<string> fileLines = new List<string>();
            StreamReader streamReader = new StreamReader(_filePath);

            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                Debug.WriteLine(line);
                fileLines.Add(line);

                count += line.Length;
                if(OnLoadingProgressChangedEvent != null)
                    OnLoadingProgressChangedEvent((int)(count * 100d / numberOfCharacters));
            }

            streamReader.Close();

            return fileLines;
        }

        public void GetCities()
        {
            var exportedCitiesList = ExportCities(FileLines());

            if (OnLoadingFinishedEvent != null)
                OnLoadingFinishedEvent(exportedCitiesList);
        }

        public delegate void OnLoadingFinishedEventHandler(List<CityModel> cities);

        public event OnLoadingFinishedEventHandler OnLoadingFinishedEvent;

        public delegate void OnLoadingStateChangedEventHandler(string change);

        public event OnLoadingStateChangedEventHandler OnLoadingStateChangedEvent;

        public delegate void OnLoadingProgressChangedEventHandler(int progress);

        public event OnLoadingProgressChangedEventHandler OnLoadingProgressChangedEvent;
    }
}
