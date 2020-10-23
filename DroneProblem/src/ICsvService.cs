namespace Derivco
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Device.Location;

    #endregion

    internal interface ICsvService
    {
        IDictionary<string, List<(GeoCoordinate, DateTime)>> LoadDronesInfo();

        IDictionary<string, GeoCoordinate> LoadTubeInfo();
    }
}