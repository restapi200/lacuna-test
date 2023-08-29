using System;
using System.Text;

namespace LumaSynchronizationApp.Models
{
    public class SyncAPI {
    public string t1 { get; set; }
    public string t2 { get; set; }
    public string code { get; set; }
    public string message { get; set; }

    public SyncAPI(string t1, string t2, string code, string message) {
        this.t1 = t1;
        this.t2 = t2;
        this.code = code;
        this.message = message;
    }
    }

    public class ProbeSync
    {
        public DateTimeOffset t1 { get; set; }
        public DateTimeOffset t2 { get; set; }
        public string encodet1 { get; set; }
        public string encodet2 { get; set; }
        public DateTimeOffset t0 { get; set; }
        public DateTimeOffset t3 { get; set; }
        public TimeSpan timeOffser { get; set; }
        public TimeSpan tripTimeOffser { get; set; }
        public string probeId { get; set; }
        public string probeName { get; set; }   

        public ProbeSync( string probeId,string encodet1,string encodet2, DateTimeOffset t0, DateTimeOffset t1, DateTimeOffset t2, DateTimeOffset t3, TimeSpan timeOffser, TimeSpan tripTimeOffser, string probeName)
        {
            this.probeId = probeId;
            this.encodet1 = encodet1;
            this.encodet2 = encodet2;
            this.t1 = t1; // Converte TimeSpan para DateTimeOffset
            this.t2 = t2; // Converte TimeSpan para DateTimeOffset
            this.t0 = t0; // Converte TimeSpan para DateTimeOffset
            this.t3 = t3; // Converte TimeSpan para DateTimeOffset
            this.timeOffser = timeOffser;
            this.tripTimeOffser = tripTimeOffser;
            this.probeName = probeName;
        }
    }

    public class ListSyncAPI {
        public List<ProbeSync> ProbeSyncList { get; set; }
        public ListSyncAPI()
        {
            ProbeSyncList = new List<ProbeSync>(); // Inicializa a lista vazia
        }
        public ProbeSync GetById(string probeId)
        {
            return ProbeSyncList.FirstOrDefault(probe => probe.probeId == probeId);
        }
        public ProbeSync GetByProbeName(string probeName)
        {
            return ProbeSyncList.FirstOrDefault(probe => probe.probeName == probeName);
        }
    }

}