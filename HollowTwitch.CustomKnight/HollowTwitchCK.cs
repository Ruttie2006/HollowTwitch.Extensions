using HollowTwitch.Extensibility;
using Modding;
using System;

namespace HollowTwitch.CustomKnight
{
    public class HollowTwitchCK : ExtensionMod
    {
        public static readonly Version Version = new(1, 1, 0, 0);

        public override string ExtensionName => "CustomKnight";

        public override string GetVersion() =>
            Version.ToString(4);
    }
}
