using HollowTwitch.Extensibility;
using System;

namespace HollowTwitch.Common
{
    public class HollowTwitchCommon : ExtensionMod
    {
        public static readonly Version Version = new(1, 0, 0, 0);

        public override string ExtensionName => "Common";

        public override string GetVersion() =>
            Version.ToString(4);
    }
}
