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
        }
        public string version { get; private set; }
        public HttpStatusCode status { get; set; }
        public string userMessage { get; set; }
    }
}
