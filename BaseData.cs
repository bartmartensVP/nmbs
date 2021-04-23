using System;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace vlapa.nmbs
{

    public class Stop
    {
        public String stop_id;
        public String stop_name;
        public double stop_lat;
        public double stop_lon;

        public Stop(String dataString)
        {
            String[] parts = dataString.Split(",");
            double d;
            if (Double.TryParse(parts[4], out d))
            {
                stop_id = parts[0];
                stop_name = parts[2];
                stop_lat = Double.Parse(parts[4].Replace(".", ","));
                stop_lon = Double.Parse(parts[5].Replace(".", ","));
            }
        }
    }

    public class Trip
    {
        public String trip_id;
        public String route_id;
        public String service_id;
        public String trip_headsign;

        public Trip(String dataString)
        {
            String[] parts = dataString.Split(",");
            trip_id = parts[2];
            route_id = parts[0];
            service_id = parts[1];
            trip_headsign = parts[3];
        }
    }

    public class CalendarDate
    {
        public String service_id;
        public String date;

        public CalendarDate(String dataString)
        {
            String[] parts = dataString.Split(",");
            int year = int.Parse(parts[1].Substring(0, 4));
            int month = int.Parse(parts[1].Substring(4, 2));
            int day = int.Parse(parts[1].Substring(6, 2));
            date = parts[1];
            service_id = parts[0];
        }
    }

    public class StopTime
    {
        public String trip_id;
        public String stop_id;
        public String arrival_time;
        public String departure_time;

        public StopTime(String dataString)
        {
            String[] parts = dataString.Split(",");
            trip_id = parts[0];
            stop_id = parts[3];
            arrival_time = parts[1];
            departure_time = parts[2];
        }
    }

    public class StopTimeOverride
    {
        public String trip_id;
        public String service_id;
        public String stop_id;

        public StopTimeOverride(String dataString)
        {
            String[] parts = dataString.Split(",");
            trip_id = parts[0];
            service_id = parts[2];
            stop_id = parts[3];
        }
    }

    public class BoardItem
    {
        public String trip_id { get; set; }
        public String headsign{ get; set; }
        public String route_id{ get; set; }
        public String stop_id{ get; set; }
        public String arrival_time{ get; set; }
        public String departure_time{ get; set; }
    }
    public class BaseData
    {
        public string dataPath = System.Environment.GetEnvironmentVariable("DATA_PATH");
        public string baseFile = "rawdata.zip";
        public Dictionary<String, Stop> stops = new Dictionary<string, Stop>();
        public Dictionary<String, Trip> trips = new Dictionary<string, Trip>();
        public List<CalendarDate> calendarDates = new List<CalendarDate>();
        public List<StopTime> stopTimes = new List<StopTime>();
        public List<StopTimeOverride> stopTimesOverrides = new List<StopTimeOverride>();

        public void loadStops(String fileName)
        {
            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);
                foreach (String l in lines)
                {
                    Stop s = new Stop(l);
                    if (s.stop_id != null) stops.Add(s.stop_id, s);
                }
            }
        }

        public void loadTrips(String fileName)
        {
            if (File.Exists(fileName))
            {
                String[] lines = File.ReadAllLines(fileName);
                Boolean first = true;
                foreach (String l in lines)
                {
                    if (!first)
                    {
                        Trip tr = new Trip(l);
                        trips.Add(tr.trip_id, tr);
                    }
                    first = false;
                }
            }
        }

        public void loadCalendarDates(String fileName)
        {
            if (File.Exists(fileName))
            {
                String[] lines = File.ReadAllLines(fileName);
                Boolean first = true;
                foreach (String l in lines)
                {
                    if (!first)
                    {
                        CalendarDate cd = new CalendarDate(l);
                        calendarDates.Add(cd);
                    }
                    first = false;
                }
            }
        }

        public void loadStopTimes(String fileName)
        {
            if (File.Exists(fileName))
            {
                String[] lines = File.ReadAllLines(fileName);
                Boolean first = true;
                foreach (String l in lines)
                {
                    if (!first)
                    {
                        StopTime st = new StopTime(l);
                        stopTimes.Add(st);
                    }
                    first = false;
                }
            }
        }


        public void itemsOnBoard(String stop_id)
        {
            List<BoardItem> items = new List<BoardItem>();

            String vandaag = DateTime.Now.ToString("yyyyMMdd");

            List<CalendarDate> servicesVandaag = new List<CalendarDate>();
            foreach (CalendarDate cd in calendarDates)
            {
                if (cd.date.Equals(vandaag)) servicesVandaag.Add(cd);
            }

            List<StopTime> trainsFound = new List<StopTime>();
            foreach (StopTime st in stopTimes)
            {
                if (st.stop_id.Equals(stop_id)) trainsFound.Add(st);
            }

            foreach (StopTime t in trainsFound)
            {
                Trip trip = trips[t.trip_id];
                Boolean rijdtVandaag = false;
                foreach (CalendarDate cd in servicesVandaag)
                {
                    if (cd.service_id.Equals(trip.service_id)) rijdtVandaag = true;
                }

                if (rijdtVandaag)
                {
                    BoardItem item = new BoardItem();
                    item.headsign = trip.trip_headsign;
                    item.arrival_time = t.arrival_time;
                    item.departure_time = t.departure_time;
                    item.trip_id = t.trip_id ;
                    items.Add(item);
                }

            };

            String jsonString = JsonSerializer.Serialize(items);
            File.WriteAllText(dataPath + stop_id + ".json", jsonString);

        }
        public void loadStopTimeOverrides(String fileName)
        {
            if (File.Exists(fileName))
            {
                String[] lines = File.ReadAllLines(fileName);
                Boolean first = true;
                foreach (String l in lines)
                {
                    if (!first)
                    {
                        StopTimeOverride sto = new StopTimeOverride(l);
                        stopTimesOverrides.Add(sto);
                    }
                    first = false;
                }
            }

        }
        public BaseData()
        {

            String fileName = dataPath + baseFile;
            Boolean download = true;
            if (File.Exists(fileName))
            {
                DateTime versionDate = File.GetLastWriteTime(fileName);
                DateTime nu = DateTime.Now;
                TimeSpan age = DateTime.Now.Subtract(versionDate);
                if (age.Hours < 10) download = false;
            }
            if (download)
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(@"https://sncb-opendata.hafas.de/gtfs/static/c21ac6758dd25af84cca5b707f3cb3de", fileName);
                ZipFile.ExtractToDirectory(fileName, dataPath, true);
            }
            loadStops(dataPath + "stops.txt");
            loadTrips(dataPath + "trips.txt");
            loadCalendarDates(dataPath + "calendar_dates.txt");
            loadStopTimes(dataPath + "stop_times.txt");
            loadStopTimeOverrides(dataPath + "stop_time_overrides.txt");
        }

        public List<Stop> getStop(String name)
        {
            List<Stop> result = new List<Stop>();
            foreach (Stop st in stops.Values)
            {
                if (st.stop_name.Contains(name))
                {
                    result.Add(st);
                }
            }
            return result;
        }



    }

}