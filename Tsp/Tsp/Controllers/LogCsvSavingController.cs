using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Tsp.Controllers
{
    public class LogCsvFileSavingController
    {
        private List<Tuple<int, ulong, double, ulong>> _logSource;

        public LogCsvFileSavingController(List<Tuple<int, ulong, double, ulong>> logSource)
        {
            _logSource = logSource;
        }

        public void Save()
        {
            if (_logSource != null)
            {
                StreamWriter strW = new StreamWriter(DateTime.Now.ToString("yyyymmdd HHmmss") + ".csv");
                strW.AutoFlush = true;

                foreach (var logLine in _logSource)
                {
                    strW.WriteLine(logLine.Item1 + " " +
                                   logLine.Item2 + " " +
                                   logLine.Item3 + " " +
                                   logLine.Item4);
                }

                strW.Close();
            }
        }
    }
}
