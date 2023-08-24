using System;
using System.Text;

namespace LumaSynchronizationApp.Models
{
    public class SyncAPI {
        public string t1 { get; set;}
        public string t2 { get; set; }
        public string code { get; set; }  
        public string message { get; set; }


        public SyncAPI (string t1, string t2, string code, string message){
            this.t1 = t1;
            this.t2 = t2;
            this.code = code;
            this.message = message;     
        }
    }



}
