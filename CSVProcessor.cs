using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMSCharacterization;
using CsvHelper;

namespace BMSManagerRebuilt
{
    internal class CSVProcessor
    {
        /// <summary>
        /// Read Voltage Watt Hours to VoltageWattHoursTable variable
        /// </summary>
        /// <param name="path">Path to csv file</param>
        /// <param name="table">Populate VoltageWattHoursTable</param>
        public static void ReadCSVWattHour(string path, VoltageWattHoursTable table)
        {
            var ReadFile = File.OpenRead(path);
            var Reader = new StreamReader(ReadFile); //There is no encoding so do not add any encoder.

            float Current;
            float StartVoltage;
            float Step;
            float[] WattHours;
            int WattNum;
            
        }   
    }
}
