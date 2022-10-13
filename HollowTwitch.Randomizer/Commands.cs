using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using Modding;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.CustomKnight
{
    public class Commands : CommandBase
    {
        [HKCommand("rando-setting")]
        [Summary("Gets the value of a specific Randomizer setting.")]
        [Cooldown(0.5)]
        [TwitchOnly]
        public void Setting([RemainingText] string setting)
        {
            if (!RandomizerMod.RandomizerMod.IsRandoSave || RandomizerMod.RandomizerMod.GS == null || RandomizerMod.RandomizerMod.GS.DefaultMenuSettings == null)
            {
                Reply("The current save is not a randomizer save.");
                return;
            }
			Reply(RandomizerMod.RandomizerMod.GS.DefaultMenuSettings.Get(setting).ToString());
        }
    }
}
