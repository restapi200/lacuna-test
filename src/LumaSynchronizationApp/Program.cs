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
            if (jobresponse != null){
                Console.WriteLine($"code: {jobresponse.code}");
                Console.WriteLine($"message: {jobresponse.message}");
                if (jobresponse.job != null) // Check if jobs list is not null
                {
                    var prob = jobresponse.job;

                    Console.WriteLine($"id: {prob.id}");
                    Console.WriteLine($"Name: {prob.probeName}");
                    var tprobe = apiService.probeResponse.GetProbesByProbeName(prob.probeName);
                    var stimetripvalue =  apiService.ListSync.GetByProbeName(prob.probeName);
                    
                    foreach (var item in tprobe){
                        Console.WriteLine($"ID: {item.id}");
                        Console.WriteLine($"Name: {item.name}");
                        Console.WriteLine($"Encoding: {item.encoding}");
                    }
                    foreach (var item in stimetripvalue){
                        Console.WriteLine($"name: {item.probeName}");
 
                    }
                    
                    for (int i = 0; i < tprobe.Count; i++)
                    {
                        var probFromTProbe = stimetripvalue[i].timeOffser;
                        var stimetripvalueAtIndex = stimetripvalue[i].tripTimeOffser.TotalNanoseconds;
                        int tripTimeOffsetMilliseconds = (int)stimetripvalueAtIndex;
                        DateTime intervalo = DateTime.UtcNow;
                        var timetripvalue = tprobe[i].DateTimeEncode(intervalo);
                        Console.WriteLine(timetripvalue);
                        Console.WriteLine(stimetripvalue[i].encodet2);
                        Console.WriteLine(stimetripvalue[i].encodet1);
                        var probrepost = await apiService.PerformJobCheckAsync(prob.id, probFromTProbe.ToString(), tripTimeOffsetMilliseconds);
                        Console.WriteLine($" apiService.PerformJobCheckAsync({prob.id}, {timetripvalue}, {tripTimeOffsetMilliseconds})");
                        if (probrepost.code =="Success"){
                            Console.WriteLine($"Sincronizado com sucesso {probrepost} performed successfully");
                        }else{
                            Console.WriteLine($"Falha |{probrepost.code}| performed failed |{probrepost.message}| statuscode |{probrepost}");
                            var json = await apiService.JobsTake();
                            Console.WriteLine(json.code);
                        }
                        // Agora você pode trabalhar com probFromTProbe e timetripvalue
                    }
                    //Console.WriteLine($"__________________________________________________________________");
                    
                }
            }
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
