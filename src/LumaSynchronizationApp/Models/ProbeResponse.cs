namespace LumaSynchronizationApp.Models
{
    public class ProbeResponse
    {
        public List<Probe> probes { get; set; }
        public string code { get; set; }
        public string message { get; set; }


        public ProbeResponse (List<Probe> probes, string code, string message){
            this.probes = probes;
            this.code = code;
            this.message = message;     
        }
        public Probe GetProbeById(string id)
        {
            return probes.Find(probe => probe.id == id);
        }
        public List<Probe> GetProbesByProbeName(string probeName)
        {
            return probes.FindAll(probe => probe.name == probeName);
        }
    }
}