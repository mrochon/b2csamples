using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RESTFunctions.Models
{
    public class ErrorMsg
    {
        public ErrorMsg()
        {
            version = "1.0.1";
            status = HttpStatusCode.Conflict;
        }
        public string version { get; private set; }
        public HttpStatusCode status { get; set; }
        public string userMessage { get; set; }
    }
}
