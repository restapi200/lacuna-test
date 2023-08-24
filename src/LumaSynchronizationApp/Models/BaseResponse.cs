using System;

namespace LumaSynchronizationApp.Models
{
    public class BaseResponse
    {
        public string code { get; private set; }
        public string message { get; private set; }
        public string accessToken { get; private set; }
        public BaseResponse(string code, string message, string accessToken) // 
        {
            this.code = code;
            this.message = message;
            this.accessToken = accessToken;
        }

        public void Debug() {
           Console.WriteLine(this.code);
           Console.WriteLine(this.message);
           Console.WriteLine(this.accessToken);
        }
    }
}
