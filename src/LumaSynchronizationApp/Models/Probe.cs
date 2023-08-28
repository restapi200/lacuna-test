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

        public string EncodeTimestamp(TimeSpan timeSpan)
        {
            long ticksValue = timeSpan.Ticks;
            byte[] ticksBinaryBytes = BitConverter.GetBytes(ticksValue);
            switch (encoding)
            {
                case "Iso8601":
                    string formattedTimeSpan = timeSpan.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
                    string offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).ToString(@"hh\:mm"); // Obtém o deslocamento de fuso horário local

                    return formattedTimeSpan + offset;
                case "Ticks":
                    string base64EncodedValue = Convert.ToBase64String(ticksBinaryBytes, Base64FormattingOptions.None); // Codifica em base64
                    return timeSpan.Ticks.ToString();
                case "TicksBinary":
                    return Convert.ToBase64String(ticksBinaryBytes, Base64FormattingOptions.None).ToString();
                case "TicksBinaryBigEndian":
                    long ticksValueBigEndian = timeSpan.Ticks;
                    byte[] ticksBinaryBigEndianBytes = BitConverter.GetBytes(ticksValueBigEndian);
                    Array.Reverse(ticksBinaryBigEndianBytes);
                    return Convert.ToBase64String(ticksBinaryBigEndianBytes, Base64FormattingOptions.None).ToString();
                default:
                    throw new ArgumentException($"Unsupported encoding: {encoding}");
            }
        }
        public string DateTimeEncode(DateTime dateTime)
        {
            long ticksValue = dateTime.ToUniversalTime().Ticks; // Converte para UTC e obtém os ticks
            byte[] ticksBinaryBytes = BitConverter.GetBytes(ticksValue);

            switch (encoding)
            {
                case "Iso8601":
                    string formattedDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
                    string offset = TimeZoneInfo.Local.GetUtcOffset(dateTime).ToString(); // Obtém o deslocamento de fuso horário local

                    return formattedDateTime;
                case "Ticks":
                    string base64EncodedValue = Convert.ToBase64String(ticksBinaryBytes, Base64FormattingOptions.None).Replace(@"""", string.Empty); // Codifica em base64
                    return base64EncodedValue;
                case "TicksBinary":
                    return Convert.ToBase64String(ticksBinaryBytes, Base64FormattingOptions.None).Replace(@"""", string.Empty);
                case "TicksBinaryBigEndian":
                    long ticksValueBigEndian = dateTime.ToUniversalTime().Ticks;
                    byte[] ticksBinaryBigEndianBytes = BitConverter.GetBytes(ticksValueBigEndian);
                    Array.Reverse(ticksBinaryBigEndianBytes);
                    return Convert.ToBase64String(ticksBinaryBigEndianBytes, Base64FormattingOptions.None).Replace(@"""", string.Empty);
                default:
                    throw new ArgumentException($"Unsupported encoding: {encoding}");
            }
        }

    }
}
