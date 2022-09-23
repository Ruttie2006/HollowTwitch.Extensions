using Modding;
using System;

namespace HollowTwitch.CustomKnight
{
    public class HollowTwitchCK : Mod
    {
        public static readonly Version Version = new(1, 0, 0, 0);

        public HollowTwitchCK() : base("HollowTwitch.CustomKnight") { }

        public override string GetVersion() =>
            Version.ToString(4);

        public override void Initialize()
        {
            TwitchMod.Instance.Processor.RegisterCommands<Commands>();
            base.Initialize();
        }
    }
}
