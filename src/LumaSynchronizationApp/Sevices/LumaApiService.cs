using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LumaSynchronizationApp.Models;




namespace LumaSynchronizationApp.Services
{
    public class LumaApiService
    {
        private readonly HttpClient _httpClient;

        public ListSyncAPI ListSync;
        public ProbeResponse probeResponse;
        private string _accessToken;
        private TimeSpan _offset = TimeSpan.Zero;
        public bool _isClockSynchronized = false;
        public LumaApiService(string baseAddress)
        {
            ListSync = new ListSyncAPI();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
        }
        public async Task<bool> StartApiAsync(string username, string email)
        {
            var requestData = new
            {
                username,
                email
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("/api/start", requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                var responseObject = JsonSerializer.Deserialize<BaseResponse>(responseBody);
                responseObject.Debug();
                if (responseObject != null && responseObject.code == "Success")
                {

                    _accessToken = responseObject.accessToken;
                    return true;
                }else{
                    responseObject.Debug();
                    return false;
                }
            }

            return false;
        }
        public async Task<BaseResponse>  ProcessJobDetailsAsync(Job job, LumaApiService _apiService)
        {
            Console.WriteLine($"id: {job.id}");
            Console.WriteLine($"Name: {job.probeName}");

            var tprobe = _apiService.probeResponse.GetProbesByProbeName(job.probeName);
            var stimetripvalue = _apiService.ListSync.GetByProbeName(job.probeName);

            Console.WriteLine($"ID: {tprobe.id}");
            Console.WriteLine($"Name: {tprobe.name}");
            Console.WriteLine($"Encoding: {tprobe.encoding}");
            Console.WriteLine($"name: {stimetripvalue.probeName}");

            var stimetripvalueAtIndex = stimetripvalue.tripTimeOffser.TotalNanoseconds;
            Console.WriteLine($"stimetripvalueAtIndex: {stimetripvalueAtIndex}");
            DateTime now = DateTime.UtcNow + stimetripvalue.timeOffser;
            int tripTimeOffsetMilliseconds = (int)stimetripvalueAtIndex;
            object timetripvalue = tprobe.DateTimeEncode(now);

            Console.WriteLine($"valor enviado :'{timetripvalue}'");
            Console.WriteLine($"valor 1 recebido :'{stimetripvalue.encodet2}'");
            Console.WriteLine($"valor 2 recebido :'{stimetripvalue.encodet1}'");

            var probrepost = await _apiService.PerformJobCheckAsync(job.id, timetripvalue, tripTimeOffsetMilliseconds);

            Console.WriteLine($" apiService.PerformJobCheckAsync({job.id}, {timetripvalue}, {tripTimeOffsetMilliseconds})");
            return  probrepost;

        }
        public async Task<ProbeResponse> ListProbesAsync()
        {
            SetAuthorizationHeader();

            var response = await _httpClient.GetAsync("/api/probe");

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                var responseObject = JsonSerializer.Deserialize<ProbeResponse>(responseBody);
                if (responseObject.code == "Success"){
                    this.probeResponse = responseObject;
                    return responseObject;
                }
            }

            return null;
        }
        public async Task<SyncAPI> SyncProbeAsync(Probe probe)
        {
            if (probe == null)
            {
                return null;
            }

            DateTimeOffset clientBeforeRequest = DateTimeOffset.UtcNow;
            var response = await _httpClient.PostAsync($"/api/probe/{probe.id}/sync", null);
            DateTimeOffset serverAfterRequest = DateTimeOffset.UtcNow;

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var syncApiResponse = JsonSerializer.Deserialize<SyncAPI>(responseBody);

                if (syncApiResponse != null && syncApiResponse.code == "Success")
                {
                    DateTimeOffset serverBeforeResponse = probe.Decode(syncApiResponse.t1);
                    DateTimeOffset clientAfterResponse = probe.Decode(syncApiResponse.t2);

                    
                    TimeSpan roundTripDelay = CalculateRoundTripDelay(clientBeforeRequest, serverBeforeResponse, clientAfterResponse, serverAfterRequest);
                    TimeSpan offset = CalculateTimeOffset(clientBeforeRequest, serverBeforeResponse, clientAfterResponse, serverAfterRequest)+roundTripDelay;
                    //Console.WriteLine($"t1: {clientBeforeRequest}");
                    //Console.WriteLine($"t0: {serverAfterRequest}");
                    TimeSpan timeDifference = serverAfterRequest - clientBeforeRequest;
                    TimeSpan timeServerDiference = clientAfterResponse-serverBeforeResponse;
                    Console.WriteLine($"t1: {serverBeforeResponse}ms");
                    Console.WriteLine($"t2: {clientAfterResponse}ms");
                    Console.WriteLine($"t0: {clientBeforeRequest}ms");
                    Console.WriteLine($"t3: {serverAfterRequest}ms");
                    Console.WriteLine($"timeDifference: {timeDifference.TotalMilliseconds}ms");       
                    Console.WriteLine($"timeServerDiference: {timeServerDiference.TotalMilliseconds}ms");       
                    Console.WriteLine($"id: {probe.id}");
                    Console.WriteLine($"encode at: {probe.encoding}");
                    Console.WriteLine($"offset: {offset.TotalMilliseconds}ms");
                    Console.WriteLine($"RoundTripDelay: {roundTripDelay.TotalMilliseconds}ms"); 

                    // Add the calculated offset to the previous offset
                    _offset += offset;
                    var probeSync = new ProbeSync(
                    
                        probe.id,
                        syncApiResponse.t1,
                        syncApiResponse.t2,                       
                        clientBeforeRequest,
                        serverAfterRequest,
                        serverBeforeResponse,
                        clientAfterResponse,
                        offset,
                        roundTripDelay,
                        probe.name

);
                    Console.WriteLine($"offset_before: {_offset}");
                    Console.WriteLine($"____________________________________"); 

                    ListSync.ProbeSyncList.Add(probeSync);

                    if (Math.Abs(roundTripDelay.TotalMilliseconds) < 5) // Check if offset is less than 5ms
                    {
                        _isClockSynchronized = true;
                        return syncApiResponse;
                    }
                }
            }

