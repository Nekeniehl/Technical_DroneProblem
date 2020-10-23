namespace Derivco
{
    #region Using

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Linq;
    using System.Threading.Tasks;

    #endregion

    internal class Dispatcher : IDispatcher
    {
        #region Properties

        private ConcurrentDictionary<BaseDrone, Queue<(GeoCoordinate GeoCoordinate, DateTime Date)>> DronesPathInfo
        {
            get;
        }
            =
            new ConcurrentDictionary<BaseDrone, Queue<(GeoCoordinate, DateTime)>>();

        private IDictionary<string, GeoCoordinate> TubeStations { get; set; } = new Dictionary<string, GeoCoordinate>();

        #endregion

        public async Task Dispatch()
        {
            var droneControTaskList = DronesPathInfo.Select(droneInfo => DroneControl(droneInfo.Key, droneInfo.Value))
                .ToList();

            await Task.WhenAll(droneControTaskList);
        }

        public void Drone_DestinationReached(object sender, (GeoCoordinate Coordinate, DateTime Time) e)
        {
            var drone = (BaseDrone) sender;

            Console.WriteLine($"Drone {drone.Id} reached coordinate {e.Coordinate} at {e.Time.TimeOfDay}.");

            //Search for nearby stations
            foreach (var keyValuePair in TubeStations.Where(keyValuePair =>
                e.Coordinate.GetDistanceTo(keyValuePair.Value) < 350))
            {
                //Tell the drone to report the traffic
                ((BaseDrone) sender).ReportTrafficConditions(e.Time);
            }
        }

        public async Task DroneControl(BaseDrone inBaseDrone, Queue<(GeoCoordinate, DateTime)> inDronePath)
        {
            DateTime shutdownTime = default;
            shutdownTime = shutdownTime.AddHours(8);
            shutdownTime = shutdownTime.AddMinutes(10);

            var shutdown = false;

            while (inDronePath.Count > 0 || !shutdown)
            {
                var buffer = new List<(GeoCoordinate, DateTime)>(10);
                for (var i = 0; i < 10; i++)
                {
                    if (!inDronePath.Any())
                    {
                        //No more points on Path
                        shutdown = true;
                        break;
                    }

                    var path = inDronePath.Dequeue();

                    //Check time 08:10
                    if (shutdownTime.TimeOfDay < path.Item2.TimeOfDay)
                    {
                        shutdown = true;
                        break;
                    }

                    buffer.Add((path.Item1, path.Item2));
                }

                await inBaseDrone.QueuePath(buffer);
            }

            //Shutdown drone command
            inBaseDrone.Shutdown();
        }

        public void LoadDrones(
            IDictionary<string, List<(GeoCoordinate Coordinate, DateTime Date)>> inDroneInfo)
        {
            var dronesToLoad = inDroneInfo.ToList();

            if (dronesToLoad.Any())
            {
                dronesToLoad.ForEach(dr =>
                {
                    var pathQueue = new Queue<(GeoCoordinate Coordinate, DateTime DateTime)>(dr.Value);

                    var drone = new Drone(dr.Key);

                    drone.DestinationReached += Drone_DestinationReached;
                    drone.TrafficReport += Drone_TrafficReportReceived;
                    drone.ShuttingDown += Drone_ShuttingDown;

                    DronesPathInfo.TryAdd(drone, pathQueue);
                });
            }
        }

        private void Drone_ShuttingDown(object sender, EventArgs e) =>
            Console.WriteLine($"Drone {((BaseDrone) sender).Id} shutting down.");
        public void LoadTubeStations(IDictionary<string, GeoCoordinate> inTubeInfo) => TubeStations = inTubeInfo;

        private static void Drone_TrafficReportReceived(object sender, TrafficReport e) =>
            Console.WriteLine($"Drone {e.DroneId} reporting traffic condition {e.TrafficCondition} at {e.Time.TimeOfDay}.");
    }
}