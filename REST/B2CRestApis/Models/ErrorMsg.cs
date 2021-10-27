using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace B2CRestApis.Models
{
    public class ErrorMsg
    {
        public ErrorMsg()
        {
            version = "1.0.1";
            status = 409;
        }
        public string version { get; set; }
        public int status { get; set; }
        public string userMessage { get; set; }

        // Optional
        public int code { get; set; }
        public string requestId { get; set; }
        public string developerMessage { get; set; }
        public string moreInfo { get; set; }
    }
}
