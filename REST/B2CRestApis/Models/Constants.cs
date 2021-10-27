using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CRestApis.Models
{
    public class Constants
    {
        public static readonly Dictionary<string, string[]> Domains = new Dictionary<string, string[]>
        {
            { "hsbc.com", new string[]
                {
                    "115.114.120.0/24" ,
                    "116.90.64.0/21"
                }
            },
            { "hotmail.com", new string[]
                {
                    "173.68.54.92/24"
                }
            },
            { "gmail.com", new string[]
                {
                    "173.68.54.92/24"
                }
            },
            { "microsoft.com", new string[]
                {
                    "67.160.24.0/24"
                }
            },
            { "gs.com", null }
        };
    }
}
