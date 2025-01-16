using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProfanityShock.Data;

namespace ProfanityShock.Config
{
    public class AccountConfig
    {
        public Uri Backend { get; set; } = new Uri("https://api.openshock.app");
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
