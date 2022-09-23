using HollowTwitch.Commands;
using HollowTwitch.Entities;
using HollowTwitch.Precondition;
using Modding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Camera = HollowTwitch.Commands.Camera;

namespace HollowTwitch.BuiltIn
{
    public class HollowTwitchBuiltIn : Mod, IGlobalSettings<Config>
    {
        public static readonly Version Version = new(1, 0, 0, 0);

        public Config Config { get; set; }

        public void OnLoadGlobal(Config s) =>
            Config = s;

        public Config OnSaveGlobal() =>
            Config;

        public HollowTwitchBuiltIn() : base("HollowTwitch.BuiltIn") { }

        public override string GetVersion() =>
            Version.ToString(4);

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            ObjectLoader.Load(preloadedObjects);
            ObjectLoader.LoadAssets();
            base.Initialize(preloadedObjects);
        }

        public override void Initialize()
        {
            TwitchMod.Instance.Processor.RegisterCommands<Player>();
            TwitchMod.Instance.Processor.RegisterCommands<Enemies>();
            TwitchMod.Instance.Processor.RegisterCommands<Area>();
            TwitchMod.Instance.Processor.RegisterCommands<Camera>();
            TwitchMod.Instance.Processor.RegisterCommands<Game>();
            TwitchMod.Instance.Processor.RegisterCommands<Meta>();
            ConfigureCooldowns();
            base.Initialize();
        }

        private void ConfigureCooldowns()
        {
            // No cooldowns configured, let's populate the dictionary.
            if (Config.Cooldowns.Count == 0)
            {
                foreach (Command c in TwitchMod.Instance.Processor.Commands)
                {
                    CooldownAttribute cd = c.Preconditions.OfType<CooldownAttribute>().FirstOrDefault();

                    if (cd == null)
                        continue;

                    Config.Cooldowns[c.Name] = (int)cd.Cooldown.TotalSeconds;
                }

                return;
            }

            foreach (Command c in TwitchMod.Instance.Processor.Commands)
            {
                if (!Config.Cooldowns.TryGetValue(c.Name, out int time))
                    continue;

                CooldownAttribute cd = c.Preconditions.OfType<CooldownAttribute>().First();

                cd.Cooldown = TimeSpan.FromSeconds(time);
            }
        }

        public override List<(string, string)> GetPreloadNames() => ObjectLoader.ObjectList.Values.ToList();

    }
}
