using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CRestApis.Models
{
    public class OTPEmail
    {
        public string email { get; set; }
        public string otp { get; set; }
        public override string ToString()
        {
            return $"email: {email}; otp: {otp}";
        }
    }
}
