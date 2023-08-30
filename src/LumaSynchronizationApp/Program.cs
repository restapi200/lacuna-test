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
            var probe = await apiService.ListProbesAsync();
            foreach (var prob in probe.probes)
            {
                var status = await apiService.SyncProbeAsync(prob);

                //Console.WriteLine($"ID: {prob.id}");
                //Console.WriteLine($"Name: {prob.name}");
                //Console.WriteLine($"Encoding: {prob.encoding}");
                //Console.WriteLine($"status {apiService._isClockSynchronized}");
                //Console.WriteLine($"status {status}");
               // Console.WriteLine($"__________________________________________________________________");
            }
            Console.WriteLine($"jobs");

            var jobresponse = await apiService.JobsTake();
            var jobrespons = await apiService.ProcessJobDetailsAsync(jobresponse.job, apiService);
            while (jobrespons.code == "Success"){
                jobresponse = await apiService.JobsTake();
                jobrespons = await apiService.ProcessJobDetailsAsync(jobresponse.job, apiService);
            }
            await apiService.Done();
            if (jobresponse != null){
                Console.WriteLine($"code: {jobresponse.code}");
                Console.WriteLine($"message: {jobresponse.message}");
                if (jobresponse.job != null) // Check if jobs list is not null
                {
                    var prob = jobresponse.job;
                    // debug 

                    Console.WriteLine($"id: {prob.id}");
                    Console.WriteLine($"Name: {prob.probeName}");
                    // end debug 

                    var tprobe = apiService.probeResponse.GetProbesByProbeName(prob.probeName);
                    var stimetripvalue =  apiService.ListSync.GetByProbeName(prob.probeName);
                    // debug 
                    
                        Console.WriteLine($"ID: {tprobe.id}");
                        Console.WriteLine($"Name: {tprobe.name}");
                        Console.WriteLine($"Encoding: {tprobe.encoding}");
                    
                    
                        Console.WriteLine($"name: {stimetripvalue.probeName}");
 
                }
            }
        }

    }
}     
    
