using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwellerBot.Models
{
    public class ChangelogContainer
    {
        public AppVersion[] Versions { get; set; }
    }

    public class AppVersion
    {
        public string versionNumber { get; set; }
        public string releaseDate { get; set; }
        public string description { get; set; }
    }
}