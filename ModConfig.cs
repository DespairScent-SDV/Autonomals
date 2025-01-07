using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DespairScent.Autonomals
{
    internal class ModConfig
    {
        public const string LOCATIONS_ANY = "Any";
        public const string LOCATIONS_BUILDABLE_ONLY = "Buildable Only";
        public const string LOCATIONS_FARM_ONLY = "Farm Only";

        public bool Enabled = true;
        public string AllowedLocations = LOCATIONS_BUILDABLE_ONLY;
    }
}
