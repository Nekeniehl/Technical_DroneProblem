namespace DroneProblem
{
    #region Using

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    #endregion

    internal interface IDispatcher
    {
        ICsvService CsvService { get; set; }

        ConcurrentDictionary<BaseDrone, Queue<ICsvLine>> DronesPathInfo { get; }

        List<ICsvLine> TubeStations { get; set; }

        Task Dispatch();

        void Drone_DestinationReached(object sender, DroneInfo e);

        Task DroneControl(BaseDrone inBaseDrone, Queue<ICsvLine> inDronePath);

        void Init(IEnumerable<ICsvLine> inCsvFiles);
    }

    internal class Dispatcher : IDispatcher
    {
        public ICsvService CsvService { get; set; }

        public Dispatcher(ICsvService inService) => CsvService = inService;

        #region Properties

        public ConcurrentDictionary<BaseDrone, Queue<ICsvLine>> DronesPathInfo
        {
            get;
        }
            =
            new ConcurrentDictionary<BaseDrone, Queue<ICsvLine>>();

        public List<ICsvLine> TubeStations { get; set; } = new List<ICsvLine>();

        #endregion

        public async Task Dispatch()
        {
            var droneControTaskList = DronesPathInfo.Select(droneInfo => DroneControl(droneInfo.Key, droneInfo.Value)).ToList();

            await Task.WhenAll(droneControTaskList);
        }

        public void Drone_DestinationReached(object sender, DroneInfo inDroneInfo)
        {
            var drone = (BaseDrone) sender;

            Console.WriteLine($"Drone {drone.Id} reached coordinate {inDroneInfo.GeoCoordinate} at {inDroneInfo.Date.TimeOfDay}.");

            //Search for nearby stations
            foreach (var _ in TubeStations.Where(pair =>
                inDroneInfo.GeoCoordinate.GetDistanceTo(pair.GeoCoordinate) < 350))
            {   
                //Tell the drone to report the traffic
                ((BaseDrone) sender).ReportTrafficConditions(inDroneInfo.Date);
            }
        }

        public async Task DroneControl(BaseDrone inBaseDrone, Queue<ICsvLine> inDronePath)
        {
            DateTime shutdownTime = default;
            shutdownTime = shutdownTime.AddHours(8);
            shutdownTime = shutdownTime.AddMinutes(10);

            var shutdown = false;

            while (inDronePath.Count > 0 || !shutdown)
            {
                var buffer = new List<DroneInfo>(10);
                for (var i = 0; i < 10; i++)
                {
                    if (!inDronePath.Any())
                    {
                        //No more points on Path
                        shutdown = true;
                        break;
                    }

                    var path = (DroneInfo)inDronePath.Dequeue();

                    //Check time 08:10
                    if (shutdownTime.TimeOfDay < path.Date.TimeOfDay)
                    {
                        shutdown = true;
                        break;
                    }

                    buffer.Add(path);
                }

                await inBaseDrone.QueuePath(buffer);
            }

            //Shutdown drone command
            inBaseDrone.Shutdown();
        }

        private static void Drone_ShuttingDown(object sender, EventArgs e) =>
            Console.WriteLine($"Drone {((BaseDrone) sender).Id} shutting down.");

        public void Init(IEnumerable<ICsvLine> inCsvFiles)
        {
            var list = inCsvFiles.GroupBy(csvFile => csvFile.Id);

            foreach (var idGroup in list)
            {
                var id = idGroup.Key;
                var coords = idGroup.ToList();

                var first = coords.First();
                switch (first)
                {
                    case ICsvLineDate _:

                        var drone = new Drone(id);
                        drone.DestinationReached += Drone_DestinationReached;
                        drone.TrafficReport += Drone_TrafficReportReceived;
                        drone.ShuttingDown += Drone_ShuttingDown;

                        DronesPathInfo.TryAdd(drone, new Queue<ICsvLine>(coords.ToList()));
                        continue;

                    case ICsvLine tube:
                        TubeStations.Add(tube);
                        continue;
                }
            }
        }

        private static void Drone_TrafficReportReceived(object sender, TrafficReport e) =>
            Console.WriteLine($"Drone {e.DroneId} reporting traffic condition {e.TrafficCondition} at {e.Time.TimeOfDay}.");
    }
}