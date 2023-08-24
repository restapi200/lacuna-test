using System;
using System.Threading.Tasks;
using LumaSynchronizationApp.Models;
using LumaSynchronizationApp.Services;

namespace LumaSynchronizationApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var apiService = new LumaApiService("https://luma.lacuna.cc");
            bool startedSuccessfully = await apiService.StartApiAsync("yourUsername", "yourEmail@example.com");
            var probe = await apiService.SyncProbesAsync();
//            foreach (var prob in probe.probes)
//            {
//                Console.WriteLine($"ID: {prob.id}");
//                Console.WriteLine($"Name: {prob.name}");
//                Console.WriteLine($"Encoding: {prob.encoding}");
//                Console.WriteLine($"Encode TimeStamp : {prob.EncodeTimestamp()}");
//                Console.WriteLine($"");
//            }                   
            if (startedSuccessfully)
            {
                Console.WriteLine("API started successfully.");
                // Set authorization header for future API calls
                apiService.SetAuthorizationHeader();
                // Now you can proceed with other API calls using the apiService instance
                // For example:
                Console.WriteLine(apiService._isClockSynchronized);
            }
            else
            {
                Console.WriteLine("Failed to start API.");
            }
        }
    }
}
