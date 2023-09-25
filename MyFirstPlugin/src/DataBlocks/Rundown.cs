using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyFirstPlugin.GeneratorData;
using Newtonsoft.Json.Linq;

namespace MyFirstPlugin.DataBlocks
{
    internal class Rundown : DataBlock
    {
        public List<Level> TierA { get; set; } = new List<Level>();
        public List<Level> TierB { get; set; } = new List<Level>();
        public List<Level> TierC { get; set; } = new List<Level>();
        public List<Level> TierD { get; set; } = new List<Level>();
        public List<Level> TierE { get; set; } = new List<Level>();

        // Values we just leave as is
        public bool NeverShowRundownTree = false;
        public bool UseTierUnlockRequirements = false;
        public int VanityItemLayerDropDataBlock = 10;

        // No restrictions on which levels can be accessed, so these are all defaults
        public ReqToReachTier ReqToReachTierB = new ReqToReachTier();
        public ReqToReachTier ReqToReachTierC = new ReqToReachTier();
        public ReqToReachTier ReqToReachTierD = new ReqToReachTier();
        public ReqToReachTier ReqToReachTierE = new ReqToReachTier();

        public JObject StorytellingData = JObject.FromObject(new
        {
            Title = "Alt Rundown",
            TextLog = 1268,
            TextLogPos = new
            {
                x = 0.0,
                y = 0.0,
                magnitude = 0.0,
                sqrMagnitude = 0.0
            },
            Visuals = new
            {
                ColorBackground = new Color { Alpha = 1.0f },
                TierAVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = 0.75,
                    ScaleYModifier = 0.3
                },
                TierBVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = 0.75,
                    ScaleYModifier = 0.3
                },
                TierCVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = 0.75,
                    ScaleYModifier = 0.3
                },
                TierDVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = 0.75,
                    ScaleYModifier = 0.3
                },
                TierEVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = 0.75,
                    ScaleYModifier = 0.3
                },
            },
            SurfaceIconPosition = new
            {
                x = 0.0,
                y = 0.0,
                magnitude = 0.0,
                sqrMagnitude = 0.0
            },
            SurfaceDescription = 1267,
            ExternalExpTitle = 3901084012
        });

        static public Rundown Build()
        {
            var name = $"{Generator.Pick(Words.Adjectives)} {Generator.Pick(Words.NounsRundown)}";

            var rundown = new Rundown()
            {
                PersistentId = 1,
                Name = $"RND Rundown {name}"
            };

            rundown.StorytellingData["Title"] = $"<color=orange>ALT://</color>RUNDOWN #?\r\nTITLE: {name.ToUpper()}";

            return rundown;
        }
    }
}
