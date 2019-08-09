using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace DeviceTwinMismatch
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please supply connection string as the only argument.");
                Environment.Exit(-1);
            }

            var connectionString = args[0];
            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            var query = registryManager.CreateQuery("SELECT * FROM devices WHERE properties.desired != properties.reported", 100);
            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                foreach (var twin in page)
                {
                     twin.Properties.Desired.ClearMetadata();
                     twin.Properties.Reported.ClearMetadata();
                     var reportedAsString = twin.Properties.Reported.ToJson(Formatting.None);
                     var desiredAsString = twin.Properties.Desired.ToJson(Formatting.None);
                     if (!string.Equals(reportedAsString, desiredAsString, StringComparison.OrdinalIgnoreCase))
                     {
                         Console.WriteLine($"Device {twin.DeviceId} has properties that don't match.");
                     }
                }
            }

            Environment.Exit(0);
        }
    }
}
