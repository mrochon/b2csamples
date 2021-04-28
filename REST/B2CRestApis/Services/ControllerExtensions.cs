using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CRestApis.Services
{
    public static class ControllerExtensions
    {
        public static JsonResult ErrorResponse(this ControllerBase ctrl, int code, string msg)
        {
            return new JsonResult(new
            {
                version = "1.0.1",
                status = code,
                userMessage = msg
            })
            { StatusCode = code };
        }
    }
}
