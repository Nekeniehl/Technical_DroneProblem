namespace DroneProblem
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            ICsvService csvService = new CsvService();
            csvService.ReadAllFiles(@".\Resources");

            IDispatcher dispatcher = new Dispatcher(csvService);
            dispatcher.Init(csvService.CsvFiles);

            await dispatcher.Dispatch();

            Console.WriteLine("Simulation finished, press key to close.");
            Console.ReadKey();
        }
    }
}