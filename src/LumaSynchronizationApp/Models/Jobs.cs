using System;
namespace LumaSynchronizationApp.Models{
    public class Job
    {
        public string id { get; set; }
        public string probeName { get; set; }
        
        public Job(string id, string probeName)
        {
            this.id = id;
            this.probeName = probeName;
        }
    }

    public class JobsResponse
    {
        public Job job { get; set; }
        public string code { get; set; }
        public string message { get; set; }

        public JobsResponse(Job job, string code, string message)
        {
            this.job = job;
            this.code = code;
            this.message = message;
        }
    }

    public class JobCheckRequest
    {
        public string probeNow { get; set; }
        public int roundTrip { get; set; }

        public JobCheckRequest(string probeNow, int roundTrip)
        {
            this.probeNow = probeNow;
            this.roundTrip = roundTrip;
        }
    }


}


