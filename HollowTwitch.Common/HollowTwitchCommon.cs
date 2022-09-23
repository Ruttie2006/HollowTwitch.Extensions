using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Common
{
    public class HollowTwitchCommon : Mod
    {
        public static readonly Version Version = new(1, 0, 0, 0);

        public HollowTwitchCommon() : base("HollowTwitch.Common") { }

        public override string GetVersion() =>
            Version.ToString(4);

        public override void Initialize()
        {
            TwitchMod.Instance.Processor.RegisterCommands<Commands>();
            base.Initialize();
        }
    }
}
