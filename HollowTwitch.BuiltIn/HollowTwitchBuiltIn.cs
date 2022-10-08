using HollowTwitch.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HollowTwitch.BuiltIn
{
    public class HollowTwitchBuiltIn : ExtensionMod
    {
        public static readonly Version Version = new(1, 0, 0, 0);

        private static HollowTwitchBuiltIn instance;
        public static HollowTwitchBuiltIn Instance {
            get
            {
                if (instance == null)
                    throw new InvalidOperationException();
                return instance;
            }
        }

        public override string ExtensionName => "BuiltIn";

        public HollowTwitchBuiltIn()
        {
            instance = this;
        }

        public override string GetVersion() =>
            Version.ToString(4);

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            ObjectLoader.Load(preloadedObjects);
            ObjectLoader.LoadAssets();
            base.Initialize(preloadedObjects);
        }

        public override List<(string, string)> GetPreloadNames() =>
            ObjectLoader.ObjectList.Values.ToList();
    }
}
