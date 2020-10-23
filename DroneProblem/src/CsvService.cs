namespace Derivco
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Globalization;
    using System.Linq;

    using Microsoft.VisualBasic.FileIO;

    #endregion

    internal class CsvService : ICsvService
    {
        private const string DRONE_1 = @".\Resources\5937.csv";

        private const string DRONE_2 = @".\Resources\6043.csv";

        private const NumberStyles STYLE_FLAGS = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        private const string TUBE_LIST = @".\Resources\tube.csv";

        #region Fields

        private static readonly object _lockObject = new object();

        private static CsvService _instance;

        #endregion

        #region Properties

        internal static CsvService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        _instance = new CsvService();
                    }
                }

                return _instance;
            }

            set => _instance = value;
        }

        #endregion

        public IDictionary<string, List<(GeoCoordinate, DateTime)>> LoadDronesInfo()
        {
            var drone1Path = GetDroneInfoFromCsvLines(ReadCsvLines(DRONE_1));
            var drone2Path = GetDroneInfoFromCsvLines(ReadCsvLines(DRONE_2));

            var drones = new Dictionary<string, List<(GeoCoordinate, DateTime)>>();
            if (drone1Path != (null, null))
            {
                drones.Add(drone1Path.Id, drone1Path.Coordinates);
            }

            if (drone2Path != (null, null))
            {
                drones.Add(drone2Path.Id, drone2Path.Coordinates);
            }

            return drones;
        }

        public IDictionary<string, GeoCoordinate> LoadTubeInfo()
        {
            var tubeInfo = ReadCsvLines(TUBE_LIST);

            var dict = new Dictionary<string, GeoCoordinate>();
            tubeInfo.ForEach(tube => dict.Add(tube.Id, tube.Coordinate));
            return dict;
        }

        private (string Id, List<(GeoCoordinate, DateTime)> Coordinates) GetDroneInfoFromCsvLines(
            IEnumerable<(string Id, GeoCoordinate Coordinate, DateTime? Date)> inDroneInfo)
        {
            var droneGroup = inDroneInfo.GroupBy(l => l.Id).ToList();

            var droneInfo = droneGroup.FirstOrDefault();

            if (droneInfo == null)
            {
                return (null, null);
            }

            var droneId = droneInfo.Key;
            var droneInfoPath = droneInfo.Select(l => (l.Coordinate, l.Date.Value)).ToList();

            return (droneId, droneInfoPath);
        }

        private List<(string Id, GeoCoordinate Coordinate, DateTime? Date)> ReadCsvLines(string inCsvFilePath)
        {
            using (var csvParser =
                new TextFieldParser(inCsvFilePath ?? throw new ArgumentNullException(nameof(inCsvFilePath))))
            {
                csvParser.SetDelimiters(",");
                csvParser.HasFieldsEnclosedInQuotes = true;

                var lines = new List<(string, GeoCoordinate, DateTime?)>();

                while (!csvParser.EndOfData)
                {
                    var columns = csvParser.ReadFields();
                    //0-Id 1-Lat 2-Lon 3-Date
                    //0-Tube 1-Lat 2-Lon

                    var id = columns?[0];
                    if (string.IsNullOrEmpty(id))
                    {
                        throw new ArgumentException("Could not read Id.");
                    }

                    if (!double.TryParse(columns[1], STYLE_FLAGS, CultureInfo.InvariantCulture, out var parsedLat))
                    {
                        throw new ArgumentException("Could not parse Latitude.");
                    }

                    if (!double.TryParse(columns[2], STYLE_FLAGS, CultureInfo.InvariantCulture, out var parsedLong))
                    {
                        throw new ArgumentException("Could not parse Longitude.");
                    }

                    if (columns.Length > 3)
                    {
                        if (!DateTime.TryParse(columns[3], out var parsedDate))
                        {
                            throw new ArgumentException("Could not parse Date.");
                        }

                        lines.Add((id, new GeoCoordinate(parsedLat, parsedLong), parsedDate));
                    }
                    else
                    {
                        lines.Add((id, new GeoCoordinate(parsedLat, parsedLong), null));
                    }
                }

                return lines;
            }
        }
    }
}