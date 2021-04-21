using System;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Collections.Generic;

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

    public class CalendarDate {
        public String service_id ;
        public DateTime date ;

        public CalendarDate( String dataString){
            String[] parts = dataString.Split (",") ;
            int year = int.Parse(parts[1].Substring (0,4) );
            int month = int.Parse(parts[1].Substring (4,2) );
            int day = int.Parse(parts[1].Substring (6,2) );
            date = new DateTime (year,month,day) ;
            service_id = parts[0] ;
        }
    }

    public class BaseData
    {
        public string dataPath = System.Environment.GetEnvironmentVariable("DATA_PATH");
        public string baseFile = "rawdata.zip";
        public Dictionary<String, Stop> stops = new Dictionary<string, Stop>();
        public Dictionary<String, Trip> trips = new Dictionary<string, Trip>();
        public List<CalendarDate> calendardates = new List<CalendarDate>() ;

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

        public void loadCalendarDates ( String fileName ) {
            if ( File.Exists ( fileName)){
                String[] lines = File.ReadAllLines ( fileName) ;
                Boolean first = true ;
                foreach ( String l in lines){
                    if (!first){
                        CalendarDate cd = new CalendarDate(l);
                        calendardates.Add ( cd) ;
                    }
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
                ZipFile.ExtractToDirectory(fileName, dataPath);
            }
            loadStops(dataPath + "stops.txt");
            loadTrips(dataPath + "trips.txt");
            loadCalendarDates(dataPath + "calendar_dates.txt") ;
        }

    }

}