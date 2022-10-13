using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using Modding;
using Modding.Patches;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace HollowTwitch.Common
{
    public class Commands : CommandBase
    {
        [HKCommand("mod")]
        [Summary("Shows whether a mod is enabled or not.")]
        [Cooldown(0.5)]
        [TwitchOnly]
        public void Mod([RemainingText] string name)
        {
            var mods = ModHooks.GetAllMods(true, true);
            var mod = mods.FirstOrDefault(x => x.GetName().ToLower() == name.ToLower());
            if (mod != null)
            {
                if (mod is ITogglableMod togglable)
                {
                    if (!ModHooks.ModEnabled(togglable))
						Reply("The mod is not enabled.");
                    else
						Reply("The mod is enabled.");
                }
            }
            else
				Reply("The mod is not installed.");
        }

        [HKCommand("mods")]
        [Summary("Lists all loaded mods.")]
        [Cooldown(0.5)]
		[TwitchOnly]
		public void Mods()
        {
            var sb = new StringBuilder();
            sb.Append("Currently installed mods:");
            foreach (var mod in ModHooks.GetAllMods(true, true))
                sb.Append($" {mod.GetName()},");
            var res = sb.ToString();
            res = res.TrimEnd(',', ' ');
            if (res.Length > 500)
                res = res.Remove(500 - 5) + "+more";
			Reply(res);
        }

        [HKCommand("modversion")]
        [Summary("Gets the installed version of the specified mod.")]
        [Cooldown(0.5)]
		[TwitchOnly]
		public void ModVersion([RemainingText] string name)
        {
            var mods = ModHooks.GetAllMods(true, true);
            var mod = mods.FirstOrDefault(x => x.GetName().ToLower() == name.ToLower());
            if (mod == null)
				Reply("The specifified mod is not installed.");
            else
				Reply($"The version is \'{mod.GetVersion()}\'.");
        }

        [HKCommand("charms")]
        [Summary("Gets ids of all equipped charms.")]
        [Cooldown(0.5)]
		[TwitchOnly]
		public void Charms()
        {
            var equipped = PlayerData.instance.equippedCharms;
            var sb = new StringBuilder();
            sb.Append("Currently equipped charms:");
            foreach (var charmId in equipped)
                sb.Append($" {GetCharmName(charmId)},");
            var res = sb.ToString();
            res = res.TrimEnd(',', ' ');
            if (res.Length > 500)
                res = res.Remove(500 - 5) + "+more";
            Reply(res);
        }

        private static string GetCharmName(int id)
        {
            return id switch
            {
                1 => "Gathering Swarm",
                2 => "Wayward Compass",
                3 => "Grubsong",
                4 => "Stallwart Shell",
                5 => "Baldur Shell",
                6 => "Fury of the Fallen",
                7 => "Quick Focus",
                8 => "Lifeblood Heart",
                9 => "Lifeblood Core",
                10 => "Defender's Crest",
                11 => "Flukenest",
                12 => "Thorns of Agony",
                13 => "Mark of Pride",
                14 => "Steady Body",
                15 => "Heavy Blow",
                16 => "Sharp Shadow",
                17 => "Spore Shroom",
                18 => "Longnail",
                19 => "Shaman Stone",
                20 => "Soul Catcher",
                21 => "Soul Eater",
                22 => "Glowing Womb",
                23 => (PlayerData.instance.fragileHealth_unbreakable ? "Unbreakable" : "Fragile") + " Heart",
                24 => (PlayerData.instance.fragileGreed_unbreakable ? "Unbreakable" : "Fragile") + " Greed",
                25 => (PlayerData.instance.fragileStrength_unbreakable ? "Unbreakable" : "Fragile") + " Strength",
                26 => "Nailmaster's Glory",
                27 => "Joni's Blessing",
                28 => "Shape of Unn",
                29 => "Hiveblood",
                30 => "Dream Wielder",
                31 => "Dashmaster",
                32 => "Quick Slash",
                33 => "Spell Twister",
                34 => "Deep Focus",
                35 => "Grubberfly's Energy",
                36 => PlayerData.instance.gotShadeCharm ? "Void Heart" : "Kingsoul",
                37 => "Sprintmaster",
                38 => "Dreamshield",
                39 => "Weaversong",
                40 => PlayerData.instance.grimmChildLevel == 5 ? "Carefree melody" : $"Grimmchild level {PlayerData.instance.grimmChildLevel}",
                _ => throw new InvalidDataException($"The specified charm of ID {id} was not found."),
            };
        }

        [HKCommand("bugreport")]
        [Summary("Creates a bug report.")]
        [Cooldown(5)]
		[TwitchOnly]
		[AdminOnly]
        public void Modlog()
        {
            int failed = 0;

            var save = JsonConvert.SerializeObject(new SaveGameData(GameManager.instance.playerData, GameManager.instance.sceneData), Formatting.Indented, new JsonSerializerSettings()
            {
                ContractResolver = ShouldSerializeContractResolver.Instance,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = JsonConverterTypes.ConverterTypes
            });
            var cd = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            cd = cd.CreateSubdirectory($"bugreport_{DateTime.Now:MM_dd_yyyy.HH_mm}");
            try { File.WriteAllText(Path.Combine(cd.FullName, "playerdata.json"), save); }
            catch { failed++; }
            try { File.Copy(Path.Combine(UnityEngine.Application.persistentDataPath, @"ModLog.txt"), Path.Combine(cd.FullName, @"ModLog.txt")); }
            catch { failed++; }
            try { File.Copy(Path.Combine(UnityEngine.Application.persistentDataPath, @"Player.log"), Path.Combine(cd.FullName, @"Player.log")); }
            catch { failed++; }

            if (failed == 0)
                Reply("Successfully made bug log.");
            else if (failed == 3)
				Reply("Failed to create bug log.");
            else
				Reply("Some steps failed. Partial bug log was created.");
        }
    }
}
