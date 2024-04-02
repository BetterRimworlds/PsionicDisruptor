using UnityEngine;
using Verse;

namespace BetterRimworlds.PsionicDisruptor
{
    public class PsionicDisruptor: Mod
    {
        public static Settings Settings;

        public PsionicDisruptor(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>() ?? new Settings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "PsionicDisruptor";
        }
    }
}
