using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;

[assembly: ComponentFactory(typeof(PBChance.UI.Components.PBChanceFactory))]

namespace PBChance.UI.Components
{
    class PBChanceFactory : IComponentFactory
    {
        public string ComponentName => "PB Chance";

        public string Description => "Shows the probability of obtaining a new Personal Best this run.";

        public ComponentCategory Category => ComponentCategory.Information;

        public IComponent Create(LiveSplitState state) => new PBChanceComponent(state);

        public string UpdateName => ComponentName;

        public string XMLURL => "http://livesplit.org/update/Components/update.PBChance.xml";

        public string UpdateURL => "http://livesplit.org/update/";

        public Version Version => Version.Parse("1.4.3");
    }
}
