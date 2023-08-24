using System;
using System.Text;

namespace LumaSynchronizationApp.Models
{
    public class Probe
    {
        public string id { get; private set; }
        public string name { get; private set; }
        public string encoding { get; private set; }
        
        public Probe(string id, string name, string encoding){
            this.id = id;
            this.name = name;
            this.encoding = encoding;   
        }
        public void Debug() {
            Console.WriteLine(this.id);
            Console.WriteLine(this.name);
            Console.WriteLine(this.encoding);
        }

        public DateTimeOffset Decode(string timestamp)
        {
            switch (this.encoding)
            {
                case "Iso8601":
                    return DateTimeOffset.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffffffzzz", null);
                case "Ticks":
                    long ticks = long.Parse(timestamp);
                    return new DateTimeOffset(ticks, TimeSpan.Zero);
                case "TicksBinary":
                    byte[] bytes = Convert.FromBase64String(timestamp);
                    long ticksBinary = BitConverter.ToInt64(bytes, 0);
                    return new DateTimeOffset(ticksBinary, TimeSpan.Zero);
                case "TicksBinaryBigEndian":
                    byte[] bigEndianBytes = Convert.FromBase64String(timestamp);
                    Array.Reverse(bigEndianBytes);
                    long ticksBinaryBigEndian = BitConverter.ToInt64(bigEndianBytes, 0);
                    return new DateTimeOffset(ticksBinaryBigEndian, TimeSpan.Zero);
                default:
                    // Return an invalid value or default value
                    return DateTimeOffset.MinValue;
            }
        }

       public string EncodeTimestamp()
        {
            DateTimeOffset dateTime = DateTimeOffset.UtcNow;

            switch (encoding)
            {
                case "Iso8601":
                    return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
                case "Ticks":
                    long ticksValue = dateTime.ToFileTime();
                    return ticksValue.ToString();
                case "TicksBinary":
                case "TicksBinaryBigEndian":
                    long ticksValueBinary = dateTime.ToFileTime();
                    byte[] ticksBinaryBytes = BitConverter.GetBytes(ticksValueBinary);
                    if (encoding == "TicksBinary")
                    {
                        Array.Reverse(ticksBinaryBytes);
                    }
                    return Convert.ToBase64String(ticksBinaryBytes);
                default:
                    throw new ArgumentException($"Unsupported encoding: {encoding}");
            }
        }
    }
}
