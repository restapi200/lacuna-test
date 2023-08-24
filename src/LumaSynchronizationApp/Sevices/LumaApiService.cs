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
        private string _accessToken;
        private TimeSpan _offset = TimeSpan.Zero;
        public bool _isClockSynchronized = false;
        public LumaApiService(string baseAddress)
        {
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

        public async Task<ProbeResponse> ListProbesAsync()
        {
            SetAuthorizationHeader();

            var response = await _httpClient.GetAsync("/api/probe");

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<ProbeResponse>(responseBody);

                return responseObject;
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

                    TimeSpan offset = CalculateTimeOffset(clientBeforeRequest, serverAfterRequest, serverBeforeResponse, clientAfterResponse);
                    TimeSpan roundTripDelay = CalculateRoundTripDelay(clientBeforeRequest, serverAfterRequest, serverBeforeResponse, clientAfterResponse);

                    // Add the calculated offset to the previous offset
                    _offset += offset;

                    if (Math.Abs(_offset.TotalMilliseconds) < 5) // Check if offset is less than 5ms
                    {
                        _isClockSynchronized = true;
                        return syncApiResponse;
                    }
                }
            }

            return null;
        }
        public async Task<bool> SyncProbesAsync()
        {
            ProbeResponse probeResponse = await ListProbesAsync();

            if (probeResponse == null || probeResponse.probes == null || probeResponse.probes.Count == 0)
            {
                return false;
            }

            foreach (Probe probe in probeResponse.probes)
            {
                SyncAPI syncResult = await SyncProbeAsync(probe);
                Console.WriteLine(syncResult);
                if (syncResult.code == "Success")
                {
                    return false; // Return false if any synchronization fails
                }
            }

            return true; // Return true if all synchronizations succeed
        }
        public TimeSpan CalculateTimeOffset(DateTimeOffset clientBeforeRequest, DateTimeOffset serverAfterRequest, DateTimeOffset serverBeforeResponse, DateTimeOffset clientAfterResponse)
        {
            TimeSpan offset = ((serverAfterRequest - clientBeforeRequest) + (serverBeforeResponse - clientAfterResponse)) / TimeSpan.TicksPerMillisecond / 2;
            return offset;
        }

        public TimeSpan CalculateRoundTripDelay(DateTimeOffset clientBeforeRequest, DateTimeOffset serverAfterRequest, DateTimeOffset serverBeforeResponse, DateTimeOffset clientAfterResponse)
        {
            TimeSpan roundTripDelay = ((clientAfterResponse - clientBeforeRequest) - (serverBeforeResponse - serverAfterRequest)) / TimeSpan.TicksPerMillisecond;
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


