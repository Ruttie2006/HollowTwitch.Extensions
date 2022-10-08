using HollowTwitch.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Randomizer
{
    public class HollowTwitchRando : ExtensionMod
    {
        public static readonly Version Version = new(1, 1, 0, 0);

        public override string ExtensionName => "Randomizer";

        public override string GetVersion() =>
            Version.ToString(4);
    }
}
