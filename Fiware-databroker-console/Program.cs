using Databroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fiware_databroker_console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool GetData = true;

            Console.WriteLine("Fiware databroker");
          

            var KPI = new ProcessKpiData();
            int i = 0;
            do // Get API-data and update status on website
            {
                KPI.ConvertJsonKPIToClassList();
                KPI.InsertKPIsInDataBase();

                Thread.Sleep(15000); // Check for new data all 15 seconds
                i++;
            } while (GetData == true);
        }
    }
}
