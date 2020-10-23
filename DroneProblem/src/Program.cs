namespace Derivco
{
    using System;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            ICsvService csvService = CsvService.Instance;

            IDispatcher dispatcher = new Dispatcher();
            dispatcher.LoadTubeStations(csvService.LoadTubeInfo());
            dispatcher.LoadDrones(csvService.LoadDronesInfo());


            await dispatcher.Dispatch();

            Console.WriteLine("Simulation finished, press key to close.");
            Console.ReadKey();
        }
    }
}