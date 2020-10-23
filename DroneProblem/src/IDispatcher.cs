namespace Derivco
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Threading.Tasks;

    #endregion

    internal interface IDispatcher
    {
        Task Dispatch();

        void Drone_DestinationReached(object sender, (GeoCoordinate Coordinate, DateTime Time) e);

        Task DroneControl(BaseDrone inBaseDrone, Queue<(GeoCoordinate, DateTime)> inDronePath);

        void LoadDrones(
            IDictionary<string, List<(GeoCoordinate Coordinate, DateTime Date)>> inDroneInfo);

        void LoadTubeStations(IDictionary<string, GeoCoordinate> inTubeInfo);
    }
}