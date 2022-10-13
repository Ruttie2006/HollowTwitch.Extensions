using CustomKnight;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

namespace HollowTwitch.CustomKnight
{
    public class Commands : CommandBase
    {
        [HKCommand("ck-skin")]
        [Summary("Shows the currently equipped Custom Knight skin.")]
        [Cooldown(0.5)]
        [TwitchOnly]
        public void Skin()
        {
            var skin = SkinManager.GetCurrentSkin();
			Reply($"The currently equipped skin is \'{skin.GetName()}\'");
        }

        [HKCommand("ck-skins")]
        [Summary("Shows the currently installed Custom Knight skins.")]
        [Cooldown(0.5)]
		[TwitchOnly]
		public void Skins()
        {
            var skins = SkinManager.GetInstalledSkins();
            var sb = new StringBuilder();
            sb.Append("Currently installed skins:");
            foreach (var skin in skins)
                sb.Append($" {skin.GetName()},");
            var res = sb.ToString();
            res = res.TrimEnd(',', ' ');
            if (res.Length > 500)
                res = res.Remove(500 - 5) + "+more";
            Reply(res);
        }

        [HKCommand("ck-wear")]
        [Summary("Sets the currently installed Custom Knight skin.")]
        [Cooldown(5)]
        public IEnumerable Wear([RemainingText] string name)
        {
            yield return null;
            yield return new WaitForFinishedEnteringScene();
            var skins = SkinManager.GetInstalledSkins();
            var skin = skins.FirstOrDefault(x => x.GetName().ToLower() == name.ToLower());
            if (skin != null)
                SkinManager.SetSkinById(skin.GetId());
        }
    }
}
