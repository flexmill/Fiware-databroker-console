using Databroker.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace Databroker
{

    // This class is used for mapping KPI-Data from JSON to a easily usable format
    public class KPIDataRaw
    {
        public string id { get; set; }
        public string type { get; set; }
        public SubEntryText Batch { get; set; }
        public SubEntryText GoodPartsCount { get; set; }
        public SubEntryText GoodPartsDateEnd { get; set; }
        public SubEntryText GoodPartsDateStart { get; set; }
        public SubEntryObject IdealCycleTime { get; set; }
        public SubEntryInt  MachiningCentre { get; set; }
        public SubEntryText  MachiningDateEnd { get; set; }
        public SubEntryText  MachiningDateStart { get; set; }
        public SubEntryText  MachiningTimeEnd { get; set; }
        public SubEntryText  MachiningTimeStart { get; set; }
    }

    public class SubEntryText
    {
        public string type { get; set; }
        public string value { get; set; }
        public object metadata { get; set; }
    }

    public class SubEntryInt
    {
        public string type { get; set; }
        public int value { get; set; }
        public object metadata { get; set; }
    }

    public class SubEntryObject
    {
        public string type { get; set; }
        public object value { get; set; }
        public object metadata { get; set; }
    }

    public class KPIData
    {
        public long Id { get; set; }
        public string WorkpieceId { get; set; }
        public float Availbility { get; set; }
        public float Ooe { get; set; }
        public float Performance { get; set; }
        public float Quality { get; set; }
        public string Batch { get; set; }
        public int GoodPartsCount { get; set; }
        public string GoodPartsDateStart { get; set; }
        public string GoodPartsDateEnd { get; set; }
        public float IdealCycleTime  { get; set; }
        public int? MachiningCentre { get; set; }
        public string MachiningDateStart { get; set; }
        public string MachiningTimeStart { get; set; }
        public string MachiningDateEnd { get; set; }
        public string MachiningTimeEnd { get; set; }
    }

    public class ProcessKpiData
    {
        APICalls api;
        public IList<KPIDataRaw> kpis { get; set; }
        public StringBuilder ids   = new StringBuilder(); // Holds the IDs the KPI-Data is provided for
        public StringBuilder types = new StringBuilder(); // Type of data
        public string lastAPIDataChange = "---";

        public ProcessKpiData()
        {
            api = new APICalls();
        }

        public string GetRawJsonKPI()
        {
            try
            {
                return api.GetEntities("?type=MachiningData");
            }   catch (Exception e)
            {
                Console.WriteLine("Exception in api.GetEntities: " + e.Message);
            }
            return "error";
        }

        public string GetLastAPIDataChange()
        {
            return lastAPIDataChange;
        }

        public void ConvertJsonKPIToClassList() // Converts raw JSON to a list that can be processed
        {
            string jsonData = api.GetEntities("?type=MachiningData");
            if (jsonData.Length > 5 && jsonData.Substring(0, 5) != "Error")
            {
                try
                {
                    kpis = JsonConvert.DeserializeObject<IList<KPIDataRaw>>(jsonData);
                }
                catch (JsonReaderException e)
                {
                    Console.WriteLine("JSON-parsing error " + e.ToString());
                }
            } else
            {
                kpis = null; 
                Console.WriteLine("Empty JSON-Data!");
            }
  
            ids.Clear();
            types.Clear();

            if (kpis != null)
            {
                foreach (var kpi in kpis)
                {
                    if (kpi.id != null)
                    {
                        ids.Append(kpi.id + ", ");
                    }
                }
            }
        }

        public void InsertKPIsInDataBase() // Parse KPIs and insert in the database
        {
            var kpiData = new KPIData();
            int goodPartsCount   = 0;
            float idealCycleTime = 0;
            Dictionary<string, string> DP = new Dictionary<string, string>();

            if (kpis != null)
            {
                Console.WriteLine("Handling KPIs");
                foreach (var kpi in kpis)
                {
                    kpiData.WorkpieceId = kpi.id != null ? kpi.id : "";
                    kpiData.MachiningCentre = kpi.MachiningCentre != null && kpi.MachiningCentre.value != null ? kpi.MachiningCentre.value : 0;
                    kpiData.MachiningDateStart = kpi.MachiningDateStart != null && kpi.MachiningDateStart.value != null ? kpi.MachiningDateStart.value : "";
                    kpiData.MachiningTimeStart = kpi.MachiningTimeStart != null && kpi.MachiningTimeStart.value != null ? kpi.MachiningTimeStart.value : "";
                    kpiData.MachiningDateEnd   = kpi.MachiningDateEnd != null && kpi.MachiningDateEnd.value != null ? kpi.MachiningDateEnd.value : "";
                    kpiData.MachiningTimeEnd   = kpi.MachiningTimeEnd != null && kpi.MachiningTimeEnd.value != null ? kpi.MachiningTimeEnd.value : "";
                    kpiData.GoodPartsDateStart = kpi.GoodPartsDateStart != null && kpi.GoodPartsDateStart.value != null ? kpi.GoodPartsDateStart.value : "";
                    kpiData.GoodPartsDateEnd = kpi.GoodPartsDateEnd != null && kpi.GoodPartsDateEnd.value != null ? kpi.GoodPartsDateEnd.value : "";
                    kpiData.Batch = kpi.Batch != null && kpi.Batch.value != null ? kpi.Batch.value : "";

                    kpiData.IdealCycleTime = 0;
                    if (kpi.IdealCycleTime != null && kpi.IdealCycleTime.value != null)
                    {
                          float.TryParse(kpi.IdealCycleTime.value.ToString(), NumberStyles.Float,CultureInfo.InvariantCulture, out idealCycleTime);
                          kpiData.IdealCycleTime = idealCycleTime;
                    } 

                    kpiData.GoodPartsCount = 0;
                    if (kpi.GoodPartsCount != null && kpi.GoodPartsCount.value != null)
                    {
                        int.TryParse(kpi.GoodPartsCount.value, out goodPartsCount);
                        kpiData.GoodPartsCount = goodPartsCount;
                    }

                    if (kpiData.MachiningCentre != 0 && kpiData.MachiningDateStart != "" && kpiData.MachiningTimeStart != "")
                    {
                        System.Diagnostics.Debug.WriteLine(kpiData.MachiningCentre + " - " + kpiData.MachiningDateStart + " - " + kpiData.MachiningTimeStart);
                        DP.Clear();
                        DP["@p_machining_centre"] = kpiData.MachiningCentre.ToString();
                        DP["@p_machining_date_start"] = kpiData.MachiningDateStart;
                        DP["@p_machining_time_start"] = kpiData.MachiningTimeStart;

                        DataTable dt = new DataTable();
                        dt = DBHelper.Query("SELECT id, machining_date_start, machining_time_start FROM KPI_DATA_RAW WHERE machining_centre = @p_machining_centre AND machining_date_start = @p_machining_date_start AND machining_time_start = @p_machining_time_start", DP);

                        if (dt.Rows.Count == 0) // Entry not in datatable, compute KPIs and write entry to database
                        {
                            DateTime dateStart =  DateTime.Parse(kpiData.MachiningDateStart + " " + kpiData.MachiningTimeStart);
                            Console.WriteLine("DateStart: " + dateStart.ToString());
                            DateTime dateEnd = DateTime.Parse(kpiData.MachiningDateEnd + " " + kpiData.MachiningTimeEnd);
                            Console.WriteLine("DateEnd: " + dateEnd.ToString());
                            var difference = (dateEnd - dateStart).Seconds;
                            Console.WriteLine("Diff in Seconds: " + difference);
                            Console.WriteLine("GoodPartdCount: " + goodPartsCount);
                            Console.WriteLine("IdealCycleTime: " + idealCycleTime);

                            DP["@p_availability"] = "0";
                            DP["@p_ooe"] = "0";
                            DP["@p_performance"] = "0";
                            DP["@p_quality"] = "0";

                            DP["@p_batch"] = kpiData.Batch;
                            DP["@p_workpiece_id"] = kpiData.WorkpieceId;
                            DP["@p_good_parts_count"] = kpiData.GoodPartsCount.ToString();
                            DP["@p_good_parts_date_start"] = kpiData.GoodPartsDateStart;
                            DP["@p_good_parts_date_end"] = kpiData.GoodPartsDateEnd;
                            DP["@p_machining_date_end"] = kpiData.MachiningDateEnd;
                            DP["@p_machining_time_end"] = kpiData.MachiningTimeEnd;
                            DP["@p_ideal_cycle_time"] = kpiData.IdealCycleTime.ToString().Replace(",", ".");

                            var result = DBHelper.Exec("INSERT INTO KPI_DATA_RAW (availability, batch, good_parts_count, good_parts_date_start, good_parts_date_end, ideal_cycle_time, machining_centre, machining_date_start, machining_time_start, machining_date_end, machining_time_end, ooe, performance, quality, workpiece_id) " +
                                " VALUES (@p_availability, @p_batch, @p_good_parts_count, @p_good_parts_date_start, @p_good_parts_date_end, @p_ideal_cycle_time, @p_machining_centre, @p_machining_date_start, @p_machining_time_start, @p_machining_date_end, @p_machining_time_end, @p_ooe, @p_performance, @p_quality, @p_workpiece_id)", DP);

                            lastAPIDataChange = dateStart.ToString();
                        } else
                        {
                            lastAPIDataChange = DateTime.Parse(dt.Rows[0]["machining_date_start"].ToString().Substring(0, 10) + " " + dt.Rows[0]["machining_time_start"].ToString()).ToString();
                        }
                        CalculateKPI(kpiData.MachiningDateStart, kpiData.MachiningCentre.ToString());
                    }
                }
            }
        }

        public void CalculateKPI(string kpiDate = "", string machiningCentre = "")
        {
            Dictionary<string, string> DP = new Dictionary<string, string>();
            double availability; // The availability in percent
            double performance; // Performance in percent
            double quality; // Quality in percent
            double ooe; // OOE in percent

            double machineToolUptime = 0;
            double machineToolDowntime = 0;
            double idealCycleTime = 0;
            double totalDowntimeOnDay = 0;
            int totalPartsCounter = 0;
            int allParts = 0;
            string workPieceId = "";
            string batch = "";
            string goodPartsDate = "";
            int goodPartsCount = 0;

            if (kpiDate == "")
            {
                Console.WriteLine("No date for KPI-calculation sent");
                return;
            }

            if (machiningCentre == "")
            {
                Console.WriteLine("No machining-centre for KPI-calculation sent");
                return;
            }

            Console.WriteLine("+++++++++++++++++++++++++++++++++++++ Start KPI-Calculation for " + kpiDate + " +++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("Current date: " + DateTime.Now.ToString() + ", last data change: " + GetLastAPIDataChange());
            // Get all entries of the selected day
            DP["@p_machining_date_start"] = kpiDate;
            DP["@p_machining_centre"] = machiningCentre;
            DataTable dt = new DataTable();
            dt = DBHelper.Query("SELECT * FROM KPI_DATA_RAW " +
                " WHERE machining_centre = @p_machining_centre AND machining_date_start = @p_machining_date_start " +
                " ORDER BY machining_time_start ASC", DP);
            if (dt.Rows.Count == 0)
            {
                Console.WriteLine("No entries found for  " + kpiDate + " -> stop execution");
                return;
            }
            int secondsToday = 0;
            DateTime dateEnd = DateTime.Now;
            foreach (DataRow dr in dt.Rows)
            {
                workPieceId = dr["workpiece_id"].ToString();
                batch = dr["batch"].ToString();
                goodPartsDate = DateTime.Parse(dr["good_parts_date_start"].ToString()).ToString("yyyy-MM-dd");
                int.TryParse( dr["good_parts_count"].ToString(), out goodPartsCount);
                DateTime dateStart = DateTime.Parse(dr["machining_date_start"].ToString().Substring(0, 10) + " " + dr["machining_time_start"]);
                dateEnd = DateTime.Parse(dr["machining_date_end"].ToString().Substring(0, 10) + " " + dr["machining_time_end"]);
                double.TryParse(dr["ideal_cycle_time"].ToString(), out idealCycleTime);
                var difference = (dateEnd - dateStart).TotalSeconds;
                if (difference < (idealCycleTime * 1.5) && difference > 0) // Time between two workpieces is less than ideal cycle time + 50% => uptime
                {
                    machineToolUptime += difference;
                    totalPartsCounter++;
                }

                allParts++;
            }
            secondsToday = (int)(dateEnd - DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"))).TotalSeconds;
            
            machineToolDowntime = secondsToday - machineToolUptime;
            Console.WriteLine("MachineToolUptime: " + machineToolUptime + ", MachineToolDowntime: " + machineToolDowntime +", AllParts: " + allParts + ", TotalPartsCounter: " + totalPartsCounter);
            Console.WriteLine("Seconds today: " + secondsToday + ", machineToolDowntime = secondsToday - machineToolUptime ~ " + secondsToday + " - " + machineToolUptime + " = " + machineToolDowntime);

            // Calculate Availability
            availability = (machineToolUptime + machineToolDowntime) > 0 ? machineToolUptime / (machineToolUptime + machineToolDowntime) : 0;
            Console.WriteLine("Availability = machineToolUptime / (machineToolUptime + machineToolDowntime) ~ " + machineToolUptime + "/" + "(" + machineToolUptime + " + " + machineToolDowntime + "): " + availability);

            // Calculate Performance
            performance = machineToolUptime > 0 ? (idealCycleTime * totalPartsCounter) / machineToolUptime : 0;
            Console.WriteLine("Performance = (idealCycleTime * totalpartsCounter) / machineToolUptime ~ (" + idealCycleTime + " * " + totalPartsCounter + ") / " + machineToolUptime + " : " + performance);

            // Write availability and performance for the current day
            // Check if entry for current day exists:
            DP["@p_date"] = kpiDate;
            DP["@p_machining_centre"] = machiningCentre;
            dt = DBHelper.Query("SELECT id FROM KPI_DATA WHERE machining_centre = @p_machining_centre AND date = @p_date", DP);
           
            DP["@p_availability"] = (availability * 100).ToString().Replace(',', '.');
            DP["@p_performance"] = (performance * 100).ToString().Replace(',', '.');
            DP["@p_workpiece_id"] = workPieceId;
            DP["@p_batch"] = batch;
            DP["@p_total_parts_count"] = totalPartsCounter.ToString();
            DP["@p_calculated_cycle_time"] = ((float)machineToolUptime / (float)totalPartsCounter).ToString().Replace(',', '.');
            DP["@p_ideal_cycle_time"] = idealCycleTime.ToString().Replace(',', '.');
            if (dt.Rows.Count > 0)
            {
                DP["@p_id"] = dt.Rows[0]["id"].ToString();
                var result = DBHelper.Exec("UPDATE KPI_DATA SET availability = @p_availability, performance = @p_performance, total_parts_count = @p_total_parts_count, " +
                    " calculated_cycle_time = @p_calculated_cycle_time, ideal_cycle_time = @p_ideal_cycle_time WHERE id = @p_id", DP);
            }
            else
            {
                Console.WriteLine("No entry -> insert");
                var result = DBHelper.Exec("INSERT INTO KPI_DATA (date,    availability,    performance,    batch,    workpiece_id,    machining_centre,    total_parts_count,     calculated_cycle_time,   ideal_cycle_time) " +
                            "                             VALUES (@p_date, @p_availability, @p_performance, @p_batch, @p_workpiece_id, @p_machining_centre, @p_total_parts_count, @p_calculated_cycle_time, @p_ideal_cycle_time)", DP);
            }

            // Check if quality and ooe is set for the day the good_parts_count is delivered
            Console.WriteLine("Check previous entry for: " + goodPartsDate);
            DP.Clear();
            DP["@p_date"] = goodPartsDate;
            DP["@p_workpiece_id"] = workPieceId;
            DP["@p_batch"] = batch;
            dt = DBHelper.Query("SELECT id, date, availability, performance, quality, ooe, total_parts_count FROM KPI_DATA WHERE date = @p_date AND workpiece_id = @p_workpiece_id AND batch = @p_batch", DP);
            if (dt.Rows.Count != 0)
            {
                System.Diagnostics.Debug.WriteLine("Existing entry found for " + goodPartsDate + " -> Check if quality and ooe is set. If not: Calculate and set them");
                if (dt.Rows[0]["quality"].ToString() == "" || dt.Rows[0]["quality"].ToString() == "0")
                {
                    Console.WriteLine("quality and ooe not set!");
                    quality = totalPartsCounter > 0 ? (float)goodPartsCount / (float)totalPartsCounter : 0;
                    Console.WriteLine("Quality = (goodPartsCount / totalPartsCounter) ~ " + goodPartsCount + " / " + totalPartsCounter + " = " + quality);
                    Double.TryParse(dt.Rows[0]["availability"].ToString(), out availability);
                    Double.TryParse(dt.Rows[0]["performance"].ToString(), out performance);
                    ooe = (availability / 100) * (performance / 100) * quality;
                    Console.WriteLine("OOE = Availability * Performance * Quality ~ " + Math.Round(availability/100, 2) + "*" + Math.Round(performance / 100, 2) + "*" + quality + " = " + ooe);

                    Console.WriteLine("Update KPI for " + goodPartsDate);
                    DP["@p_id"] = dt.Rows[0]["id"].ToString();
                    DP["@p_quality"] = (quality * 100).ToString().Replace(",", ".");
                    DP["@p_ooe"] = (ooe * 100).ToString().Replace(",", ".");
                    DP["@p_good_parts_count"] = goodPartsCount.ToString();

                    var result = DBHelper.Exec("UPDATE KPI_DATA SET quality = @p_quality, ooe = @p_ooe, good_parts_count = @p_good_parts_count WHERE id = @p_id", DP);
                }
            }
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++ KPI-Calculation finished +++++++++++++++++++++++++++++++++++++");
            return;

            /*
             KPI-Calculation:
            machineToolUptime: Sum of all processing times that are lower than ideal cycle time + 50%
            machineToolDowntime: Sum of all downtimes per processing + (86400 - machineToolUptime)
            if (difference > (idealCycleTime * 1.5)) // Time between two workpieces is more than ideal cycle time + 50% => downtime
            {
                machineToolDowntime += (difference - idealCycleTime);
            } else
            {
                machineToolUptime += difference;
                totalPartsCounter++;
            }

            Availability = machineToolUptime / (machineToolUptime + machineToolDowntime) = Values from 0-1

            Performance = (idealCycleTime * totalpartsCounter) / machineToolUptime = Value from 0 - 1
            
            Quality = (goodPartsCount / totalPartsCounter) = Value from 0-1

            Availability * Performance * Quality = Value from 0 - 1
             */
        }
    }
}