            return null;
        }

        public async Task Done()
        {
                string jsonContent = "{}"; // Substitua pelo JSON desejado
                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                string url = "/api/start/2"; // Substitua pelo URL correto
                var response = await _httpClient.PostAsync(url,content);
                var name = response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Solicitação POST bem-sucedida!{name}");
                }
                else
                {
                    Console.WriteLine($"Falha na solicitação POST. Status code: {response.StatusCode}");
                }
        }
        public async Task<JobsResponse> JobsTake()
        {
            var response = await _httpClient.PostAsync("/api/job/take", null);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                var jsonresponse = JsonSerializer.Deserialize<JobsResponse>(responseBody);
                
                if (jsonresponse.code == "Success")
                {
                    return jsonresponse;
                }
            }
            
            return null;
        }
        public async Task<BaseResponse> PerformJobCheckAsync(string jobId, object probeNow, int roundTrip)
        {

            
            var checkRequest = new JobCheckRequest(probeNow, roundTrip);

            var checkRequestJson = JsonSerializer.Serialize(checkRequest);
            var content = new StringContent(checkRequestJson, Encoding.UTF8, "application/json");
            
            SetAuthorizationHeader();
            Console.WriteLine($"contexto enviado {checkRequestJson}");
            var checkResponse = await _httpClient.PostAsync($"/api/job/{jobId}/check", content);
            //Console.WriteLine($"Request URI: {checkResponse.RequestMessage.RequestUri}");
            //Console.WriteLine($"Request Method: {checkResponse.RequestMessage.Method}");
            //Console.WriteLine($"Request Headers: {checkResponse.RequestMessage.Headers}");
            //Console.WriteLine($"Response Status Code: {checkResponse.StatusCode}");
            //Console.WriteLine($"Response Headers: {checkResponse.Headers}");     
            //Console.Out.Flush();
        Console.WriteLine(checkResponse.IsSuccessStatusCode);
        Console.Out.Flush();
        if (checkResponse.IsSuccessStatusCode)
        {
            var responseBody = await checkResponse.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
            var checkApiResponse = JsonSerializer.Deserialize<BaseResponse>(responseBody);
            return checkApiResponse;
        }
        else
        {
            // Algo deu errado ao fazer a verificação do trabalho
            Console.WriteLine($"Request failed with status code: {checkResponse.StatusCode}");
            Console.WriteLine($"Response content: {await checkResponse.Content.ReadAsStringAsync()}");
            return new BaseResponse("Error", "Job check failed", $"{checkResponse}");
        }
        }
        public TimeSpan CalculateTimeOffset(DateTimeOffset clientBeforeRequest, DateTimeOffset serverAfterRequest, DateTimeOffset serverBeforeResponse, DateTimeOffset clientAfterResponse)
        {
            TimeSpan offset = ((serverAfterRequest - clientBeforeRequest) + (serverBeforeResponse - clientAfterResponse)) / 2 ;
            return offset;
        }

        public TimeSpan CalculateRoundTripDelay(DateTimeOffset clientBeforeRequest, DateTimeOffset serverAfterRequest, DateTimeOffset serverBeforeResponse, DateTimeOffset clientAfterResponse)
        {
            TimeSpan roundTripDelay = ((clientAfterResponse - clientBeforeRequest) - (serverBeforeResponse - serverAfterRequest)) ;
            return roundTripDelay;
        }

        public void SetAuthorizationHeader()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }
    }
}


