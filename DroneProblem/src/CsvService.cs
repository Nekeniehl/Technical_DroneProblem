namespace DroneProblem
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualBasic.FileIO;

    #endregion

    internal interface ICsvService
    {
        #region Properties

         List<ICsvLine> CsvFiles { get; set; }

        #endregion

        void ReadAllFiles(string inPath);
    }

    internal class CsvService : ICsvService
    {
        private const NumberStyles STYLE_FLAGS = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        #region Properties

        public List<ICsvLine> CsvFiles { get; set; } = new List<ICsvLine>();

        #endregion

        public void ReadAllFiles(string inPath)
        {
            var resDir = new DirectoryInfo(inPath);
            foreach (var csvFileInfo in resDir.GetFiles("*.csv"))
            {
                var csvGroup = ReadCsvLines(csvFileInfo.FullName).ToList();

                CsvFiles.AddRange(csvGroup.ToList());
            }
        }

        private static GeoCoordinate ParseCsvGeoCoordinate(string inLat, string inLong)
        {
            if (!double.TryParse(inLat, STYLE_FLAGS, CultureInfo.InvariantCulture, out var parsedLat))
            {
                throw new ArgumentException("Could not parse Latitude.");
            }

            if (!double.TryParse(inLong, STYLE_FLAGS, CultureInfo.InvariantCulture, out var parsedLong))
            {
                throw new ArgumentException("Could not parse Longitude.");
            }

            return new GeoCoordinate(parsedLat, parsedLong);
        }

        /// <summary>
        ///     0-Id 1-Lat 2-Lon 3-Date
        ///     0-Tube 1-Lat 2-Lon
        /// </summary>
        /// <param name="inLineFields"> </param>
        /// <returns> </returns>
        private static ICsvLine ReadCsvLine(IReadOnlyList<string> inLineFields)
        {
            ICsvLine csvLine;

            var id = inLineFields?[0];
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Could not read Id.");
            }

            var geoCoordinate = ParseCsvGeoCoordinate(inLineFields[1], inLineFields[2]);

            if (inLineFields.Count > 3)
            {
                if (!DateTime.TryParse(inLineFields[3], out var parsedDate))
                {
                    throw new ArgumentException("Could not parse Date.");
                }

                csvLine = new DroneInfo
                {
                    Id = id,
                    Date = parsedDate,
                    GeoCoordinate = geoCoordinate
                };
            }
            else
            {
                csvLine = new TubeInfo
                {
                    Id = id,
                    GeoCoordinate = geoCoordinate
                };
            }

            return csvLine;
        }

        private static IEnumerable<ICsvLine> ReadCsvLines(string inCsvFilePath)
        {
            using (var csvParser =
                new TextFieldParser(inCsvFilePath ?? throw new ArgumentNullException(nameof(inCsvFilePath))))
            {
                csvParser.SetDelimiters(",");
                csvParser.HasFieldsEnclosedInQuotes = true;

                var lines = new List<ICsvLine>();

                while (!csvParser.EndOfData)
                {
                    var csvLine = ReadCsvLine(csvParser.ReadFields());
                    lines.Add(csvLine);
                }

                return lines;
            }
        }
    }

    internal interface ICsvLine
    {
        #region Properties

        GeoCoordinate GeoCoordinate { get; set; }

        string Id { get; set; }

        #endregion
    }

    internal interface ICsvLineDate
    {
        #region Properties

        DateTime Date { get; set; }

        #endregion
    }

    internal class TubeInfo : ICsvLine
    {
        #region Properties

        /// <inheritdoc />
        public GeoCoordinate GeoCoordinate { get; set; }

        /// <inheritdoc />
        public string Id { get; set; }

        #endregion
    }

    internal class DroneInfo : ICsvLineDate, ICsvLine
    {
        #region Properties

        /// <inheritdoc />
        public DateTime Date { get; set; }

        /// <inheritdoc />
        public GeoCoordinate GeoCoordinate { get; set; }

        /// <inheritdoc />
        public string Id { get; set; }

        #endregion
    }
}