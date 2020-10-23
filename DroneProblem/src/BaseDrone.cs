namespace DroneProblem
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Threading.Tasks;

    #endregion

    internal abstract class BaseDrone
    {
        private const int DRONE_SPEED = 1;

        #region

        internal BaseDrone(string inId) => Id = inId;

        #endregion

        #region Event Handling

        protected internal virtual event EventHandler<DroneInfo> DestinationReached;

        protected internal virtual event EventHandler ShuttingDown;

        protected internal virtual event EventHandler<TrafficReport> TrafficReport;

        #endregion

        #region Properties

        internal string Id { get; }

        protected virtual Queue<DroneInfo> Path { get; set; }

        #endregion

        internal virtual async Task QueuePath(IEnumerable<DroneInfo> inPath)
        {
            Path = new Queue<DroneInfo>(inPath);

            while (Path.Count != 0)
            {
                var csvDroneLine = Path.Dequeue();

                await MoveToNextPosition(csvDroneLine.GeoCoordinate);
                RaiseDestinationReached(csvDroneLine);
            }
        }

        internal virtual void ReportTrafficConditions(DateTime inTime) =>
            RaiseTrafficReport(new TrafficReport {DroneId = Id, Speed = 1, Time = inTime});

        internal virtual void Shutdown() => ShuttingDown?.Invoke(this, EventArgs.Empty);

        protected virtual Task MoveToNextPosition(GeoCoordinate inGeoCoordinate) => Task.Delay(DRONE_SPEED);

        protected virtual void RaiseDestinationReached(DroneInfo inInfo) =>
            DestinationReached?.Invoke(this, inInfo);

        protected virtual void RaiseTrafficReport(TrafficReport inTrafficReport) =>
            TrafficReport?.Invoke(this, inTrafficReport);
    }
}