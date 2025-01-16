using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfanityShock.Config
{
    internal class SettingsConfig
    {
        public required string Language { get; set; } = "en-US";
        public required int MinConfidence { get; set; } = 90;
    }
}
