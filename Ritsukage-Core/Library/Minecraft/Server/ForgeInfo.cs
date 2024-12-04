using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Ritsukage.Library.Minecraft.Server
{
    /// <summary>
    /// Contains information about a modded server install.
    /// </summary>
    public class ForgeInfo
    {
        /// <summary>
        /// Represents an individual forge mod.
        /// </summary>
        public class ForgeMod
        {
            public ForgeMod(string ModID, string Version)
            {
                this.ModID = ModID;
                this.Version = Version;
            }

            public readonly string ModID;
            public readonly string Version;

            public override string ToString()
            {
                return ModID + " [" + Version + ']';
            }
        }

        public List<ForgeMod> Mods;

        /// <summary>
        /// Create a new ForgeInfo from the given data.
        /// </summary>
        /// <param name="data">The modinfo JSON tag.</param>
        internal ForgeInfo(JToken data)
        {
            Mods = new();
            foreach (var mod in data["modList"])
            {
                var modid = mod["modid"].ToString();
                var version = mod["version"].ToString();

                Mods.Add(new(modid, version));
            }
        }
    }
}